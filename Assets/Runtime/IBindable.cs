#nullable enable

namespace BetterBinding.Runtime
{
    public interface IBindable<in T> : IBindable
    {
        void Bind(T value);
    }

    public interface IBindable
    {
        void Bind(object? value);
        void Unbind();
    }
}