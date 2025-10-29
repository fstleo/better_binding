using BetterBinding.Runtime;
using BetterBinding.Runtime.Bindings;
using NUnit.Framework;

namespace BetterBinding.Tests
{
    public class CommandBindingTests
    {
        private class TestCommandBinding<T> : CommandBinding<T>
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

            public void Execute()
            {
                base.Execute(Value);
            }
        }
        
        private TestCommandBinding<Unit> _commandBinding;
        private Property<Unit> _testProperty;
        
        [SetUp]
        public void Setup()
        {
            _commandBinding = new TestCommandBinding<Unit>();
            _testProperty = new Property<Unit>();
        }

        [Test]
        public void CommandBind_Subscribed()
        {
            _commandBinding.Bind(_testProperty);
            Assert.True(_commandBinding.Subscribed);
        }
        
        [Test]
        public void CommandUnbind_Unsubscribed()
        {
            _commandBinding.Unbind();
            Assert.False(_commandBinding.Subscribed);
        }

        [Test]
        public void Command_PropertyUnbind_ChangeNotCalled()
        {
            var changed = false;
            _commandBinding.Bind(_testProperty);
            _testProperty.Subscribe(_=> changed = true);
            _commandBinding.Unbind();
            
            _commandBinding.Execute();
            
            Assert.False(changed);
        }

        [Test]
        public void CommandExecute_PropertyBind_PropertyChangeCalled()
        {
            var changed = false;
            
            _commandBinding.Bind(_testProperty);
            _testProperty.Subscribe(_=> changed = true);
            
            _commandBinding.Value = Unit.Default;
            _commandBinding.Execute();
            
            Assert.True(changed);
        }
        
        [Test]
        public void CommandExecute_PropertyValueChanged()
        {
            var testIntCommand = new TestCommandBinding<int>();
            var testIntProperty = new Property<int>();
            testIntCommand.Bind(testIntProperty);
            var value = 0;
            
            testIntProperty.Subscribe(newValue => value = newValue);
            
            testIntCommand.Value = 5;
            testIntCommand.Execute();
            
            Assert.AreEqual(5, value);
        }
    }
}
