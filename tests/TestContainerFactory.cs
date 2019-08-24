using Stator;

namespace StatorExample
{
    public partial class TestContainerFactory : ContainerFactory
    {
        public TestContainerFactory()
        {
            AddSingleton<Foo>();
            AddSingleton<Bar>();
            AddSingleton<FooBar>();
            AddSingleton<object>(nameof(CreateFoo));
            AddTransient<BarFoo>(nameof(CreateBarFoo));
        }

        public Foo CreateFoo()
        {
            return new Foo();
        }

        public BarFoo CreateBarFoo()
        {
            return new BarFoo();
        }
    }

    public partial class TestFactory : ContainerFactory
    {
        public TestFactory()
        {
            AddTransient<Foo>();
            AddTransient<Bar>();
        }
    }

    public class Foo
    {
    }

    public class Bar
    {
        public Foo Foo;
        public Bar(Foo foo)
        {
            Foo = foo;
        }
    }

    public class FooBar
    {
        public Foo Foo;
        public Bar Bar;
        public FooBar(Foo foo, Bar bar)
        {
            Foo = foo;
            Bar = bar;
        }
    }

    public class BarFoo
    {
    }
}