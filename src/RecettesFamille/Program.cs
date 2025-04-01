using BitzArt.Blazor.Cookies;
using Blazored.LocalStorage;
using Cropper.Blazor.Extensions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using MudBlazor.Services;
using RecettesFamille;
using RecettesFamille.Components;
using RecettesFamille.Components.Account;
using RecettesFamille.Data;
using RecettesFamille.Data.Repository;
using RecettesFamille.Dto.Models;
using RecettesFamille.Managers;
using RecettesFamille.Managers.AiGenerators;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

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

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents(
    options =>
    {
        options.DetailedErrors = builder.Environment.IsDevelopment();
    });

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddScoped<EmailManager>();

builder.Services.AddRepository();

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
    var postgresCs = "Host=recettesfamille.data;Port=5432;Database=recettesfamilledb;Username=pguser;Password=PGUserPwd;Pooling=true";
    options.UseNpgsql(postgresCs);
}, ServiceLifetime.Scoped);

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();
builder.Services.Configure<IdentityOptions>(options =>
{
    options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
});

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
builder.Services.AddServerSideBlazor()
    .AddHubOptions(options =>
    {
        options.MaximumReceiveMessageSize = 32 * 1024 * 100;
    });

builder.Logging.AddConsole();

var app = builder.Build();

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

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

// Define a minimal API endpoint that calls the AskImage method of AiManager
app.MapPost("/api/youtube-summary", async (HttpRequest request, [FromServices] AiManager aiManager, CancellationToken cancellationToken) =>
{
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();
    var requestBody = JsonSerializer.Deserialize<YoutubeSummaryJson>(body);
    var resume = await aiManager.GetYoutubeResume(requestBody, cancellationToken);
    return Results.Ok(new { result = resume });
});

await app.RunAsync();

public class YoutubeSummaryJson
{
    public required string Transcript { get; set; }
    public required string Url { get; set; }
    public required string Title { get; set; }
}
