using System.Text.Json;
using AppTurismoIndustrial.Api.Infrastructure.Persistence;
using AppTurismoIndustrial.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace AppTurismoIndustrial.Api.Features.Empresas.GoogleMapsImport;

public class ImportCandidateService : IImportCandidateService
{
    private readonly AppDbContext _db;
    private readonly IEmpresaService _empresaService;
    private readonly IPontoInstitucionalService _pontoService;
    private readonly ITelefoneUtilService _telefoneService;
    private readonly ILogger<ImportCandidateService> _logger;

    public ImportCandidateService(
        AppDbContext db,
        IEmpresaService empresaService,
        IPontoInstitucionalService pontoService,
        ITelefoneUtilService telefoneService,
        ILogger<ImportCandidateService> logger)
    {
        _db = db;
        _empresaService = empresaService;
        _pontoService = pontoService;
        _telefoneService = telefoneService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DTOImportCandidateResponse>> ListAsync(
        CandidatePromotionStatus? filtroStatusQualquerDestino,
        CancellationToken cancellationToken = default)
    {
        var query = _db.GoogleMapsImportCandidates.AsNoTracking();

        if (filtroStatusQualquerDestino is { } status)
        {
            // Casamento por qualquer destino: util pra UI "mostrar pendentes" (algum
            // destino ainda pendente) ou "mostrar rejeitados completos" (todos os 3
            // destinos rejeitados — pode ser preciso filtro do client-side em vez).
            query = query.Where(c =>
                c.EmpresaStatus == status ||
                c.PontoStatus == status ||
                c.TelefoneStatus == status);
        }

        var candidates = await query
            .OrderByDescending(c => c.CriadoEm)
            .ToListAsync(cancellationToken);

        return candidates.Select(MapToDto).ToList();
    }

    public async Task<DTOImportCandidateResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var candidate = await _db.GoogleMapsImportCandidates
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        return candidate is null ? null : MapToDto(candidate);
    }

    public async Task<DTORespostaEmpresa> PromoteToEmpresaAsync(
        Guid candidateId, DTOEmpresaCriar dto, CancellationToken cancellationToken = default)
    {
        var candidate = await LoadCandidateAsync(candidateId, cancellationToken);
        EnsureNotDecided(candidate.EmpresaStatus, CandidateDestino.Empresa);

        var empresa = await _empresaService.CriarAsync(dto, cancellationToken);

        candidate.EmpresaStatus = CandidatePromotionStatus.Aprovado;
        candidate.EmpresaId = empresa.Id;
        candidate.EmpresaDecididoEm = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Candidate {CandidateId} promovido a Empresa {EmpresaId}", candidateId, empresa.Id);
        return empresa;
    }

    public async Task<DTOPontoInstitucional> PromoteToPontoAsync(
        Guid candidateId, DTOPontoInstitucionalCriar dto, CancellationToken cancellationToken = default)
    {
        var candidate = await LoadCandidateAsync(candidateId, cancellationToken);
        EnsureNotDecided(candidate.PontoStatus, CandidateDestino.Ponto);

        var ponto = await _pontoService.CriarAsync(dto, cancellationToken);

        candidate.PontoStatus = CandidatePromotionStatus.Aprovado;
        candidate.PontoInstitucionalId = ponto.Id;
        candidate.PontoDecididoEm = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Candidate {CandidateId} promovido a PontoInstitucional {PontoId}", candidateId, ponto.Id);
        return ponto;
    }

    public async Task<DTOTelefoneUtil> PromoteToTelefoneAsync(
        Guid candidateId, DTOTelefoneUtilCriar dto, CancellationToken cancellationToken = default)
    {
        var candidate = await LoadCandidateAsync(candidateId, cancellationToken);
        EnsureNotDecided(candidate.TelefoneStatus, CandidateDestino.Telefone);

        var telefone = await _telefoneService.CriarAsync(dto, cancellationToken);

        candidate.TelefoneStatus = CandidatePromotionStatus.Aprovado;
        candidate.TelefoneUtilId = telefone.Id;
        candidate.TelefoneDecididoEm = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Candidate {CandidateId} promovido a TelefoneUtil {TelefoneId}", candidateId, telefone.Id);
        return telefone;
    }

    public async Task RejectAsync(Guid candidateId, CandidateDestino destino, CancellationToken cancellationToken = default)
    {
        var candidate = await LoadCandidateAsync(candidateId, cancellationToken);

        switch (destino)
        {
            case CandidateDestino.Empresa:
                EnsureNotDecided(candidate.EmpresaStatus, destino);
                candidate.EmpresaStatus = CandidatePromotionStatus.Rejeitado;
                candidate.EmpresaDecididoEm = DateTime.UtcNow;
                break;
            case CandidateDestino.Ponto:
                EnsureNotDecided(candidate.PontoStatus, destino);
                candidate.PontoStatus = CandidatePromotionStatus.Rejeitado;
                candidate.PontoDecididoEm = DateTime.UtcNow;
                break;
            case CandidateDestino.Telefone:
                EnsureNotDecided(candidate.TelefoneStatus, destino);
                candidate.TelefoneStatus = CandidatePromotionStatus.Rejeitado;
                candidate.TelefoneDecididoEm = DateTime.UtcNow;
                break;
            default:
                throw new ValidationException($"Destino invalido: {destino}");
        }

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            "Candidate {CandidateId} rejeitado pro destino {Destino}", candidateId, destino);
    }

    private async Task<GoogleMapsImportCandidate> LoadCandidateAsync(Guid id, CancellationToken ct)
    {
        var c = await _db.GoogleMapsImportCandidates.FirstOrDefaultAsync(x => x.Id == id, ct);
        return c ?? throw new NotFoundException($"Candidate {id} nao encontrado.");
    }

    private static void EnsureNotDecided(CandidatePromotionStatus current, CandidateDestino destino)
    {
        if (current != CandidatePromotionStatus.Pendente)
        {
            throw new ConflictException(
                $"Candidate ja tem decisao registrada pro destino {destino}: {current}. " +
                "Decisoes sao terminais — pra reverter, exclua a entidade-fim criada.");
        }
    }

    private static DTOImportCandidateResponse MapToDto(GoogleMapsImportCandidate c)
    {
        IReadOnlyList<string> types;
        try
        {
            types = JsonSerializer.Deserialize<List<string>>(c.TypesJson) ?? new List<string>();
        }
        catch
        {
            types = Array.Empty<string>();
        }

        return new DTOImportCandidateResponse
        {
            Id = c.Id,
            GooglePlaceId = c.GooglePlaceId,
            Nome = c.Nome,
            FormattedAddress = c.FormattedAddress,
            Latitude = c.Latitude,
            Longitude = c.Longitude,
            Telefone = c.Telefone,
            Types = types,
            CepOrigem = c.CepOrigem,
            CriadoEm = c.CriadoEm,
            EmpresaStatus = c.EmpresaStatus,
            EmpresaId = c.EmpresaId,
            EmpresaDecididoEm = c.EmpresaDecididoEm,
            PontoStatus = c.PontoStatus,
            PontoInstitucionalId = c.PontoInstitucionalId,
            PontoDecididoEm = c.PontoDecididoEm,
            TelefoneStatus = c.TelefoneStatus,
            TelefoneUtilId = c.TelefoneUtilId,
            TelefoneDecididoEm = c.TelefoneDecididoEm,
        };
    }
}
