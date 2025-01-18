﻿using Microsoft.AspNetCore.Components;
using MudBlazor;
using RecettesFamille.Data;
using RecettesFamille.Data.EntityModel.RecipeSubEntity;

namespace RecettesFamille.Components.RecetteBook.BlockDefinitions;

public abstract class BaseBlockDefinition<TBlock> : ComponentBase where TBlock : BlockBase
{
    [Inject] IDialogService DialogService { get; set; } = null!;
    [Inject] ApplicationDbContext dbContext { get; set; } = null!;

    [Parameter] public TBlock Block { get; set; } = null!;
    [Parameter] public bool EditMode { get; set; }
    [Parameter] public Func<BlockBase, Task> OnBlockUpdated { get; set; } = null!;
    [Parameter] public Func<BlockBase, Task> OnBlockDeleted { get; set; } = null!;
    [Parameter] public Func<BlockBase, Task> MoveUp { get; set; } = null!;
    [Parameter] public Func<BlockBase, Task> MoveDown { get; set; } = null!;
    [Parameter] public Func<Task> OnBlockHasChanged { get; set; } = null!;

    
}
