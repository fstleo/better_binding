#nullable enable

using System;
using UnityEngine;

namespace BetterBinding.Runtime
{
    public class Binder : MonoBehaviour, IBindable<IViewModel>
    {
        [Serializable]
        public class SerializedBinding
        {
            public ulong Id;
            
            [SerializeReference]
            public IBindable[] Bindings;
        }

        [SerializeField]
        private ulong _contractId;
        
        [SerializeField]
        private SerializedBinding[] _serializedBindings;

        public void Bind(IViewModel value)
        {
            if (!value.ImplementsContract(_contractId))
            {
#if BETTER_BINDING_DEBUG
                Debug.LogError("Binder {gameObject.name} has no contract {contractId}");
#endif
                return;
            }
            
            foreach (var serializedBinding in _serializedBindings)
            {
                if (!value.TryGetProperty(serializedBinding.Id, out var property))
                {
                    continue;
                }
                
                foreach (var binding in serializedBinding.Bindings)
                {
                    binding.Bind(property);
                }
            }    
        }

        public void Bind(object? value)
        {
            if (value is not IViewModel viewModel)
            {
#if BETTER_BINDING_DEBUG
                Debug.LogError("Invalid object type");
#endif
                return;
            }
            
            Bind(viewModel);
        }

        public void Unbind()
        {
            foreach (var serializedBinding in _serializedBindings)
            {
                foreach (var binding in serializedBinding.Bindings)
                {
                    binding.Unbind();
                }
            }    
        }
    }
}