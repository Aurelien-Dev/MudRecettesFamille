using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RecettesFamille.Data.EntityModel;
using RecettesFamille.Data.EntityModel.Blocks;
using RecettesFamille.Data.Repository.IRepositories;
using RecettesFamille.Dto.Models;
using RecettesFamille.Dto.Models.Blocks;

namespace RecettesFamille.Data.Repository.Repositories;

public class AiRepository(IMapper Mapper, ApplicationDbContext Context) : IAiRepository
{

    public async Task<PromptDto> GetPrompt(string promptName, CancellationToken cancellationToken = default)
    {
        var result = await Context.Prompts.Where(r => r.Name == promptName).FirstOrDefaultAsync();

        return Mapper.Map<PromptDto>(result);
    }

    public async Task<List<PromptDto>> GetPrompt(CancellationToken cancellationToken = default)
    {
        var result = await Context.Prompts.ToListAsync();

        return Mapper.Map< List<PromptDto>>(result);
    }

    public async Task UpdatePrompt(PromptDto prompt, CancellationToken cancellationToken = default)
    {
        var element = await Context.Set<PromptEntity>().FindAsync(prompt.Id, cancellationToken);
        if (prompt is null)
            return;

        Mapper.Map(prompt, element);

        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task ReportConsumption(AiConsumptionDto aiConsumptionDto, CancellationToken cancellationToken = default)
    {
        var element = Mapper.Map<AiConsumptionEntity>(aiConsumptionDto);
        await Context.AddAsync(element, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
    }



    public async Task Delete(int recipeId, CancellationToken cancellationToken = default)
    {
        var element = await Context.Recettes.FindAsync(recipeId, cancellationToken);
        if (element != null)
        {
            Context.Recettes.Remove(element);
            await Context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task DeleteBlock(int blockId, CancellationToken cancellationToken = default)
    {
        var element = await Context.Set<BlockBaseEntity>().FindAsync(blockId, cancellationToken);
        if (element is null)
            return;

        Context.Set<BlockBaseEntity>().Remove(element);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task Update(RecipeDto recipe, CancellationToken cancellationToken = default)
    {
        var element = await Context.Recettes.FindAsync(recipe.Id, cancellationToken);
        if (recipe is null)
            return;

        Mapper.Map(recipe, element);

        await Context.SaveChangesAsync(cancellationToken);
    }
}
