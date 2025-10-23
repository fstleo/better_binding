#nullable enable

using BetterBinding.Runtime;
using UnityEngine;

namespace BetterBinding.Example
{
    public partial class ExampleViewModel 
    {
        public Property<Color> ColorProperty { get; } = new();
        public Property<Color> SomeText { get; } = new();
    }
}