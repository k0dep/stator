namespace Stator.Tests
{
    public partial class TestFactorySingletons : ContainerFactory
    {
        public TestFactorySingletons()
        {
            AddSingleton<Foo>();
        }
    }
    
    public partial class TestFactoryTransients : ContainerFactory
    {
        public TestFactoryTransients()
        {
            AddTransient<Foo>();
        }
    }
    
    public partial class TestFactoryTransientsWithSingletonDep : ContainerFactory
    {
        public TestFactoryTransientsWithSingletonDep()
        {
            AddSingleton<Foo>();
            AddTransient<Bar>();
        }
    }
    
    public partial class TestFactoryFromMethodsSet1 : ContainerFactory
    {
        public TestFactoryFromMethodsSet1()
        {
            AddSingleton<Foo>(nameof(GetFooSingleton));
            AddTransient<Bar>();
        }

        private Foo GetFooSingleton()
        {
            return new Foo() {Payload = "GetFooSingleton"};
        }
    }
    
    public partial class TestFactoryFromMethodsSet2 : ContainerFactory
    {
        public TestFactoryFromMethodsSet2()
        {
            AddTransient<Foo>(nameof(GetFooFromTransient));
            AddTransient<Bar>();
        }

        private Foo GetFooFromTransient()
        {
            return new Foo() {Payload = "GetFooFromTransient"};
        }
    }

    public class Foo
    {
        public string Payload;
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