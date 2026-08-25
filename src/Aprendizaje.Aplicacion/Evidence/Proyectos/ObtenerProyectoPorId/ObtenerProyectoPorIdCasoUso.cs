using Aprendizaje.Dominio.Evidence.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.Proyectos.ObtenerProyectoPorId;

public sealed class ObtenerProyectoPorIdCasoUso
{
    private readonly IProyectoRepository _proyectos;

    public ObtenerProyectoPorIdCasoUso(IProyectoRepository proyectos)
    {
        _proyectos = proyectos;
    }

    public async Task<ObtenerProyectoPorIdResultado> EjecutarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El id debe ser un Guid válido.", nameof(id));

        var proyecto = await _proyectos.ObtenerPorIdAsync(id, cancellationToken);

        if (proyecto is null)
            return ObtenerProyectoPorIdResultado.NoEncontrado();

        return ObtenerProyectoPorIdResultado.EncontradoCon(new ProyectoDetalle(
            proyecto.Id,
            proyecto.UsuarioId,
            proyecto.Nombre,
            proyecto.Descripcion,
            proyecto.Estado,
            proyecto.EstadoMadurez,
            proyecto.RepositorioUrl,
            proyecto.FechaInicio,
            proyecto.FechaFin));
    }
}
