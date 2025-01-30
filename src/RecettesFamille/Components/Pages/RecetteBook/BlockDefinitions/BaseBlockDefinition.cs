using Microsoft.AspNetCore.Components;
using RecettesFamille.Dto.Models.Blocks;

namespace RecettesFamille.Components.Pages.RecetteBook.BlockDefinitions;

public abstract class BaseBlockDefinition<TBlock> : ComponentBase where TBlock : BlockBaseDto
{
    [Parameter] public TBlock Block { get; set; } = null!;
    [Parameter] public bool EditMode { get; set; }
    [Parameter] public Func<BlockBaseDto, Task> OnBlockUpdated { get; set; } = null!;
    [Parameter] public Func<BlockBaseDto, Task> OnBlockDeleted { get; set; } = null!;
    [Parameter] public Func<BlockBaseDto, Task> MoveUp { get; set; } = null!;
    [Parameter] public Func<BlockBaseDto, Task> MoveDown { get; set; } = null!;
    [Parameter] public Func<Task> OnBlockHasChanged { get; set; } = null!;
}
