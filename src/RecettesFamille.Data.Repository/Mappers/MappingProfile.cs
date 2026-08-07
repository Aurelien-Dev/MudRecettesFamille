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
        CreateMap<RecipeEntity, RecipeDto>()
            .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUser != null ? src.CreatedByUser.AccountName : null))
            .ReverseMap();

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

        // Mapping Categories
        CreateMap<CategoryEntity, CategoryDto>().ReverseMap();

        // Mapping Entity → DTO (inclut TravelName et les catégories depuis les navigation properties)
        CreateMap<YoutubeResumeEntity, YoutubeResumeDto>()
            .ForMember(dest => dest.TravelName, opt => opt.MapFrom(src => src.Travel != null ? src.Travel.Name : null))
            .ForMember(dest => dest.CategoryIds, opt => opt.MapFrom(src => src.Categories.Select(c => c.Id).ToList()))
            .ForMember(dest => dest.CategoryNames, opt => opt.MapFrom(src => src.Categories.Select(c => c.Name).ToList()))
            .ForMember(dest => dest.CategoryColors, opt => opt.MapFrom(src => src.Categories.Select(c => c.Color).ToList()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (Dto.Models.SummaryStatus)src.Status));

        // Mapping DTO → Entity (ignore la navigation property, seul TravelId est mappé)
        CreateMap<YoutubeResumeDto, YoutubeResumeEntity>()
            .ForMember(dest => dest.Travel, opt => opt.Ignore())
            .ForMember(dest => dest.Categories, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (EntityModel.SummaryStatus)src.Status));

        CreateMap<TravelEntity, TravelDto>().ReverseMap();
    }
}