using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Aplicacion.Roadmap.Temas.AsignarTemaPadre;

public sealed class AsignarTemaPadreCasoUso
{
    private readonly ITemaRepository _temas;
    private readonly IUnitOfWork _unitOfWork;

    public AsignarTemaPadreCasoUso(ITemaRepository temas, IUnitOfWork unitOfWork)
    {
        _temas = temas;
        _unitOfWork = unitOfWork;
    }

    public async Task<AsignarTemaPadreResultado> EjecutarAsync(
        AsignarTemaPadreSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.TemaId == Guid.Empty)
            throw new ArgumentException("El temaId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.TemaPadreId == Guid.Empty)
            throw new ArgumentException("El temaPadreId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.TemaId == solicitud.TemaPadreId)
            throw new ArgumentException("Un Tema no puede ser subtema de sí mismo.", nameof(solicitud));

        var tema = await _temas.ObtenerPorIdAsync(solicitud.TemaId, cancellationToken);

        if (tema is null)
            return AsignarTemaPadreResultado.TemaNoEncontrado();

        var padre = await _temas.ObtenerPorIdAsync(solicitud.TemaPadreId, cancellationToken);

        if (padre is null)
            return AsignarTemaPadreResultado.TemaPadreNoEncontrado();

        if (tema.UsuarioId != padre.UsuarioId)
            return AsignarTemaPadreResultado.UsuarioNoCoincide();

        if (padre.TemaPadreId == tema.Id)
            return AsignarTemaPadreResultado.ConflictoJerarquia();

        tema.AsignarTemaPadre(padre.Id);

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return AsignarTemaPadreResultado.Actualizado();
    }
}
