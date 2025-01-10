using Microsoft.AspNetCore.Components;
using MudBlazor;
using RecettesFamille.Components.RecetteBook.BlockEditors;
using RecettesFamille.Data;
using RecettesFamille.Data.EntityModel.RecipeSubEntity;

namespace RecettesFamille.Components.RecetteBook.BlockDefinitions;

public abstract class BaseBlockDefinition<TBlock> : ComponentBase where TBlock : BlockBase
{
    [Inject] IDialogService DialogService { get; set; }
    [Inject] ApplicationDbContext dbContext { get; set; }
    [Parameter] public TBlock Block { get; set; }
    [Parameter] public Action BlockUpdated { get; set; }


    public async Task OpenEditor()
    {
        DialogOptions dialogOptions = new() { MaxWidth = MaxWidth.Medium, FullWidth = true };
        DialogResult result;
        IDialogReference dialog = default!;

        switch (Block)
        {
            case BlockImageEntity blockImage:
                break;
            case BlockIngredientListEntity blockIngredientList:
                break;
            case BlockInstructionEntity blockInstruction:
                var parameters = new DialogParameters<InstructionEditor> { { x => x.blockInstructionEntity, blockInstruction } };
                dialog = await DialogService.ShowAsync<InstructionEditor>("test", parameters, dialogOptions);
                break;

            default:
                throw new InvalidOperationException($"Type de bloc non pris en charge : {typeof(TBlock).Name}");
        }

        result = await dialog.Result;

        if (result.Canceled)
            return;

        dbContext.Update(Block);
        await dbContext.SaveChangesAsync();
    }

}
