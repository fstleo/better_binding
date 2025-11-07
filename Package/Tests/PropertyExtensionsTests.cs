#nullable enable

using BetterBinding.Runtime;
using NUnit.Framework;

namespace BetterBinding.Tests
{
    public class PropertyExtensionsTests
    {
        [Test]
        public void PropertyUnit_Execute_CallsPropertyChange()
        {
            var property = Property<Unit>.Command();
            var changedCount = 0;
            property.Subscribe(_ => changedCount++);
            
            property.Execute();
            
            Assert.AreEqual(1, changedCount);
        }
    }
}