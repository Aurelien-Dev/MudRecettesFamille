using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using RecettesFamille.Data;

namespace RecettesFamille.Components.Pages;

public class MyComponentBase : ComponentBase
{
    [Inject] public IDbContextFactory<ApplicationDbContext> DbContextFactory { get; set; } = null!;

    public ApplicationDbContext DbContext { get; set; } = null!;


    protected async override Task OnInitializedAsync()
    {
        DbContext = await DbContextFactory.CreateDbContextAsync();
    }
}