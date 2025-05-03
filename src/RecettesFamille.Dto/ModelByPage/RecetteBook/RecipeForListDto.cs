namespace RecettesFamille.Dto.ModelByPage.RecetteBook
{
    public class RecipeForListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public DateOnly CreatedDate { get; set; }
        public byte[]? Image { get; set; }
    }
}
