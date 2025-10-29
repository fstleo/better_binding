#nullable enable

using System;
using BetterBinding.Runtime.Bindings;
using TMPro;
using UnityEngine;

namespace BetterBinding.Sample.Bindings
{
    [Serializable]
    public class TextColorBinding : PropertyBinding<Color>
    {
        [SerializeField]
        private TextMeshProUGUI? _text;
        
        public override void OnNext(Color value)
        {
            if (_text != null)
            {
                _text.color = value;
            }
        }
    }
}