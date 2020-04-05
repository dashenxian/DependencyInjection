using System;
using System.Collections.Generic;
using System.Text;

namespace DI
{
    public enum Lifetime
    {
        Root=1,
        Self=2,
        Transient=3,
    }
}
