using Imeritas.Agent.DadJokes.CLI.Orchestrator;
using Imeritas.Agent.DadJokes.CLI.Services;
using Imeritas.Agent.Models;
using Imeritas.Agent.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Imeritas.Agent.DadJokes.CLI.Tests;

public class DadJokeOrchestratorTests
{
    private readonly DadJokeOrchestrator _orchestrator;
    private readonly IStorageService _storage;

    public DadJokeOrchestratorTests()
    {
        _storage = Substitute.For<IStorageService>();
        _storage.SaveTaskAsync(Arg.Any<string>(), Arg.Any<AgentTask>()).Returns(Task.CompletedTask);
        var logger = NullLogger<DadJokeOrchestrator>.Instance;
        _orchestrator = new DadJokeOrchestrator(_storage, logger);
    }

    [Fact]
    public void CanHandle_DadJokeTaskType_ReturnsTrue()
    {
        Assert.True(_orchestrator.CanHandle("t1", "dad_joke", "tell me a joke"));
    }

    [Fact]
    public void CanHandle_OtherTaskType_ReturnsFalse()
    {
        Assert.False(_orchestrator.CanHandle("t1", "email", "send email"));
    }

    [Fact]
    public void CanHandle_NullTaskType_ReturnsFalse()
    {
        Assert.False(_orchestrator.CanHandle("t1", null, "tell me a joke"));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCompletedTaskWithJoke()
    {
        var result = await _orchestrator.ExecuteAsync("t1", "u1", "tell me a joke");

        Assert.Equal(Imeritas.Agent.Models.TaskStatus.Completed, result.Status);
        Assert.True(result.OutputData.ContainsKey("result"));
        Assert.NotEmpty(result.OutputData["result"]?.ToString()!);
    }

    [Fact]
    public async Task ExecuteAsync_UsesCategoryFromInputData()
    {
        var category = JokeRepository.GetAllCategories()[0];
        var inputData = new Dictionary<string, object> { ["category"] = category };

        var result = await _orchestrator.ExecuteAsync("t1", "u1", "tell me a joke",
            inputData: inputData);

        Assert.Equal(Imeritas.Agent.Models.TaskStatus.Completed, result.Status);
        Assert.NotEmpty(result.OutputData["result"]?.ToString()!);
    }

    [Fact]
    public async Task ExecuteAsync_FallsBackToRandomWhenCategoryNotFound()
    {
        var inputData = new Dictionary<string, object> { ["category"] = "nonexistent_xyz" };

        var result = await _orchestrator.ExecuteAsync("t1", "u1", "tell me a joke",
            inputData: inputData);

        Assert.Equal(Imeritas.Agent.Models.TaskStatus.Completed, result.Status);
        Assert.NotEmpty(result.OutputData["result"]?.ToString()!);
    }

    [Fact]
    public async Task ExecuteAsync_MapsQueueTaskIdFromInputData()
    {
        var inputData = new Dictionary<string, object> { ["_queue_task_id"] = "queue-123" };

        var result = await _orchestrator.ExecuteAsync("t1", "u1", "tell me a joke",
            inputData: inputData);

        Assert.Equal("queue-123", result.TaskId);
    }
}
