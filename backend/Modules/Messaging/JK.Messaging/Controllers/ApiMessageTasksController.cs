using JK.Messaging.Contracts;
using JK.Messaging.Grains;
using JK.Messaging.Models;
using JK.Messaging.States;
using JK.Platform.Core.Correlation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Orleans;

namespace JK.Messaging.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
public class ApiMessageTasksController : ControllerBase
{
    private readonly IClusterClient _clusterClient;
    private readonly ICorrelationContextAccessor _correlationContextAccessor;

    public ApiMessageTasksController(
        IClusterClient clusterClient,
        ICorrelationContextAccessor correlationContextAccessor)
    {
        _clusterClient = clusterClient;
        _correlationContextAccessor = correlationContextAccessor;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiMessageTaskState), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiMessageTaskState>> Create(
        [FromBody] CreateApiMessageTaskRequest request,
        CancellationToken cancellationToken)
    {
        var grain = _clusterClient.GetGrain<IApiMessageTaskGrain>(request.TaskId);

        var registered = await grain.Register(new RegisterApiMessageTaskCommand
        {
            Id = request.TaskId,
            TaskName = request.TaskName,
            MaxAttempts = request.MaxAttempts,
            Delay = request.Delay,
            RetryDelay = request.RetryDelay,
            OriginalCorrelationId = _correlationContextAccessor.GetOrCreateCorrelationId()
        });

        if (!registered)
            return BadRequest($"Task '{request.TaskId}' has already been registered.");

        var state = await grain.GetState();

        return CreatedAtAction(nameof(GetState), new { taskId = request.TaskId }, state);
    }

    [HttpGet("{taskId}")]
    [ProducesResponseType(typeof(ApiMessageTaskState), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiMessageTaskState>> GetState(
        string taskId,
        CancellationToken cancellationToken)
    {
        var grain = _clusterClient.GetGrain<IApiMessageTaskGrain>(taskId);
        var state = await grain.GetState();
        return Ok(state);
    }

    [HttpPost("{taskId}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cancel(
        string taskId,
        CancellationToken cancellationToken)
    {
        var grain = _clusterClient.GetGrain<IApiMessageTaskGrain>(taskId);
        await grain.CancelAsync();
        return NoContent();
    }
}