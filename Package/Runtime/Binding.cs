#nullable enable

namespace BetterBinding.Runtime
{
    public abstract class Binding<T> : IBindable<T>
    {
        public abstract void Bind(T value);

        public void Bind(object? value)
        {
            if (value is not T typedValue)
            {
#if BETTER_BINDING_DEBUG
                Debug.LogError("Invalid object type");
#endif
                return;
            }
            
            Bind(typedValue);
        }

        public abstract void Unbind();
    }
}