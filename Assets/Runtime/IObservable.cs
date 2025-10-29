using System;

namespace BetterBinding.Runtime
{
    public interface IObservable<out T>
    {
        IDisposable Subscribe(IObserver<T> observer);
    }
}