#nullable enable

using BetterBinding.Runtime;
using UnityEngine;

namespace BetterBinding.Sample
{
    public partial class SimpleViewModel
    {
        public Property<string> Text { get; } = new();
        
        public Property<Unit> ResetText { get; } = new();

        public SimpleViewModel()
        {
            ResetText.Subscribe(_ => Text.OnNext(string.Empty));
        }
    }
}