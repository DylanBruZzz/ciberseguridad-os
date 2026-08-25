using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Evidence.Repositorios;
using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.Laboratorios.VincularLaboratorioATema;

public sealed class VincularLaboratorioATemaCasoUso
{
    private readonly ILaboratorioRepository _laboratorios;
    private readonly ITemaRepository _temas;
    private readonly IUnitOfWork _unitOfWork;

    public VincularLaboratorioATemaCasoUso(
        ILaboratorioRepository laboratorios,
        ITemaRepository temas,
        IUnitOfWork unitOfWork)
    {
        _laboratorios = laboratorios;
        _temas = temas;
        _unitOfWork = unitOfWork;
    }

    public async Task<VincularLaboratorioATemaResultado> EjecutarAsync(
        VincularLaboratorioATemaSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.LaboratorioId == Guid.Empty)
            throw new ArgumentException("El laboratorioId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.TemaId == Guid.Empty)
            throw new ArgumentException("El temaId debe ser un Guid válido.", nameof(solicitud));

        var laboratorio = await _laboratorios.ObtenerPorIdAsync(solicitud.LaboratorioId, cancellationToken);

        if (laboratorio is null)
            return VincularLaboratorioATemaResultado.LaboratorioNoEncontrado();

        var tema = await _temas.ObtenerPorIdAsync(solicitud.TemaId, cancellationToken);

        if (tema is null)
            return VincularLaboratorioATemaResultado.TemaNoEncontrado();

        if (laboratorio.UsuarioId != tema.UsuarioId)
            return VincularLaboratorioATemaResultado.UsuarioNoCoincide();

        var yaExiste = await _laboratorios.ExisteVinculoTemaAsync(
            solicitud.LaboratorioId,
            solicitud.TemaId,
            cancellationToken);

        if (!yaExiste)
        {
            _laboratorios.VincularTema(solicitud.LaboratorioId, solicitud.TemaId);
            await _unitOfWork.GuardarCambiosAsync(cancellationToken);
        }

        return VincularLaboratorioATemaResultado.Actualizado();
    }
}
