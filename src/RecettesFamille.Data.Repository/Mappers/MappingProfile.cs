using AutoMapper;
using RecettesFamille.Data.EntityModel;
using RecettesFamille.Data.EntityModel.Blocks;
using RecettesFamille.Dto.Models;
using RecettesFamille.Dto.Models.Blocks;

namespace RecettesFamille.Data.Repository.Mappers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Mapping des entités principales
        CreateMap<RecipeEntity, RecipeDto>().ReverseMap();

        // Gestion du polymorphisme
        CreateMap<BlockBaseEntity, BlockBaseDto>()
            .Include<BlockImageEntity, BlockImageDto>()
            .Include<BlockIngredientListEntity, BlockIngredientListDto>()
            .Include<BlockInstructionEntity, BlockInstructionDto>().ReverseMap();

        CreateMap<BlockImageEntity, BlockImageDto>().ReverseMap();
        CreateMap<BlockIngredientListEntity, BlockIngredientListDto>().ReverseMap();
        CreateMap<BlockInstructionEntity, BlockInstructionDto>().ReverseMap();

        // Mapping des ingrédients
        CreateMap<IngredientEntity, IngredientDto>().ReverseMap();

        // Mapping AI
        CreateMap<PromptEntity, PromptDto>().ReverseMap();
        CreateMap<AiConsumptionEntity, AiConsumptionDto>().ReverseMap();
        CreateMap<TagEntity, TagDto>().ReverseMap();

        CreateMap<YoutubeSummaryRequestEntity, YoutubeSummaryRequestDto>().ReverseMap();
    }

}