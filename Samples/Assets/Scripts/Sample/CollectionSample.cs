#nullable enable

using BetterBinding.Runtime.Bindings;
using UnityEngine;

namespace BetterBinding.Sample
{
    public class CollectionSample : MonoBehaviour
    {
        [SerializeField]
        private Binder _binder = null!;

        private readonly CollectionsExampleViewModel _viewModel = new();
        
        private void Awake()
        {
            _viewModel.AddElementCommand.Subscribe(_ => AddElement());
            _viewModel.RemoveLastElementCommand.Subscribe(_ => RemoveLastElement());
            _binder.Bind(_viewModel);
        }

        private void AddElement()
        {
            _viewModel.Collection.AddElement.OnNext(new SimpleViewModel());    
        }
        
        private void RemoveLastElement()
        {
            if (_viewModel.Collection.Elements.Count > 0)
            {
                _viewModel.Collection.RemoveElement.OnNext(_viewModel.Collection.Elements.Count - 1);    
            }
        }
        
        private void OnDestroy()
        {
            _binder.Unbind();
            _viewModel.Dispose();
        }
    }
}