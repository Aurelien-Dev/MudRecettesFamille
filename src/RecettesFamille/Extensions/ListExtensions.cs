namespace RecettesFamille.Extensions
{
    public static class ListExtensions
    {
        public static void MoveUp<T>(this List<T> list, int index)
        {
            if (index > 0 && index < list.Count)
            {
                (list[index], list[index - 1]) = (list[index - 1], list[index]);
            }
        }

        public static void MoveDown<T>(this List<T> list, int index)
        {
            if (index >= 0 && index < list.Count - 1)
            {
                (list[index], list[index + 1]) = (list[index + 1], list[index]);
            }
        }
    }
}
