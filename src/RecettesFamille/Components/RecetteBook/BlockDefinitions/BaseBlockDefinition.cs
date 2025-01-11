using Microsoft.AspNetCore.Components;
using MudBlazor;
using RecettesFamille.Data;
using RecettesFamille.Data.EntityModel.RecipeSubEntity;

namespace RecettesFamille.Components.RecetteBook.BlockDefinitions;

public abstract class BaseBlockDefinition<TBlock> : ComponentBase where TBlock : BlockBase
{
    [Inject] IDialogService DialogService { get; set; }
    [Inject] ApplicationDbContext dbContext { get; set; }

    [Parameter] public TBlock Block { get; set; }
    [Parameter] public bool EditMode { get; set; }

    [Parameter] public Func<BlockBase, Task> OnBlockUpdated { get; set; }
    [Parameter] public Func<BlockBase, Task> OnBlockDeleted { get; set; }
}
