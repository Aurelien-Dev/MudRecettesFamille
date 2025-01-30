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

        return Mapper.Map<List<PromptDto>>(result);
    }

    public async Task UpdatePrompt(PromptDto prompt, CancellationToken cancellationToken = default)
    {
        var element = await Context.Set<PromptEntity>().FindAsync(prompt.Id, cancellationToken);
        if (prompt is null)
            return;

        Mapper.Map(prompt, element);

        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> AddPrompt(PromptDto prompt, CancellationToken cancellationToken = default)
    {
        PromptEntity promptEntity = Mapper.Map<PromptEntity>(prompt);

        await Context.AddAsync(promptEntity);
        var result = await Context.SaveChangesAsync(cancellationToken);

        return result > 0;
    }

    public async Task ReportConsumption(AiConsumptionDto aiConsumptionDto, CancellationToken cancellationToken = default)
    {
        var element = Mapper.Map<AiConsumptionEntity>(aiConsumptionDto);
        await Context.AddAsync(element, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
    }



}
