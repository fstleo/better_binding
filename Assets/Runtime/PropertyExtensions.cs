#nullable enable

namespace BetterBinding.Runtime
{
    public static class PropertyExtensions
    {
        public static void Execute(this Property<Unit> property)
        {
            property.OnNext(Unit.Default, true);
        }
    
        public static void Execute<T>(this Property<T> property, T? value)
        {
            property.OnNext(value, true);
        }

        public static void Flip(this Property<bool> property)
        {
            property.OnNext(!property.Value);
        }
    }
}