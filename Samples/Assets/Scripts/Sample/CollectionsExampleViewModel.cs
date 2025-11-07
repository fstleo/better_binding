#nullable enable

using BetterBinding.Runtime;

namespace BetterBinding.Sample
{
    public partial class CollectionsExampleViewModel
    {
        public Property<Unit> AddElementCommand { get; } = new();
        public Property<Unit> RemoveLastElementCommand { get; } = new();
        public CollectionViewModel<SimpleViewModel> Collection { get; } = new();
    }
}