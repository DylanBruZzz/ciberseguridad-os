using System.Text.Json;
using Aprendizaje.Aplicacion.Resource.Recursos.CrearRecurso;
using Aprendizaje.Aplicacion.Resource.Recursos.VincularRecursoATema;
using Aprendizaje.Aplicacion.Roadmap.Certificaciones.VincularCertificacionATema;
using Aprendizaje.Aplicacion.Roadmap.Temas.DefinirCriteriosRelevantes;
using Aprendizaje.Aplicacion.Roadmap.Temas.VincularHerramientaATema;
using Aprendizaje.Dominio.Resource;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Infraestructura.Persistencia;
using Aprendizaje.Infraestructura.Persistencia.Interceptores;
using Aprendizaje.Infraestructura.Persistencia.Repositorios;
using Microsoft.EntityFrameworkCore;

var cancellationToken = CancellationToken.None;

try
{
    var argumentos = Argumentos.Parsear(args);

    if (!argumentos.EsValido)
    {
        Console.Error.WriteLine(argumentos.Error);
        ImprimirUso();
        return 2;
    }

    var repoRoot = ResolverRaizRepositorio();
    var connectionString = argumentos.ConnectionString ?? ResolverConnectionString(repoRoot);
    var opciones = new DbContextOptionsBuilder<AprendizajeDbContext>()
        .UseSqlServer(connectionString)
        .AddInterceptors(new AuditoriaInterceptor())
        .Options;

    await using var contexto = new AprendizajeDbContext(opciones);
    var servicio = new ContentPopulationService(contexto);
    var plan = await servicio.CrearPlanAsync(cancellationToken);

    if (argumentos.Mode == "inventory")
    {
        ImprimirInventario(plan);
        return 0;
    }

    if (argumentos.Mode == "plan")
    {
        ImprimirPlan(plan);
        return 0;
    }

    if (!argumentos.ConfirmPersonalWrite)
        throw new InvalidOperationException("Para aplicar cambios en AprendizajePersonalDb debes pasar --confirm-personal-write.");

    var preBackup = await servicio.CrearBackupVerificadoAsync("PreContentPopulationV1", cancellationToken);
    Console.WriteLine($"PreBackup: {preBackup.Path}");
    Console.WriteLine($"PreBackupVerifyOnly: {preBackup.VerifyOnly}");

    var resultado = await servicio.AplicarAsync(plan, cancellationToken);
    ImprimirResultadoAplicacion(resultado);

    var segundoPlan = await servicio.CrearPlanAsync(cancellationToken);
    Console.WriteLine($"IdempotencyPendingDelta: {segundoPlan.TotalDelta}");

    if (segundoPlan.TotalDelta != 0)
        throw new InvalidOperationException("La segunda ejecucion no seria idempotente; se detiene antes del post-backup.");

    var postBackup = await servicio.CrearBackupVerificadoAsync("PostContentPopulationV1", cancellationToken);
    Console.WriteLine($"PostBackup: {postBackup.Path}");
    Console.WriteLine($"PostBackupVerifyOnly: {postBackup.VerifyOnly}");

    return 0;
}
catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or DbUpdateException or IOException or JsonException)
{
    Console.Error.WriteLine("CONTENT POPULATION V1 FALLIDA");
    Console.Error.WriteLine(ex.Message);
    return 1;
}

static void ImprimirUso()
{
    Console.Error.WriteLine("Uso:");
    Console.Error.WriteLine("  dotnet run --project tools/Aprendizaje.ContentPopulation -- inventory");
    Console.Error.WriteLine("  dotnet run --project tools/Aprendizaje.ContentPopulation -- plan");
    Console.Error.WriteLine("  dotnet run --project tools/Aprendizaje.ContentPopulation -- apply --confirm-personal-write");
}

static string ResolverRaizRepositorio()
{
    var directorio = new DirectoryInfo(Directory.GetCurrentDirectory());

    while (directorio is not null)
    {
        if (File.Exists(Path.Combine(directorio.FullName, "Aprendizaje.slnx")))
            return directorio.FullName;

        directorio = directorio.Parent;
    }

    throw new InvalidOperationException("No se pudo resolver la raiz del repositorio.");
}

static string ResolverConnectionString(string repoRoot)
{
    var rutaAppSettings = Path.Combine(repoRoot, "src", "Aprendizaje.Api", "appsettings.Personal.json");
    using var documento = JsonDocument.Parse(File.ReadAllText(rutaAppSettings));

    if (documento.RootElement
        .GetProperty("ConnectionStrings")
        .TryGetProperty("AprendizajeDb", out var connectionString)
        && !string.IsNullOrWhiteSpace(connectionString.GetString()))
    {
        return connectionString.GetString()!;
    }

    throw new InvalidOperationException("No se encontro ConnectionStrings:AprendizajeDb en appsettings.Personal.json.");
}

static void ImprimirInventario(PopulationPlan plan)
{
    Console.WriteLine("INVENTORY");
    Console.WriteLine($"Usuario: {plan.UsuarioId}");
    foreach (var item in plan.GlobalCounts)
        Console.WriteLine($"{item.Nombre}: {item.Valor}");

    foreach (var fase in plan.Fases)
    {
        Console.WriteLine($"Fase {fase.Orden}: temas={fase.TemasExistentes}, criterios={fase.CriteriosActuales}, temaHerramienta={fase.TemaHerramientaActuales}, certificacionTema={fase.CertificacionTemaActuales}, recursoTema={fase.RecursoTemaActuales}");
    }
}

static void ImprimirPlan(PopulationPlan plan)
{
    ImprimirInventario(plan);
    Console.WriteLine("PLAN");
    foreach (var fase in plan.Fases)
    {
        Console.WriteLine($"Fase {fase.Orden}: criterios+={fase.CriteriosPorAgregar}, temaHerramienta+={fase.TemaHerramientaPorAgregar}, certificacionTema+={fase.CertificacionTemaPorAgregar}, recursos+={fase.RecursosPorCrear}, recursoTema+={fase.RecursoTemaPorAgregar}, subtemas+=0");
    }

    Console.WriteLine($"TotalDelta: {plan.TotalDelta}");
}

static void ImprimirResultadoAplicacion(PopulationApplyResult resultado)
{
    Console.WriteLine("APPLIED");
    foreach (var fase in resultado.Fases)
    {
        Console.WriteLine($"Fase {fase.Orden}: criterios+={fase.CriteriosAgregados}, temaHerramienta+={fase.TemaHerramientaAgregados}, certificacionTema+={fase.CertificacionTemaAgregados}, recursos+={fase.RecursosCreados}, recursoTema+={fase.RecursoTemaAgregados}, subtemas+=0");
    }
}

internal sealed record Argumentos(string Mode, bool ConfirmPersonalWrite, string? ConnectionString, string? Error)
{
    public bool EsValido => Error is null;

    public static Argumentos Parsear(string[] args)
    {
        if (args.Length == 0)
            return new Argumentos("", false, null, "Modo requerido.");

        var mode = args[0].Trim().ToLowerInvariant();
        if (mode is not ("inventory" or "plan" or "apply"))
            return new Argumentos(mode, false, null, "Modo invalido.");

        var confirm = false;
        string? connectionString = null;

        for (var i = 1; i < args.Length; i++)
        {
            if (args[i] == "--confirm-personal-write")
            {
                confirm = true;
                continue;
            }

            if (args[i] == "--connection-string" && i + 1 < args.Length)
            {
                connectionString = args[++i];
                continue;
            }

            return new Argumentos(mode, confirm, connectionString, $"Argumento invalido: {args[i]}");
        }

        return new Argumentos(mode, confirm, connectionString, null);
    }
}

internal sealed class ContentPopulationService
{
    private const string DatabaseName = "AprendizajePersonalDb";
    private readonly AprendizajeDbContext _context;
    private readonly IReadOnlyCollection<TopicPopulation> _matrix = PopulationMatrix.Items;

    public ContentPopulationService(AprendizajeDbContext context)
    {
        _context = context;
    }

    public async Task<PopulationPlan> CrearPlanAsync(CancellationToken cancellationToken)
    {
        var usuarioId = await ResolverUsuarioPersonalAsync(cancellationToken);
        var fases = await _context.Fases
            .AsNoTracking()
            .Where(f => f.UsuarioId == usuarioId)
            .OrderBy(f => f.Orden)
            .ToListAsync(cancellationToken);
        var temas = await _context.Temas
            .AsNoTracking()
            .Include(t => t.Criterios)
            .Where(t => t.UsuarioId == usuarioId)
            .ToListAsync(cancellationToken);
        var herramientas = await _context.Herramientas.AsNoTracking().ToListAsync(cancellationToken);
        var certificaciones = await _context.Certificaciones.AsNoTracking().ToListAsync(cancellationToken);
        var recursos = await _context.Recursos.AsNoTracking().Where(r => r.UsuarioId == usuarioId).ToListAsync(cancellationToken);

        ValidarMatrix(fases, temas, herramientas, certificaciones);

        var globalCounts = await ObtenerCountsGlobalesAsync(cancellationToken);
        var fasesPlan = new List<PhasePlan>();

        foreach (var fase in fases.Where(f => f.Orden is >= 1 and <= 4))
        {
            var temasFase = temas.Where(t => t.FaseId == fase.Id).ToArray();
            var items = _matrix.Where(i => i.Phase == fase.Orden).ToArray();
            var criteriosPorAgregar = items
                .Select(item => ObtenerTema(temasFase, item.Topic))
                .Where(t => t.Criterios.Count == 0)
                .Sum(t => _matrix.Single(i => i.Phase == fase.Orden && i.Topic == t.Nombre).Criteria.Count);

            var temaHerramientaPorAgregar = 0;
            var certificacionTemaPorAgregar = 0;
            var recursoTemaPorAgregar = 0;
            var recursosPorCrear = 0;
            var recursosContados = new HashSet<ResourceSpec>();

            foreach (var item in items)
            {
                var tema = ObtenerTema(temasFase, item.Topic);
                foreach (var toolName in item.Tools)
                {
                    var herramientaId = herramientas.Single(h => h.Nombre == toolName).Id;
                    if (!await ExisteTemaHerramientaAsync(tema.Id, herramientaId, cancellationToken))
                        temaHerramientaPorAgregar++;
                }

                foreach (var certName in item.Certifications)
                {
                    var certificacionId = certificaciones.Single(c => c.Nombre == certName).Id;
                    if (!await ExisteCertificacionTemaAsync(certificacionId, tema.Id, cancellationToken))
                        certificacionTemaPorAgregar++;
                }

                foreach (var resource in item.Resources)
                {
                    var existente = BuscarRecurso(recursos, resource);
                    if (existente is null && recursosContados.Add(resource))
                        recursosPorCrear++;

                    if (existente is null || !await ExisteRecursoTemaAsync(existente.Id, tema.Id, cancellationToken))
                        recursoTemaPorAgregar++;
                }
            }

            fasesPlan.Add(new PhasePlan(
                fase.Orden,
                temasFase.Length,
                temasFase.Sum(t => t.Criterios.Count),
                await CountPhaseAsync("roadmap.TemaHerramienta", fase.Id, cancellationToken),
                await CountPhaseAsync("roadmap.CertificacionTema", fase.Id, cancellationToken),
                await CountPhaseAsync("resource.RecursoTema", fase.Id, cancellationToken),
                criteriosPorAgregar,
                temaHerramientaPorAgregar,
                certificacionTemaPorAgregar,
                recursosPorCrear,
                recursoTemaPorAgregar));
        }

        return new PopulationPlan(usuarioId, globalCounts, fasesPlan);
    }

    public async Task<BackupResult> CrearBackupVerificadoAsync(string marker, CancellationToken cancellationToken)
    {
        var backupPath = await ResolverBackupPathAsync(marker, cancellationToken);
        if (File.Exists(backupPath))
            throw new InvalidOperationException($"El backup ya existe y no se sobrescribira: {backupPath}");

        var backupName = $"{DatabaseName}_{marker}";
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"BACKUP DATABASE [AprendizajePersonalDb] TO DISK = {backupPath} WITH CHECKSUM, INIT, NAME = {backupName}",
            cancellationToken);
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"RESTORE VERIFYONLY FROM DISK = {backupPath} WITH CHECKSUM",
            cancellationToken);

        return new BackupResult(backupPath, "PASS");
    }

    public async Task<PopulationApplyResult> AplicarAsync(PopulationPlan plan, CancellationToken cancellationToken)
    {
        var resultadoFases = new List<PhaseApplyResult>();

        foreach (var phase in new[] { 1, 2, 3, 4 })
        {
            var resultado = await AplicarFaseAsync(plan.UsuarioId, phase, cancellationToken);
            resultadoFases.Add(resultado);
        }

        return new PopulationApplyResult(resultadoFases);
    }

    private async Task<PhaseApplyResult> AplicarFaseAsync(Guid usuarioId, int phase, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var temaRepo = new TemaRepository(_context);
        var herramientaRepo = new HerramientaRepository(_context);
        var certificacionRepo = new CertificacionRepository(_context);
        var recursoRepo = new RecursoRepository(_context);

        var definirCriterios = new DefinirCriteriosRelevantesTemaCasoUso(temaRepo, _context);
        var vincularHerramienta = new VincularHerramientaATemaCasoUso(temaRepo, herramientaRepo, _context);
        var vincularCertificacion = new VincularCertificacionATemaCasoUso(certificacionRepo, temaRepo, _context);
        var crearRecurso = new CrearRecursoCasoUso(recursoRepo, _context);
        var vincularRecurso = new VincularRecursoATemaCasoUso(recursoRepo, temaRepo, _context);

        var temas = await _context.Temas
            .Include(t => t.Criterios)
            .Where(t => t.UsuarioId == usuarioId)
            .ToListAsync(cancellationToken);
        var fases = await _context.Fases.Where(f => f.UsuarioId == usuarioId).ToListAsync(cancellationToken);
        var fase = fases.Single(f => f.Orden == phase);
        var temasFase = temas.Where(t => t.FaseId == fase.Id).ToArray();
        var herramientas = await _context.Herramientas.ToListAsync(cancellationToken);
        var certificaciones = await _context.Certificaciones.ToListAsync(cancellationToken);
        var recursos = await _context.Recursos.Where(r => r.UsuarioId == usuarioId).ToListAsync(cancellationToken);

        var criteriosAgregados = 0;
        var temaHerramientaAgregados = 0;
        var certificacionTemaAgregados = 0;
        var recursosCreados = 0;
        var recursoTemaAgregados = 0;

        foreach (var item in _matrix.Where(i => i.Phase == phase))
        {
            var tema = ObtenerTema(temasFase, item.Topic);

            if (tema.Criterios.Count == 0)
            {
                var resultadoCriterios = await definirCriterios.EjecutarAsync(
                    new DefinirCriteriosRelevantesTemaSolicitud(tema.Id, item.Criteria),
                    cancellationToken);

                if (resultadoCriterios.Estado != DefinirCriteriosRelevantesTemaEstado.Actualizado)
                    throw new InvalidOperationException($"No se pudieron definir criterios para {item.Topic}: {resultadoCriterios.Estado}");

                criteriosAgregados += item.Criteria.Count;
            }

            foreach (var toolName in item.Tools)
            {
                var herramienta = herramientas.Single(h => h.Nombre == toolName);
                if (await ExisteTemaHerramientaAsync(tema.Id, herramienta.Id, cancellationToken))
                    continue;

                var resultado = await vincularHerramienta.EjecutarAsync(
                    new VincularHerramientaATemaSolicitud(usuarioId, tema.Id, herramienta.Id),
                    cancellationToken);

                if (resultado.Estado != VincularHerramientaATemaEstado.Actualizado)
                    throw new InvalidOperationException($"No se pudo vincular herramienta {toolName} a {item.Topic}: {resultado.Estado}");

                temaHerramientaAgregados++;
            }

            foreach (var certName in item.Certifications)
            {
                var certificacion = certificaciones.Single(c => c.Nombre == certName);
                if (await ExisteCertificacionTemaAsync(certificacion.Id, tema.Id, cancellationToken))
                    continue;

                var resultado = await vincularCertificacion.EjecutarAsync(
                    new VincularCertificacionATemaSolicitud(certificacion.Id, tema.Id),
                    cancellationToken);

                if (resultado.Estado != VincularCertificacionATemaEstado.Actualizado)
                    throw new InvalidOperationException($"No se pudo vincular certificacion {certName} a {item.Topic}: {resultado.Estado}");

                certificacionTemaAgregados++;
            }

            foreach (var resource in item.Resources)
            {
                var recurso = BuscarRecurso(recursos, resource);

                if (recurso is null)
                {
                    var creado = await crearRecurso.EjecutarAsync(
                        new CrearRecursoSolicitud(usuarioId, resource.Type, resource.Title, resource.Url),
                        cancellationToken);
                    recursosCreados++;
                    recurso = await _context.Recursos.FirstAsync(r => r.Id == creado.Id, cancellationToken);
                    recursos.Add(recurso);
                }

                if (await ExisteRecursoTemaAsync(recurso.Id, tema.Id, cancellationToken))
                    continue;

                var resultado = await vincularRecurso.EjecutarAsync(
                    new VincularRecursoATemaSolicitud(recurso.Id, tema.Id),
                    cancellationToken);

                if (resultado.Estado != VincularRecursoATemaEstado.Actualizado)
                    throw new InvalidOperationException($"No se pudo vincular recurso {resource.Title} a {item.Topic}: {resultado.Estado}");

                recursoTemaAgregados++;
            }
        }

        await transaction.CommitAsync(cancellationToken);

        return new PhaseApplyResult(phase, criteriosAgregados, temaHerramientaAgregados, certificacionTemaAgregados, recursosCreados, recursoTemaAgregados);
    }

    private async Task<Guid> ResolverUsuarioPersonalAsync(CancellationToken cancellationToken)
    {
        var usuarios = await _context.Usuarios.AsNoTracking().ToListAsync(cancellationToken);

        return usuarios.Count switch
        {
            1 => usuarios[0].Id,
            0 => throw new InvalidOperationException("AprendizajePersonalDb no tiene un Usuario visible."),
            _ => throw new InvalidOperationException("AprendizajePersonalDb tiene multiples Usuarios visibles."),
        };
    }

    private void ValidarMatrix(
        IReadOnlyCollection<Fase> fases,
        IReadOnlyCollection<Tema> temas,
        IReadOnlyCollection<Aprendizaje.Dominio.Study.Herramienta> herramientas,
        IReadOnlyCollection<Certificacion> certificaciones)
    {
        var herramientasDisponibles = herramientas.Select(h => h.Nombre).ToHashSet(StringComparer.Ordinal);
        var certificacionesDisponibles = certificaciones.Select(c => c.Nombre).ToHashSet(StringComparer.Ordinal);

        foreach (var item in _matrix)
        {
            if (item.Phase is < 1 or > 4)
                throw new InvalidOperationException($"La matriz contiene una fase fuera de alcance: {item.Phase}.");

            var fase = fases.SingleOrDefault(f => f.Orden == item.Phase)
                ?? throw new InvalidOperationException($"No existe la Fase {item.Phase}.");

            if (!temas.Any(t => t.FaseId == fase.Id && t.Nombre == item.Topic))
                throw new InvalidOperationException($"No existe el Tema '{item.Topic}' en Fase {item.Phase}.");

            foreach (var toolName in item.Tools)
            {
                if (!herramientasDisponibles.Contains(toolName))
                    throw new InvalidOperationException($"La herramienta '{toolName}' no existe en el catalogo actual.");
            }

            foreach (var certName in item.Certifications)
            {
                if (!certificacionesDisponibles.Contains(certName))
                    throw new InvalidOperationException($"La certificacion '{certName}' no existe en el catalogo actual.");
            }
        }
    }

    private async Task<IReadOnlyCollection<NamedCount>> ObtenerCountsGlobalesAsync(CancellationToken cancellationToken)
    {
        var counts = new List<NamedCount>
        {
            new("Fases", await CountSqlAsync("SELECT COUNT(*) AS Value FROM roadmap.Fase", cancellationToken)),
            new("Temas", await CountSqlAsync("SELECT COUNT(*) AS Value FROM roadmap.Tema WHERE FechaEliminacionUtc IS NULL", cancellationToken)),
            new("Herramientas", await CountSqlAsync("SELECT COUNT(*) AS Value FROM study.Herramienta", cancellationToken)),
            new("Certificaciones", await CountSqlAsync("SELECT COUNT(*) AS Value FROM roadmap.Certificacion", cancellationToken)),
            new("RecursosVisibles", await CountSqlAsync("SELECT COUNT(*) AS Value FROM resource.Recurso WHERE FechaEliminacionUtc IS NULL", cancellationToken)),
            new("CriterioTema", await CountSqlAsync("SELECT COUNT(*) AS Value FROM roadmap.CriterioTema", cancellationToken)),
            new("TemaHerramienta", await CountSqlAsync("SELECT COUNT(*) AS Value FROM roadmap.TemaHerramienta", cancellationToken)),
            new("CertificacionTema", await CountSqlAsync("SELECT COUNT(*) AS Value FROM roadmap.CertificacionTema", cancellationToken)),
            new("RecursoTema", await CountSqlAsync("SELECT COUNT(*) AS Value FROM resource.RecursoTema", cancellationToken)),
            new("EvidenceVisible", await CountSqlAsync("SELECT (SELECT COUNT(*) FROM evidence.Proyecto WHERE FechaEliminacionUtc IS NULL)+(SELECT COUNT(*) FROM evidence.Laboratorio WHERE FechaEliminacionUtc IS NULL)+(SELECT COUNT(*) FROM evidence.Writeup WHERE FechaEliminacionUtc IS NULL)+(SELECT COUNT(*) FROM evidence.ArtefactoTecnico WHERE FechaEliminacionUtc IS NULL)+(SELECT COUNT(*) FROM evidence.CertificacionObtenida WHERE FechaEliminacionUtc IS NULL) AS Value", cancellationToken)),
            new("SesionesVisible", await CountSqlAsync("SELECT COUNT(*) AS Value FROM study.SesionEstudio WHERE FechaEliminacionUtc IS NULL", cancellationToken)),
        };

        return counts;
    }

    private async Task<int> CountSqlAsync(string sql, CancellationToken cancellationToken) =>
        await _context.Database.SqlQueryRaw<int>(sql).SingleAsync(cancellationToken);

    private async Task<int> CountPhaseAsync(string table, Guid faseId, CancellationToken cancellationToken)
    {
        var sql = table switch
        {
            "roadmap.TemaHerramienta" => $"SELECT COUNT(*) AS Value FROM roadmap.TemaHerramienta th INNER JOIN roadmap.Tema t ON th.TemaId = t.Id WHERE t.FaseId = '{faseId}'",
            "roadmap.CertificacionTema" => $"SELECT COUNT(*) AS Value FROM roadmap.CertificacionTema ct INNER JOIN roadmap.Tema t ON ct.TemaId = t.Id WHERE t.FaseId = '{faseId}'",
            "resource.RecursoTema" => $"SELECT COUNT(*) AS Value FROM resource.RecursoTema rt INNER JOIN roadmap.Tema t ON rt.TemaId = t.Id WHERE t.FaseId = '{faseId}'",
            _ => throw new ArgumentOutOfRangeException(nameof(table), table, null),
        };

        return await CountSqlAsync(sql, cancellationToken);
    }

    private async Task<bool> ExisteTemaHerramientaAsync(Guid temaId, Guid herramientaId, CancellationToken cancellationToken) =>
        await CountSqlAsync($"SELECT COUNT(*) AS Value FROM roadmap.TemaHerramienta WHERE TemaId = '{temaId}' AND HerramientaId = '{herramientaId}'", cancellationToken) > 0;

    private async Task<bool> ExisteCertificacionTemaAsync(Guid certificacionId, Guid temaId, CancellationToken cancellationToken) =>
        await CountSqlAsync($"SELECT COUNT(*) AS Value FROM roadmap.CertificacionTema WHERE CertificacionId = '{certificacionId}' AND TemaId = '{temaId}'", cancellationToken) > 0;

    private async Task<bool> ExisteRecursoTemaAsync(Guid recursoId, Guid temaId, CancellationToken cancellationToken) =>
        await CountSqlAsync($"SELECT COUNT(*) AS Value FROM resource.RecursoTema WHERE RecursoId = '{recursoId}' AND TemaId = '{temaId}'", cancellationToken) > 0;

    private async Task<string> ResolverBackupPathAsync(string marker, CancellationToken cancellationToken)
    {
        var basePath = await _context.Database
            .SqlQueryRaw<string>("SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS nvarchar(4000)) AS Value")
            .SingleOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(basePath))
            throw new InvalidOperationException("SQL Server no devolvio InstanceDefaultBackupPath.");

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var fileName = $"{DatabaseName}_{marker}_{timestamp}.bak";
        return Path.Combine(basePath, fileName);
    }

    private static Tema ObtenerTema(IReadOnlyCollection<Tema> temas, string nombre) =>
        temas.SingleOrDefault(t => t.Nombre == nombre)
        ?? throw new InvalidOperationException($"No existe el Tema '{nombre}'.");

    private static Recurso? BuscarRecurso(IReadOnlyCollection<Recurso> recursos, ResourceSpec resource) =>
        recursos.FirstOrDefault(r =>
            r.Titulo == resource.Title
            && r.Tipo == resource.Type
            && string.Equals(r.Url ?? string.Empty, resource.Url ?? string.Empty, StringComparison.Ordinal));
}

internal static class PopulationMatrix
{
    private static readonly ResourceSpec CiscoNetAcad = new("Cisco NetAcad: IT Essentials + CCNA", TipoRecurso.Curso, null);
    private static readonly ResourceSpec TryHackMePreSecurity = new("TryHackMe: Pre-Security Path", TipoRecurso.Laboratorio, null);
    private static readonly ResourceSpec Tanenbaum = new("Computer Networking - Tanenbaum", TipoRecurso.Libro, null);
    private static readonly ResourceSpec OverTheWireBandit = new("OverTheWire: Bandit", TipoRecurso.Laboratorio, null);
    private static readonly ResourceSpec MicrosoftWindowsServer = new("Microsoft Learn: Windows Server free modules", TipoRecurso.Curso, null);
    private static readonly ResourceSpec LearnPythonHardWay = new("Learn Python the Hard Way", TipoRecurso.Libro, null);
    private static readonly ResourceSpec TcmPeh = new("TCM Security: Practical Ethical Hacking", TipoRecurso.Curso, null);
    private static readonly ResourceSpec TryHackMeSoc1 = new("TryHackMe: SOC Level 1 Path", TipoRecurso.Laboratorio, null);
    private static readonly ResourceSpec SplunkBots = new("Splunk Free Training + BOTS", TipoRecurso.Curso, null);
    private static readonly ResourceSpec TryHackMeBlue = new("TryHackMe: SOC Level 1 + Blue Team Path", TipoRecurso.Laboratorio, null);
    private static readonly ResourceSpec BlueTeamLabs = new("Blue Team Labs Online", TipoRecurso.Laboratorio, null);
    private static readonly ResourceSpec NsmBook = new("The Practice of Network Security Monitoring", TipoRecurso.Libro, null);
    private static readonly ResourceSpec CiscoCyberOps = new("Cisco NetAcad: CyberOps Associate", TipoRecurso.Curso, null);
    private static readonly ResourceSpec HtbStartingPoint = new("HackTheBox: Starting Point + Labs", TipoRecurso.Laboratorio, null);
    private static readonly ResourceSpec PortSwiggerExisting = new("PortSwigger Web Academy", TipoRecurso.Laboratorio, null);
    private static readonly ResourceSpec PentestingBook = new("Penetration Testing - Georgia Weidman", TipoRecurso.Libro, null);
    private static readonly ResourceSpec TcmAd = new("TCM: AD Attacks & Defense", TipoRecurso.Curso, null);
    private static readonly ResourceSpec VulnHub = new("VulnHub: Máquinas vulnerables", TipoRecurso.Laboratorio, null);

    private static readonly ResourceSpec BashManual = new("GNU Bash Reference Manual", TipoRecurso.Documentacion, "https://www.gnu.org/software/bash/manual/bash.html");
    private static readonly ResourceSpec WiresharkUserGuide = new("Wireshark User's Guide", TipoRecurso.Documentacion, "https://www.wireshark.org/docs/wsug_html_chunked/");
    private static readonly ResourceSpec NmapReference = new("Nmap Reference Guide", TipoRecurso.Documentacion, "https://nmap.org/book/man.html");
    private static readonly ResourceSpec PythonTutorial = new("Python Tutorial", TipoRecurso.Documentacion, "https://docs.python.org/3/tutorial/");
    private static readonly ResourceSpec PowerShellDocs = new("PowerShell Documentation", TipoRecurso.Documentacion, "https://learn.microsoft.com/powershell/");
    private static readonly ResourceSpec ActiveDirectoryDocs = new("Microsoft Learn: Active Directory Domain Services overview", TipoRecurso.Documentacion, "https://learn.microsoft.com/windows-server/identity/ad-ds/get-started/virtual-dc/active-directory-domain-services-overview");
    private static readonly ResourceSpec GroupPolicyDocs = new("Microsoft Learn: Group Policy overview", TipoRecurso.Documentacion, "https://learn.microsoft.com/windows-server/identity/ad-ds/manage/group-policy/group-policy-overview");
    private static readonly ResourceSpec MitreAttack = new("MITRE ATT&CK Knowledge Base", TipoRecurso.Documentacion, "https://attack.mitre.org/");
    private static readonly ResourceSpec WazuhDocs = new("Wazuh Documentation", TipoRecurso.Documentacion, "https://documentation.wazuh.com/current/");
    private static readonly ResourceSpec OwaspTop10 = new("OWASP Top 10", TipoRecurso.Documentacion, "https://owasp.org/projects/top-ten");
    private static readonly ResourceSpec MetasploitDocs = new("Metasploit Documentation", TipoRecurso.Documentacion, "https://docs.metasploit.com/");
    private static readonly ResourceSpec BurpDocs = new("Burp Suite Documentation", TipoRecurso.Documentacion, "https://portswigger.net/burp/documentation");

    public static readonly IReadOnlyCollection<TopicPopulation> Items =
    [
        T(1, "Modelo OSI / TCP-IP", ["Wireshark", "tcpdump"], ["CompTIA A+", "Cisco CyberOps Associate"], C("Teoria", "Explicacion", "Ejercicios"), [CiscoNetAcad, Tanenbaum, WiresharkUserGuide]),
        T(1, "Subnetting / CIDR", [], ["CompTIA A+", "Cisco CyberOps Associate"], C("Teoria", "Practica", "Ejercicios"), [CiscoNetAcad, Tanenbaum]),
        T(1, "DNS, DHCP, HTTP/S", ["Wireshark", "tcpdump", "curl"], ["CompTIA Security+", "Cisco CyberOps Associate"], C("Teoria", "Practica", "Explicacion"), [CiscoNetAcad, Tanenbaum, WiresharkUserGuide]),
        T(1, "Bash scripting", ["Bash", "sed", "awk", "grep"], ["CompTIA A+"], C("Practica", "Ejercicios", "Laboratorio"), [BashManual, OverTheWireBandit]),
        T(1, "Permisos Linux", ["Bash"], ["CompTIA A+", "CompTIA Security+"], C("Teoria", "Practica", "Explicacion"), [BashManual, TryHackMePreSecurity]),
        T(1, "Virtualización", ["VirtualBox", "VMware", "Proxmox"], ["CompTIA A+"], C("Teoria", "Practica", "Laboratorio"), [TryHackMePreSecurity]),
        T(1, "Wireshark basics", ["Wireshark", "tcpdump"], ["Cisco CyberOps Associate"], C("Practica", "Explicacion", "Laboratorio"), [WiresharkUserGuide, CiscoNetAcad]),
        T(1, "SSH, FTP, SMB", ["Wireshark", "Nmap", "curl"], ["CompTIA Security+", "Cisco CyberOps Associate"], C("Teoria", "Practica", "Explicacion"), [NmapReference, TryHackMePreSecurity]),
        T(1, "Routing & Switching", ["Wireshark", "tcpdump"], ["Cisco CyberOps Associate"], C("Teoria", "Explicacion", "Ejercicios"), [CiscoNetAcad, Tanenbaum]),
        T(1, "Firewall básico", ["Nmap", "Wireshark"], ["CompTIA Security+", "Cisco CyberOps Associate"], C("Teoria", "Practica", "Explicacion"), [CiscoNetAcad, NmapReference]),

        T(2, "Active Directory", ["PowerShell"], ["CompTIA Security+"], C("Teoria", "Explicacion", "Laboratorio"), [ActiveDirectoryDocs, MicrosoftWindowsServer, TcmPeh]),
        T(2, "Group Policy (GPO)", ["PowerShell"], ["CompTIA Security+"], C("Teoria", "Practica", "Laboratorio"), [GroupPolicyDocs, MicrosoftWindowsServer]),
        T(2, "PowerShell scripting", ["PowerShell"], ["CompTIA A+", "CompTIA Security+"], C("Practica", "Ejercicios", "Laboratorio"), [PowerShellDocs, MicrosoftWindowsServer]),
        T(2, "Python para seguridad", ["Python 3"], ["eJPT"], C("Practica", "Ejercicios", "Laboratorio"), [PythonTutorial, LearnPythonHardWay, TcmPeh]),
        T(2, "Event Viewer / Syslog", ["PowerShell", "Wazuh", "Elastic Stack"], ["Cisco CyberOps Associate", "CompTIA Security+"], C("Teoria", "Practica", "Explicacion"), [MicrosoftWindowsServer, TryHackMeSoc1, WazuhDocs]),
        T(2, "Hardening SSOO", ["PowerShell", "Bash"], ["CompTIA Security+"], C("Teoria", "Practica", "Explicacion", "Laboratorio"), [MicrosoftWindowsServer, TryHackMeSoc1]),
        T(2, "Cron jobs", ["Bash"], ["CompTIA A+"], C("Practica", "Ejercicios"), [BashManual]),
        T(2, "VMware Workstation", ["VMware", "VirtualBox"], ["CompTIA A+"], C("Practica", "Laboratorio"), [MicrosoftWindowsServer, TryHackMePreSecurity]),
        T(2, "Kali Linux intro", ["Kali Linux", "Nmap"], ["eJPT"], C("Teoria", "Practica", "Laboratorio"), [TcmPeh, NmapReference]),
        T(2, "Gestión de usuarios", ["PowerShell", "Bash"], ["CompTIA A+", "CompTIA Security+"], C("Teoria", "Practica", "Explicacion"), [MicrosoftWindowsServer, BashManual]),

        T(3, "MITRE ATT&CK Framework", ["Wazuh", "Splunk"], ["Cisco CyberOps Associate", "CompTIA Security+"], C("Teoria", "Explicacion", "Ejercicios"), [MitreAttack, TryHackMeBlue, CiscoCyberOps]),
        T(3, "Cyber Kill Chain", [], ["Cisco CyberOps Associate", "CompTIA Security+"], C("Teoria", "Explicacion", "Ejercicios"), [TryHackMeBlue, CiscoCyberOps]),
        T(3, "Splunk / Wazuh", ["Splunk", "Wazuh", "Elastic Stack"], ["Cisco CyberOps Associate"], C("Practica", "Explicacion", "Laboratorio"), [SplunkBots, WazuhDocs, BlueTeamLabs]),
        T(3, "IDS/IPS (Snort/Suricata)", ["Snort", "Suricata", "Zeek"], ["Cisco CyberOps Associate", "CompTIA Security+"], C("Teoria", "Practica", "Laboratorio"), [NsmBook, TryHackMeBlue]),
        T(3, "Threat Intelligence", ["Maltego", "Shodan", "theHarvester"], ["Cisco CyberOps Associate", "CompTIA Security+"], C("Teoria", "Practica", "Explicacion"), [MitreAttack, BlueTeamLabs]),
        T(3, "Criptografía simétrica/asimétrica", [], ["CompTIA Security+"], C("Teoria", "Explicacion", "Ejercicios"), [CiscoCyberOps]),
        T(3, "PKI y certificados TLS", ["Wireshark", "curl"], ["CompTIA Security+"], C("Teoria", "Practica", "Explicacion"), [WiresharkUserGuide, CiscoCyberOps]),
        T(3, "OSINT básico", ["Maltego", "Shodan", "theHarvester"], ["CompTIA Security+"], C("Practica", "Explicacion", "Laboratorio"), [TryHackMeBlue]),
        T(3, "Análisis de logs", ["Splunk", "Wazuh", "Elastic Stack"], ["Cisco CyberOps Associate", "CompTIA Security+"], C("Practica", "Explicacion", "Laboratorio"), [SplunkBots, WazuhDocs, NsmBook]),
        T(3, "Nessus / OpenVAS", ["Nessus", "OpenVAS", "Nuclei"], ["CompTIA Security+", "Cisco CyberOps Associate"], C("Teoria", "Practica", "Laboratorio"), [BlueTeamLabs, TryHackMeBlue]),

        T(4, "Nmap / Nessus avanzado", ["Nmap", "Nessus", "Nuclei"], ["eJPT", "CompTIA PenTest+", "CEH"], C("Practica", "Explicacion", "Laboratorio"), [NmapReference, HtbStartingPoint, TcmPeh]),
        T(4, "Metasploit Framework", ["Metasploit", "Metasploitable 2/3"], ["eJPT", "PNPT", "CEH"], C("Teoria", "Practica", "Laboratorio"), [MetasploitDocs, HtbStartingPoint, VulnHub]),
        T(4, "Burp Suite Pro", ["Burp Suite", "Burp Suite Pro", "OWASP ZAP"], ["eJPT", "CompTIA PenTest+", "PNPT"], C("Practica", "Explicacion", "Laboratorio"), [BurpDocs, PortSwiggerExisting]),
        T(4, "SQLi / XSS / SSRF", ["Burp Suite Pro", "OWASP ZAP", "SQLmap", "DVWA"], ["eJPT", "CompTIA PenTest+", "PNPT"], C("Teoria", "Practica", "Explicacion", "Laboratorio"), [PortSwiggerExisting, OwaspTop10, VulnHub]),
        T(4, "OWASP Top 10", ["Burp Suite", "OWASP ZAP", "DVWA"], ["CompTIA Security+", "eJPT", "CompTIA PenTest+"], C("Teoria", "Explicacion", "Laboratorio"), [OwaspTop10, PortSwiggerExisting]),
        T(4, "Buffer Overflow básico", ["Kali Linux", "Metasploitable 2/3"], ["eJPT", "OSCP"], C("Teoria", "Practica", "Laboratorio"), [PentestingBook, HtbStartingPoint]),
        T(4, "AD Attacks", ["BloodHound", "Mimikatz", "Responder"], ["PNPT", "CEH", "CompTIA PenTest+"], C("Teoria", "Practica", "Explicacion", "Laboratorio"), [TcmAd, TcmPeh]),
        T(4, "BloodHound / Mimikatz", ["BloodHound", "Mimikatz", "Responder"], ["PNPT", "CEH"], C("Practica", "Explicacion", "Laboratorio"), [TcmAd]),
        T(4, "C2 Frameworks (Cobalt Strike/Havoc)", ["Cobalt Strike", "Havoc C2", "Sliver"], ["PNPT", "CEH"], C("Teoria", "Explicacion", "Laboratorio"), [TcmPeh]),
        T(4, "Pivoting & Tunneling", ["Metasploit", "Nmap", "Kali Linux"], ["eJPT", "PNPT", "CompTIA PenTest+"], C("Teoria", "Practica", "Laboratorio"), [HtbStartingPoint, TcmPeh]),
        T(4, "Escritura de reportes", [], ["PNPT", "CompTIA PenTest+"], C("Explicacion", "Ejercicios"), [TcmPeh, PentestingBook]),
    ];

    private static TopicPopulation T(int phase, string topic, string[] tools, string[] certs, TipoCriterio[] criteria, ResourceSpec[] resources) =>
        new(phase, topic, tools, certs, criteria, resources);

    private static TipoCriterio[] C(params string[] criteria) =>
        criteria.Select(Enum.Parse<TipoCriterio>).ToArray();
}

internal sealed record TopicPopulation(
    int Phase,
    string Topic,
    IReadOnlyCollection<string> Tools,
    IReadOnlyCollection<string> Certifications,
    IReadOnlyCollection<TipoCriterio> Criteria,
    IReadOnlyCollection<ResourceSpec> Resources);

internal sealed record ResourceSpec(string Title, TipoRecurso Type, string? Url);

internal sealed record PopulationPlan(
    Guid UsuarioId,
    IReadOnlyCollection<NamedCount> GlobalCounts,
    IReadOnlyCollection<PhasePlan> Fases)
{
    public int TotalDelta => Fases.Sum(f =>
        f.CriteriosPorAgregar
        + f.TemaHerramientaPorAgregar
        + f.CertificacionTemaPorAgregar
        + f.RecursosPorCrear
        + f.RecursoTemaPorAgregar);
}

internal sealed record NamedCount(string Nombre, int Valor);

internal sealed record PhasePlan(
    int Orden,
    int TemasExistentes,
    int CriteriosActuales,
    int TemaHerramientaActuales,
    int CertificacionTemaActuales,
    int RecursoTemaActuales,
    int CriteriosPorAgregar,
    int TemaHerramientaPorAgregar,
    int CertificacionTemaPorAgregar,
    int RecursosPorCrear,
    int RecursoTemaPorAgregar);

internal sealed record PopulationApplyResult(IReadOnlyCollection<PhaseApplyResult> Fases);

internal sealed record PhaseApplyResult(
    int Orden,
    int CriteriosAgregados,
    int TemaHerramientaAgregados,
    int CertificacionTemaAgregados,
    int RecursosCreados,
    int RecursoTemaAgregados);

internal sealed record BackupResult(string Path, string VerifyOnly);
