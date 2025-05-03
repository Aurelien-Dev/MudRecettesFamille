using System.ComponentModel.DataAnnotations;

namespace RecettesFamille.Data.EntityModel.Blocks;
public class BlockImageEntity : BlockBaseEntity
{

    [MaxLength(1 * 1024 * 1024)] // 1 Mo en bytes
    public byte[]? Image { get; set; }

    public BlockImageEntity()
    {
        HalfPage = true;
    }
}
