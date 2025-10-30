#nullable enable

using System;
using BetterBinding.Runtime.Bindings;
using TMPro;
using UnityEngine;

namespace BetterBinding.Sample.Bindings
{
    [Serializable]
    public class TextBinding : PropertyBinding<string>
    {
        [SerializeField]
        private TextMeshProUGUI? _text;
       
        public override void OnNext(string? value)
        {
            if (_text != null)
            {
                _text.text = value;
            }
            else
            {
                #if BETTER_BINDING_DEBUG
                Debug.LogError($"{nameof(TextBinding)}: Text is null.");
                #endif
            }
        }
    }
}