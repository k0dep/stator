using UnityEditor;
using UnityEngine.Assertions;

namespace Stator.Tests
{
    public class StatorContainerFactoryUnitTests : UnitTests
    {
        [MenuItem("Window/Stator/Run unit tests")]
        public static void RunTests()
        {
            new StatorContainerFactoryUnitTests().RunAll();
        }

        public void Test_Resolve_ShouldThrowBecouseNotRegistred()
        {
            // Arrange
            var container = new TestFactorySingletons();
            
            // Act/Assert
            AssertThrow<StatorUnresolvedException>(() => container.Resolve<object>());
        }
        
        public void Test_Resolve_ShouldReturnSomeObjectForSingleton()
        {
            // Arrange
            var container = new TestFactorySingletons();
            
            // Act
            var fooFirst = container.Resolve<Foo>();
            var fooSecond = container.Resolve<Foo>();
            
            // Assert
            Assert.IsNotNull(fooFirst);
            Assert.IsNotNull(fooSecond);
            Assert.AreEqual(fooFirst, fooSecond);
        }
        
        public void Test_Resolve_ShouldReturnDifferentObjectForTransients()
        {
            // Arrange
            var container = new TestFactoryTransients();
            
            // Act
            var fooFirst = container.Resolve<Foo>();
            var fooSecond = container.Resolve<Foo>();
            
            // Assert
            Assert.IsNotNull(fooFirst);
            Assert.IsNotNull(fooSecond);
            Assert.AreNotEqual(fooFirst, fooSecond);
        }
        
        public void Test_Resolve_ShouldReturnDiffTransientWithSomeSingletonDep()
        {
            // Arrange
            var container = new TestFactoryTransientsWithSingletonDep();
            
            // Act
            var barFirst = container.Resolve<Bar>();
            var barSecond = container.Resolve<Bar>();
            
            // Assert
            Assert.IsNotNull(barFirst);
            Assert.IsNotNull(barFirst.Foo);
            Assert.IsNotNull(barSecond);
            Assert.IsNotNull(barSecond.Foo);
            Assert.AreNotEqual(barFirst, barSecond);
            Assert.AreEqual(barFirst.Foo, barSecond.Foo);
        }
        
        public void Test_Resolve_ShouldReturnSomeObjectForSingletonUsingMethod()
        {
            // Arrange
            var container = new TestFactoryFromMethodsSet1();
            
            // Act
            var fooFirst = container.Resolve<Foo>();
            var fooSecond = container.Resolve<Foo>();
            
            // Assert
            Assert.IsNotNull(fooFirst);
            Assert.IsNotNull(fooSecond);
            Assert.AreEqual(fooFirst, fooSecond);
            Assert.AreEqual(fooFirst.Payload, "GetFooSingleton");
        }
        
        public void Test_Resolve_ShouldReturnDifferentObjectForTransientsUsingMethod()
        {
            // Arrange
            var container = new TestFactoryFromMethodsSet2();
            
            // Act
            var fooFirst = container.Resolve<Foo>();
            var fooSecond = container.Resolve<Foo>();
            
            // Assert
            Assert.IsNotNull(fooFirst);
            Assert.IsNotNull(fooSecond);
            Assert.AreNotEqual(fooFirst, fooSecond);
            Assert.AreEqual(fooFirst.Payload, fooSecond.Payload);
            Assert.AreEqual(fooFirst.Payload, "GetFooFromTransient");
        }
        
        public void Test_Resolve_ShouldReturnDiffTransientWithSomeSingletonDepUsingMethod()
        {
            // Arrange
            var container = new TestFactoryFromMethodsSet1();
            
            // Act
            var barFirst = container.Resolve<Bar>();
            var barSecond = container.Resolve<Bar>();
            
            // Assert
            Assert.IsNotNull(barFirst);
            Assert.IsNotNull(barFirst.Foo);
            Assert.IsNotNull(barSecond);
            Assert.IsNotNull(barSecond.Foo);
            Assert.AreNotEqual(barFirst, barSecond);
            Assert.AreEqual(barFirst.Foo, barSecond.Foo);
        }

        public void Test_Resolve_ShouldReturnDiffTransientWithTransientDepUsingMethod()
        {
            // Arrange
            var container = new TestFactoryFromMethodsSet2();

            // Act
            var barFirst = container.Resolve<Bar>();
            var barSecond = container.Resolve<Bar>();

            // Assert
            Assert.IsNotNull(barFirst);
            Assert.IsNotNull(barFirst.Foo);
            Assert.IsNotNull(barSecond);
            Assert.IsNotNull(barSecond.Foo);
            Assert.AreNotEqual(barFirst, barSecond);
            Assert.AreNotEqual(barFirst.Foo, barSecond.Foo);
        }
    }
}
