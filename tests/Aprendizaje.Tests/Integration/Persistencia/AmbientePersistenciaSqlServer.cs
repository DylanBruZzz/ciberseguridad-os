using System.Data.Common;
using Aprendizaje.Infraestructura.Persistencia;
using Aprendizaje.Infraestructura.Persistencia.Interceptores;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Aprendizaje.Tests.Integration.Persistencia;

internal sealed class AmbientePersistenciaSqlServer : IAsyncDisposable
{
    internal const string ServerEsperado = @".\MSSQLSERVER01";
    internal const string DatabaseEsperada = "AprendizajeTestsDb";
    internal const string ConnectionString =
        @"Server=.\MSSQLSERVER01;Database=AprendizajeTestsDb;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True";

    private readonly AprendizajeDbContext _contextoInicial;

    private AmbientePersistenciaSqlServer(AprendizajeDbContext contextoInicial)
    {
        _contextoInicial = contextoInicial;
    }

    public static async Task<AmbientePersistenciaSqlServer> CrearAsync(CancellationToken cancellationToken)
    {
        ValidarGuardRail(ConnectionString);

        var contexto = CrearContexto();

        await contexto.Database.EnsureDeletedAsync(cancellationToken);
        await contexto.Database.MigrateAsync(cancellationToken);
        await ValidarMigracionesAplicadasAsync(contexto, cancellationToken);

        return new AmbientePersistenciaSqlServer(contexto);
    }

    public AprendizajeDbContext CrearNuevoContexto() => CrearContexto();

    public async ValueTask DisposeAsync()
    {
        await _contextoInicial.DisposeAsync();
    }

    private static AprendizajeDbContext CrearContexto()
    {
        ValidarGuardRail(ConnectionString);

        var opciones = new DbContextOptionsBuilder<AprendizajeDbContext>()
            .UseSqlServer(ConnectionString)
            .AddInterceptors(new AuditoriaInterceptor())
            .Options;

        return new AprendizajeDbContext(opciones);
    }

    private static void ValidarGuardRail(string connectionString)
    {
        var builder = new DbConnectionStringBuilder
        {
            ConnectionString = connectionString
        };

        var database = ObtenerValor(builder, "Database", "Initial Catalog");
        var server = ObtenerValor(builder, "Server", "Data Source", "Addr", "Address", "Network Address");

        if (!string.Equals(server, ServerEsperado, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Servidor SQL no autorizado para tests de persistencia: '{server}'.");

        if (string.IsNullOrWhiteSpace(database))
            throw new InvalidOperationException("Base de datos vacía no autorizada para tests de persistencia.");

        if (string.Equals(database, "AprendizajeDb", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("AprendizajeDb está prohibida para tests de persistencia.");

        if (!string.Equals(database, DatabaseEsperada, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Base de datos no autorizada para tests de persistencia: '{database}'.");
    }

    private static string? ObtenerValor(DbConnectionStringBuilder builder, params string[] claves)
    {
        foreach (var clave in claves)
        {
            if (builder.TryGetValue(clave, out var valor))
                return valor?.ToString();
        }

        return null;
    }

    private static async Task ValidarMigracionesAplicadasAsync(
        AprendizajeDbContext contexto,
        CancellationToken cancellationToken)
    {
        var migraciones = await contexto.Database
            .SqlQueryRaw<string>("SELECT [MigrationId] AS [Value] FROM [dbo].[__EFMigrationsHistory] ORDER BY [MigrationId]")
            .ToListAsync(cancellationToken);

        Assert.Equal(
            new[]
            {
                "20260818153945_Inicial",
                "20260818174900_HacerObjetivosTemaNullable"
            },
            migraciones);
    }
}
