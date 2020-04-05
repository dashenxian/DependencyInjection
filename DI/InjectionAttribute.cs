using System;
using System.Collections.Generic;
using System.Text;

namespace DI
{
    [AttributeUsage(AttributeTargets.Constructor)]
    public class InjectionAttribute : Attribute
    {

    }
}
