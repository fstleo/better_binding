using BetterBinding.Runtime;
using BetterBinding.Runtime.Bindings;
using NUnit.Framework;

namespace BetterBinding.Tests
{
    public class PropertyBindingTests
    {
        private class TestPropertyBinding<T> : PropertyBinding<T>
        {
            public T Value { get; private set; }
            public override void OnNext(T value)
            {
                Value = value;
            }
        }

        private TestPropertyBinding<int> _testPropertyBinding;
        private Property<int> _testProperty;
        
        [SetUp]
        public void Setup()
        {
            _testPropertyBinding = new TestPropertyBinding<int>();
            _testProperty = new Property<int>();
        }

        [Test]
        public void PropertyBind_ValueApplied()
        {
            _testProperty.OnNext(5);
            
            _testPropertyBinding.Bind(_testProperty);
            
            Assert.AreEqual(5, _testPropertyBinding.Value);
        }
        
        [Test]
        public void PropertyChanges_BindingChanges()
        {
            _testPropertyBinding.Bind(_testProperty);

            _testProperty.OnNext(5);
            
            Assert.AreEqual(5, _testPropertyBinding.Value);
        }

        [Test]
        public void PropertyUnbind_ValueDidntChange()
        {
            _testProperty.OnNext(4);
            _testPropertyBinding.Bind(_testProperty);
            
            _testPropertyBinding.Unbind();
            
            _testProperty.OnNext(5);   
            Assert.AreEqual(4, _testPropertyBinding.Value);
        }

    }
}