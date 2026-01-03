using System.Collections;
using System.Collections.Generic;

namespace BetterBinding.Runtime
{
    public interface ICollectionViewModel<T> 
    {
        Property<(T Item, int Index)> Added { get; }
        Property<int> Removed { get; }
        List<T> Elements { get; }
    }
    
    public partial class CollectionViewModel<T> : ICollectionViewModel<T>, IList<T>
    {
        public Property<(T Item, int Index)> Added { get; } = new();
        public Property<int> Removed { get; } = new();
        public List<T> Elements { get; } = new();

        public int Count => Elements.Count; 

        public bool IsReadOnly => false; 

        public T this[int index] { get => Elements[index]; set => Elements[index] = value; }

        public int IndexOf(T item)
        {
            return Elements.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            Elements.Insert(index, item);
            Added.Execute((item, index));
        }

        public void RemoveAt(int index)
        {
            Elements.RemoveAt(index);
            Removed.Execute(index);
        }

        public void Add(T item)
        {
            Elements.Add(item);
            Added.Execute((item, Elements.Count - 1));
        }

        public void Clear()
        {
            for(int index = Elements.Count - 1; index >= 0; index--)
            {            
                Removed.Execute(index);
            }

            Elements.Clear();
        }

        public bool Contains(T item)
        {
            return Elements.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Elements.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            var index = Elements.IndexOf(item);
            if (index < 0 || index > Elements.Count - 1)
            {
                return false;
            }

            Elements.RemoveAt(index);
            Removed.Execute(index);
            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Elements.GetEnumerator();
        }
    }
}