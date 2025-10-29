#nullable enable

using System;
using UnityEngine;

namespace BetterBinding.Runtime.Bindings
{
    [Serializable]
    public class BindableMonoBehaviour : IBindable<IViewModel>
    {
        [SerializeField] 
        private Binder _binder = null!;
        
        public void Bind(IViewModel value)
        {   
            _binder.Bind(value);
        }

        public void Bind(object? value)
        {
            _binder.Bind(value);
        }

        public void Unbind()
        {
            _binder.Unbind();
        }
    }
}