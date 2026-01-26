# Assessment: Upgrade to .NET 10 (LTS)

## Scope
Solution `RecettesFamille.sln` contains 5 projects:
- `RecettesFamille` (Blazor, `Microsoft.NET.Sdk.Web`)
- `RecettesFamille.Data`
- `RecettesFamille.Data.Repository`
- `RecettesFamille.Ai`
- `RecettesFamille.Dto`

All projects currently target `net9.0`.

## Current package landscape (high level)
`RecettesFamille` (Blazor):
- UI: `MudBlazor`, `MudBlazor.Markdown`
- Data/EF: `Microsoft.EntityFrameworkCore.Design` (`9.0.*`)
- AI: `Microsoft.Extensions.AI` / `Microsoft.Extensions.AI.OpenAI` (includes preview)
- Imaging/Mail: `Magick.NET*`, `MailKit`, `MimeKit`

`RecettesFamille.Data`:
- Identity EF Core: `Microsoft.AspNetCore.Identity.EntityFrameworkCore` `9.0.9`
- EF tools: `Microsoft.EntityFrameworkCore.Tools` `9.0.9`
- Provider: `Npgsql.EntityFrameworkCore.PostgreSQL` `9.0.*`

`RecettesFamille.Ai`:
- EF Sqlite `9.0.7`
- `Microsoft.SemanticKernel.Core` `1.60.0`
- `Microsoft.Extensions.AI.OpenAI` preview

## Key upgrade considerations / risks
1. **Target framework bump**: switching from `net9.0` to `net10.0` across all projects.
2. **Package alignment**:
   - EF Core packages should move from `9.x` to `10.x` (including tooling packages).
   - `Npgsql.EntityFrameworkCore.PostgreSQL` should align to a compatible major version for EF Core 10.
   - `Microsoft.AspNetCore.Identity.EntityFrameworkCore` should align with .NET 10.
3. **Preview dependencies**:
   - `Microsoft.Extensions.AI.OpenAI` is currently a preview version. .NET 10 upgrade may require picking a newer preview/stable that supports `net10.0`.
4. **Build artifacts in repo**:
   - `obj\Debug\net9.0\...` files appear in workspace project file listings. These shouldn’t be committed; they can interfere with analysis and produce noise.
5. **Blazor-specific changes**:
   - The app is a Blazor project; any .NET 10 Blazor hosting model / authentication template differences may require minor code updates (usually `Program.cs`, auth components, endpoints).

## What will be upgraded
- All `TargetFramework` entries `net9.0` -> `net10.0`.
- NuGet packages updated to versions compatible with .NET 10 / EF Core 10.
- Validation via restore/build/test.

## Known unknowns (need verification during execution)
- Exact .NET SDK available on the machine/agent (must have .NET 10 SDK installed).
- Whether any 3rd-party packages pinned to `9.x` block upgrade.
- Whether solution contains integration/unit tests (none discovered yet).
