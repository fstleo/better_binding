using System.Collections.Generic;
using BetterBinding.Runtime;

namespace BetterBinding.Runtime
{
    public interface ICollectionViewModel<T> 
    {
        Property<T> AddElement { get; }
        Property<int> RemoveElement { get; }
        List<T> Elements { get; }
    }
    
    public partial class CollectionViewModel<T> : ICollectionViewModel<T>
    {
        public Property<T> AddElement { get; } = new();
        public Property<int> RemoveElement { get; } = new();
        public List<T> Elements { get; } = new();

        public CollectionViewModel()
        {
            AddElement.Subscribe(Elements.Add);
            RemoveElement.Subscribe(RemoveInternal);
        }

        private void RemoveInternal(int index)
        {
            Elements.RemoveAt(index);
        }
    }
}