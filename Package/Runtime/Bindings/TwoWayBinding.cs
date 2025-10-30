#nullable enable
using System;

namespace BetterBinding.Runtime.Bindings
{
    public abstract class TwoWayBinding<T> : Binding<Property<T>>, IObserver<T>
    {
        private IDisposable? _subscription;
        private IObserver<T>? _listener;
        
        public override void Bind(Property<T>? value)
        {
            if (value == null)
            {
                return;
            }
         
            Unbind();
            _listener = value;
            _subscription = value.Subscribe(this);
            Subscribe();
        }

        public override void Unbind()
        {
            if (_listener == null)
            {
                return;
            }
            
            Unsubscribe();
            _listener = null; 
            _subscription?.Dispose();
            _subscription = null;
        }

        protected void Execute(T? value)
        {
            _listener?.OnNext(value);
        }
        
        protected abstract void Subscribe();
        protected abstract void Unsubscribe();
        public abstract void OnNext(T? value);
    }
}