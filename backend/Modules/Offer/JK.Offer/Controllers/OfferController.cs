using JK.Offer.Contracts;
using JK.Offer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JK.Offer.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
public class OfferController : ControllerBase
{
    private readonly IOfferService _service;

    public OfferController(IOfferService service)
    {
        _service = service;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OfferDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OfferDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _service.GetByIdAsync(id, cancellationToken);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<OfferDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<OfferDto>>> List(
        [FromQuery] ListOffersRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.ListAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(OfferDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OfferDto>> Create(
        [FromBody] CreateOfferRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(OfferDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OfferDto>> Update(
        Guid id,
        [FromBody] UpdateOfferRequest request,
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

}
