using System.Diagnostics;
using System.Net.Sockets;
using Aprendizaje.Infraestructura.Persistencia;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Aprendizaje.Api.Configuracion;

public static class RuntimeLocalWindows
{
    public const string EntornoPersonal = "Personal";
    public const string PackagedMarkerFileName = "CiberseguridadOS.packaged";
    public const string DashboardUrl = "http://localhost:64021/dashboard";

    private const string MutexName = "CiberseguridadOS.LocalExecutable";
    private const string NoBrowserEnvironmentVariable = "CIBERSEGURIDADOS_NO_BROWSER";

    public static void ConfigurarEntornoPersonalSiEsEmpaquetado()
    {
        var entornoInicial = ResolverEntornoInicial(
            EsEjecucionEmpaquetada(),
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"));

        if (!string.IsNullOrWhiteSpace(entornoInicial))
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", entornoInicial);
    }

    public static string? ResolverEntornoInicial(
        bool esEjecucionEmpaquetada,
        string? aspnetCoreEnvironment,
        string? dotnetEnvironment)
    {
        if (!esEjecucionEmpaquetada)
            return null;

        if (!string.IsNullOrWhiteSpace(aspnetCoreEnvironment))
            return null;

        if (!string.IsNullOrWhiteSpace(dotnetEnvironment))
            return null;

        return EntornoPersonal;
    }

    public static bool EsEjecucionEmpaquetada() =>
        File.Exists(Path.Combine(AppContext.BaseDirectory, PackagedMarkerFileName));

    public static Mutex? IntentarTomarInstanciaUnica()
    {
        if (!EsEjecucionEmpaquetada() || !OperatingSystem.IsWindows())
            return null;

        var mutex = new Mutex(true, MutexName, out var createdNew);

        if (createdNew)
            return mutex;

        mutex.Dispose();
        return null;
    }

    public static bool DebeCerrarPorInstanciaExistente(Mutex? mutex) =>
        EsEjecucionEmpaquetada() && OperatingSystem.IsWindows() && mutex is null;

    public static async Task<bool> ValidarBasePersonalSiCorrespondeAsync(WebApplication app)
    {
        if (!EsEjecucionEmpaquetada() || !app.Environment.IsEnvironment(EntornoPersonal))
            return true;

        try
        {
            await using var scope = app.Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AprendizajeDbContext>();

            if (await dbContext.Database.CanConnectAsync())
                return true;
        }
        catch (Exception ex) when (EsErrorConexionSql(ex))
        {
            EscribirErrorSql(ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            EscribirErrorSql(ex.Message);
            return false;
        }

        EscribirErrorSql(null);
        return false;
    }

    public static void RegistrarAperturaNavegador(WebApplication app)
    {
        if (!DebeAbrirNavegador(app.Environment))
            return;

        app.Lifetime.ApplicationStarted.Register(AbrirDashboard);
    }

    public static void AbrirDashboard()
    {
        try
        {
            Process.Start(new ProcessStartInfo(DashboardUrl)
            {
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Ciberseguridad OS inicio correctamente, pero no pudo abrir el navegador automaticamente.");
            Console.Error.WriteLine($"Abre manualmente: {DashboardUrl}");
            Console.Error.WriteLine($"Detalle: {ex.Message}");
        }
    }

    private static bool DebeAbrirNavegador(IHostEnvironment environment) =>
        EsEjecucionEmpaquetada()
        && environment.IsEnvironment(EntornoPersonal)
        && !string.Equals(
            Environment.GetEnvironmentVariable(NoBrowserEnvironmentVariable),
            "1",
            StringComparison.OrdinalIgnoreCase);

    private static bool EsErrorConexionSql(Exception ex) =>
        ex is SqlException or SocketException || ex.InnerException is not null && EsErrorConexionSql(ex.InnerException);

    private static void EscribirErrorSql(string? detalle)
    {
        Console.Error.WriteLine("Ciberseguridad OS no pudo conectarse a la base de datos local.");
        Console.Error.WriteLine("Verifica que SQL Server este iniciado y que la instancia MSSQLSERVER01 este disponible.");

        if (!string.IsNullOrWhiteSpace(detalle))
            Console.Error.WriteLine($"Detalle: {detalle}");
    }
}
