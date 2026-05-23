
using Microsoft.AspNetCore.Mvc;

namespace AppTurismoIndustrial.Api.Controllers;

[ApiController]
[Route("api/empresas")]
public class EmpresasController : ControllerBase
{
    private readonly IEmpresaService _empresaService;
    private readonly IEmpresaNeighborhoodService _empresaNeighborhoodService;
    private readonly IEmpresaMapService _empresaMapService;
    private readonly IEmpresaFilterService _empresaFilterService;
    private readonly IGeocodingService _geocodingService;

    public EmpresasController(
        IEmpresaService empresaService,
        IEmpresaNeighborhoodService empresaNeighborhoodService,
        IEmpresaMapService empresaMapService,
        IEmpresaFilterService empresaFilterService,
        IGeocodingService geocodingService)
    {
        _empresaService = empresaService;
        _empresaNeighborhoodService = empresaNeighborhoodService;
        _empresaMapService = empresaMapService;
        _empresaFilterService = empresaFilterService;
        _geocodingService = geocodingService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(DTORespostaEmpresa), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DTORespostaEmpresa>> Create([FromBody] DTOEmpresaCriar dto)
    {
        var (empresa, cnpjDuplicado) = await _empresaService.CriarAsync(dto);

        if (cnpjDuplicado)
        {
            return Conflict(new { message = "Ja existe uma empresa cadastrada com este CNPJ." });
        }

        return CreatedAtAction(nameof(GetById), new { id = empresa!.Id }, empresa);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<EmpresaMapDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<EmpresaMapDTO>>> GetAll(CancellationToken cancellationToken)
    {
        var empresas = await _empresaMapService.ListarParaMapaAsync(cancellationToken);

        return Ok(empresas.ToList());
    }

    [HttpGet("filter")]
    [ProducesResponseType(typeof(List<EmpresaFilterDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<EmpresaFilterDTO>>> Filter(
        [FromQuery] EmpresaFilterParams filtros,
        CancellationToken cancellationToken)
    {
        if (filtros.MinFuncionarios.HasValue
            && filtros.MaxFuncionarios.HasValue
            && filtros.MinFuncionarios > filtros.MaxFuncionarios)
        {
            return BadRequest(new
            {
                message = "minFuncionarios nao pode ser maior que maxFuncionarios."
            });
        }

        var empresasFiltradas = await _empresaFilterService.FiltrarAsync(filtros, cancellationToken);
        return Ok(empresasFiltradas.ToList());
    }

    [HttpGet("{id:guid}/neighbors")]
    [ProducesResponseType(typeof(DTOEmpresaVizinhancaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DTOEmpresaVizinhancaResponse>> GetNeighbors(
        Guid id,
        [FromQuery] int radius = 5000,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        if (radius <= 0)
        {
            return BadRequest(new { message = "radius deve ser maior que zero." });
        }

        if (limit <= 0)
        {
            return BadRequest(new { message = "limit deve ser maior que zero." });
        }

        var vizinhanca = await _empresaNeighborhoodService.ObterVizinhancaAsync(id, radius, limit, cancellationToken);

        if (vizinhanca is null)
        {
            return NotFound();
        }

        return Ok(vizinhanca);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DTORespostaEmpresa), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DTORespostaEmpresa>> GetById(Guid id)
    {
        var empresa = await _empresaService.ObterPorIdAsync(id);

        if (empresa is null)
        {
            return NotFound();
        }

        return Ok(empresa);
    }

    [HttpPost("geocode")]
    [ProducesResponseType(typeof(DTOGeocodeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DTOGeocodeResponse>> Geocode(
        [FromBody] DTOGeocodeRequest dto,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Endereco))
        {
            return BadRequest(new { message = "Endereco e obrigatorio para geocodificacao." });
        }

        var result = await _geocodingService.GeocodeAsync(
            dto.Endereco,
            dto.Municipio,
            dto.Estado,
            cancellationToken);

        if (result is null)
        {
            return NotFound(new { message = "Nao foi possivel geocodificar o endereco informado." });
        }

        return Ok(new DTOGeocodeResponse
        {
            Latitude = result.Latitude,
            Longitude = result.Longitude,
            Accuracy = result.Accuracy,
            Provider = result.Provider,
        });
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] DTOEmpresaAtualizar dto)
    {
        var (atualizado, naoEncontrada, cnpjDuplicado) = await _empresaService.AtualizarAsync(id, dto);

        if (naoEncontrada)
        {
            return NotFound();
        }

        if (cnpjDuplicado)
        {
            return Conflict(new { message = "Ja existe uma empresa cadastrada com este CNPJ." });
        }

        if (!atualizado)
        {
            return BadRequest();
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var removida = await _empresaService.RemoverAsync(id);

        if (!removida)
        {
            return NotFound();
        }

        return NoContent();
    }
}
