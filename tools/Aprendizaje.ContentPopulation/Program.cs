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

    if (argumentos.ExternalBackupsVerified)
    {
        Console.WriteLine("ExternalBackupsVerified: true");
    }
    else
    {
        var preBackup = await servicio.CrearBackupVerificadoAsync("PreContentPopulationV1", cancellationToken);
        Console.WriteLine($"PreBackup: {preBackup.Path}");
        Console.WriteLine($"PreBackupVerifyOnly: {preBackup.VerifyOnly}");
    }

    var resultado = await servicio.AplicarAsync(plan, cancellationToken);
    ImprimirResultadoAplicacion(resultado);

    var segundoPlan = await servicio.CrearPlanAsync(cancellationToken);
    Console.WriteLine($"IdempotencyPendingDelta: {segundoPlan.TotalDelta}");

    if (segundoPlan.TotalDelta != 0)
        throw new InvalidOperationException("La segunda ejecucion no seria idempotente; se detiene antes del post-backup.");

    if (!argumentos.ExternalBackupsVerified)
    {
        var postBackup = await servicio.CrearBackupVerificadoAsync("PostContentPopulationV1", cancellationToken);
        Console.WriteLine($"PostBackup: {postBackup.Path}");
        Console.WriteLine($"PostBackupVerifyOnly: {postBackup.VerifyOnly}");
    }

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
    Console.Error.WriteLine("  dotnet run --project tools/Aprendizaje.ContentPopulation -- apply --confirm-personal-write --external-backups-verified");
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
        Console.WriteLine($"Fase {fase.Orden}: temas={fase.TemasExistentes}, criterios={fase.CriteriosActuales}, criteriosConDescripcion={fase.CriteriosConDescripcionActuales}, temaHerramienta={fase.TemaHerramientaActuales}, certificacionTema={fase.CertificacionTemaActuales}, recursoTema={fase.RecursoTemaActuales}");
    }
}

static void ImprimirPlan(PopulationPlan plan)
{
    ImprimirInventario(plan);
    Console.WriteLine("PLAN");
    foreach (var fase in plan.Fases)
    {
        Console.WriteLine($"Fase {fase.Orden}: criterios+={fase.CriteriosPorAgregar}, descripciones+={fase.DescripcionesCriterioPorCompletar}, temaHerramienta+={fase.TemaHerramientaPorAgregar}, certificacionTema+={fase.CertificacionTemaPorAgregar}, recursos+={fase.RecursosPorCrear}, recursoTema+={fase.RecursoTemaPorAgregar}, subtemas+=0");
    }

    Console.WriteLine($"TotalDelta: {plan.TotalDelta}");
}

static void ImprimirResultadoAplicacion(PopulationApplyResult resultado)
{
    Console.WriteLine("APPLIED");
    foreach (var fase in resultado.Fases)
    {
        Console.WriteLine($"Fase {fase.Orden}: criterios+={fase.CriteriosAgregados}, descripciones+={fase.DescripcionesCriterioCompletadas}, temaHerramienta+={fase.TemaHerramientaAgregados}, certificacionTema+={fase.CertificacionTemaAgregados}, recursos+={fase.RecursosCreados}, recursoTema+={fase.RecursoTemaAgregados}, subtemas+=0");
    }
}

internal sealed record Argumentos(
    string Mode,
    bool ConfirmPersonalWrite,
    bool ExternalBackupsVerified,
    string? ConnectionString,
    string? Error)
{
    public bool EsValido => Error is null;

    public static Argumentos Parsear(string[] args)
    {
        if (args.Length == 0)
            return new Argumentos("", false, false, null, "Modo requerido.");

        var mode = args[0].Trim().ToLowerInvariant();
        if (mode is not ("inventory" or "plan" or "apply"))
            return new Argumentos(mode, false, false, null, "Modo invalido.");

        var confirm = false;
        var externalBackupsVerified = false;
        string? connectionString = null;

        for (var i = 1; i < args.Length; i++)
        {
            if (args[i] == "--confirm-personal-write")
            {
                confirm = true;
                continue;
            }

            if (args[i] == "--external-backups-verified")
            {
                externalBackupsVerified = true;
                continue;
            }

            if (args[i] == "--connection-string" && i + 1 < args.Length)
            {
                connectionString = args[++i];
                continue;
            }

            return new Argumentos(mode, confirm, externalBackupsVerified, connectionString, $"Argumento invalido: {args[i]}");
        }

        return new Argumentos(mode, confirm, externalBackupsVerified, connectionString, null);
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
            var descripcionesPorCompletar = items
                .Sum(item =>
                {
                    var tema = ObtenerTema(temasFase, item.Topic);
                    return tema.Criterios.Count == 0
                        ? 0
                        : item.Criteria.Count(criterio =>
                        {
                            var existente = tema.Criterios.SingleOrDefault(c => c.Tipo == criterio.Type);
                            return existente is not null && string.IsNullOrWhiteSpace(existente.Descripcion);
                        });
                });

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
                temasFase.SelectMany(t => t.Criterios).Count(c => !string.IsNullOrWhiteSpace(c.Descripcion)),
                await CountPhaseAsync("roadmap.TemaHerramienta", fase.Id, cancellationToken),
                await CountPhaseAsync("roadmap.CertificacionTema", fase.Id, cancellationToken),
                await CountPhaseAsync("resource.RecursoTema", fase.Id, cancellationToken),
                criteriosPorAgregar,
                descripcionesPorCompletar,
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
        var descripcionesCriterioCompletadas = 0;
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
                    new DefinirCriteriosRelevantesTemaSolicitud(
                        tema.Id,
                        item.Criteria.Select(c => new DefinicionCriterioTemaSolicitud(c.Type, c.Description)).ToArray()),
                    cancellationToken);

                if (resultadoCriterios.Estado != DefinirCriteriosRelevantesTemaEstado.Actualizado)
                    throw new InvalidOperationException($"No se pudieron definir criterios para {item.Topic}: {resultadoCriterios.Estado}");

                criteriosAgregados += item.Criteria.Count;
            }
            else
            {
                foreach (var criterio in item.Criteria)
                {
                    var existente = tema.Criterios.SingleOrDefault(c => c.Tipo == criterio.Type);
                    if (existente is null)
                        throw new InvalidOperationException($"El criterio {criterio.Type} no existe para {item.Topic}; se detiene para no recrear criterios ni perder progreso.");

                    if (!string.IsNullOrWhiteSpace(existente.Descripcion))
                        continue;

                    tema.ActualizarDescripcionCriterio(criterio.Type, criterio.Description);
                    descripcionesCriterioCompletadas++;
                }
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

        await _context.GuardarCambiosAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new PhaseApplyResult(phase, criteriosAgregados, descripcionesCriterioCompletadas, temaHerramientaAgregados, certificacionTemaAgregados, recursosCreados, recursoTemaAgregados);
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
            new("CriterioTemaConDescripcion", await CountSqlAsync("SELECT COUNT(*) AS Value FROM roadmap.CriterioTema WHERE Descripcion IS NOT NULL AND LTRIM(RTRIM(Descripcion)) <> ''", cancellationToken)),
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
        T(1, "Modelo OSI / TCP-IP", ["Wireshark", "tcpdump"], ["CompTIA A+", "Cisco CyberOps Associate"], C(("Teoria", "Explicar las funciones de las capas OSI y relacionarlas con protocolos habituales de TCP/IP."), ("Explicacion", "Describir el recorrido de un dato encapsulado desde una aplicacion hasta la red fisica."), ("Ejercicios", "Clasificar protocolos y problemas comunes segun la capa OSI o TCP/IP correspondiente.")), [CiscoNetAcad, Tanenbaum, WiresharkUserGuide]),
        T(1, "Subnetting / CIDR", [], ["CompTIA A+", "Cisco CyberOps Associate"], C(("Teoria", "Distinguir mascara, prefijo CIDR, red, broadcast y rango util en IPv4."), ("Practica", "Calcular subredes IPv4 a partir de una red base y una cantidad requerida de hosts."), ("Ejercicios", "Resolver ejercicios de CIDR verificando direccion de red, broadcast y hosts disponibles.")), [CiscoNetAcad, Tanenbaum]),
        T(1, "DNS, DHCP, HTTP/S", ["Wireshark", "tcpdump", "curl"], ["CompTIA Security+", "Cisco CyberOps Associate"], C(("Teoria", "Explicar resolucion DNS, asignacion DHCP y flujo basico de una peticion HTTP/S."), ("Practica", "Consultar registros DNS y probar respuestas HTTP/S con herramientas de linea de comandos."), ("Explicacion", "Diferenciar fallos de DNS, direccionamiento DHCP y disponibilidad de servicio web.")), [CiscoNetAcad, Tanenbaum, WiresharkUserGuide]),
        T(1, "Bash scripting", ["Bash", "sed", "awk", "grep"], ["CompTIA A+"], C(("Practica", "Escribir scripts Bash simples con variables, condicionales, bucles y manejo basico de errores."), ("Ejercicios", "Resolver tareas de filtrado y transformacion de texto usando grep, sed y awk."), ("Laboratorio", "Automatizar una tarea repetible en un entorno Linux de practica y documentar entradas y salidas.")), [BashManual, OverTheWireBandit]),
        T(1, "Permisos Linux", ["Bash"], ["CompTIA A+", "CompTIA Security+"], C(("Teoria", "Explicar permisos de usuario, grupo y otros junto con bits especiales comunes."), ("Practica", "Aplicar chmod, chown y umask sobre archivos de laboratorio verificando el resultado."), ("Explicacion", "Justificar por que un usuario puede o no leer, modificar o ejecutar un archivo concreto.")), [BashManual, TryHackMePreSecurity]),
        T(1, "Virtualización", ["VirtualBox", "VMware", "Proxmox"], ["CompTIA A+"], C(("Teoria", "Distinguir hipervisor, maquina virtual, snapshot, red NAT y red bridge."), ("Practica", "Crear una maquina virtual de practica configurando recursos, almacenamiento y conectividad local."), ("Laboratorio", "Preparar un laboratorio aislado con al menos dos maquinas y validar comunicacion controlada.")), [TryHackMePreSecurity]),
        T(1, "Wireshark basics", ["Wireshark", "tcpdump"], ["Cisco CyberOps Associate"], C(("Practica", "Capturar trafico basico y aplicar filtros de visualizacion por protocolo, host o puerto."), ("Explicacion", "Interpretar una captura sencilla identificando origen, destino, protocolo y proposito del intercambio."), ("Laboratorio", "Documentar una captura controlada de DNS o HTTP señalando paquetes clave y observaciones.")), [WiresharkUserGuide, CiscoNetAcad]),
        T(1, "SSH, FTP, SMB", ["Wireshark", "Nmap", "curl"], ["CompTIA Security+", "Cisco CyberOps Associate"], C(("Teoria", "Comparar los usos, puertos habituales y riesgos basicos de SSH, FTP y SMB."), ("Practica", "Identificar servicios SSH, FTP y SMB en un entorno propio usando herramientas de enumeracion basica."), ("Explicacion", "Explicar que evidencia permite distinguir un servicio expuesto de un problema de conectividad.")), [NmapReference, TryHackMePreSecurity]),
        T(1, "Routing & Switching", ["Wireshark", "tcpdump"], ["Cisco CyberOps Associate"], C(("Teoria", "Explicar la diferencia entre switching de capa 2, routing de capa 3 y gateway por defecto."), ("Explicacion", "Describir como una trama o paquete viaja entre hosts de la misma red y redes distintas."), ("Ejercicios", "Resolver ejercicios basicos de tablas de rutas, gateways y dominios de broadcast.")), [CiscoNetAcad, Tanenbaum]),
        T(1, "Firewall básico", ["Nmap", "Wireshark"], ["CompTIA Security+", "Cisco CyberOps Associate"], C(("Teoria", "Explicar reglas de firewall por origen, destino, puerto, protocolo y accion."), ("Practica", "Probar una regla simple en laboratorio y verificar el cambio con escaneo local controlado."), ("Explicacion", "Diferenciar trafico permitido, bloqueado y servicio no disponible a partir de evidencias basicas.")), [CiscoNetAcad, NmapReference]),

        T(2, "Active Directory", ["PowerShell"], ["CompTIA Security+"], C(("Teoria", "Explicar dominio, controlador, usuario, grupo, OU y autenticacion centralizada en Active Directory."), ("Explicacion", "Describir como una cuenta obtiene permisos mediante grupos y politicas dentro de un dominio."), ("Laboratorio", "Explorar una estructura AD de practica y documentar usuarios, grupos y OUs relevantes.")), [ActiveDirectoryDocs, MicrosoftWindowsServer, TcmPeh]),
        T(2, "Group Policy (GPO)", ["PowerShell"], ["CompTIA Security+"], C(("Teoria", "Explicar alcance, herencia, precedencia y aplicacion de una Group Policy."), ("Practica", "Crear o inspeccionar una GPO de laboratorio y verificar su efecto esperado."), ("Laboratorio", "Aplicar una politica simple en un entorno Windows controlado y documentar antes y despues.")), [GroupPolicyDocs, MicrosoftWindowsServer]),
        T(2, "PowerShell scripting", ["PowerShell"], ["CompTIA A+", "CompTIA Security+"], C(("Practica", "Escribir scripts PowerShell con variables, pipeline, filtros y manejo basico de errores."), ("Ejercicios", "Resolver tareas administrativas usando cmdlets, Where-Object y Select-Object."), ("Laboratorio", "Automatizar una revision de configuracion local y registrar la salida en un archivo.")), [PowerShellDocs, MicrosoftWindowsServer]),
        T(2, "Python para seguridad", ["Python 3"], ["eJPT"], C(("Practica", "Crear scripts Python simples para leer archivos, procesar texto y consumir argumentos."), ("Ejercicios", "Resolver ejercicios de parsing, listas, diccionarios y manejo de excepciones aplicados a datos de seguridad."), ("Laboratorio", "Construir una utilidad de laboratorio que procese indicadores o logs de ejemplo sin afectar sistemas reales.")), [PythonTutorial, LearnPythonHardWay, TcmPeh]),
        T(2, "Event Viewer / Syslog", ["PowerShell", "Wazuh", "Elastic Stack"], ["Cisco CyberOps Associate", "CompTIA Security+"], C(("Teoria", "Distinguir eventos de sistema, aplicacion y seguridad junto con severidades comunes."), ("Practica", "Buscar eventos relevantes en Windows Event Viewer o syslog usando filtros basicos."), ("Explicacion", "Explicar una secuencia de eventos indicando origen, impacto probable y siguiente verificacion.")), [MicrosoftWindowsServer, TryHackMeSoc1, WazuhDocs]),
        T(2, "Hardening SSOO", ["PowerShell", "Bash"], ["CompTIA Security+"], C(("Teoria", "Explicar principios de reduccion de superficie, minimo privilegio y configuracion segura base."), ("Practica", "Aplicar comprobaciones de hardening en una maquina de laboratorio sin romper servicios requeridos."), ("Explicacion", "Justificar una recomendacion de hardening conectandola con el riesgo que reduce."), ("Laboratorio", "Documentar un checklist de hardening antes/despues en Windows o Linux de practica.")), [MicrosoftWindowsServer, TryHackMeSoc1]),
        T(2, "Cron jobs", ["Bash"], ["CompTIA A+"], C(("Practica", "Crear tareas cron simples con horarios correctos y comandos verificables."), ("Ejercicios", "Interpretar expresiones cron y corregir horarios que no coinciden con el objetivo.")), [BashManual]),
        T(2, "VMware Workstation", ["VMware", "VirtualBox"], ["CompTIA A+"], C(("Practica", "Configurar maquinas virtuales, snapshots y redes locales segun una necesidad de laboratorio."), ("Laboratorio", "Construir un entorno reproducible con snapshots antes de cambios potencialmente riesgosos.")), [MicrosoftWindowsServer, TryHackMePreSecurity]),
        T(2, "Kali Linux intro", ["Kali Linux", "Nmap"], ["eJPT"], C(("Teoria", "Explicar el rol de Kali como distribucion de laboratorio y sus limites de uso autorizado."), ("Practica", "Navegar herramientas basicas de Kali y ejecutar comandos de reconocimiento en un entorno propio."), ("Laboratorio", "Preparar una VM Kali aislada, actualizarla y registrar herramientas iniciales disponibles.")), [TcmPeh, NmapReference]),
        T(2, "Gestión de usuarios", ["PowerShell", "Bash"], ["CompTIA A+", "CompTIA Security+"], C(("Teoria", "Explicar cuentas, grupos, privilegios y separacion entre usuarios administrativos y estandar."), ("Practica", "Crear, modificar y revisar usuarios/grupos en laboratorio usando herramientas del sistema."), ("Explicacion", "Analizar un problema de acceso relacionandolo con pertenencia a grupos y permisos efectivos.")), [MicrosoftWindowsServer, BashManual]),

        T(3, "MITRE ATT&CK Framework", ["Wazuh", "Splunk"], ["Cisco CyberOps Associate", "CompTIA Security+"], C(("Teoria", "Explicar tacticas, tecnicas y procedimientos de ATT&CK y su uso como lenguaje comun defensivo."), ("Explicacion", "Mapear una actividad observada a una tecnica ATT&CK justificando la evidencia usada."), ("Ejercicios", "Clasificar escenarios defensivos sencillos por tactica y tecnica ATT&CK probable.")), [MitreAttack, TryHackMeBlue, CiscoCyberOps]),
        T(3, "Cyber Kill Chain", [], ["Cisco CyberOps Associate", "CompTIA Security+"], C(("Teoria", "Explicar las fases de Cyber Kill Chain y su utilidad para estructurar deteccion."), ("Explicacion", "Describir en que fase se encuentra una actividad observada y que controles ayudan a detectarla."), ("Ejercicios", "Ordenar eventos de un incidente simulado segun las fases de la Kill Chain.")), [TryHackMeBlue, CiscoCyberOps]),
        T(3, "Splunk / Wazuh", ["Splunk", "Wazuh", "Elastic Stack"], ["Cisco CyberOps Associate"], C(("Practica", "Ejecutar busquedas basicas en logs para filtrar por host, evento, usuario o severidad."), ("Explicacion", "Explicar que muestra una consulta de SIEM y que hipotesis defensiva valida."), ("Laboratorio", "Analizar un dataset de laboratorio y documentar hallazgos con consultas reproducibles.")), [SplunkBots, WazuhDocs, BlueTeamLabs]),
        T(3, "IDS/IPS (Snort/Suricata)", ["Snort", "Suricata", "Zeek"], ["Cisco CyberOps Associate", "CompTIA Security+"], C(("Teoria", "Distinguir IDS, IPS, firmas, alertas y falsos positivos en monitoreo de red."), ("Practica", "Revisar alertas de Snort o Suricata y asociarlas con trafico observado."), ("Laboratorio", "Generar trafico controlado y documentar alertas resultantes sin afectar redes externas.")), [NsmBook, TryHackMeBlue]),
        T(3, "Threat Intelligence", ["Maltego", "Shodan", "theHarvester"], ["Cisco CyberOps Associate", "CompTIA Security+"], C(("Teoria", "Explicar indicadores, contexto, fuentes y ciclo basico de inteligencia de amenazas."), ("Practica", "Consultar indicadores en fuentes abiertas y registrar contexto defensivo verificable."), ("Explicacion", "Diferenciar un indicador aislado de una conclusion accionable para defensa.")), [MitreAttack, BlueTeamLabs]),
        T(3, "Criptografía simétrica/asimétrica", [], ["CompTIA Security+"], C(("Teoria", "Comparar cifrado simetrico, asimetrico, hash y firma digital con casos de uso comunes."), ("Explicacion", "Explicar por que TLS combina intercambio de claves, certificados y cifrado de sesion."), ("Ejercicios", "Resolver preguntas de seleccion de mecanismo criptografico segun objetivo de confidencialidad o integridad.")), [CiscoCyberOps]),
        T(3, "PKI y certificados TLS", ["Wireshark", "curl"], ["CompTIA Security+"], C(("Teoria", "Explicar CA, certificado, cadena de confianza, CN/SAN y expiracion en TLS."), ("Practica", "Inspeccionar certificados TLS con navegador, curl u openssl y reconocer campos clave."), ("Explicacion", "Diagnosticar errores comunes de certificado distinguiendo expiracion, nombre incorrecto y confianza.")), [WiresharkUserGuide, CiscoCyberOps]),
        T(3, "OSINT básico", ["Maltego", "Shodan", "theHarvester"], ["CompTIA Security+"], C(("Practica", "Recolectar informacion publica de un objetivo de practica sin autenticacion ni intrusion."), ("Explicacion", "Explicar la diferencia entre informacion publica, dato sensible expuesto y acceso no autorizado."), ("Laboratorio", "Documentar hallazgos OSINT de un dominio de laboratorio con fuentes y limites claros.")), [TryHackMeBlue]),
        T(3, "Análisis de logs", ["Splunk", "Wazuh", "Elastic Stack"], ["Cisco CyberOps Associate", "CompTIA Security+"], C(("Practica", "Filtrar logs por tiempo, host, usuario y evento para reconstruir una actividad."), ("Explicacion", "Narrar una secuencia de eventos indicando evidencia, incertidumbres y siguiente paso defensivo."), ("Laboratorio", "Analizar logs de laboratorio y producir un resumen tecnico con indicadores observados.")), [SplunkBots, WazuhDocs, NsmBook]),
        T(3, "Nessus / OpenVAS", ["Nessus", "OpenVAS", "Nuclei"], ["CompTIA Security+", "Cisco CyberOps Associate"], C(("Teoria", "Explicar escaneo autenticado, severidad, CVE, falso positivo y priorizacion defensiva."), ("Practica", "Ejecutar un escaneo en un objetivo de laboratorio y revisar hallazgos principales."), ("Laboratorio", "Documentar vulnerabilidades de un entorno propio con evidencia, riesgo y remediacion sugerida.")), [BlueTeamLabs, TryHackMeBlue]),

        T(4, "Nmap / Nessus avanzado", ["Nmap", "Nessus", "Nuclei"], ["eJPT", "CompTIA PenTest+", "CEH"], C(("Practica", "Ejecutar enumeracion avanzada con Nmap en un laboratorio autorizado y guardar evidencia reproducible."), ("Explicacion", "Interpretar resultados de escaneo diferenciando servicio, version, exposicion y posible falso positivo."), ("Laboratorio", "Comparar hallazgos de Nmap y Nessus en maquinas de practica y documentar prioridades.")), [NmapReference, HtbStartingPoint, TcmPeh]),
        T(4, "Metasploit Framework", ["Metasploit", "Metasploitable 2/3"], ["eJPT", "PNPT", "CEH"], C(("Teoria", "Explicar modulo, payload, session y post-explotacion dentro de un laboratorio autorizado."), ("Practica", "Usar Metasploit contra una maquina vulnerable de practica siguiendo una guia controlada."), ("Laboratorio", "Documentar condiciones, evidencia y mitigacion de una explotacion realizada en entorno propio.")), [MetasploitDocs, HtbStartingPoint, VulnHub]),
        T(4, "Burp Suite Pro", ["Burp Suite", "Burp Suite Pro", "OWASP ZAP"], ["eJPT", "CompTIA PenTest+", "PNPT"], C(("Practica", "Interceptar y modificar peticiones de una aplicacion vulnerable de practica con Burp Suite."), ("Explicacion", "Explicar que parametros, cabeceras o respuestas sustentan una hipotesis de vulnerabilidad."), ("Laboratorio", "Registrar un flujo de prueba web autorizado con request, response, impacto y recomendacion.")), [BurpDocs, PortSwiggerExisting]),
        T(4, "SQLi / XSS / SSRF", ["Burp Suite Pro", "OWASP ZAP", "SQLmap", "DVWA"], ["eJPT", "CompTIA PenTest+", "PNPT"], C(("Teoria", "Distinguir SQLi, XSS y SSRF por causa, impacto y evidencia observable en aplicaciones web."), ("Practica", "Reproducir vulnerabilidades web en plataformas deliberadamente vulnerables y autorizadas."), ("Explicacion", "Explicar impacto y mitigacion de un hallazgo web sin orientar acciones contra sistemas reales."), ("Laboratorio", "Documentar pruebas controladas en DVWA o PortSwigger con payload, resultado y remediacion.")), [PortSwiggerExisting, OwaspTop10, VulnHub]),
        T(4, "OWASP Top 10", ["Burp Suite", "OWASP ZAP", "DVWA"], ["CompTIA Security+", "eJPT", "CompTIA PenTest+"], C(("Teoria", "Explicar las categorias OWASP Top 10 y asociarlas con riesgos habituales de aplicaciones web."), ("Explicacion", "Relacionar un hallazgo de laboratorio con la categoria OWASP correspondiente y su impacto."), ("Laboratorio", "Evaluar una aplicacion vulnerable de practica contra categorias OWASP seleccionadas y documentar evidencias.")), [OwaspTop10, PortSwiggerExisting]),
        T(4, "Buffer Overflow básico", ["Kali Linux", "Metasploitable 2/3"], ["eJPT", "OSCP"], C(("Teoria", "Explicar stack, registro de instruccion, overflow y control de flujo en un ejemplo educativo."), ("Practica", "Reproducir un overflow basico en binario de laboratorio siguiendo limites controlados."), ("Laboratorio", "Documentar el proceso de identificacion, prueba y mitigacion conceptual en entorno aislado.")), [PentestingBook, HtbStartingPoint]),
        T(4, "AD Attacks", ["BloodHound", "Mimikatz", "Responder"], ["PNPT", "CEH", "CompTIA PenTest+"], C(("Teoria", "Explicar tecnicas comunes contra AD desde la perspectiva de laboratorio y defensa."), ("Practica", "Enumerar un dominio de practica autorizado para identificar relaciones y configuraciones riesgosas."), ("Explicacion", "Describir la ruta de ataque simulada y los controles defensivos que la interrumpen."), ("Laboratorio", "Documentar una cadena AD en entorno propio con evidencia, impacto y remediacion.")), [TcmAd, TcmPeh]),
        T(4, "BloodHound / Mimikatz", ["BloodHound", "Mimikatz", "Responder"], ["PNPT", "CEH"], C(("Practica", "Usar BloodHound en un dominio de laboratorio para analizar relaciones de privilegio autorizadas."), ("Explicacion", "Explicar que muestra una ruta de BloodHound y que configuraciones la hacen posible."), ("Laboratorio", "Documentar hallazgos de privilegio en laboratorio y proponer cambios defensivos concretos.")), [TcmAd]),
        T(4, "C2 Frameworks (Cobalt Strike/Havoc)", ["Cobalt Strike", "Havoc C2", "Sliver"], ["PNPT", "CEH"], C(("Teoria", "Explicar arquitectura C2, beaconing e indicadores defensivos en un contexto de laboratorio."), ("Explicacion", "Describir señales observables de actividad C2 simulada y posibles detecciones defensivas."), ("Laboratorio", "Analizar documentacion o simulacion controlada de C2 sin operar contra sistemas reales.")), [TcmPeh]),
        T(4, "Pivoting & Tunneling", ["Metasploit", "Nmap", "Kali Linux"], ["eJPT", "PNPT", "CompTIA PenTest+"], C(("Teoria", "Explicar pivoting, tunneling y segmentacion de red dentro de un laboratorio autorizado."), ("Practica", "Configurar un tunel de practica entre maquinas controladas y validar alcance esperado."), ("Laboratorio", "Documentar una prueba de pivoting en entorno propio con diagrama, comandos y limites.")), [HtbStartingPoint, TcmPeh]),
        T(4, "Escritura de reportes", [], ["PNPT", "CompTIA PenTest+"], C(("Explicacion", "Comunicar hallazgos tecnicos con evidencia, impacto, alcance autorizado y remediacion accionable."), ("Ejercicios", "Convertir notas de laboratorio en un reporte claro con resumen ejecutivo y detalle tecnico.")), [TcmPeh, PentestingBook]),
    ];

    private static TopicPopulation T(int phase, string topic, string[] tools, string[] certs, CriterionSpec[] criteria, ResourceSpec[] resources) =>
        new(phase, topic, tools, certs, criteria, resources);

    private static CriterionSpec[] C(params (string Type, string Description)[] criteria) =>
        criteria
            .Select(c => new CriterionSpec(Enum.Parse<TipoCriterio>(c.Type), c.Description))
            .ToArray();
}

internal sealed record TopicPopulation(
    int Phase,
    string Topic,
    IReadOnlyCollection<string> Tools,
    IReadOnlyCollection<string> Certifications,
    IReadOnlyCollection<CriterionSpec> Criteria,
    IReadOnlyCollection<ResourceSpec> Resources);

internal sealed record CriterionSpec(TipoCriterio Type, string Description);

internal sealed record ResourceSpec(string Title, TipoRecurso Type, string? Url);

internal sealed record PopulationPlan(
    Guid UsuarioId,
    IReadOnlyCollection<NamedCount> GlobalCounts,
    IReadOnlyCollection<PhasePlan> Fases)
{
    public int TotalDelta => Fases.Sum(f =>
        f.CriteriosPorAgregar
        + f.DescripcionesCriterioPorCompletar
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
    int CriteriosConDescripcionActuales,
    int TemaHerramientaActuales,
    int CertificacionTemaActuales,
    int RecursoTemaActuales,
    int CriteriosPorAgregar,
    int DescripcionesCriterioPorCompletar,
    int TemaHerramientaPorAgregar,
    int CertificacionTemaPorAgregar,
    int RecursosPorCrear,
    int RecursoTemaPorAgregar);

internal sealed record PopulationApplyResult(IReadOnlyCollection<PhaseApplyResult> Fases);

internal sealed record PhaseApplyResult(
    int Orden,
    int CriteriosAgregados,
    int DescripcionesCriterioCompletadas,
    int TemaHerramientaAgregados,
    int CertificacionTemaAgregados,
    int RecursosCreados,
    int RecursoTemaAgregados);

internal sealed record BackupResult(string Path, string VerifyOnly);
