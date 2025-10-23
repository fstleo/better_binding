#nullable enable

using System;
using System.Collections.Generic;

namespace BetterBinding.Runtime
{
    public class Property<T>
    {
        private T? _value;

        public event Action<T?>? Changed;

        public void OnNext(T? value, bool forceNotify = false)
        {
            if (forceNotify || !EqualityComparer<T?>.Default.Equals(_value, value))
            {
                Changed?.Invoke(value);    
            }
        
            _value = value;
        }

        public T? Value
        {
            get => _value;
            set => OnNext(value);
        }

        public Property()
        {
        }

        public Property(T? value)
        {
            _value = value;
        }
    }
}