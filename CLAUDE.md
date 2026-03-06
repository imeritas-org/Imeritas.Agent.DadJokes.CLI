# CLAUDE.md — Imeritas.Agent.DadJokes.CLI

## What This Is

Dad jokes agent plugin (CLI backend test)

## Project Structure

```
Imeritas.Agent.DadJokes.CLI/
├── src/
│   ├── Imeritas.Agent.DadJokes.CLI/
│   │   ├── Imeritas.Agent.DadJokes.CLI.csproj
│   │   ├── Plugin/
│   │   ├── Orchestrator/
│   │   ├── Services/
│   │   └── Models/
│   └── Imeritas.Agent.DadJokes.CLI.Tests/
│       └── Imeritas.Agent.DadJokes.CLI.Tests.csproj
├── docs/
│   ├── REQUIREMENTS.md
│   ├── TODO.md
│   ├── EVOLUTION.md
│   ├── GAP_ANALYSIS.md
│   ├── CONSIDERATIONS.md
│   ├── DEPLOYMENT.md
│   └── plan-schema.json
├── Imeritas.Agent.DadJokes.CLI.slnx
├── CLAUDE.md
├── agent.yml
├── README.md
├── nuget.config
└── .gitignore
```

## Build & Test

```
dotnet build Imeritas.Agent.DadJokes.CLI.slnx
dotnet test src/Imeritas.Agent.DadJokes.CLI.Tests
```

## Key Architecture

- Follow existing framework extension patterns (reference `HttpPlugin` for multi-instance singleton, `RunbookOrchestrator` for orchestrator lifecycle)
- Plugin is singleton; orchestrator is scoped per task
- Joke data is a static embedded collection (no external API, no database)
- Use `PluginHostContext` for logging and service resolution

## Dependencies

- `Imeritas.Agent.Contracts` — extension point interfaces (`IAgentPlugin`, `ITaskOrchestrator`, `IClassificationContributor`, etc.). Required for all extensions.
- `Imeritas.Agent.Client` — typed REST API + SignalR client for external integrations (dashboards, CLI tools). Only needed if your extension calls the framework API externally.

Both packages are published to GitHub Packages (`imeritas-org`). See `nuget.config` for auth setup.
- `Imeritas.Agent.Contracts` (NuGet from GitHub Packages)
- No additional external dependencies — jokes are embedded in code

## Core Framework Reference (READ-ONLY)

The core Imeritas.Agent framework lives at `../Imeritas.Agent/`. You MUST read it
for context when implementing features — it defines the extension points, patterns,
and conventions this project builds on. **Do NOT modify any files under
`../Imeritas.Agent/`.**

Key references:
- `../Imeritas.Agent/CLAUDE.md` — framework architecture, DI patterns, conventions
- `../Imeritas.Agent/docs/SOLUTION_DEVELOPMENT.md` — **primary extension development guide** (plugin lifecycle, orchestrator patterns, configuration, deployment)
- `../Imeritas.Agent/src/Imeritas.Agent.Contracts/Plugins/` — plugin interfaces (`IAgentPlugin`, `IMultiInstancePlugin`, `IConfigurablePlugin<T>`, `IUIContributor`)
- `../Imeritas.Agent/src/Imeritas.Agent.Contracts/Orchestrators/` — orchestrator interfaces (`ITaskOrchestrator`, `IMultiTurnOrchestrator`)
- `../Imeritas.Agent/src/Imeritas.Agent.Contracts/Services/` — service abstractions (`IClassificationContributor`, `IFileStorageService`)
- `../Imeritas.Agent/src/Imeritas.Agent.Samples/` — reference implementations (HelloWorldPlugin, DocumentReviewOrchestrator)

### Extension Model Summary

| Interface | Purpose | Lifetime |
|-----------|---------|----------|
| `IAgentPlugin` | Expose tools the AI can call via `[KernelFunction]` methods | Singleton |
| `IMultiInstancePlugin` | Multiple named instances of same plugin (e.g., `Http:stripe`) | Factory |
| `IConfigurablePlugin<T>` | Attribute-driven settings with auto-generated admin UI | — |
| `IClassificationContributor` | Slash commands, classification examples, keyword hints | Singleton |
| `ITaskOrchestrator` | Custom task execution strategy (`CanHandle()` + priority) | Scoped |
| `IMultiTurnOrchestrator` | Multi-turn conversation flows with completion tracking | Scoped |

### Key Patterns

- **`PluginHostContext`** — injected into plugin constructors; provides `ITenantContext`, `ILoggerFactory`, `IHttpClientFactory`, `IConfiguration`
- **Tenant-aware singletons** — plugins are singletons; resolve tenant config at call time via `_host.TenantContext.GetPluginSettingsAsync<T>(PluginKey)`, never cache tenant state
- **Kernel functions return strings** — return error messages instead of throwing exceptions so the AI can recover
- **Separate functions over combined** — prefer `SearchContacts()` + `CreateContact()` over monolithic `ManageContacts(action, ...)`
- **`[PluginSetting]` attributes** — decorate settings POCOs for auto-generated config schema and admin UI

If a feature requires core framework changes, do NOT make them directly. Instead,
document the need and create a core enhancement issue.

## Best Practices

- **Unit testing**: All new public methods must have unit tests. Use xUnit + NSubstitute.
- **Manual regression tests**: For any user-facing behavior change, document manual
  regression test steps in the PR description.
- **Structured logging**: Use `ILogger<T>` with parameter placeholders throughout.
  No `Console.WriteLine`.
- **Error handling**: Return structured result objects from services. Use try-catch
  with logging in orchestrators. Return error strings from plugin functions (not
  exceptions) to let the AI recover.
- **Build validation**: Run `dotnet build` and `dotnet test` before marking any
  task complete. Both must pass with 0 errors, 0 warnings.
- **Code style**: C# 12, nullable enabled, file-scoped namespaces, expression-bodied
  members where natural.
- **Security**: No hardcoded secrets. Validate all external input. Follow path
  enforcement patterns.
- **Naming**: Follow framework conventions. XML documentation on public APIs.

## Configuration

Plugin settings configured via Imeritas.Agent admin UI per tenant.

## Common Gotchas

- **Contracts namespaces**: The NuGet package is `Imeritas.Agent.Contracts` but
  code namespaces are `Imeritas.Agent.Plugins`, `Imeritas.Agent.Orchestrators`, etc.
- **Singleton plugin, scoped orchestrator**: Plugin is singleton — resolve tenant
  config at call time via `GetPluginSettingsAsync`, never cache tenant-specific state. Orchestrator is scoped per task.
- **Plugin constructor detection**: Framework tries `PluginHostContext` constructor first, then falls back to parameterless. Do NOT use full DI constructor injection in plugins.
- **Plugin `Name` stability**: Changing a plugin's `Name` property breaks existing tenant configurations. Treat it as immutable after first release.
- **`DirectlyInvocableFunctions`**: Must return at least one entry for the plugin to appear in `/api/v1/plugins`.
- **Kernel plugin naming**: Colons in instance names (e.g., `Http:stripe`) are auto-converted to underscores (`Http_stripe`) for Semantic Kernel compatibility.
- - Plugin `Name` must be unique across all loaded plugins
- - `DirectlyInvocableFunctions` must return at least one entry for the plugin to appear in `/api/v1/plugins`
- - Orchestrator must persist task status via `_storage.SaveTaskAsync` (see framework convention)
- - Use `inputData["_queue_task_id"]` and `inputData["_queue_session_id"]` for task ID mapping
