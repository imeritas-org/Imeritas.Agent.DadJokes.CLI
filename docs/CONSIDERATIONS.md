# Considerations — Imeritas.Agent.DadJokes.CLI

Design decisions, trade-offs, and forward-looking notes.

---

## 1. Architecture Decisions

### Decision 1: Singleton Plugin with `PluginHostContext` Constructor

**Decision:** `DadJokesPlugin` implements `IAgentPlugin` as a singleton, accepting `PluginHostContext` in its constructor — the standard extension plugin pattern used by the framework's `ExtensionLoader`.

**Alternatives Considered:**
- Full DI constructor injection — not supported by `ExtensionLoader`, which tries `PluginHostContext` first, then falls back to parameterless
- Parameterless constructor — would lose access to logging, tenant context, and HTTP client factory

**Rationale:** `PluginHostContext` provides curated access to `ITenantContext`, `ILoggerFactory`, `IHttpClientFactory`, and `IConfiguration` — exactly what external extension plugins need. Following the framework's extension loading convention ensures the plugin works when dynamically loaded. Tenant-specific config (e.g., `DadJokesSettings`) is available at call time via `_host.TenantContext.GetPluginSettingsAsync<T>()`, never cached, because the plugin is a singleton shared across tenants.

### Decision 2: Static Embedded Joke Collection (Static Class, No External Data Source)

**Decision:** Jokes are stored as a static in-memory collection in the `JokeRepository` static class. Each joke is modeled as an immutable `Joke` record tagged with 1–3 categories. The repository exposes `GetByCategory()`, `GetRandom()`, and `GetAllCategories()` methods.

**Alternatives Considered:**
- External REST API (e.g., icanhazdadjoke.com) — adds latency, network dependency, rate limits, and requires `IHttpClientFactory`
- Database storage (EF Core / SQLite) — adds migration overhead, deployment complexity, and a `ConnectionStrings` dependency
- JSON embedded resource file — adds file I/O, deserialization step, and resource embedding configuration

**Rationale:** The plugin's purpose is pipeline validation, not production joke delivery. A static collection eliminates all external dependencies, makes the plugin fully self-contained, ensures deterministic test behavior, and keeps deployment trivial (single assembly, no config needed). The 20 jokes across 8 categories fit comfortably in memory. If the collection needs to grow, migrating to an embedded JSON resource is straightforward.

### Decision 3: Simple `ITaskOrchestrator` (Not Multi-Turn or Pipeline)

**Decision:** `DadJokeOrchestrator` implements `ITaskOrchestrator` directly with a `CanHandle()`/`ExecuteAsync()` pattern at priority 50. It does not use `IMultiTurnOrchestrator` or a pipeline base class.

**Alternatives Considered:**
- `IMultiTurnOrchestrator` — adds `ICompletionState` tracking, `ContinueSession` management, and multi-turn conversation flow
- Pipeline orchestrator (step-based execution) — adds step definitions and state machine complexity
- No orchestrator at all (rely on `GenericTaskOrchestrator` fallback) — the AI would call `tell_joke` via function calling, but with no prescribed flow

**Rationale:** A dad joke request is inherently single-turn: receive category, select joke, return result. There is no multi-step workflow, no user interaction needed mid-task, and no external system to wait on. A simple orchestrator routes `dad_joke` tasks deterministically without AI overhead, while keeping the implementation minimal. The `GenericTaskOrchestrator` fallback was rejected because it would invoke AI classification and function calling for what is essentially a lookup operation.

### Decision 4: `IClassificationContributor` for Deterministic Routing

**Decision:** `DadJokesPlugin` implements `IClassificationContributor` directly on the plugin class, declaring a `/joke` slash command, two classification examples, and keyword hints — all routing to the `dad_joke` task type.

**Alternatives Considered:**
- No classification contributor — rely on AI classification to infer `dad_joke` task type from natural language (non-deterministic, slower)
- Hardcoded classification in the framework — violates the plugin model's self-describing principle

**Rationale:** Slash commands provide instant, deterministic routing without an AI call. Classification examples teach the AI to recognize joke requests in natural language. Keyword hints (joke, dad joke, funny, humor, laugh, punchline) further improve classification accuracy. All three mechanisms follow the framework's dynamic classification pattern where plugins self-describe their routing needs. Implementing `IClassificationContributor` on the plugin class itself (rather than a separate class) keeps routing declarations co-located with the plugin identity.

### Decision 5: Error Strings over Exceptions in Plugin Functions

**Decision:** The `[KernelFunction] TellJoke()` method returns error messages as strings (e.g., `"Error telling joke: {message}"`) wrapped in a try-catch, rather than throwing exceptions.

**Alternatives Considered:**
- Throwing exceptions — breaks the AI's function-calling loop, produces unhelpful error messages in chat, and may cause retry storms

**Rationale:** Framework convention. Returning error strings lets the AI read the error, understand the context, and either retry with different parameters or explain the issue to the user. This is explicitly documented in the framework's CLAUDE.md and SOLUTION_DEVELOPMENT.md as a best practice for all `[KernelFunction]` methods.

---

## 2. Key Design Principles

1. **CLAUDE.md is the contract.** It defines what the agent knows about the solution.
2. **agent.yml is the boundary.** Allowed paths, build commands, and context paths are declared upfront.
3. **Issues are units of work, not units of code.** Describe capabilities, not files.
4. **Gates are cheap, rework is expensive.** Review plans before implementation.
5. **Context flows forward.** Each completed issue enriches context for subsequent issues.
6. **Build and test are non-negotiable.** Every issue must leave the build green.
