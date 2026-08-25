using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Roadmap.Repositorios;
using Aprendizaje.Dominio.Roadmap.ValueObjects;

namespace Aprendizaje.Aplicacion.Roadmap.Temas.ConfigurarIntervaloRepaso;

public sealed class ConfigurarIntervaloRepasoTemaCasoUso
{
    private readonly ITemaRepository _temas;
    private readonly IUnitOfWork _unitOfWork;

    public ConfigurarIntervaloRepasoTemaCasoUso(ITemaRepository temas, IUnitOfWork unitOfWork)
    {
        _temas = temas;
        _unitOfWork = unitOfWork;
    }

    public async Task<ConfigurarIntervaloRepasoTemaResultado> EjecutarAsync(
        ConfigurarIntervaloRepasoTemaSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.TemaId == Guid.Empty)
            throw new ArgumentException("El temaId debe ser un Guid válido.", nameof(solicitud));

        var tema = await _temas.ObtenerPorIdAsync(solicitud.TemaId, cancellationToken);

        if (tema is null)
            return ConfigurarIntervaloRepasoTemaResultado.NoEncontrado();

        tema.ConfigurarIntervaloRepaso(
            solicitud.Dias.HasValue
                ? IntervaloRepaso.Crear(solicitud.Dias.Value)
                : null);

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return ConfigurarIntervaloRepasoTemaResultado.Actualizado();
    }
}
