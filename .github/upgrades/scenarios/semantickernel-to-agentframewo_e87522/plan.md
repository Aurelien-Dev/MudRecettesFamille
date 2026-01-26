# SemanticKernel-To-AgentFramework Migration Plan

> **Strategy**: **All-at-Once Strategy** — all impacted project(s) and shared configuration updated in a single coordinated operation, with no intermediate states.

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Migration Strategy](#2-migration-strategy)
3. [Detailed Dependency Analysis](#3-detailed-dependency-analysis)
4. [Project-by-Project Plans](#4-project-by-project-plans)
   1. [`RecettesFamille.Ai`](#41-project-recettesfamilleai)
5. [Ingestion Improvements Plan](#5-ingestion-improvements-plan)
6. [Risk Management](#6-risk-management)
7. [Testing & Validation Strategy](#7-testing--validation-strategy)
8. [Complexity & Effort Assessment](#8-complexity--effort-assessment)
9. [Source Control Strategy](#9-source-control-strategy)
10. [Success Criteria](#10-success-criteria)

---

## 1. Executive Summary

### Scenario
Migrate from Semantic Kernel Agents to the newer Agent Framework in .NET projects.

### In-Scope Projects
- `src/RecettesFamille.Ai/RecettesFamille.Ai.csproj`

### Current State (from assessment / repo inspection)
- Target framework: `net10.0`
- Package reference present: `Microsoft.SemanticKernel.Core` `1.70.0`
- No source-level usage of Semantic Kernel (including Agents or `Kernel`) was identified within `RecettesFamille.Ai`.
- AI implementation currently uses:
  - `Microsoft.Extensions.AI` (`10.2.0`)
  - `Microsoft.Extensions.AI.OpenAI` (`9.5.0-preview.1.25265.7`)
  - Custom ingestion + vector store + semantic search pipeline
- Ingestion is manually triggered from:
  - `src/RecettesFamille/Components/Pages/Adminitstrateur/AiPage.razor`

### Target State
- Preferred outcome for this scenario:
  - If SK Agents are present (now or added soon): replace SK Agent APIs with Agent Framework equivalents.
- If SK Agents are *not* present:
  - De-scope the scenario to either (a) removing the unused `Microsoft.SemanticKernel.Core` reference, or (b) explicitly introducing Agent Framework only when there is a concrete agent orchestration requirement.

In addition, implement ingestion improvements so that **recipe vectors are refreshed synchronously on each recipe create/update** (rather than only by manual, admin-triggered ingestion).

⚠️ **Requires validation**: confirm whether SK Agents exist elsewhere in the repo or are planned but not yet implemented.

### Selected Strategy
**All-at-Once Strategy** — single coordinated update across the in-scope project(s).

**Rationale**
- In-scope change set is currently small (scenario tooling identified a single project).
- The ingestion improvements are localized but cross-cut project boundaries (web app triggers + AI ingestion pipeline). Doing them in a single coordinated update reduces the chance of mismatched behavior.

### Complexity Classification
**Medium**
- While the scenario output identifies 1 project, the ingestion improvements touch:
  - `RecettesFamille.Ai` (ingestion + version tracking)
  - `RecettesFamille` (where recipe create/update happens, and ingestion should be triggered)
- Main risks: scenario mismatch (agents not used), preview AI packages, and ensuring ingestion remains reliable and fast enough when executed synchronously.

### Critical Issues
- **Scenario applicability risk**: `Microsoft.SemanticKernel.Core` is referenced but not used in code; confirm whether SK Agents exist.
- **Functional gap**: vectors can become stale because ingestion is manual (`/Admin/ai`).

### Notes / Data Gaps
- `upgrade_get_feature_instructions` failed (SQLite exception), and `upgrade_query_assessment` fails (`reportConfig` null). The plan relies on repo inspection + `git grep` + project dependency order.

---

## 2. Migration Strategy

### Approach
**All-at-Once Strategy** as an atomic upgrade within the scenario scope.

### Strategy Details (what changes together)
Perform the following as a single coordinated operation (no intermediate half-migrated states):
- Adjust `RecettesFamille.Ai` package references and any required code changes for the new agent framework (if applicable).
- Implement ingestion improvements to refresh vectors *synchronously* when recipes change.
- Ensure the full solution restores and builds after the coordinated package/code update.

### Key Decision Gate (scenario fit)
Because SK Agents are not currently detected in source:
- **Gate A (confirm SK Agents exist)**: proceed with true Agent Framework migration.
- **Gate B (SK Agents absent)**: treat this as cleanup/realignment (remove unused SK dependency) and optionally defer Agent Framework adoption until agent orchestration is introduced.

### Ordering and Coordination
Even though the work is atomic, think in this order:
1. Identify and lock the target Agent Framework package(s) and versions that support `net10.0` (if Gate A).
2. Identify the recipe create/update save points where synchronous ingestion will run.
3. Implement the targeted “refresh vectors for recipe ID” ingestion API.
4. Wire the web app save points to call the targeted ingestion API after DB commit.
5. Resolve compile errors caused by package API changes.
6. Validate at solution level (build + tests if present).

---

## 3. Detailed Dependency Analysis

### Solution Project Graph (topological order)
From `upgrade_get_projects_in_topological_order`:
1. `src/RecettesFamille.Dto/RecettesFamille.Dto.csproj`
2. `src/RecettesFamille.Data/RecettesFamille.Data.csproj`
3. `src/RecettesFamille.Data.Repository/RecettesFamille.Data.Repository.csproj`
4. `src/RecettesFamille.Ai/RecettesFamille.Ai.csproj`
5. `src/RecettesFamille.Docker/docker-compose.dcproj`
6. `src/RecettesFamille/RecettesFamille.csproj`

### In-Scope Dependency Context
`RecettesFamille.Ai` references:
- `RecettesFamille.Data.Repository`
- `RecettesFamille.Data`
- `RecettesFamille.Dto`

The web app (`RecettesFamille`) consumes AI registrations via:
- `src/RecettesFamille/Program.cs` → `builder.Services.AddAi(builder.Configuration);`

Ingestion is currently triggered from the web app via:
- `src/RecettesFamille/Components/Pages/Adminitstrateur/AiPage.razor` → `DataIngestor.IngestDataAsync(...)`

### Circular Dependencies
None identified from the available dependency ordering.

---

## 4. Project-by-Project Plans

### 4.1 Project: `RecettesFamille.Ai`

**Path**: `src/RecettesFamille.Ai/RecettesFamille.Ai.csproj`

**Current State**
- Target framework: `net10.0`
- Key packages:
  - `Microsoft.SemanticKernel.Core` `1.70.0`
  - `Microsoft.Extensions.AI` `10.2.0`
  - `Microsoft.Extensions.AI.OpenAI` `9.5.0-preview.1.25265.7`
- Observed code areas:
  - DI wiring in `src/RecettesFamille.Ai/DependencyInjection.cs`
  - ingestion pipeline in `src/RecettesFamille.Ai/Services/Ingestion/*`
  - semantic search in `src/RecettesFamille.Ai/Services/SemanticSearch.cs`

**Target State**
- `net10.0` remains unchanged.
- Agent-related dependencies:
  - Replace SK Agent usage with Agent Framework **if SK Agents are used/introduced**, otherwise remove unused SK packages.
- Ingestion subsystem:
  - Supports targeted vector refresh when a recipe is created/updated.
  - Refresh is performed **synchronously** in the recipe save flow (after DB commit).

**Migration Steps (atomic / all-at-once)**
1. Confirm whether Agent Framework is required now (Gate A/B in §2).
2. If Gate A:
   - Introduce required Agent Framework package(s) and align versions with `net10.0`.
   - Replace any SK agent types/usages (if discovered) with Agent Framework equivalents.
3. If Gate B:
   - Remove `Microsoft.SemanticKernel.Core` if truly unused.
   - Keep existing `Microsoft.Extensions.AI` integration as-is.
4. Implement ingestion improvements described in §5 (service surface in `RecettesFamille.Ai`):
   - Add a dedicated method/service to (re)ingest a single recipe by ID and refresh its vectors.
   - Ensure deletion/upsert semantics remain consistent with `IngestionCacheDbContext` tracking.

**Package Update Reference**
⚠️ Requires validation: exact Agent Framework NuGet package IDs and versions were not provided by scenario instructions (tool failure in `upgrade_get_feature_instructions`).

**Expected Breaking Changes**
- If Agent Framework packages are introduced:
  - DI registration / service wiring changes are likely.
  - Tool/function invocation behavior may differ vs SK.
- If only removing SK dependency:
  - Expected breaking changes are low (but ensure no transitive dependency relied on SK packages).
- Ingestion improvements:
  - Synchronous ingestion may affect perceived latency on recipe save.

**Testing Strategy**
- Validate solution compile.
- Validate ingestion flows:
  - Manual admin trigger page still works.
  - Recipe create/update triggers a synchronous refresh.

**Validation Checklist**
- Solution restores successfully.
- Solution builds successfully.
- No leftover references to Semantic Kernel namespaces/types (if Gate B).
- Ingestion can be triggered manually from `/Admin/ai`.
- Updating or creating a recipe results in updated vectors for that recipe immediately after the save completes.

---

## 5. Ingestion Improvements Plan

### Goal
Ensure the system refreshes embeddings / vector records for a recipe whenever that recipe is created or updated, **synchronously as part of the save flow**, without requiring an admin to click “Ingest new data”.

### Explicit Choice
**Chosen model for this plan**: synchronous refresh on recipe create/update.

This implies:
- Recipe save completes **only after** the vectors for that recipe have been regenerated and persisted in the vector store.
- If ingestion fails, the save flow must decide whether to fail the request or allow save-with-stale-vectors (see “Failure policy” below).

### Current Behavior (baseline)
- Ingestion is manually invoked via `src/RecettesFamille/Components/Pages/Adminitstrateur/AiPage.razor`.
- “Modified” detection relies on `IngestedDocument.Version` vs `recipe.UpdatedDate.ToString()`.

### Target Behavior
- On recipe create/update:
  - Delete old vectors for that recipe (based on cached record IDs).
  - Regenerate embeddings.
  - Upsert new vector records.
  - Update ingestion cache so subsequent refreshes do not leak old IDs.
- Manual ingestion remains available as a maintenance tool.

### Required Code/Component Changes (planning-level)

#### 1) Add a targeted ingestion API (single recipe)
In `RecettesFamille.Ai`:
- Add a method/service such as: “refresh vectors for recipe `{id}`”.
- Reuse logic already present in `SQLRecipeSource.CreateRecordsForDocumentAsync(...)` to ensure paragraph splitting and embedding generation are consistent.

Key behavioral requirements:
- **Idempotent**: repeated calls for the same recipe ID should converge to the same state.
- **Atomicity at recipe level**: old vectors for that recipe should be removed before inserting new ones.

#### 2) Improve version tracking
In `SQLRecipeSource` and ingestion cache:
- Replace `UpdatedDate.ToString()` versioning with a stable representation (e.g., UTC ticks or ISO 8601 round-trip) to avoid culture/format issues.

#### 3) Integrate the synchronous trigger in the web app
In `RecettesFamille`:
- Locate where recipes are created/updated.
- After a successful DB commit, call the targeted ingestion API.
- Ensure the call runs in the same request flow so the UI only returns success when vectors are updated.

#### 4) Failure policy (must be specified)
Because the refresh is synchronous, define consistent behavior when ingestion fails:
- **Policy 1 (strict)**: fail the recipe save if vector refresh fails.
- **Policy 2 (lenient)**: allow recipe save, but surface a warning and keep vectors stale until manual reindex.

⚠️ Requires validation: pick strict vs lenient depending on your UX and reliability requirements.

#### 5) Performance/UX constraints
Synchronous ingestion can be slow depending on:
- number of paragraphs per recipe
- embedding model latency

To keep UX acceptable, define constraints to implement during execution:
- cap the maximum paragraphs embedded per save (if necessary)
- add clear UI feedback (loading state) while saving

#### 6) Maintain admin/manual ingestion
- Keep `/Admin/ai` ingestion button as a fallback (bulk reindex).
- Optional: add UI feedback about “last ingested version” vs “current version” for transparency.

### Ingestion Improvement Validation
- Update a recipe (change name/instructions/ingredients) and confirm:
  - save completes successfully
  - vectors are regenerated (old records removed, new records present)
  - semantic search returns updated content

---

## 6. Risk Management

| Risk | Level | Description | Mitigation |
|------|-------|-------------|------------|
| Scenario mismatch | High | SK Agents are not present in code. | Planning gate to confirm/locate SK agent usage before changing dependencies. |
| Preview AI dependencies | High | `Microsoft.Extensions.AI.OpenAI` is a preview package. | Pin exact versions; avoid unrelated upgrades during this scenario unless required. |
| Ingestion consistency | High | Incremental ingestion could leave stale vectors if delete/upsert logic is wrong. | Ensure targeted refresh deletes old vectors for the recipe before upserting; add validation steps. |
| Save latency regression | High | Synchronous embeddings can slow down recipe save. | Add performance constraints (caps), and consider switching to async queue if unacceptable. |
| External dependency failure | Medium | OpenAI calls may fail transiently; in sync mode this blocks saves. | Define strict vs lenient failure policy; implement retries/backoff if chosen. |
| DI integration coupling | Medium | `AddAi` is referenced from `RecettesFamille` (`Program.cs`). | Validate the web app still composes services and starts after changes. |

---

## 7. Testing & Validation Strategy

### Scope
Given limited test discovery data (test discovery returned no test projects), validation focuses on build-level checks and controlled functional smoke checks.

### Validation Checkpoints
- After the atomic dependency/code update: solution builds cleanly.
- AI wiring composes: `AddAi(...)` resolves at runtime (no missing service registrations).

⚠️ Manual functional checks (not automatable tasks):
- Navigate to `/Admin/ai` and trigger ingestion to ensure no runtime exceptions.
- Create/update a recipe and ensure vectors and search results reflect the changes immediately after save (sync model).

---

## 8. Complexity & Effort Assessment

| Project / Area | Complexity | Drivers |
|---------|------------|---------|
| `RecettesFamille.Ai` | Medium | Targeted ingestion API, cache/version handling, and data consistency. |
| `RecettesFamille` | Medium | Identifying correct save hooks and ensuring UX is acceptable with synchronous embeddings. |
| Agent Framework migration | Low → High | Depends on whether Gate A is true; currently no agent usage detected. |

---

## 9. Source Control Strategy

- Work on a dedicated branch: `assess/sk-to-agentframework-e87522` (current).
- For All-at-Once strategy, prefer a **single PR** that contains:
  - all package reference changes
  - all code changes needed for compilation
  - ingestion improvements (sync trigger + targeted refresh)
  - any required configuration updates
- Keep commits minimal (ideally 1–2 commits):
  - Commit 1: atomic migration changes
  - Commit 2 (optional): follow-up polishing (docs / minor refactors) if needed

---

## 10. Success Criteria

### Technical
- `RecettesFamille.Ai` is aligned with the chosen target state:
  - Gate A: Uses Agent Framework instead of SK Agents.
  - Gate B: SK dependency removed if unused.
- Solution restores and builds successfully.

### Functional (ingestion)
- After creating a recipe:
  - vectors for that recipe are generated immediately as part of the save flow.
- After updating a recipe:
  - vectors for that recipe are refreshed (old vectors removed / replaced) before the save returns success.
- `/Admin/ai` manual ingestion remains available and succeeds.

### Quality
- No new warnings/errors introduced by the migration changes.
- No regressions in the ingestion pathway.

### Process (All-at-Once)
- All required dependency/code changes land together (no intermediate broken states).
