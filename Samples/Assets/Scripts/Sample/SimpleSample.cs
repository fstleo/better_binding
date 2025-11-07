using BetterBinding.Runtime.Bindings;
using UnityEngine;

namespace BetterBinding.Sample
{
    public class SimpleSample : MonoBehaviour
    {
        [SerializeField]
        private Binder _binder;

        private SimpleViewModel _viewModel;
        private void Awake()
        {
            _viewModel = new SimpleViewModel();
            _binder.Bind(_viewModel);
        }

        private void OnDestroy()
        {
            _binder.Unbind();
            _viewModel.Dispose();
        }
    }
}
