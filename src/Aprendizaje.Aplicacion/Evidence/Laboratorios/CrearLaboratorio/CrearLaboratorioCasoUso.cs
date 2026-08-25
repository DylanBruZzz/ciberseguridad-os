using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Evidence.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.Laboratorios.CrearLaboratorio;

public sealed class CrearLaboratorioCasoUso
{
    private readonly ILaboratorioRepository _laboratorios;
    private readonly IUnitOfWork _unitOfWork;

    public CrearLaboratorioCasoUso(ILaboratorioRepository laboratorios, IUnitOfWork unitOfWork)
    {
        _laboratorios = laboratorios;
        _unitOfWork = unitOfWork;
    }

    public async Task<CrearLaboratorioResultado> EjecutarAsync(
        CrearLaboratorioSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var laboratorio = Laboratorio.Crear(solicitud.UsuarioId, solicitud.Nombre);
        laboratorio.ActualizarObjetivo(solicitud.Objetivo);
        laboratorio.ActualizarEntornoVms(solicitud.EntornoVms);
        laboratorio.RegistrarHallazgos(solicitud.Hallazgos);
        laboratorio.ActualizarTiempoInvertido(solicitud.TiempoInvertidoMinutos);

        if (solicitud.Fecha is not null)
            laboratorio.RegistrarFecha(solicitud.Fecha.Value);

        _laboratorios.Agregar(laboratorio);

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return new CrearLaboratorioResultado(
            laboratorio.Id,
            laboratorio.UsuarioId,
            laboratorio.Nombre,
            laboratorio.Objetivo,
            laboratorio.EntornoVms,
            laboratorio.Hallazgos,
            laboratorio.TiempoInvertidoMinutos,
            laboratorio.EstadoMadurez,
            laboratorio.Fecha);
    }
}
