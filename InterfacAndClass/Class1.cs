using System;
using DI;

namespace InterfacAndClass
{
    public interface IFoo { }
    public interface IBar { }
    public interface IBaz { }
    public interface IQux { }
    public interface IFoobar<T1, T2> { }

    public class Base : IDisposable
    {
        public Base()
        {
            Console.WriteLine($"创建类型{GetType().Name}");
        }

        public void Dispose()
        {
            Console.WriteLine($"释放类型{GetType().Name}");
        }
    }
    public class Foo : Base, IFoo { }
    public class Bar : Base, IBar { }
    public class Baz : Base, IBaz { }
    [MapTo(typeof(IQux), Lifetime.Root)]
    public class Qux : Base, IQux { }

    public class Foobar<T1, T2> : IFoobar<T1, T2>
    {
        public IFoo Foo { get; }
        public IBar Bar { get; }

        public Foobar(IFoo foo, IBar bar)
        {
            Foo = foo;
            Bar = bar;
        }
    }
}
