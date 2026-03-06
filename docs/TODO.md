# Future Work

Known gaps, architectural debt, and planned features for Imeritas.Agent.DadJokes.CLI.

---

## Test Coverage

### Integration Tests Needed
- Plugin loading via `ExtensionLoader` end-to-end (requires framework host, out of scope for unit tests)
- Orchestrator integration with `TaskRouter` routing (verifying `CanHandle` priority ordering in a real DI container)
- Tenant-scoped plugin settings resolution (requires `ITenantContext` with real tenant config)

### Unit Tests Needed
- `DadJokesPlugin.TellJoke("")` — empty string category (currently only null and unmatched category are tested)
- `DadJokesPlugin.GetSystemPromptContributionAsync` — verify returns expected system prompt content
- `DadJokesPlugin.ClassificationExamples` — verify example declarations and task type mappings
- `DadJokesPlugin.ClassificationHints` — verify keyword hints and task type
- `DadJokeOrchestrator.ExecuteAsync` — verify `_queue_session_id` mapping from inputData
- `DadJokeOrchestrator.ExecuteAsync` — verify `SaveTaskAsync` is called with correct tenant ID

---

## Architectural Debt

- **Joke collection is hardcoded in a static class** — no mechanism to add, remove, or update jokes without recompiling the assembly. A future iteration could load jokes from an embedded JSON resource or external configuration.
- **`MaxJokesPerSession` is declared but not enforced** — `DadJokesSettings.MaxJokesPerSession` exists with a `[PluginSetting]` attribute and default value of 10, but neither `DadJokesPlugin.TellJoke()` nor `DadJokeOrchestrator.ExecuteAsync()` reads or enforces this limit. Enforcement would require session-level state tracking, which conflicts with the singleton plugin pattern (the plugin cannot cache per-session state). A scoped service or session-aware middleware would be needed.
- **No localization support** — all 20 jokes are English-only. Supporting multiple languages would require a localized joke collection and language detection or tenant locale settings.
- **`IConfigurablePlugin<DadJokesSettings>` interface is implemented but settings are unused at runtime** — the plugin declares configurable settings for admin UI generation, but never calls `GetPluginSettingsAsync<DadJokesSettings>()` to read them.

---

## v1 Features

- Joke rating/feedback mechanism (would require persistent storage via `IStorageService`)
- Dynamic joke sources (load from external API or tenant-level configuration)
- Category management via admin UI (`IUIContributor`)
- Multi-language joke support with tenant locale awareness
- Joke-of-the-day scheduled task (would use `IBackgroundTaskQueue`)
- Enforce `MaxJokesPerSession` with a scoped session-tracking service
