using AppTurismoIndustrial.Api.Infrastructure.Persistence;
using AppTurismoIndustrial.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace AppTurismoIndustrial.Api.Tests.Services;

public class ImportCandidateServiceTests
{
    [Fact]
    public async Task PromoteToEmpresa_DeveCriarEmpresaEAtualizarCandidate()
    {
        var ctx = CreateContext();
        var candidate = SeedCandidate(ctx);

        var empresaCriada = new DTORespostaEmpresa { Id = Guid.NewGuid(), NomeFantasia = "Empresa Promovida" };
        var empresaService = MockEmpresaService(empresaCriada);
        var service = CreateService(ctx, empresaService.Object);

        var dto = new DTOEmpresaCriar { Cnpj = "12345678000195", NomeFantasia = "X", RazaoSocial = "X" };
        var result = await service.PromoteToEmpresaAsync(candidate.Id, dto);

        Assert.Equal(empresaCriada.Id, result.Id);
        empresaService.Verify(s => s.CriarAsync(dto, It.IsAny<CancellationToken>()), Times.Once);

        var atualizado = await ctx.GoogleMapsImportCandidates.AsNoTracking().FirstAsync(c => c.Id == candidate.Id);
        Assert.Equal(CandidatePromotionStatus.Aprovado, atualizado.EmpresaStatus);
        Assert.Equal(empresaCriada.Id, atualizado.EmpresaId);
        Assert.NotNull(atualizado.EmpresaDecididoEm);
        // Outros destinos continuam pendentes (decisao independente)
        Assert.Equal(CandidatePromotionStatus.Pendente, atualizado.PontoStatus);
        Assert.Equal(CandidatePromotionStatus.Pendente, atualizado.TelefoneStatus);
    }

    [Fact]
    public async Task PromoteToEmpresa_QuandoJaDecidido_LancaConflictException()
    {
        var ctx = CreateContext();
        var candidate = SeedCandidate(ctx, empresaStatus: CandidatePromotionStatus.Rejeitado);
        var service = CreateService(ctx, MockEmpresaService().Object);

        var dto = new DTOEmpresaCriar { Cnpj = "12345678000195" };
        await Assert.ThrowsAsync<ConflictException>(() =>
            service.PromoteToEmpresaAsync(candidate.Id, dto));
    }

    [Fact]
    public async Task PromoteToEmpresa_QuandoCandidateNaoExiste_LancaNotFound()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx, MockEmpresaService().Object);

        var dto = new DTOEmpresaCriar { Cnpj = "12345678000195" };
        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.PromoteToEmpresaAsync(Guid.NewGuid(), dto));
    }

    [Fact]
    public async Task MesmoCandidate_PodeSerPromovidoPraEmpresaEPontoEmSequencia()
    {
        var ctx = CreateContext();
        var candidate = SeedCandidate(ctx);

        var empresaCriada = new DTORespostaEmpresa { Id = Guid.NewGuid() };
        var pontoCriado = new DTOPontoInstitucional { Id = Guid.NewGuid() };
        var empresaService = MockEmpresaService(empresaCriada);
        var pontoService = MockPontoService(pontoCriado);
        var service = CreateService(ctx, empresaService.Object, pontoService.Object);

        await service.PromoteToEmpresaAsync(candidate.Id, new DTOEmpresaCriar { Cnpj = "12345678000195" });
        await service.PromoteToPontoAsync(candidate.Id, new DTOPontoInstitucionalCriar { Nome = "X" });

        var atualizado = await ctx.GoogleMapsImportCandidates.AsNoTracking().FirstAsync(c => c.Id == candidate.Id);
        Assert.Equal(CandidatePromotionStatus.Aprovado, atualizado.EmpresaStatus);
        Assert.Equal(empresaCriada.Id, atualizado.EmpresaId);
        Assert.Equal(CandidatePromotionStatus.Aprovado, atualizado.PontoStatus);
        Assert.Equal(pontoCriado.Id, atualizado.PontoInstitucionalId);
        // Telefone ainda pendente — decisao independente.
        Assert.Equal(CandidatePromotionStatus.Pendente, atualizado.TelefoneStatus);
    }

    [Fact]
    public async Task Reject_MarcaDestinoComoRejeitadoSemTocarOutros()
    {
        var ctx = CreateContext();
        var candidate = SeedCandidate(ctx);
        var service = CreateService(ctx);

        await service.RejectAsync(candidate.Id, CandidateDestino.Ponto);

        var atualizado = await ctx.GoogleMapsImportCandidates.AsNoTracking().FirstAsync(c => c.Id == candidate.Id);
        Assert.Equal(CandidatePromotionStatus.Rejeitado, atualizado.PontoStatus);
        Assert.NotNull(atualizado.PontoDecididoEm);
        // Empresa e Telefone seguem pendentes — rejeicao por destino e granular.
        Assert.Equal(CandidatePromotionStatus.Pendente, atualizado.EmpresaStatus);
        Assert.Equal(CandidatePromotionStatus.Pendente, atualizado.TelefoneStatus);
    }

    [Fact]
    public async Task Reject_QuandoJaDecidido_LancaConflictException()
    {
        var ctx = CreateContext();
        var candidate = SeedCandidate(ctx, telefoneStatus: CandidatePromotionStatus.Aprovado);
        var service = CreateService(ctx);

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.RejectAsync(candidate.Id, CandidateDestino.Telefone));
    }

    [Fact]
    public async Task ListAsync_SemFiltro_RetornaTodosOrdenadosPorMaisRecente()
    {
        var ctx = CreateContext();
        SeedCandidate(ctx, criadoEm: DateTime.UtcNow.AddDays(-2));
        SeedCandidate(ctx, criadoEm: DateTime.UtcNow);
        SeedCandidate(ctx, criadoEm: DateTime.UtcNow.AddDays(-1));
        var service = CreateService(ctx);

        var lista = await service.ListAsync(null);

        Assert.Equal(3, lista.Count);
        Assert.True(lista[0].CriadoEm >= lista[1].CriadoEm && lista[1].CriadoEm >= lista[2].CriadoEm);
    }

    [Fact]
    public async Task ListAsync_FiltroAprovado_RetornaApenasComAlgumDestinoAprovado()
    {
        var ctx = CreateContext();
        SeedCandidate(ctx);  // todos pendentes
        SeedCandidate(ctx, empresaStatus: CandidatePromotionStatus.Aprovado);
        SeedCandidate(ctx, pontoStatus: CandidatePromotionStatus.Aprovado);
        SeedCandidate(ctx, telefoneStatus: CandidatePromotionStatus.Rejeitado);  // nada aprovado
        var service = CreateService(ctx);

        var lista = await service.ListAsync(CandidatePromotionStatus.Aprovado);

        Assert.Equal(2, lista.Count);
    }

    [Fact]
    public async Task GetById_QuandoNaoExiste_RetornaNull()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var result = await service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetById_DeserializaTypesJsonComoArray()
    {
        var ctx = CreateContext();
        var candidate = SeedCandidate(ctx, typesJson: "[\"store\",\"restaurant\"]");
        var service = CreateService(ctx);

        var result = await service.GetByIdAsync(candidate.Id);

        Assert.NotNull(result);
        Assert.Equal(2, result!.Types.Count);
        Assert.Contains("store", result.Types);
        Assert.Contains("restaurant", result.Types);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static GoogleMapsImportCandidate SeedCandidate(
        AppDbContext ctx,
        CandidatePromotionStatus empresaStatus = CandidatePromotionStatus.Pendente,
        CandidatePromotionStatus pontoStatus = CandidatePromotionStatus.Pendente,
        CandidatePromotionStatus telefoneStatus = CandidatePromotionStatus.Pendente,
        DateTime? criadoEm = null,
        string typesJson = "[]")
    {
        var log = new GoogleMapsImportLog
        {
            Id = Guid.NewGuid(),
            Cep = "17000000",
            RaioMetros = 800,
            Tipo = "loja",
            LatitudeOrigem = -22.5m,
            LongitudeOrigem = -49.0m,
        };
        ctx.GoogleMapsImportLogs.Add(log);

        var candidate = new GoogleMapsImportCandidate
        {
            Id = Guid.NewGuid(),
            GoogleMapsImportLogId = log.Id,
            GooglePlaceId = $"PLACE-{Guid.NewGuid():N}",
            Nome = "Lugar Teste",
            Latitude = -22.5m,
            Longitude = -49.0m,
            TypesJson = typesJson,
            CriadoEm = criadoEm ?? DateTime.UtcNow,
            EmpresaStatus = empresaStatus,
            PontoStatus = pontoStatus,
            TelefoneStatus = telefoneStatus,
        };
        ctx.GoogleMapsImportCandidates.Add(candidate);
        ctx.SaveChanges();
        return candidate;
    }

    private static Mock<IEmpresaService> MockEmpresaService(DTORespostaEmpresa? empresaRetorno = null)
    {
        var mock = new Mock<IEmpresaService>();
        mock.Setup(s => s.CriarAsync(It.IsAny<DTOEmpresaCriar>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(empresaRetorno ?? new DTORespostaEmpresa { Id = Guid.NewGuid() });
        return mock;
    }

    private static Mock<IPontoInstitucionalService> MockPontoService(DTOPontoInstitucional? pontoRetorno = null)
    {
        var mock = new Mock<IPontoInstitucionalService>();
        mock.Setup(s => s.CriarAsync(It.IsAny<DTOPontoInstitucionalCriar>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pontoRetorno ?? new DTOPontoInstitucional { Id = Guid.NewGuid() });
        return mock;
    }

    private static Mock<ITelefoneUtilService> MockTelefoneService(DTOTelefoneUtil? telefoneRetorno = null)
    {
        var mock = new Mock<ITelefoneUtilService>();
        mock.Setup(s => s.CriarAsync(It.IsAny<DTOTelefoneUtilCriar>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(telefoneRetorno ?? new DTOTelefoneUtil { Id = Guid.NewGuid() });
        return mock;
    }

    private static ImportCandidateService CreateService(
        AppDbContext ctx,
        IEmpresaService? empresaService = null,
        IPontoInstitucionalService? pontoService = null,
        ITelefoneUtilService? telefoneService = null)
    {
        return new ImportCandidateService(
            ctx,
            empresaService ?? MockEmpresaService().Object,
            pontoService ?? MockPontoService().Object,
            telefoneService ?? MockTelefoneService().Object,
            NullLogger<ImportCandidateService>.Instance);
    }
}
