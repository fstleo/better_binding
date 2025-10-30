#nullable enable

using System;
using BetterBinding.Runtime;
using BetterBinding.Runtime.Bindings;
using UnityEngine;
using UnityEngine.UI;

namespace BetterBinding.Sample.Bindings
{
    [Serializable]
    public class ButtonBinding : CommandBinding<Unit>
    {
        [SerializeField]
        private Button _button = null!;

        protected override void Subscribe()
        {
            _button.onClick.AddListener(Execute);
        }

        protected override void Unsubscribe()
        {
            _button.onClick.RemoveListener(Execute);
        }

        private void Execute()
        {
            Execute(Unit.Default);
        }
    }
}