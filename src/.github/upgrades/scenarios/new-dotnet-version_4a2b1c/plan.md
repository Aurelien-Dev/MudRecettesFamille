# Plan: Fix BL0008 warnings after .NET 10 upgrade

## Background
Build produces `BL0008` warnings in several Blazor Identity UI `.razor` pages:
> Property '...Input' has `[SupplyParameterFromForm]` and a property initializer. This can be overwritten with null during form posts.

## Goal
Eliminate `BL0008` warnings by ensuring `[SupplyParameterFromForm]`-bound properties do not rely on property initializers.

## Approach
For each affected `.razor` file:
1. Remove inline property initializer from the `[SupplyParameterFromForm]` property (usually `= new();`).
2. Ensure it is not null by initializing in lifecycle:
   - Prefer `OnInitialized()` or `OnParametersSet()` to set `Input ??= new();`
   - Or use a private backing field and expose a non-null property.
3. Ensure any access to `Input.*` is safe (either via non-null initialization or null-forgiving `Input!` only after initialization).

## Validation
- `dotnet build "src/RecettesFamille.sln" -c Debug` should succeed.
- `BL0008` warnings should be eliminated (or reduced to zero).

