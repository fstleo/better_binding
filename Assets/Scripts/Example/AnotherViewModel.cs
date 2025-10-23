#nullable enable

using BetterBinding.Runtime;

namespace BetterBinding.Example
{
    public partial class AnotherViewModel 
    {
        public Property<string> TextProperty { get; } = new();
    }
}