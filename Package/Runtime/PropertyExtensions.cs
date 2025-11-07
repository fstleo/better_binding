#nullable enable

namespace BetterBinding.Runtime
{
    public static class PropertyExtensions
    {
        public static void Execute(this Property<Unit> property)
        {
            property.Execute(Unit.Default);
        }
    
        public static void Execute<T>(this Property<T> property, T? value)
        {
            property.OnNext(value);
        }
    }
}