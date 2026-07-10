# AGENTS.md

Guidance for coding agents working in this repository.

## Scope

These instructions apply to the entire repository.

## Project Overview

Finbuckle.MultiTenant is a .NET 10 multi-tenancy library. The solution is organized around NuGet package projects, xUnit test projects, docs, and runnable samples.

- `Finbuckle.MultiTenant.slnx` is the solution file.
- `src/` contains package projects:
  - `Finbuckle.MultiTenant.Abstractions`
  - `Finbuckle.MultiTenant`
  - `Finbuckle.MultiTenant.AspNetCore`
  - `Finbuckle.MultiTenant.EntityFrameworkCore`
  - `Finbuckle.MultiTenant.Identity.EntityFrameworkCore`
- `test/` contains xUnit tests for the packages.
- `docs/` contains the documentation copied to the website.
- `samples/` contains sample apps; `samples/IdentityAppSample/wwwroot/lib/` is vendored client-side library content.

## Build and Test

Use the .NET 10 SDK.

- Build from the repository root:
  ```bash
  dotnet build
  ```
- CI-equivalent build:
  ```bash
  dotnet build -p:ContinuousIntegrationBuild=true
  ```
- Run all tests:
  ```bash
  dotnet test
  ```
- CI runs these test projects after build:
  ```bash
  dotnet test --no-build -v q -f net10.0 test/Finbuckle.MultiTenant.Test
  dotnet test --no-build -v q -f net10.0 test/Finbuckle.MultiTenant.AspNetCore.Test
  dotnet test --no-build -v q -f net10.0 test/Finbuckle.MultiTenant.EntityFrameworkCore.Test
  dotnet test --no-build -v q -f net10.0 test/Finbuckle.MultiTenant.Identity.EntityFrameworkCore.Test
  ```
- Release workflows use Release configuration:
  ```bash
  dotnet restore
  dotnet build -c Release --no-restore -p:ContinuousIntegrationBuild=true
  dotnet test -c Release --no-build
  ```

Run the narrowest relevant tests while iterating, then run `dotnet test` or the applicable CI-equivalent commands for broader changes.

## Coding Conventions

- Keep code nullable-clean; source projects enable nullable reference types and treat warnings as errors.
- Use file-scoped namespaces and the existing C# style in nearby files.
- Preserve the copyright header on C# source and test files:
  ```csharp
  // Copyright Finbuckle LLC, Andrew White, and Contributors.
  // Refer to the solution LICENSE file for more information.
  ```
- Prefer standard .NET abstractions and existing library patterns over new helper layers.
- Keep public API changes deliberate and documented with XML comments where nearby public APIs use them.
- Avoid broad formatting-only changes. There is no checked-in `.editorconfig`, so match the surrounding file.
- Do not modify generated, vendored, or third-party assets under `samples/IdentityAppSample/wwwroot/lib/` unless explicitly requested.

## Tests

- Tests use xUnit and Moq.
- Test classes are generally named `*Should`, with behavior-oriented `[Fact]` methods such as `ResolveTenantContextIfTenantFound`.
- Place tests in the matching project under `test/` for the package being changed.
- When changing tenant resolution, stores, strategies, ASP.NET Core middleware, EF Core behavior, Identity integration, or options behavior, add or update focused tests for the changed behavior.
- Test projects may override package versions from `test/Directory.Build.props`; follow the local pattern rather than introducing central package management.

## Package and Dependency Notes

- Package metadata and shared source settings live in `src/Directory.Build.props`.
- Package lock files are enabled. If package references change, update the corresponding `packages.lock.json` files intentionally.
- All source projects currently target `net10.0`.
- Do not change package versions, package metadata, target frameworks, or lock files as a side effect of unrelated work.

## Docs and Samples

- Update `docs/` and sample README files when public behavior, setup, or usage changes.
- Keep docs version markers and release-note blocks intact:
  - `<span class="_version">...`
  - `<!--_release-notes-->`
  - `<!--_history-->`
- Prefer small, runnable sample changes that demonstrate real package usage.

## Release Workflow

- `prepare-release.cs` is a release automation script. Do not run or edit it unless the task is explicitly release-related.
- Release notes are based on Conventional Commits. Use conventional commit syntax for commit messages.
- Release preparation updates `CHANGELOG.md`, `README.md`, `docs/*.md`, `docs/History.md`, and `src/Directory.Build.props`; avoid manual release-version churn outside release tasks.

## Git Workflow

- Prefer squash and rebase for branch cleanup.
- Keep multiple version-changing commits separate when they exist in the same branch, so release automation can calculate and document the intended version impact.
- Use Conventional Commit messages in the form `<type>(<scope>)!: <imperative summary>`. Omit the optional scope only for genuinely repository-wide work such as `ci:` or `build:`.
- Use the package-oriented scopes already established in history:
  - `finbuckle` for `Finbuckle.MultiTenant` and its core behavior, stores, options, and abstractions.
  - `aspnetcore` for `Finbuckle.MultiTenant.AspNetCore`.
  - `efcore` for `Finbuckle.MultiTenant.EntityFrameworkCore`.
  - `efcoreidentity` for `Finbuckle.MultiTenant.Identity.EntityFrameworkCore`.
  - `samples`, `docs`, `ci`, and `build` for changes primarily affecting those areas.
- Make each commit one reviewable intent. Include the production change with its focused tests and required documentation/sample update; do not combine unrelated refactors, formatting, dependency upgrades, or generated artifacts.
- Use `!` and a `BREAKING CHANGE:` footer for intentional public API or behavior breaks. Keep breaking changes separate from unrelated feature or fix commits.
- Before each commit, inspect `git diff`, stage only the files belonging to that intent, then run `git diff --cached --check` and the narrowest relevant tests. Do not stage `bin/`, `obj/`, `TestResults/`, coverage reports, or package output.
- Preserve any pre-existing user changes in the worktree. Do not amend, reorder, squash, or rewrite commits outside the current task without explicit user direction.
- Rewriting local branch history to improve commit messages or split commits requires explicit user direction. After a rewrite, verify `git log <base>..HEAD`, remove temporary rewrite refs, and clearly report that a force-push is required if the branch was previously published.

## Agent Workflow

- Inspect existing code before editing and keep changes scoped to the requested behavior.
- Preserve user changes already present in the worktree.
- Prefer `rg`/`rg --files` for repository searches.
- For implementation changes, run relevant tests and report exactly what was run. If a command cannot be run, say why.
- For documentation-only changes, tests are usually unnecessary; verify the changed Markdown directly.
