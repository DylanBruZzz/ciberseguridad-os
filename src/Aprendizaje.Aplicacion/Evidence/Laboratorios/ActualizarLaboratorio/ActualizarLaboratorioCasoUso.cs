using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Evidence.Repositorios;
using Aprendizaje.Dominio.Nucleo.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.Laboratorios.ActualizarLaboratorio;

public sealed class ActualizarLaboratorioCasoUso
{
    private readonly ILaboratorioRepository _laboratorios;
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarLaboratorioCasoUso(
        ILaboratorioRepository laboratorios,
        IUsuarioRepository usuarios,
        IUnitOfWork unitOfWork)
    {
        _laboratorios = laboratorios;
        _usuarios = usuarios;
        _unitOfWork = unitOfWork;
    }

    public async Task<ActualizarLaboratorioResultado> EjecutarAsync(
        ActualizarLaboratorioSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.LaboratorioId == Guid.Empty)
            throw new ArgumentException("El laboratorioId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        var usuario = await _usuarios.ObtenerPorIdAsync(solicitud.UsuarioId, cancellationToken);
        if (usuario is null)
            return ActualizarLaboratorioResultado.UsuarioNoEncontrado();

        var laboratorio = await _laboratorios.ObtenerPorIdAsync(solicitud.LaboratorioId, cancellationToken);
        if (laboratorio is null)
            return ActualizarLaboratorioResultado.LaboratorioNoEncontrado();

        if (laboratorio.UsuarioId != solicitud.UsuarioId)
            return ActualizarLaboratorioResultado.UsuarioNoCoincide();

        laboratorio.CambiarNombre(solicitud.Nombre);
        laboratorio.ActualizarObjetivo(solicitud.Objetivo);
        laboratorio.ActualizarEntornoVms(solicitud.EntornoVms);
        laboratorio.RegistrarHallazgos(solicitud.Hallazgos);
        laboratorio.ActualizarTiempoInvertido(solicitud.TiempoInvertidoMinutos);
        laboratorio.ActualizarFecha(solicitud.Fecha);
        laboratorio.AvanzarMadurez(solicitud.EstadoMadurez);

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return ActualizarLaboratorioResultado.Actualizado();
    }
}
