#nullable enable

namespace BetterBinding.Runtime
{
    public interface IViewModel
    {
        bool ImplementsContract(ulong contractId);
        bool TryGetProperty(ulong id, out object? value);
    }
}