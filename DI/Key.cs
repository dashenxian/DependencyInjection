using System;
using System.Collections.Generic;
using System.Text;

namespace DI
{
    internal class Key : IEquatable<Key>
    {
        public ServiceReistry Registry { get; }
        public Type[] GenericArguments { get; }

        public Key(ServiceReistry registry, Type[] genericArguments)
        {
            Registry = registry;
            GenericArguments = genericArguments;
        }
        public bool Equals(Key other)
        {
            if (Registry != other.Registry)
            {
                return false;
            }

            if (GenericArguments.Length != other.GenericArguments.Length)
            {
                return false;
            }

            for (int index = 0; index < GenericArguments.Length; index++)
            {
                if (GenericArguments[index] != other.GenericArguments[index])
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            var hashCode = Registry.GetHashCode();
            for (int index = 0; index < GenericArguments.Length; index++)
            {
                hashCode ^= GenericArguments[index].GetHashCode();
            }

            return hashCode;
        }

        public override bool Equals(object obj)
        {
            return obj is Key key && Equals(key);
        }
    }
}
