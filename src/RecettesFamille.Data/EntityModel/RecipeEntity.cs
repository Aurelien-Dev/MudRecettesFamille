﻿using System.ComponentModel.DataAnnotations;
using RecettesFamille.Data.EntityModel.Blocks;

namespace RecettesFamille.Data.EntityModel;

public class RecipeEntity
{
    public int Id { get; set; }
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(50)]
    public string InformationPreparation { get; set; } = string.Empty;
    public int PrepTime { get; set; }
    public int CookingTime { get; set; }
    public int RestTime { get; set; }
    public int Portion { get; set; }
 
    [MaxLength(200)]
    public string Tags { get; set; } = string.Empty;

    public List<BlockBaseEntity> BlocksInstructions { get; set; } = [];
}
