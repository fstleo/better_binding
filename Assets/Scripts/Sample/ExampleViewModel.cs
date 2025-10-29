#nullable enable

using System;
using BetterBinding.Runtime;
using UnityEngine;

namespace BetterBinding.Sample
{
    public partial class ExampleViewModel : IDisposable
    {
        public Property<Color> ColorProperty { get; } = new(Color.white);
        public Property<string> SomeText { get; } = new();
        public Property<bool> ColorChangeEnabled { get; } = new();
        public Property<Unit> ResetColorCommand { get; } = new();

        public void Dispose()
        {
            Debug.Log($"{nameof(ExampleViewModel)} disposed.");
            DisposeInternal();
        }
    }
}