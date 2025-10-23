#nullable enable

namespace BetterBinding.Runtime
{
    public abstract class BaseBinding<T> : IBindable<T>
    {
        public abstract void Bind(T value);

        public void Bind(object value)
        {
            if (value is T typedValue)
            {
                Bind(typedValue);
            }
        }

        public abstract void Unbind();
    }
}