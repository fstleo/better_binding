#nullable enable

using BetterBinding.Runtime;
using NUnit.Framework;

namespace BetterBinding.Tests
{
    public class PropertyTests
    {
        private Property<int> _property = null!;
        
        [SetUp]
        public void Setup()
        {
            _property = new Property<int>();
        }


        [Test]
        public void Subscribe_PropertyChange()
        {
            var changedCount = 0;
            _property.Subscribe(_ => changedCount++);
            
            _property.Value = 2;
            _property.Value = 3;
            
            Assert.AreEqual(2, changedCount);
        }
        
        [Test]
        public void PropertyNotChanged_SubscriptionNotCalled()
        {
            var changedCount = 0;
            _property.Subscribe(_ => changedCount++);
            
            _property.Value = 2;
            _property.Value = 2;
            
            Assert.AreEqual(1, changedCount);
        }
        
    }
}