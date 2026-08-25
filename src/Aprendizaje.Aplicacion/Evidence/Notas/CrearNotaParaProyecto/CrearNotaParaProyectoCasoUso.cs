using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Aplicacion.Evidence.Notas.Comun;
using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Evidence.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.Notas.CrearNotaParaProyecto;

public sealed class CrearNotaParaProyectoCasoUso
{
    private readonly INotaRepository _notas;
    private readonly IProyectoRepository _proyectos;
    private readonly IUnitOfWork _unitOfWork;

    public CrearNotaParaProyectoCasoUso(
        INotaRepository notas,
        IProyectoRepository proyectos,
        IUnitOfWork unitOfWork)
    {
        _notas = notas;
        _proyectos = proyectos;
        _unitOfWork = unitOfWork;
    }

    public async Task<CrearNotaResultado> EjecutarAsync(
        CrearNotaParaProyectoSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.ProyectoId == Guid.Empty)
            throw new ArgumentException("El proyectoId debe ser un Guid válido.", nameof(solicitud));

        var proyecto = await _proyectos.ObtenerPorIdAsync(solicitud.ProyectoId, cancellationToken);

        if (proyecto is null)
            return CrearNotaResultado.PadreNoEncontrado();

        if (proyecto.UsuarioId != solicitud.UsuarioId)
            return CrearNotaResultado.UsuarioNoCoincide();

        var nota = Nota.SobreProyecto(solicitud.UsuarioId, solicitud.ProyectoId, solicitud.Texto, solicitud.Tipo);

        _notas.Agregar(nota);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return CrearNotaResultado.Creada(nota);
    }
}
