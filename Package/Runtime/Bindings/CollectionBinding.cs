#nullable enable

using System;
using System.Collections.Generic;
using BetterBinding.Runtime.Bindings;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace BetterBinding.Runtime
{
    [Serializable]
    public class CollectionBinding<T> : IBindable<ICollectionViewModel<T>>
    {
        [SerializeField]
        private Binder _collectionElementPrefab = null!;
        
        [SerializeField]
        private Transform _collectionRoot = null!;

        private List<IDisposable>? _subscriptions;
        private List<Binder> _elements = new();
        
        public void Bind(ICollectionViewModel<T> collection)
        {
            for (var index = 0; index < collection.Elements.Count; index++)
            {
                CreateElement(collection.Elements[index]);
            }

            _subscriptions = ListPool<IDisposable>.Get();
            _subscriptions.Add(collection.AddElement.Subscribe(CreateElement));
            _subscriptions.Add(collection.RemoveElement.Subscribe(RemoveElement));
        }

        private void RemoveElement(int index)
        {
            var binder = _elements[index];
            binder.Unbind();
            Object.Destroy(binder.gameObject);
            _elements.RemoveAt(index);
        }

        private void CreateElement(T? element)
        {
            var binder = Object.Instantiate(_collectionElementPrefab, _collectionRoot);
            binder.Bind(element);
            _elements.Add(binder);
        }

        public void Bind(object? value)
        {
            if (value is ICollectionViewModel<T> collection)
            {
                Bind(collection);
            }
        }

        public void Unbind()
        {
            if (_subscriptions == null)
            {
                return;
            }
            
            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }
            
            ListPool<IDisposable>.Release(_subscriptions);
            
            foreach (var element in _elements)
            {
                if (element == null)
                {
                    continue;
                }
                
                element.Unbind();
                Object.Destroy(element.gameObject);
            }
            
            _elements.Clear();
        }
    }
}
