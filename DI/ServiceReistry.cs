using System;
using System.Collections.Generic;

namespace DI
{
    public class ServiceReistry
    {
        public Type ServiceType { get; }
        public Lifetime Lifetime { get; }
        public Func<Cat, Type[], object> Factory { get; }
        internal ServiceReistry Next { get; set; }

        public ServiceReistry(Type serviceType, Lifetime lifetime, Func<Cat, Type[], object> factory)
        {
            ServiceType = serviceType;
            Lifetime = lifetime;
            Factory = factory;
        }

        internal IEnumerable<ServiceReistry> AsEnumerable()
        {
            var list = new List<ServiceReistry>();
            for (var self = this; self != null; self = self.Next)
            {
                list.Add(self);
            }
            return list;
        }
    }
}
