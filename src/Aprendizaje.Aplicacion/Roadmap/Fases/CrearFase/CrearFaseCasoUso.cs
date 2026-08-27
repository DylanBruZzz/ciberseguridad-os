using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Aplicacion.Roadmap.Fases.CrearFase;

public sealed class CrearFaseCasoUso
{
    private readonly IFaseRepository _fases;
    private readonly IUnitOfWork _unitOfWork;

    public CrearFaseCasoUso(IFaseRepository fases, IUnitOfWork unitOfWork)
    {
        _fases = fases;
        _unitOfWork = unitOfWork;
    }

    public async Task<CrearFaseResultado> EjecutarAsync(
        CrearFaseSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var fase = Fase.Crear(solicitud.UsuarioId, solicitud.Nombre, solicitud.Orden);
        fase.ConfigurarMetadataPedagogica(
            solicitud.Objetivos ?? [],
            solicitud.CriteriosAvance ?? [],
            solicitud.MesInicioRecomendado,
            solicitud.MesFinRecomendado,
            solicitud.CargaSemanalRecomendada);

        _fases.Agregar(fase);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return new CrearFaseResultado(
            fase.Id,
            fase.UsuarioId,
            fase.Nombre,
            fase.Orden,
            fase.Objetivos,
            fase.CriteriosAvance,
            fase.MesInicioRecomendado,
            fase.MesFinRecomendado,
            fase.CargaSemanalRecomendada);
    }
}
