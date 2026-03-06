namespace Imeritas.Agent.DadJokes.CLI.Models;

/// <summary>
/// Represents a dad joke with setup, punchline, and category tags.
/// </summary>
public sealed record Joke(
    string Id,
    string Setup,
    string Punchline,
    IReadOnlyList<string> Categories);
