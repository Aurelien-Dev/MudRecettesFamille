using RecettesFamille.Data.EntityModel.Blocks;
using RecettesFamille.Dto.Models.Blocks;

namespace RecettesFamille.Extensions
{
    public static class ListExtensions
    {
        public static void MoveUp(this List<BlockBaseDto> blocks, BlockBaseDto block)
        {
            blocks.Sort((a, b) => a.Order.CompareTo(b.Order)); // Toujours trier avant d’agir
            int index = blocks.IndexOf(block);
            if (index > 0)
            {
                Swap(blocks, index, index - 1);
            }
        }

        public static void MoveDown(this List<BlockBaseDto> blocks, BlockBaseDto block)
        {
            blocks.Sort((a, b) => a.Order.CompareTo(b.Order)); // Toujours trier avant d’agir
            int index = blocks.IndexOf(block);
            if (index >= 0 && index < blocks.Count - 1)
            {
                Swap(blocks, index, index + 1);
            }
        }

        private static void Swap(List<BlockBaseDto> blocks, int i, int j)
        {
            (blocks[i].Order, blocks[j].Order) = (blocks[j].Order, blocks[i].Order);
            blocks.Sort((a, b) => a.Order.CompareTo(b.Order)); // Assure la cohérence
        }
        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? source)
        {
            return source ?? Enumerable.Empty<T>();
        }
    }
}
