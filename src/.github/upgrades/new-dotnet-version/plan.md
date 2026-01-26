# Plan: Upgrade solution to .NET 10 (LTS)

## 1. Preparation
- Ensure .NET 10 SDK is installed (`dotnet --list-sdks`).
- Ensure working tree is clean (already committed) and work is on branch `upgrade-to-NET10`.

## 2. Update target frameworks
- Update all projects:
  - `RecettesFamille\RecettesFamille.csproj`
  - `RecettesFamille.Data\RecettesFamille.Data.csproj`
  - `RecettesFamille.Data.Repository\RecettesFamille.Data.Repository.csproj`
  - `RecettesFamille.Ai\RecettesFamille.Ai.csproj`
  - `RecettesFamille.Dto\RecettesFamille.Dto.csproj`
  from `net9.0` -> `net10.0`.

## 3. Update packages (compatibility sweep)
- Update Microsoft/EF packages from `9.x` to `10.x` series:
  - `Microsoft.EntityFrameworkCore.Design`
  - `Microsoft.EntityFrameworkCore.Tools`
  - `Microsoft.EntityFrameworkCore.Sqlite`
  - `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- Update database provider:
  - `Npgsql.EntityFrameworkCore.PostgreSQL` to an EF Core 10 compatible major version.
- Keep non-framework packages as-is unless restore/build indicates incompatibilities.
- Re-evaluate preview package:
  - `Microsoft.Extensions.AI.OpenAI` pick the newest compatible version available for `net10.0` (preview or stable).

## 4. Build validation
- `dotnet restore` on solution.
- `dotnet build` on solution.

## 5. Runtime validation (Blazor)
- `dotnet run` for `RecettesFamille` and verify startup.
- Smoke-check key app paths if feasible.

## 6. Cleanup follow-ups
- Confirm `obj/` is ignored (ensure `.gitignore` contains `**/obj/` and `**/bin/`).
- Remove any inadvertently tracked build artifacts.

## Deliverables
- Updated csproj files targeting `net10.0`.
- Updated package references to compatible versions.
- Build passes.
