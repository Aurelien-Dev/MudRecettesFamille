# Tasks: Upgrade to .NET 10

- [ ] Verify installed SDKs include .NET 10 (`dotnet --list-sdks`).
- [ ] Update all projects `net9.0` -> `net10.0`.
- [ ] Update NuGet packages to .NET 10 compatible versions:
  - [ ] EF Core packages -> `10.x`
  - [ ] Identity EF Core package -> `10.x`
  - [ ] Npgsql EF provider -> EF Core 10 compatible version
  - [ ] Validate `Microsoft.Extensions.AI.OpenAI` compatibility and update if necessary
- [ ] `dotnet restore` solution.
- [ ] `dotnet build` solution.
- [ ] Run Blazor project and confirm startup.
- [ ] Ensure `bin/` and `obj/` are ignored and not tracked.
- [ ] Commit upgrade changes.
