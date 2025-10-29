#nullable enable

using System;
using BetterBinding.Runtime.Bindings;
using UnityEngine;
using UnityEngine.UI;

namespace BetterBinding.Sample.Bindings
{
    [Serializable]
    public class ToggleBinding : TwoWayBinding<bool>
    {
        [SerializeField] 
        private Toggle _toggle = null!;
        
        protected override void Subscribe()
        {
            _toggle.onValueChanged.AddListener(Execute);
        }

        protected override void Unsubscribe()
        {
            _toggle.onValueChanged.RemoveListener(Execute);
        }

        public override void OnNext(bool value)
        {
            _toggle.SetIsOnWithoutNotify(value);
        }
    }
}