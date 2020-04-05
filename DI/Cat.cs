﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DI
{
    public class Cat : IServiceProvider, IDisposable
    {
        internal readonly Cat _root;
        internal readonly ConcurrentDictionary<Type, ServiceReistry> _registries;
        internal readonly ConcurrentDictionary<Key, object> _services;
        private readonly ConcurrentBag<IDisposable> _disposables;
        private volatile bool _disposed;

        public Cat()
        {
            _registries = new ConcurrentDictionary<Type, ServiceReistry>();
            _root = this;
            _services = new ConcurrentDictionary<Key, object>();
            _disposables = new ConcurrentBag<IDisposable>();
        }

        public Cat(Cat parent)
        {
            _root = parent._root;
            _registries = _root._registries;
            _services = new ConcurrentDictionary<Key, object>();
            _disposables = new ConcurrentBag<IDisposable>();

        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("Cat");
            }
        }

        public Cat Register(ServiceReistry registry)
        {
            EnsureNotDisposed();
            if (_registries.TryGetValue(registry.ServiceType, out var existing))
            {
                _registries[registry.ServiceType] = registry;
                registry.Next = existing;
            }
            else
            {
                _registries[registry.ServiceType] = registry;
            }
            return this;
        }

        private object GetServiceCore(ServiceReistry registry, Type[] genericArguments)
        {
            var key = new Key(registry, genericArguments);
            var serviceType = registry.ServiceType;
            switch (registry.Lifetime)
            {
                case Lifetime.Root: return GetOrCreate(_root._services, _root._disposables);
                case Lifetime.Self: return GetOrCreate(_services, _disposables);
                default:
                    {
                        var service = registry.Factory(this, genericArguments);
                        if (service is IDisposable disposable && disposable != this)
                        {
                            _disposables.Add(disposable);
                        }

                        return service;
                    }
            }

            object GetOrCreate(ConcurrentDictionary<Key, object> services, ConcurrentBag<IDisposable> disposables)
            {
                if (services.TryGetValue(key, out var service))
                {
                    return service;
                }

                service = registry.Factory(this, genericArguments);
                services[key] = service;
                if (service is IDisposable disposable)
                {
                    _disposables.Add(disposable);
                }

                return service;
            }
        }

        public object GetService(Type serviceType)
        {
            EnsureNotDisposed();
            if (serviceType == typeof(Cat) || serviceType == typeof(IServiceProvider))
            {
                return this;
            }

            ServiceReistry registry;
            if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var elementType = serviceType.GetGenericArguments()[0];
                //没找到返回空数组
                if (!_registries.TryGetValue(elementType, out registry))
                {
                    return Array.CreateInstance(elementType, 0);
                }

                var registries = registry.AsEnumerable();
                var services = registries.Select(it => GetServiceCore(it, Type.EmptyTypes)).ToArray();
                Array array = Array.CreateInstance(elementType, services.Length);
                services.CopyTo(array, 0);
                return array;
            }

            if (serviceType.IsGenericType && !_registries.ContainsKey(serviceType))
            {
                var definition = serviceType.GetGenericTypeDefinition();
                return _registries.TryGetValue(definition, out registry)
                    ? GetServiceCore(registry, serviceType.GetGenericArguments())
                    : null;
            }

            return _registries.TryGetValue(serviceType, out registry) ? GetServiceCore(registry, new Type[0]) : null;
        }

        public void Dispose()
        {
            _disposed = true;
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }

            while (!_disposables.IsEmpty)
            {
                _disposables.TryTake(out _);
            }
            _services.Clear();
        }
    }
}