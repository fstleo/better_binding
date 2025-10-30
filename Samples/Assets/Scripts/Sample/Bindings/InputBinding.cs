#nullable enable

using System;
using BetterBinding.Runtime.Bindings;
using TMPro;
using UnityEngine;

namespace BetterBinding.Sample.Bindings
{
    [Serializable]
    public class InputBinding : TwoWayBinding<string>
    {
        [SerializeField]
        private TMP_InputField _input = null!;
        
        protected override void Subscribe()
        {
            _input.onValueChanged.AddListener(Execute);
        }
        
        protected override void Unsubscribe()
        {
            _input.onValueChanged.RemoveListener(Execute);
        }

        public override void OnNext(string? value)
        {
            _input.SetTextWithoutNotify(value);
        }
    }
}