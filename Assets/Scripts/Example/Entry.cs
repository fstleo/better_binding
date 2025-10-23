#nullable enable

using BetterBinding.Runtime;
using TMPro;
using UnityEngine;

namespace BetterBinding.Example
{
    public class Entry : MonoBehaviour
    {
        [SerializeField]
        private Binder _bindable = null!;
        
        [SerializeField]
        private TMP_InputField _inputField = null!;
    
        private readonly AnotherViewModel _viewModel = new();
    
        private void Awake()
        {
            _bindable.Bind(_viewModel);
            _inputField.onValueChanged.AddListener(text => _viewModel.TextProperty.OnNext(text));
        }

        private void OnDestroy()
        {
            _bindable.Unbind();
        }
    }
}