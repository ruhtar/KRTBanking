using KRTBank.Application.DTOs;
using KRTBank.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KRTBank.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _service;

    public AccountsController(IAccountService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateAccountDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);

        if (!result.IsSuccess)
        {
            return Problem(
                title: "Error creating account",
                detail: result.Message,
                statusCode: result.Code
            );
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data!.Id },
            result.Data
        );
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            return Problem(
                title: "Error retrieving account",
                detail: result.Message,
                statusCode: result.Code
            );
        }

        return Ok(result.Data);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateAccountDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, dto, cancellationToken);

        if (!result.IsSuccess)
        {
            return Problem(
                title: "Error updating account",
                detail: result.Message,
                statusCode: result.Code
            );
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            return Problem(
                title: "Error deleting account",
                detail: result.Message,
                statusCode: result.Code
            );
        }

        return NoContent();
    }
}