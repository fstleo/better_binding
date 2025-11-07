#nullable enable

using BetterBinding.Runtime;
using NUnit.Framework;

namespace BetterBinding.Tests
{
    public class UnitTests
    {
        [Test]
        public void Unit_NotEqualBoxed()
        {
            var unit = new Unit();

            Assert.IsFalse(unit.Equals((object) unit));
        }
        
        [Test]
        public void Unit_NotEqual()
        {
            var unit = new Unit();

            Assert.IsFalse(unit.Equals(unit));
        }
        
        [Test]
        public void Unit_GetHashCodeIsZero()
        {
            var unit = new Unit();
            
            Assert.AreEqual(0, unit.GetHashCode());
        }

        [Test]
        public void Unit_CompareTo_NotEqual()
        {
            var unit = new Unit();
            
            Assert.AreNotEqual(0, unit.CompareTo(unit));
        }
    }
}