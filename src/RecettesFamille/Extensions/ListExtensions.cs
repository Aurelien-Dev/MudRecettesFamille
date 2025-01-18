using RecettesFamille.Data.EntityModel.RecipeSubEntity;

namespace RecettesFamille.Extensions
{
    public static class ListExtensions
    {
        public static void MoveUp(this List<BlockBase> blocks, BlockBase block)
        {
            blocks.Sort((a, b) => a.Order.CompareTo(b.Order)); // Toujours trier avant d’agir
            int index = blocks.IndexOf(block);
            if (index > 0)
            {
                Swap(blocks, index, index - 1);
            }
        }

        public static void MoveDown(this List<BlockBase> blocks, BlockBase block)
        {
            blocks.Sort((a, b) => a.Order.CompareTo(b.Order)); // Toujours trier avant d’agir
            int index = blocks.IndexOf(block);
            if (index >= 0 && index < blocks.Count - 1)
            {
                Swap(blocks, index, index + 1);
            }
        }

        private static void Swap(List<BlockBase> blocks, int i, int j)
        {
            (blocks[i].Order, blocks[j].Order) = (blocks[j].Order, blocks[i].Order);
            blocks.Sort((a, b) => a.Order.CompareTo(b.Order)); // Assure la cohérence
        }

        //public static void MoveUp<T>(this List<T> list, int index)
        //{
        //    if (index > 0 && index < list.Count)
        //    {
        //        (list[index], list[index - 1]) = (list[index - 1], list[index]);
        //    }
        //}

        //public static void MoveDown<T>(this List<T> list, int index)
        //{
        //    if (index >= 0 && index < list.Count - 1)
        //    {
        //        (list[index], list[index + 1]) = (list[index + 1], list[index]);
        //    }
        //}
        private static void Reorder(List<BlockBase> dd)
        {
            for (int i = 0; i < dd.Count; i++)
            {
                dd[i].Order = i;
            }
        }

        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? source)
        {
            return source ?? Enumerable.Empty<T>();
        }

    }
}
