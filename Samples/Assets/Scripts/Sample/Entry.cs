#nullable enable

using System.Collections;
using BetterBinding.Runtime.Bindings;
using UnityEngine;

namespace BetterBinding.Sample
{
    public class Entry : MonoBehaviour
    {
        [SerializeField]
        private Binder _bindable = null!;
        
        private readonly ExampleViewModel _viewModel = new();
    
        private void Awake()
        {
            _bindable.Bind(_viewModel);
            _viewModel.ResetColorCommand.Subscribe(_ =>
            {
                _viewModel.ColorProperty.Value = Color.white;
            });
        }

        private IEnumerator Start()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.5f);
                if (_viewModel.ColorChangeEnabled.Value)
                {
                    _viewModel.ColorProperty.Value = new Color(Random.value, Random.value, Random.value);
                }
            }
        }

        private void OnDestroy()
        {
            _bindable.Unbind();
        }
    }
}