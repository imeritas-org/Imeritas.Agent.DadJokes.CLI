# Imeritas.Agent.DadJokes

An extension plugin for the Imeritas.Agent framework that delivers contextually relevant dad jokes through chat and invocable functions. A lightweight, self-contained plugin used to validate the end-to-end delivery pipeline (scaffold → plan → decompose → implement).

## Features

1. **Joke Collection Service** — An in-memory joke repository with ~20 embedded dad jokes, each tagged with 1-3 categories (e.g. "tech", "food", "animals", "work"). Supports lookup by category and random selection.

2. **Plugin** — A singleton `IAgentPlugin` named `"DadJokes"` that registers a `tell_joke` directly invocable function. The function accepts an optional `category` parameter and returns a joke. Settings class with a `MaxJokesPerSession` integer (default 10).

3. **Orchestrator** — A simple `ITaskOrchestrator` for task type `"dad_joke"`. Receives a topic/category in input data, selects a matching joke via the service, and returns it as the task result. If no category match, returns a random joke.

4. **Unit Tests** — xUnit tests covering: joke service category lookup, random fallback, plugin function invocation, orchestrator input parsing and result formatting.

## Architecture

- Follow existing framework extension patterns (reference `HttpPlugin` for multi-instance singleton, `RunbookOrchestrator` for orchestrator lifecycle)
- Plugin is singleton; orchestrator is scoped per task
- Joke data is a static embedded collection (no external API, no database)
- Use `PluginHostContext` for logging and service resolution

## Dependencies

- `Imeritas.Agent.Contracts` (NuGet from GitHub Packages)
- No additional external dependencies — jokes are embedded in code

## Gotchas

- Plugin `Name` must be unique across all loaded plugins
- `DirectlyInvocableFunctions` must return at least one entry for the plugin to appear in `/api/v1/plugins`
- Orchestrator must persist task status via `_storage.SaveTaskAsync` (see framework convention)
- Use `inputData["_queue_task_id"]` and `inputData["_queue_session_id"]` for task ID mapping
