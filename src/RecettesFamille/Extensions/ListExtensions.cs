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

        public static void Reorder(this List<BlockBaseDto> blocks)
        {
            blocks.Sort((a, b) => a.Order.CompareTo(b.Order)); // Trier par ordre
            for (int i = 0; i < blocks.Count; i++)
            {
                blocks[i].Order = i + 1; // Réécrire les ordres pour qu'ils soient continus
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
