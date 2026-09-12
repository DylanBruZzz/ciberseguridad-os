using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Roadmap.Repositorios;
using Aprendizaje.Dominio.Study.Repositorios;

namespace Aprendizaje.Aplicacion.Roadmap.Temas.DesvincularHerramientaDeTema;

public sealed class DesvincularHerramientaDeTemaCasoUso
{
    private readonly ITemaRepository _temas;
    private readonly IHerramientaRepository _herramientas;
    private readonly IUnitOfWork _unitOfWork;

    public DesvincularHerramientaDeTemaCasoUso(
        ITemaRepository temas,
        IHerramientaRepository herramientas,
        IUnitOfWork unitOfWork)
    {
        _temas = temas;
        _herramientas = herramientas;
        _unitOfWork = unitOfWork;
    }

    public async Task<DesvincularHerramientaDeTemaResultado> EjecutarAsync(
        DesvincularHerramientaDeTemaSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.TemaId == Guid.Empty)
            throw new ArgumentException("El temaId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.HerramientaId == Guid.Empty)
            throw new ArgumentException("El herramientaId debe ser un Guid válido.", nameof(solicitud));

        var tema = await _temas.ObtenerPorIdAsync(solicitud.TemaId, cancellationToken);

        if (tema is null)
            return DesvincularHerramientaDeTemaResultado.TemaNoEncontrado();

        if (tema.UsuarioId != solicitud.UsuarioId)
            return DesvincularHerramientaDeTemaResultado.UsuarioNoCoincide();

        var herramienta = await _herramientas.ObtenerPorIdAsync(solicitud.HerramientaId, cancellationToken);

        if (herramienta is null)
            return DesvincularHerramientaDeTemaResultado.HerramientaNoEncontrada();

        var eliminado = await _temas.DesvincularHerramientaAsync(
            solicitud.TemaId,
            solicitud.HerramientaId,
            cancellationToken);

        if (eliminado)
            await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return DesvincularHerramientaDeTemaResultado.Actualizado();
    }
}
