#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace BetterBinding.Editor.Utils
{
    public static class Utils
    {
        public static bool IsNullOrEmpty([NotNullWhen(false)] this string? s)
        {
            return string.IsNullOrEmpty(s);
        }
    }
}