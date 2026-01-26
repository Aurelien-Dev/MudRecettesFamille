# Assessment Report: SemanticKernel-To-AgentFramework

**Date**: 2026-01-26  
**Repository**: `D:\01_dev\MudRecettesFamille\MudRecettesFamille`  
**Solution**: `src\RecettesFamille.sln`  
**Assessment Mode**: Generic (scenario assessment instructions were not returned by `upgrade_get_feature_instructions`)  
**Assessor**: Modernization Assessment Agent

---

## Executive Summary

This assessment reviewed the current state of AI-related code primarily in `RecettesFamille.Ai\RecettesFamille.Ai.csproj` to determine readiness for migrating from Semantic Kernel Agents to a newer .NET Agent Framework.

The project currently references `Microsoft.SemanticKernel.Core`, but no usage of Semantic Kernel APIs (including agents or `Kernel`) was identified in the assessed source files. The implemented AI functionality is centered around `Microsoft.Extensions.AI` abstractions, OpenAI client adapters, and a custom ingestion + vector search pipeline.

A practical modernization concern was also raised and verified: ingestion is **manually triggered from the web UI** (admin page), so vectors can remain stale after recipe changes unless ingestion is re-run.

---

## Scenario Context

**Scenario Objective**: Migrate from Semantic Kernel Agents to Agent Framework.

**Assessment Scope**:
- Project: `src\RecettesFamille.Ai\RecettesFamille.Ai.csproj`
- Related integration points in the web app where AI services are wired and ingestion is triggered:
  - `src\RecettesFamille\Program.cs`
  - `src\RecettesFamille\Components\Pages\Adminitstrateur\AiPage.razor`

**Methodology**:
- Read-only inspection of project files to identify package references and target framework.
- Enumeration of project code files.
- Searches for Semantic Kernel / agent-related identifiers via `git grep`.
- Review of the AI DI wiring and ingestion pipeline files.

---

## Current State Analysis

### Project Overview (`RecettesFamille.Ai`)

- `RecettesFamille.Ai\RecettesFamille.Ai.csproj` targets **`.NET 10`** (`<TargetFramework>net10.0</TargetFramework>`).

**Key package references** (`RecettesFamille.Ai\RecettesFamille.Ai.csproj`):
- `Microsoft.Extensions.AI` `10.2.0`
- `Microsoft.Extensions.AI.OpenAI` `9.5.0-preview.1.25265.7`
- `Microsoft.SemanticKernel.Core` `1.70.0`

### AI Wiring / Dependency Injection

File: `src\RecettesFamille.Ai\DependencyInjection.cs`

Observed:
- Creates an `OpenAIClient` and adapts OpenAI chat and embedding clients via `.AsIChatClient()` and `.AsIEmbeddingGenerator()`.
- Registers chat + function invocation middleware:
  - `services.AddChatClient(chatClient).UseFunctionInvocation().UseLogging();`
- Registers embeddings and custom vector store:
  - `services.AddEmbeddingGenerator(embeddingGenerator);`
  - `services.AddSingleton<IVectorStore>(vectorStore);`

File: `src\RecettesFamille\Program.cs`

Observed:
- The web app wires AI services with `builder.Services.AddAi(builder.Configuration);` (line 62).

### Ingestion Triggering (Manual)

File: `src\RecettesFamille\Components\Pages\Adminitstrateur\AiPage.razor`

Observed:
- A button `Ingest new data` is exposed to admins.
- On click, it calls ingestion manually:
  - `await DataIngestor.IngestDataAsync(service, new SQLRecipeSource(dbContext));` (line 24)

**Implication**:
- Ingestion runs only when an admin triggers it; vectors will not reflect recipe adds/updates until ingestion is executed again.

**User expectation (requirement clarification for planning)**:
- You stated: “il faudrait pouvoir mettre à jour le Vector de la recette à chaque mise à jour”. This is an enhancement requirement that is separate from the SK Agents → Agent Framework scenario, but relevant to overall AI modernization readiness.

### Ingestion Pipeline (Vector Refresh Logic)

Files:
- `src\RecettesFamille.Ai\Services\Ingestion\DataIngestor.cs`
- `src\RecettesFamille.Ai\Services\Ingestion\SQLRecipeSource.cs`

Observed:
- Ingestion maintains a cache (`IngestionCacheDbContext`) of ingested documents and associated vector record IDs.
- The logic attempts to ingest only “new or modified” documents and delete vector records for removed documents.

**Change detection mechanism**:
- `SQLRecipeSource.GetNewOrModifiedDocumentsAsync(...)` compares cached version to `recipe.UpdatedDate.ToString()`.

**What this implies**:
- If ingestion is not re-run after a recipe is added/updated, the vector store will not reflect the change.
- Version tracking via `UpdatedDate.ToString()` can be sensitive to formatting/culture/precision issues.

### Vector Store / Retrieval

- `src\RecettesFamille.Ai\Services\JsonVectorStore.cs`:
  - Custom JSON-backed `IVectorStore` with cosine similarity ranking.
  - Uses reflection to read `Key` and `Vector` properties from record types.

- `src\RecettesFamille.Ai\Services\SemanticSearch.cs`:
  - Generates an embedding for the query and performs vector search against the collection.

---

## Evidence: Semantic Kernel / Agents Usage Search

Searches performed:
- `git grep` for `SemanticKernel`, `Microsoft.SemanticKernel`, `Kernel`, `Kernel.` limited to `src/RecettesFamille.Ai`.

Result:
- Only hit: `src/RecettesFamille.Ai/RecettesFamille.Ai.csproj` includes `Microsoft.SemanticKernel.Core` (`1.70.0`).
- No hits in `.cs` code showing SK usage.

---

## Issues and Concerns

### Critical Issues

1. **Scenario mismatch: no Semantic Kernel Agents usage identified**
   - **Description**: The selected scenario targets migrating SK *Agents* to Agent Framework. The project references `Microsoft.SemanticKernel.Core`, but no code uses SK agents or even SK `Kernel`.
   - **Impact**: There may be nothing agent-specific to migrate in the current codebase. Planning must confirm whether SK Agents are used elsewhere (another project/branch) or whether the scenario should be adjusted.
   - **Evidence**:
     - `RecettesFamille.Ai\RecettesFamille.Ai.csproj` contains `Microsoft.SemanticKernel.Core`.
     - `git grep` found no SK usage beyond the csproj.
   - **Severity**: Critical

### High Priority Issues

1. **Ingestion is manual; vectors can be stale after recipe updates**
   - **Description**: Ingestion is triggered manually via an admin page and not automatically on recipe changes.
   - **Impact**: Users may observe outdated semantic search results after adding/updating recipes until ingestion is re-run.
   - **Evidence**:
     - `src\RecettesFamille\Components\Pages\Adminitstrateur\AiPage.razor` line 24.
   - **Severity**: High

2. **Mixed / preview dependency surface (AI packages)**
   - **Description**: `Microsoft.Extensions.AI.OpenAI` is a preview package (`9.5.0-preview.1.25265.7`).
   - **Impact**: Migration decisions (including any future agent framework adoption) may be constrained by preview API churn.
   - **Evidence**: `RecettesFamille.Ai\RecettesFamille.Ai.csproj`.
   - **Severity**: High

### Medium Priority Issues

1. **Version tracking uses `UpdatedDate.ToString()`**
   - **Description**: The ingestion cache compares versions using a date formatted as a string.
   - **Impact**: Potential false positives/negatives depending on formatting/culture/precision; can contribute to “not updated” reports.
   - **Evidence**: `src\RecettesFamille.Ai\Services\Ingestion\SQLRecipeSource.cs` (sets `Version = recipe.UpdatedDate.ToString()`).
   - **Severity**: Medium

2. **Namespace inconsistency**
   - **Description**: `DependencyInjection.cs` declares `namespace RecettesFamille.Data.Repository;` while located in `src\RecettesFamille.Ai`.
   - **Impact**: Can make discovery/usage patterns confusing.
   - **Evidence**: `src\RecettesFamille.Ai\DependencyInjection.cs`.
   - **Severity**: Medium

---

## Risks and Considerations

### Identified Risks

1. **Agent Framework migration may not apply unless agent usage exists**
   - **Likelihood**: High
   - **Impact**: Medium

2. **Preview package API churn**
   - **Likelihood**: Medium
   - **Impact**: Medium

### Unknowns / Requires Follow-up

- Whether Semantic Kernel *Agents* are used in other projects or branches.
- Whether `Microsoft.SemanticKernel.Core` is a leftover (unused) reference.
- Whether ingestion is expected to become automatic (event-driven) vs staying manual.

---

## Opportunities and Strengths

- Modern `.NET 10` target with `Microsoft.Extensions.AI` already in use.
- Clear separation between ingestion, retrieval, and storage.

---

## Recommendations for Planning Stage (Non-Prescriptive)

These are observations to help planning; they are not an implementation plan.

- Confirm scenario fit by locating any actual SK Agent usage (if any).
- If search freshness is a key requirement, planning should explicitly account for the ingestion triggering model (manual vs automatic) and the versioning mechanism used for change detection.

---

## Assessment Artifacts

### Tools Used

- `upgrade_get_repo_state`: branch / pending change detection
- `upgrade_get_projects_info` / `get_projects_in_solution` / `get_files_in_project`: project discovery
- `get_file`: inspected key files
- `git grep` (via terminal): searched for Semantic Kernel / ingestion trigger references

### Files Analyzed (key)

- `src\RecettesFamille.Ai\RecettesFamille.Ai.csproj`
- `src\RecettesFamille.Ai\DependencyInjection.cs`
- `src\RecettesFamille.Ai\Services\Ingestion\DataIngestor.cs`
- `src\RecettesFamille.Ai\Services\Ingestion\SQLRecipeSource.cs`
- `src\RecettesFamille\Components\Pages\Adminitstrateur\AiPage.razor`
- `src\RecettesFamille\Program.cs`

---

## Conclusion

`RecettesFamille.Ai` currently implements AI functionality via `Microsoft.Extensions.AI` and a custom ingestion + vector search pipeline. While `Microsoft.SemanticKernel.Core` is referenced, no Semantic Kernel agent usage was identified, so the selected migration scenario may not be applicable without further confirmation.

Separately, ingestion freshness is currently dependent on a manual admin action, which can lead to stale vectors after recipe edits.

**Next Steps**: The assessment is ready for the Planning stage, where scenario fit and scope can be confirmed.

---

*This assessment was generated by the Assessment Agent to support the Planning and Execution stages of the modernization workflow.*
