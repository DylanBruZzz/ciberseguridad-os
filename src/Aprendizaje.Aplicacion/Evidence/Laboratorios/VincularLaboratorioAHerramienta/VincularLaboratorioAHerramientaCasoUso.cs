using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Evidence.Repositorios;
using Aprendizaje.Dominio.Study.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.Laboratorios.VincularLaboratorioAHerramienta;

public sealed class VincularLaboratorioAHerramientaCasoUso
{
    private readonly ILaboratorioRepository _laboratorios;
    private readonly IHerramientaRepository _herramientas;
    private readonly IUnitOfWork _unitOfWork;

    public VincularLaboratorioAHerramientaCasoUso(
        ILaboratorioRepository laboratorios,
        IHerramientaRepository herramientas,
        IUnitOfWork unitOfWork)
    {
        _laboratorios = laboratorios;
        _herramientas = herramientas;
        _unitOfWork = unitOfWork;
    }

    public async Task<VincularLaboratorioAHerramientaResultado> EjecutarAsync(
        VincularLaboratorioAHerramientaSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.LaboratorioId == Guid.Empty)
            throw new ArgumentException("El laboratorioId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.HerramientaId == Guid.Empty)
            throw new ArgumentException("El herramientaId debe ser un Guid válido.", nameof(solicitud));

        var laboratorio = await _laboratorios.ObtenerPorIdAsync(solicitud.LaboratorioId, cancellationToken);

        if (laboratorio is null)
            return VincularLaboratorioAHerramientaResultado.LaboratorioNoEncontrado();

        var herramienta = await _herramientas.ObtenerPorIdAsync(solicitud.HerramientaId, cancellationToken);

        if (herramienta is null)
            return VincularLaboratorioAHerramientaResultado.HerramientaNoEncontrada();

        var yaExiste = await _laboratorios.ExisteVinculoHerramientaAsync(
            solicitud.LaboratorioId,
            solicitud.HerramientaId,
            cancellationToken);

        if (!yaExiste)
        {
            _laboratorios.VincularHerramienta(solicitud.LaboratorioId, solicitud.HerramientaId);
            await _unitOfWork.GuardarCambiosAsync(cancellationToken);
        }

        return VincularLaboratorioAHerramientaResultado.Actualizado();
    }
}
