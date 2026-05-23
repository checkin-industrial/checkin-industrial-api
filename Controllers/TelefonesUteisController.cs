using AppTurismoIndustrial.Api.Application.DTOs;
using AppTurismoIndustrial.Api.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppTurismoIndustrial.Api.Controllers;

[ApiController]
[Route("api/telefones-uteis")]
public class TelefonesUteisController : ControllerBase
{
    private readonly ITelefoneUtilService _telefoneUtilService;

    public TelefonesUteisController(ITelefoneUtilService telefoneUtilService)
    {
        _telefoneUtilService = telefoneUtilService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<DTOTelefoneUtil>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DTOTelefoneUtil>>> Listar(
        [FromQuery] DTOTelefoneUtilFiltroParams filtros,
        CancellationToken cancellationToken)
    {
        var telefones = await _telefoneUtilService.ListarAsync(filtros, cancellationToken);
        return Ok(telefones.ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DTOTelefoneUtil), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DTOTelefoneUtil>> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var telefoneUtil = await _telefoneUtilService.ObterPorIdAsync(id, cancellationToken);

        if (telefoneUtil is null)
        {
            return NotFound();
        }

        return Ok(telefoneUtil);
    }

    [HttpPost]
    [ProducesResponseType(typeof(DTOTelefoneUtil), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DTOTelefoneUtil>> Criar(
        [FromBody] DTOTelefoneUtilCriar dto,
        CancellationToken cancellationToken)
    {
        var telefoneCriado = await _telefoneUtilService.CriarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = telefoneCriado.Id }, telefoneCriado);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(
        Guid id,
        [FromBody] DTOTelefoneUtilAtualizar dto,
        CancellationToken cancellationToken)
    {
        var atualizado = await _telefoneUtilService.AtualizarAsync(id, dto, cancellationToken);

        if (!atualizado)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(Guid id, CancellationToken cancellationToken)
    {
        var removido = await _telefoneUtilService.RemoverAsync(id, cancellationToken);

        if (!removido)
        {
            return NotFound();
        }

        return NoContent();
    }
}
