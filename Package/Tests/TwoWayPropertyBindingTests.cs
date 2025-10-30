using BetterBinding.Runtime;
using BetterBinding.Runtime.Bindings;
using NUnit.Framework;

namespace BetterBinding.Tests
{
    public class TwoWayPropertyBindingTests
    {
        private class TestTwoWayBinding<T> : TwoWayBinding<T>
        {
            public bool Subscribed { get; private set; }
            public T Value { get; set; }
            protected override void Subscribe()
            {
                Subscribed = true;
            }

            protected override void Unsubscribe()
            {
                Subscribed = false;
            }

            public override void OnNext(T value)
            {
                Value = value;
            }

            public void ApplyValue()
            {
                Execute(Value);
            }
        }

        private TestTwoWayBinding<int> _testBinding;
        private Property<int> _testProperty;

        [SetUp]
        public void Setup()
        {
            _testBinding = new TestTwoWayBinding<int>();
            _testProperty = new Property<int>();
        }
        
        [Test]
        public void Bind_BindingSubscribed()
        {
            _testBinding.Bind(_testProperty);
         
            Assert.True(_testBinding.Subscribed);
        }

        [Test]
        public void PropertyChanged_ValueApplied()
        {
            _testBinding.Bind(_testProperty);
            
            _testProperty.Value = 1;
            
            Assert.AreEqual(1, _testBinding.Value);
        }

        [Test]
        public void CommandExecuted_PropertyChanged()
        {
            _testBinding.Bind(_testProperty);
            _testBinding.Value = 5;
            
            _testBinding.ApplyValue();
            
            Assert.AreEqual(5, _testProperty.Value);
        }
    }
}