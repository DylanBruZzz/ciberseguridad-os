using Aprendizaje.Dominio.Evidence.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.Proyectos.ListarProyectos;

public sealed class ListarProyectosCasoUso
{
    private readonly IProyectoRepository _proyectos;

    public ListarProyectosCasoUso(IProyectoRepository proyectos)
    {
        _proyectos = proyectos;
    }

    public async Task<ListarProyectosResultado> EjecutarAsync(
        ListarProyectosSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        var proyectos = await _proyectos.ListarPorUsuarioAsync(solicitud.UsuarioId, cancellationToken);

        return new ListarProyectosResultado(proyectos
            .Select(p => new ProyectoResumen(
                p.Id,
                p.UsuarioId,
                p.Nombre,
                p.Estado,
                p.EstadoMadurez,
                p.FechaInicio,
                p.FechaFin))
            .ToArray());
    }
}
