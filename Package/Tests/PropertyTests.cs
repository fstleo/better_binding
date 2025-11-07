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
        
        [Test]
        public void Property_CreateCommand_SubscriptionCalledEveryTime()
        {
            var changedCount = 0;
            var property = Property<int>.Command();
            property.Subscribe(_ => changedCount++);
            
            property.OnNext(4);
            property.OnNext(4);

            Assert.AreEqual(2, changedCount);
        }

        [Test]
        public void Property_PropertyDispose_SubscriptionNotCalled()
        {
            var changedCount = 0;
            var property = Property<int>.Command();
            property.Subscribe(_ => changedCount++);
            
            property.OnNext(4);
            property.Dispose();
            property.OnNext(4);

            Assert.AreEqual(1, changedCount);
        }
        
        [Test]
        public void Property_SubscriptionDispose_SubscriptionNotCalled()
        {
            var changedCount = 0;
            var property = Property<int>.Command();
            var subscription0 = property.Subscribe(_ => changedCount++);
            var subscription1 = property.Subscribe(_ => changedCount++);
            var subscription2 = property.Subscribe(_ => changedCount++);
            
            property.OnNext(4);
            subscription1.Dispose();
            subscription0.Dispose();
            subscription2.Dispose();
            property.OnNext(4);

            Assert.AreEqual(3, changedCount);
        }
    }
}