using System.ComponentModel.DataAnnotations;

namespace RecettesFamille.Data.EntityModel
{
    public class GptConsumptionEntity
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public Decimal Cost { get; set; }
        public string UseCase { get; set; } = string.Empty;
    }
}
