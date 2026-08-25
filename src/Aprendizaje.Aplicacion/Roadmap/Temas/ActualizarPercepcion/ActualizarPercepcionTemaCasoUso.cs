using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Roadmap.Repositorios;
using Aprendizaje.Dominio.Roadmap.ValueObjects;

namespace Aprendizaje.Aplicacion.Roadmap.Temas.ActualizarPercepcion;

public sealed class ActualizarPercepcionTemaCasoUso
{
    private readonly ITemaRepository _temas;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarPercepcionTemaCasoUso(ITemaRepository temas, IUnitOfWork unitOfWork)
    {
        _temas = temas;
        _unitOfWork = unitOfWork;
    }

    public async Task<ActualizarPercepcionTemaResultado> EjecutarAsync(
        ActualizarPercepcionTemaSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.TemaId == Guid.Empty)
            throw new ArgumentException("El temaId debe ser un Guid válido.", nameof(solicitud));

        var tema = await _temas.ObtenerPorIdAsync(solicitud.TemaId, cancellationToken);

        if (tema is null)
            return ActualizarPercepcionTemaResultado.NoEncontrado();

        tema.ActualizarDificultadPercibida(
            solicitud.DificultadPercibida.HasValue
                ? NivelPercepcion.Crear(solicitud.DificultadPercibida.Value)
                : null);
        tema.ActualizarConfianza(
            solicitud.Confianza.HasValue
                ? NivelPercepcion.Crear(solicitud.Confianza.Value)
                : null);

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return ActualizarPercepcionTemaResultado.Actualizado();
    }
}
