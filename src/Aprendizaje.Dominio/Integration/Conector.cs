using Aprendizaje.Dominio.Comun;

namespace Aprendizaje.Dominio.Integration;

public sealed class Conector : AggregateRoot
{
    private readonly List<LogSincronizacion> _logs = new();

    public Guid UsuarioId { get; private set; }
    public Plataforma Plataforma { get; private set; }
    public EstadoConexion EstadoConexion { get; private set; }
    public DateTime? UltimaSincronizacion { get; private set; }
    public string? CredencialRef { get; private set; }

    public IReadOnlyCollection<LogSincronizacion> Logs => _logs.AsReadOnly();

    private Conector() { } // requerido por EF Core

    private Conector(Guid id, Guid usuarioId, Plataforma plataforma) : base(id)
    {
        UsuarioId = usuarioId;
        Plataforma = plataforma;
        EstadoConexion = EstadoConexion.Desconectado;
    }

    public static Conector Crear(Guid usuarioId, Plataforma plataforma)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("El Conector debe pertenecer a un usuario válido.", nameof(usuarioId));

        return new Conector(Guid.CreateVersion7(), usuarioId, plataforma);
    }

    public void ActualizarCredencialRef(string? credencialRef) => CredencialRef = credencialRef;

    public void MarcarConectado()
    {
        EstadoConexion = EstadoConexion.Conectado;
        UltimaSincronizacion = DateTime.UtcNow;
    }

    public void MarcarDesconectado() => EstadoConexion = EstadoConexion.Desconectado;

    public void MarcarError() => EstadoConexion = EstadoConexion.Error;

    public void RegistrarLog(ResultadoSincronizacion resultado, string? resumen = null)
    {
        _logs.Add(new LogSincronizacion(Id, resultado, resumen));
    }
}
