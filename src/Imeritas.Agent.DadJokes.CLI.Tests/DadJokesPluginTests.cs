using Imeritas.Agent.DadJokes.CLI.Plugin;
using Imeritas.Agent.DadJokes.CLI.Services;
using Imeritas.Agent.Extensions;
using Imeritas.Agent.Models;
using Imeritas.Agent.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Imeritas.Agent.DadJokes.CLI.Tests;

public class DadJokesPluginTests
{
    private readonly DadJokesPlugin _plugin;

    public DadJokesPluginTests()
    {
        var tenantContext = Substitute.For<ITenantContext>();
        tenantContext.ResolveTenantId().Returns("test-tenant");
        tenantContext.GetTenantConfigAsync("test-tenant").Returns(Task.FromResult(new TenantConfig
        {
            TenantId = "test-tenant",
            Name = "Test",
            Plugins = new Dictionary<string, PluginTenantConfig>()
        }));

        var loggerFactory = NullLoggerFactory.Instance;
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var configuration = new ConfigurationBuilder().Build();

        var host = new PluginHostContext(tenantContext, loggerFactory, httpClientFactory, configuration);
        _plugin = new DadJokesPlugin(host);
    }

    [Fact]
    public void TellJoke_ValidCategory_ReturnsFormattedJokeString()
    {
        var category = JokeRepository.GetAllCategories()[0];

        var result = _plugin.TellJoke(category);

        Assert.NotEmpty(result);
        Assert.Contains("\n", result);
    }

    [Fact]
    public void TellJoke_NullCategory_ReturnsRandomJoke()
    {
        var result = _plugin.TellJoke(null);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void TellJoke_UnmatchedCategory_ReturnsRandomJoke()
    {
        var result = _plugin.TellJoke("nonexistent_xyz_category");

        Assert.NotEmpty(result);
    }

    [Fact]
    public void Plugin_NameDisplayNameDescription_AreCorrect()
    {
        Assert.Equal("DadJokes", _plugin.Name);
        Assert.NotEmpty(_plugin.DisplayName);
        Assert.NotEmpty(_plugin.Description);
    }

    [Fact]
    public void DirectlyInvocableFunctions_IncludesTellJoke()
    {
        Assert.Contains("tell_joke", _plugin.DirectlyInvocableFunctions);
    }

    [Fact]
    public void SlashCommands_IncludesJokeCommand()
    {
        var contributor = (IClassificationContributor)_plugin;

        Assert.Contains(contributor.SlashCommands, c => c.Command == "/joke");
    }
}
