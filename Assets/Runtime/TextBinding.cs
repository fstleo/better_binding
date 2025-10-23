#nullable enable

using System;
using TMPro;
using UnityEngine;

namespace BetterBinding.Runtime
{
    [Serializable]
    public class TextBinding : PropertyBinding<string>
    {
        [SerializeField]
        private TextMeshProUGUI _text = null!;
        protected override void SetValue(string? value)
        {
            _text.text = value;
        }
    }
}