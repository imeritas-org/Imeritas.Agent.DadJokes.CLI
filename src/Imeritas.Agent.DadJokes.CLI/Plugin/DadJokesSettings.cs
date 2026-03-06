using Imeritas.Agent.Plugins.Configuration;

namespace Imeritas.Agent.DadJokes.CLI.Plugin;

/// <summary>
/// Configuration settings for the DadJokes plugin.
/// </summary>
public class DadJokesSettings
{
    /// <summary>
    /// Maximum number of jokes the AI should tell per session.
    /// </summary>
    [PluginSetting("Max Jokes Per Session",
        Key = "maxJokesPerSession",
        Description = "Maximum number of jokes the plugin will deliver in a single session.",
        Order = 1)]
    public int MaxJokesPerSession { get; set; } = 10;
}
