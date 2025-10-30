#nullable enable

using System;

namespace BetterBinding.Runtime
{
    public struct Unit : IComparable<Unit>, IEquatable<Unit>
    {
        public override bool Equals(object? obj)
        {
            return false;
        }

        public bool Equals(Unit other)
        {
            return false;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public int CompareTo(Unit other)
        {
            return 1;
        }

        public static Unit Default = new();
    }
}