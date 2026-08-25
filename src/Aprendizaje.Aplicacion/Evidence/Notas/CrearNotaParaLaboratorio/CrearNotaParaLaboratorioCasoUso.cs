using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Aplicacion.Evidence.Notas.Comun;
using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Evidence.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.Notas.CrearNotaParaLaboratorio;

public sealed class CrearNotaParaLaboratorioCasoUso
{
    private readonly INotaRepository _notas;
    private readonly ILaboratorioRepository _laboratorios;
    private readonly IUnitOfWork _unitOfWork;

    public CrearNotaParaLaboratorioCasoUso(
        INotaRepository notas,
        ILaboratorioRepository laboratorios,
        IUnitOfWork unitOfWork)
    {
        _notas = notas;
        _laboratorios = laboratorios;
        _unitOfWork = unitOfWork;
    }

    public async Task<CrearNotaResultado> EjecutarAsync(
        CrearNotaParaLaboratorioSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.LaboratorioId == Guid.Empty)
            throw new ArgumentException("El laboratorioId debe ser un Guid válido.", nameof(solicitud));

        var laboratorio = await _laboratorios.ObtenerPorIdAsync(solicitud.LaboratorioId, cancellationToken);

        if (laboratorio is null)
            return CrearNotaResultado.PadreNoEncontrado();

        if (laboratorio.UsuarioId != solicitud.UsuarioId)
            return CrearNotaResultado.UsuarioNoCoincide();

        var nota = Nota.SobreLaboratorio(solicitud.UsuarioId, solicitud.LaboratorioId, solicitud.Texto, solicitud.Tipo);

        _notas.Agregar(nota);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return CrearNotaResultado.Creada(nota);
    }
}
