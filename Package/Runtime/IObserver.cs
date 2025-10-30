#nullable enable

namespace BetterBinding.Runtime
{
    public interface IObserver<in T>
    {
        void OnNext(T? value);
    }
}