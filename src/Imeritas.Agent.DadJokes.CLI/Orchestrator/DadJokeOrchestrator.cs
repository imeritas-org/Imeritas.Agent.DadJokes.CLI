using Imeritas.Agent.DadJokes.CLI.Models;
using Imeritas.Agent.DadJokes.CLI.Services;
using Imeritas.Agent.Models;
using Imeritas.Agent.Orchestrators;
using Imeritas.Agent.Services;
using Microsoft.Extensions.Logging;

namespace Imeritas.Agent.DadJokes.CLI.Orchestrator;

/// <summary>
/// Orchestrator for 'dad_joke' task type. Selects a joke from the static
/// JokeRepository and returns it as the task result.
/// </summary>
public class DadJokeOrchestrator : ITaskOrchestrator
{
    private readonly IStorageService _storage;
    private readonly ILogger<DadJokeOrchestrator> _logger;

    public DadJokeOrchestrator(
        IStorageService storage,
        ILogger<DadJokeOrchestrator> logger)
    {
        _storage = storage;
        _logger = logger;
    }

    /// <inheritdoc />
    public string Name => "DadJokeOrchestrator";

    /// <inheritdoc />
    public int Priority => 50;

    /// <inheritdoc />
    public bool CanHandle(string tenantId, string? taskType, string prompt)
        => taskType == "dad_joke";

    /// <inheritdoc />
    public async Task<AgentTask> ExecuteAsync(
        string tenantId,
        string userId,
        string prompt,
        string? taskType = null,
        Dictionary<string, object>? inputData = null,
        string? parentTaskId = null,
        CancellationToken cancellationToken = default,
        ITaskProgressCallback? progress = null)
    {
        // Map task/session IDs from queue metadata
        var taskId = inputData?.TryGetValue("_queue_task_id", out var tid) == true
            && tid is string tidStr && !string.IsNullOrEmpty(tidStr)
            ? tidStr
            : Guid.NewGuid().ToString();

        var sessionId = inputData?.TryGetValue("_queue_session_id", out var sid) == true
            ? sid?.ToString()
            : null;

        var task = new AgentTask
        {
            TaskId = taskId,
            SessionId = sessionId,
            UserId = userId,
            Title = "Dad Joke",
            Description = prompt,
            Type = TaskType.Manual,
            Status = Imeritas.Agent.Models.TaskStatus.Running,
            StartedAt = DateTime.UtcNow,
            ParentTaskId = parentTaskId,
            InputData = inputData ?? new Dictionary<string, object>()
        };

        try
        {
            _logger.LogInformation(
                "Selecting dad joke for tenant {TenantId}, user {UserId}", tenantId, userId);

            // Extract optional category from inputData
            string? category = null;
            if (inputData?.TryGetValue("category", out var catObj) == true)
            {
                category = catObj?.ToString();
            }

            // Select joke: by category first, fall back to random
            Joke? joke = null;
            if (!string.IsNullOrEmpty(category))
            {
                var matches = JokeRepository.GetByCategory(category);
                if (matches.Count > 0)
                {
                    joke = matches[Random.Shared.Next(matches.Count)];
                }
                else
                {
                    _logger.LogInformation(
                        "No joke found for category '{Category}', falling back to random", category);
                }
            }

            joke ??= JokeRepository.GetRandom();

            // Populate OutputData with structured joke data
            task.OutputData["setup"] = joke.Setup;
            task.OutputData["punchline"] = joke.Punchline;
            task.OutputData["category"] = string.Join(", ", joke.Categories);
            task.OutputData["result"] = $"{joke.Setup}\n{joke.Punchline}";

            task.Status = Imeritas.Agent.Models.TaskStatus.Completed;
            task.CompletedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "Dad joke selected: category={Category}, taskId={TaskId}",
                string.Join(", ", joke.Categories), task.TaskId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error selecting dad joke for tenant {TenantId}, user {UserId}", tenantId, userId);
            task.Status = Imeritas.Agent.Models.TaskStatus.Failed;
            task.ErrorMessage = ex.Message;
        }
        finally
        {
            task.LastUpdatedAt = DateTime.UtcNow;
            await _storage.SaveTaskAsync(tenantId, task);
        }

        return task;
    }
}
