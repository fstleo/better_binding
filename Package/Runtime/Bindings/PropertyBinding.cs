#nullable enable

using System;

namespace BetterBinding.Runtime.Bindings
{
    public abstract class PropertyBinding<T> : Binding<Property<T>>, IObserver<T>
    {
        private IDisposable? _subscription;
        public override void Bind(Property<T>? property)
        {
            if (property == null)
            {
                return;
            }
            
            _subscription = property.Subscribe(this);
        }

        public override void Unbind()
        {
            _subscription?.Dispose();
            _subscription = null;
        }
        
        public abstract void OnNext(T? value);
    }
}