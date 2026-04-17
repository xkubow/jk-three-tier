using JK.Messaging.Contracts;
using JK.Messaging.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JK.Messaging.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
public class MessagingController : ControllerBase
{
    private readonly IMessagingService _service;

    public MessagingController(IMessagingService service)
    {
        _service = service;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MessagingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MessagingDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _service.GetByIdAsync(id, cancellationToken);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<MessagingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<MessagingDto>>> List(
        [FromQuery] ListMessagingRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.ListAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(MessagingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MessagingDto>> Create(
        [FromBody] CreateMessagingRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(MessagingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MessagingDto>> Update(
        Guid id,
        [FromBody] UpdateMessagingRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _service.UpdateAsync(id, request, cancellationToken);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [HttpPost("orleans/echo")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<ActionResult<string>> EchoViaOrleans(
        [FromBody] EchoViaOrleansRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.EchoViaOrleansAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("api-message/register")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<bool>> RegisterApiMessage(
        [FromBody] RegisterApiMessageRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.RegisterApiMessageAsync(request, cancellationToken);
        if (!result) return BadRequest("Invalid cron expression");
        return Ok(result);
    }

    [HttpPost("api-message/send")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SendApiMessage(
        [FromBody] SendApiMessageRequest request,
        CancellationToken cancellationToken)
    {
        await _service.SendApiMessageAsync(request, cancellationToken);
        return Ok();
    }
}
