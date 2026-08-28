using System.Text.Json;
using Aprendizaje.Aplicacion.Roadmap.Importacion;
using Aprendizaje.Infraestructura.Importacion;
using Aprendizaje.Infraestructura.Persistencia;
using Aprendizaje.Infraestructura.Persistencia.Interceptores;
using Aprendizaje.Infraestructura.Persistencia.Repositorios;
using Microsoft.EntityFrameworkCore;

var cancellationToken = CancellationToken.None;

try
{
    var argumentos = ArgumentosImportador.Parsear(args);

    if (!argumentos.EsValido)
    {
        Console.Error.WriteLine(argumentos.Error);
        Console.Error.WriteLine("Uso: dotnet run --project tools/Aprendizaje.ImportadorRoadmap -- --usuario-id <guid>");
        return 2;
    }

    var repoRoot = ResolverRaizRepositorio();
    var rutaDataset = Path.Combine(repoRoot, "data", "roadmap", "roadmap-v1.json");
    var connectionString = ResolverConnectionString(repoRoot);
    var documento = await new LectorRoadmapV1Json().LeerAsync(rutaDataset, cancellationToken);

    var opciones = new DbContextOptionsBuilder<AprendizajeDbContext>()
        .UseSqlServer(connectionString)
        .AddInterceptors(new AuditoriaInterceptor())
        .Options;

    await using var contexto = new AprendizajeDbContext(opciones);

    var casoUso = new ImportarRoadmapV1CasoUso(
        new UsuarioRepository(contexto),
        new FaseRepository(contexto),
        new TemaRepository(contexto),
        new HerramientaRepository(contexto),
        new CertificacionRepository(contexto),
        new RecursoRepository(contexto),
        contexto,
        new TransaccionAplicacion(contexto));

    var resultado = await casoUso.EjecutarAsync(
        new ImportarRoadmapV1Solicitud(argumentos.UsuarioId, documento),
        cancellationToken);

    ImprimirResultado(resultado);

    return resultado.Estado == ImportarRoadmapV1Estado.Importado ? 0 : 1;
}
catch (Exception ex) when (ex is JsonException or IOException or InvalidOperationException or ArgumentException or DbUpdateException)
{
    Console.Error.WriteLine("IMPORTACION ROADMAP V1 FALLIDA");
    Console.Error.WriteLine(ex.Message);
    return 1;
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

    throw new InvalidOperationException("No se pudo resolver la raíz del repositorio.");
}

static string ResolverConnectionString(string repoRoot)
{
    const string variableEntorno = "APRENDIZAJE_IMPORTADOR_CONNECTION_STRING";

    var desdeEntorno = Environment.GetEnvironmentVariable(variableEntorno);

    if (!string.IsNullOrWhiteSpace(desdeEntorno))
        return desdeEntorno;

    var rutaAppSettings = Path.Combine(
        repoRoot,
        "src",
        "Aprendizaje.Api",
        "appsettings.Development.json");

    using var documento = JsonDocument.Parse(File.ReadAllText(rutaAppSettings));

    if (documento.RootElement
        .GetProperty("ConnectionStrings")
        .TryGetProperty("AprendizajeDb", out var connectionString)
        && !string.IsNullOrWhiteSpace(connectionString.GetString()))
    {
        return connectionString.GetString()!;
    }

    throw new InvalidOperationException("No se encontró ConnectionStrings:AprendizajeDb.");
}

static void ImprimirResultado(ImportarRoadmapV1Resultado resultado)
{
    if (resultado.Estado != ImportarRoadmapV1Estado.Importado)
    {
        Console.Error.WriteLine($"IMPORTACION ROADMAP V1 NO COMPLETADA: {resultado.Estado}");

        foreach (var error in resultado.Errores)
            Console.Error.WriteLine($"- {error}");

        return;
    }

    Console.WriteLine("ROADMAP V1 IMPORTADO");
    Console.WriteLine($"Usuario: {resultado.UsuarioId}");
    Console.WriteLine($"Dataset: {resultado.VersionDataset}");
    Console.WriteLine($"Fases: creadas={resultado.Fases.Creados}, existentes={resultado.Fases.Existentes}, actualizadas={resultado.Fases.Actualizados}");
    Console.WriteLine($"Temas: creados={resultado.Temas.Creados}, existentes={resultado.Temas.Existentes}, actualizados={resultado.Temas.Actualizados}");
    Console.WriteLine($"Herramientas: creadas={resultado.Herramientas.Creados}, reutilizadas={resultado.Herramientas.Reutilizados}, actualizadas={resultado.Herramientas.Actualizados}");
    Console.WriteLine($"Certificaciones: creadas={resultado.Certificaciones.Creados}, reutilizadas={resultado.Certificaciones.Reutilizados}, actualizadas={resultado.Certificaciones.Actualizados}");
    Console.WriteLine($"Recursos: creados={resultado.Recursos.Creados}, existentes={resultado.Recursos.Existentes}");
    Console.WriteLine($"Relaciones RecursoTema: creadas={resultado.Relaciones.RecursoTemaCreadas}, existentes={resultado.Relaciones.RecursoTemaExistentes}");
    Console.WriteLine($"Relaciones CertificacionTema: creadas={resultado.Relaciones.CertificacionTemaCreadas}, existentes={resultado.Relaciones.CertificacionTemaExistentes}");
    Console.WriteLine($"Evidence: {resultado.EvidenceCreadas}");

    foreach (var advertencia in resultado.Advertencias)
        Console.WriteLine($"Advertencia: {advertencia}");
}

internal sealed record ArgumentosImportador(Guid UsuarioId, string? Error)
{
    public bool EsValido => Error is null;

    public static ArgumentosImportador Parsear(string[] args)
    {
        if (args.Length != 2 || args[0] != "--usuario-id")
            return new ArgumentosImportador(Guid.Empty, "Argumentos inválidos.");

        if (!Guid.TryParse(args[1], out var usuarioId))
            return new ArgumentosImportador(Guid.Empty, "UsuarioId debe ser un GUID válido.");

        if (usuarioId == Guid.Empty)
            return new ArgumentosImportador(Guid.Empty, "UsuarioId no puede ser Guid.Empty.");

        return new ArgumentosImportador(usuarioId, null);
    }
}
