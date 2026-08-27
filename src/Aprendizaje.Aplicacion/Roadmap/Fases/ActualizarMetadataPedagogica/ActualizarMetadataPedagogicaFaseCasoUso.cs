using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Aplicacion.Roadmap.Fases.ActualizarMetadataPedagogica;

public sealed class ActualizarMetadataPedagogicaFaseCasoUso
{
    private readonly IFaseRepository _fases;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarMetadataPedagogicaFaseCasoUso(IFaseRepository fases, IUnitOfWork unitOfWork)
    {
        _fases = fases;
        _unitOfWork = unitOfWork;
    }

    public async Task<ActualizarMetadataPedagogicaFaseResultado> EjecutarAsync(
        ActualizarMetadataPedagogicaFaseSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.FaseId == Guid.Empty)
            throw new ArgumentException("La faseId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        var fase = await _fases.ObtenerPorIdAsync(solicitud.FaseId, cancellationToken);

        if (fase is null)
            return ActualizarMetadataPedagogicaFaseResultado.FaseNoEncontrada();

        if (fase.UsuarioId != solicitud.UsuarioId)
            return ActualizarMetadataPedagogicaFaseResultado.UsuarioNoCoincide();

        fase.ConfigurarMetadataPedagogica(
            solicitud.Objetivos,
            solicitud.CriteriosAvance,
            solicitud.MesInicioRecomendado,
            solicitud.MesFinRecomendado,
            solicitud.CargaSemanalRecomendada);

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return ActualizarMetadataPedagogicaFaseResultado.Actualizada();
    }
}
