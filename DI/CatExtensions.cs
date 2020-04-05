using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DI
{
    public static class CatExtensions
    {
        public static Cat Register(this Cat cat, Type from, Type to, Lifetime lifetime)
        {
            Func<Cat, Type[], object> factory = (_, arguments) => Create(_, to, arguments);
            cat.Register(new ServiceReistry(from, lifetime, factory));
            return cat;
        }

        public static Cat Register<TFrom, TTo>(this Cat cat, Lifetime lifetime) where TTo : TFrom
        {
            return cat.Register(typeof(TFrom), typeof(TTo), lifetime);
        }

        public static Cat Register<TService>(this Cat cat, TService instance)
        {
            Func<Cat, Type[], object> factory = (_, arguments) => instance;
            cat.Register(new ServiceReistry(typeof(TService), Lifetime.Root, factory));
            return cat;
        }

        public static Cat Register(this Cat cat, Type serviceType, Func<Cat, object> factory, Lifetime lifetime)
        {
            cat.Register(new ServiceReistry(serviceType, lifetime, (_, argumnets) => factory(_)));
            return cat;
        }

        public static Cat Register<TService>(this Cat cat, Func<Cat, TService> factory, Lifetime lifetime)
        {
            cat.Register(new ServiceReistry(typeof(TService), lifetime, (_, arguments) => factory(_)));
            return cat;
        }

        private static object Create(Cat cat, Type type, Type[] genericArguments)
        {
            if (genericArguments.Length > 0)
            {
                type = type.MakeGenericType(genericArguments);

            }

            var constructors = type.GetConstructors();
            if (constructors.Length == 0)
            {
                throw new InvalidOperationException($"{type}类型没有pulic级别的构造函数");
            }

            var constructor =
                constructors.FirstOrDefault(it => it.GetCustomAttributes(false).OfType<InjectionAttribute>().Any());
            constructor = constructor ?? constructors.First();
            var parameters = constructor.GetParameters();
            if (parameters.Length == 0)
            {
                return Activator.CreateInstance(type);
            }

            var arguments = new object[parameters.Length];
            for (int index = 0; index < arguments.Length; index++)
            {
                arguments[index] = cat.GetService(parameters[index].ParameterType);
            }

            return constructor.Invoke(arguments);
        }

        public static Cat Register(this Cat cat, Assembly assembly)
        {
            var typeAttributes = from type in assembly.GetExportedTypes()
                                 let attribute = type.GetCustomAttribute<MapToAttribute>()
                                 where attribute != null
                                 select new { ServiceType = type, Attribute = attribute };
            foreach (var typeAttribute in typeAttributes)
            {
                cat.Register(typeAttribute.Attribute.ServiceType, typeAttribute.ServiceType,
                    typeAttribute.Attribute.Lifetime);
            }
            return cat;
        }

        public static T GetService<T>(this Cat cat) => (T)cat.GetService(typeof(T));
        public static IEnumerable<T> GetServices<T>(this Cat cat) => cat.GetService<IEnumerable<T>>();

        public static Cat CreateChild(this Cat cat) => new Cat(cat);
    }
}
