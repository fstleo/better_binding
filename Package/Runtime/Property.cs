#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace BetterBinding.Runtime
{
    public class Property<T> : IObserver<T>, IObservable<T>, IDisposable
    {
        private readonly IEqualityComparer<T?>? _comparer;

        private class Observer : IDisposable, IObserver<T>
        {
            private static readonly ObjectPool<Observer> SubscriberPool =
                new(createFunc: Create);

            private Action<T?>? _action;
            
            private static Observer Create()
            {
                return new Observer();
            }

            public static Observer Create(Action<T?> action)
            {
                var listener = SubscriberPool.Get();
                listener._action = action;
                return listener;
            }
            
            public void OnNext(T? value)
            {
                _action?.Invoke(value);
            }

            public void Dispose()
            {
                _action = null;
                SubscriberPool.Release(this);
            }
        }
        
        private class Subscription : IDisposable, IObserver<T>
        {
            internal Subscription? Previous;
            internal Subscription? Next;
            
            private static readonly ObjectPool<Subscription> SubscriberPool =
                new(
                    createFunc: Create, 
                    actionOnRelease: subscription => subscription.Dispose()
                );

            private IObserver<T>? _observer;
            private IDisposable? _toDispose;
            private Property<T>? _owner;

            private static Subscription Create()
            {
                return new Subscription();
            }

            public static IDisposable Create(Property<T> property, Action<T?> action)
            {
                var observer = Observer.Create(action);
                var disposable = CreateInternally(property, observer, out var subscription);
                subscription._toDispose = observer;
                return disposable;
            }

            public static IDisposable Create(Property<T> property, IObserver<T> observer)
            {
                return CreateInternally(property, observer, out _);
            }

            private static IDisposable CreateInternally(
                Property<T> property, 
                IObserver<T> observer, 
                out Subscription subscription)
            {
                var pooledItem = SubscriberPool.Get(out subscription);
                subscription._observer = observer;
                subscription._owner = property; 
                if (property._subscription != null)
                {
                    property._subscription.AddToTheEnd(subscription);
                }
                else
                {
                    property._subscription = subscription;
                }
                
                return pooledItem;
            }

            private void AddToTheEnd(Subscription subscription)
            {
                if (subscription == this)
                {
                    return;
                }
                
                if (Next == null)
                {
                    Next = subscription;
                    Next.Previous = this;
                }
                else
                {
                    Next.AddToTheEnd(subscription);
                }
            }
            
            public void Dispose()
            {
                if (_owner == null)
                {
                    return;
                }
                
                if (_owner._subscription == this)
                {
                    _owner._subscription = Next;
                }
                else if (Previous != null)
                {
                    Previous.Next = Next;
                }
                
                if (Next != null)
                {
                    Next.Previous = Previous;
                }
                
                _toDispose?.Dispose();
                Previous = null;
                Next = null;
                _observer = null;
                _owner = null;
            }

            public void OnNext(T? value)
            {
                _observer?.OnNext(value);
                Next?.OnNext(value);
            }
        }
        
        private Subscription? _subscription;
        
        private T? _value;

        public IDisposable Subscribe(IObserver<T> subscriber)
        {
            subscriber.OnNext(_value);
            return Subscription.Create(this, subscriber);
        }
        
        public IDisposable Subscribe(Action<T?> subscriber)
        {
            return Subscription.Create(this, subscriber);
        }
        
        public T? Value
        {
            get => _value;
            set => OnNext(value);
        }

        public Property() : this(EqualityComparer<T?>.Default)
        {
        }
        
        public Property(T value) : this(EqualityComparer<T?>.Default)
        {
            _value = value;
        }

        public Property(IEqualityComparer<T?>? comparer)
        {
            _comparer = comparer;
        }

        public static Property<T> Command()
        {
            return new Property<T>(null);
        }
        
        public void OnNext(T? value)
        {
            if (_comparer == null || !_comparer.Equals(_value, value))
            {
                _subscription?.OnNext(value);
            }
        
            _value = value;
        }
        
        public void Dispose()
        {
            while (_subscription != null)
            {
                var subscription = _subscription;
                _subscription = _subscription.Next;
                subscription.Dispose();
            }
        }
    }
}