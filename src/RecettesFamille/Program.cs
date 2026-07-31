using BitzArt.Blazor.Cookies;
using Blazored.LocalStorage;
using Cropper.Blazor.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using MudBlazor.Services;
using RecettesFamille;
using RecettesFamille.Api;
using RecettesFamille.Components;
using RecettesFamille.Data;
using RecettesFamille.Data.EntityModel;
using RecettesFamille.Data.Repository;
using RecettesFamille.Managers;
using RecettesFamille.Managers.AiGenerators;
using RecettesFamille.Rag.Extensions;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

builder.Configuration
       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
       //.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
       .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);


// Configure Kestrel
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080); // HTTP
});

// Add MudBlazor services
builder.Services.AddMudServices();
builder.Services.AddMudMarkdownServices();
builder.Services.AddBlazoredLocalStorage();
builder.AddBlazorCookies();

builder.Services.AddCropper();

builder.Services.Configure<SecurityStampValidatorOptions>(o => o.ValidationInterval = TimeSpan.FromSeconds(5));

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents(
    options =>
    {
        options.DetailedErrors = builder.Environment.IsDevelopment();
    });

builder.Services.AddHttpContextAccessor();

// Ajouter HttpClient pour les appels API internes
builder.Services.AddScoped(sp =>
{
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navigationManager.BaseUri) };
});

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<EmailManager>();

builder.Services.AddRepository();

// Configure RAG services
var serverUrl = builder.Configuration["DB_HOST_URL"];
var serverPort = builder.Configuration["DB_HOST_PORT"];
var postgresCs = $"Host={serverUrl};Port={serverPort};Database=test;Username=pguser;Password=PGUserPwd;Pooling=true";

builder.Services.AddRecetteFamilleRag(options =>
{
    options.ConnectionString = postgresCs;
    options.OpenAIKey = builder.Configuration["OPENAI_SECRET"] ?? throw new InvalidOperationException("Missing OPENAI_SECRET configuration");
    options.EmbeddingModel = "text-embedding-3-small";
    options.ChatModel = "gpt-4o-mini";
});

// Register RAG search service for RecipeEntity
builder.Services.AddRagSearchService<RecipeEntity>(sp => sp.GetRequiredService<ApplicationDbContext>());

// Register RecipeRagService wrapper
builder.Services.AddScoped<RecettesFamille.Services.RecipeRagService>();

builder.Services.AddManagers(builder.Configuration);
builder.Services.AddScoped<AiManager>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
{
    var serverUrl = builder.Configuration["DB_HOST_URL"];
    var serverPort = builder.Configuration["DB_HOST_PORT"];

    var postgresCs = $"Host={serverUrl};Port={serverPort};Database=recettesfamilledb;Username=pguser;Password=PGUserPwd;Pooling=true";
    options.UseNpgsql(postgresCs);
}, ServiceLifetime.Scoped);

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;          
    options.Password.RequireLowercase = false;      
    options.Password.RequireUppercase = false;      
    options.Password.RequireNonAlphanumeric = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders()
    .AddApiEndpoints(); // Active les endpoints API Identity

// Configuration des cookies Identity pour partage entre API et circuits Blazor
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = ".RecettesFamille.Identity";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;

    // Configurer les chemins de redirection personnalisés
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/login";
});
builder.Services.Configure<IdentityOptions>(options =>
{
    options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
});

builder.Services.AddServerSideBlazor()
    .AddHubOptions(options =>
    {
        options.MaximumReceiveMessageSize = 32 * 1024 * 100;
    });

builder.Logging.AddConsole();

var app = builder.Build();

// Initialize RAG database (creates pgvector collection if needed)
app.Services.InitializeRagDatabase();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

// IMPORTANT: Ajouter Authentication et Authorization AVANT MapRazorComponents
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

// Enregistrer les endpoints d'authentification personnalisés
app.MapAuthEndpoints();

// Enregistrer les endpoints utilitaires
app.MapUtilityEndpoints();

await app.RunAsync();

public record LoginRequest(string Email, string Password, bool RememberMe, string? ReturnUrl);
