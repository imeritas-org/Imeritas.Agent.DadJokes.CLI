using System.ComponentModel;
using Imeritas.Agent.DadJokes.CLI.Services;
using Imeritas.Agent.Extensions;
using Imeritas.Agent.Models;
using Imeritas.Agent.Plugins;
using Imeritas.Agent.Plugins.Configuration;
using Imeritas.Agent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace Imeritas.Agent.DadJokes.CLI.Plugin;

/// <summary>
/// Dad Jokes plugin — delivers dad jokes through a tell_joke kernel function.
/// Singleton plugin with configurable settings and classification contributions.
/// </summary>
public class DadJokesPlugin : IAgentPlugin, IConfigurablePlugin<DadJokesSettings>, IClassificationContributor
{
    private readonly PluginHostContext _host;
    private readonly ILogger<DadJokesPlugin> _logger;

    // ── Plugin Identity ──────────────────────────────────────────────

    /// <inheritdoc />
    public string Name => "DadJokes";

    /// <inheritdoc />
    public string DisplayName => "Dad Jokes";

    /// <inheritdoc />
    public string Description => "Delivers dad jokes on demand, optionally filtered by category.";

    // ── Constructor ──────────────────────────────────────────────────

    /// <summary>
    /// Extension plugin constructor — PluginHostContext injected by ExtensionLoader.
    /// </summary>
    public DadJokesPlugin(PluginHostContext host)
    {
        _host = host;
        _logger = host.LoggerFactory.CreateLogger<DadJokesPlugin>();
    }

    // ── System Prompt ────────────────────────────────────────────────

    /// <inheritdoc />
    public Task<string?> GetSystemPromptContributionAsync(string tenantId, string? taskType = null)
    {
        return Task.FromResult<string?>(
            "You have access to a dad jokes plugin. When a user asks for a joke, " +
            "use the DadJokes-tell_joke function. You can optionally pass a category " +
            "parameter (e.g., 'tech', 'food', 'animals', 'work') to get a themed joke.");
    }

    // ── Directly Invocable Functions ─────────────────────────────────

    /// <inheritdoc />
    public IReadOnlySet<string> DirectlyInvocableFunctions => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "tell_joke"
    };

    // ── Kernel Functions ─────────────────────────────────────────────

    /// <summary>
    /// Tells a dad joke, optionally filtered by category.
    /// </summary>
    [KernelFunction("tell_joke")]
    [Description("Tells a dad joke, optionally filtered by category (e.g., 'tech', 'food', 'animals', 'work')")]
    public string TellJoke(
        [Description("Optional joke category to filter by (e.g., 'tech', 'food', 'animals', 'work')")] string? category = null)
    {
        try
        {
            Models.Joke joke;

            if (string.IsNullOrWhiteSpace(category))
            {
                joke = JokeRepository.GetRandom();
            }
            else
            {
                var matches = JokeRepository.GetByCategory(category);
                joke = matches.Count > 0
                    ? matches[Random.Shared.Next(matches.Count)]
                    : JokeRepository.GetRandom();
            }

            _logger.LogInformation("Told joke {JokeId} for category {Category}", joke.Id, category ?? "(random)");

            return $"{joke.Setup}\n{joke.Punchline}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error telling joke for category {Category}", category);
            return $"Error telling joke: {ex.Message}";
        }
    }

    // ── IClassificationContributor ───────────────────────────────────

    /// <inheritdoc />
    public IReadOnlyList<SlashCommandMapping> SlashCommands => new[]
    {
        new SlashCommandMapping(
            Command: "/joke",
            Intent: IntentType.Task,
            TaskType: "dad_joke",
            ScheduleFrequency: null,
            ResponseText: "Let me tell you a dad joke!",
            SourceName: Name)
    };

    /// <inheritdoc />
    public IReadOnlyList<ClassificationExample> ClassificationExamples => new[]
    {
        new ClassificationExample(
            UserMessage: "Tell me a joke",
            ClassificationPrefix: "[task:dad_joke]",
            ResponseText: "Let me tell you a dad joke!",
            SourceName: Name),
        new ClassificationExample(
            UserMessage: "I need a dad joke about food",
            ClassificationPrefix: "[task:dad_joke]",
            ResponseText: "Here's a food joke for you!",
            SourceName: Name)
    };

    /// <inheritdoc />
    public ClassificationHints ClassificationHints => new()
    {
        SourceName = Name,
        TaskType = "dad_joke",
        TaskTypeKeywords = new[] { "joke", "dad joke", "funny", "humor", "laugh", "punchline" }
    };
}
