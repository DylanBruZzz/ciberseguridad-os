using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Aplicacion.Evidence.Notas.Comun;
using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Evidence.Repositorios;
using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.Notas.CrearNotaParaTema;

public sealed class CrearNotaParaTemaCasoUso
{
    private readonly INotaRepository _notas;
    private readonly ITemaRepository _temas;
    private readonly IUnitOfWork _unitOfWork;

    public CrearNotaParaTemaCasoUso(
        INotaRepository notas,
        ITemaRepository temas,
        IUnitOfWork unitOfWork)
    {
        _notas = notas;
        _temas = temas;
        _unitOfWork = unitOfWork;
    }

    public async Task<CrearNotaResultado> EjecutarAsync(
        CrearNotaParaTemaSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.TemaId == Guid.Empty)
            throw new ArgumentException("El temaId debe ser un Guid válido.", nameof(solicitud));

        var tema = await _temas.ObtenerPorIdAsync(solicitud.TemaId, cancellationToken);

        if (tema is null)
            return CrearNotaResultado.PadreNoEncontrado();

        if (tema.UsuarioId != solicitud.UsuarioId)
            return CrearNotaResultado.UsuarioNoCoincide();

        var nota = Nota.SobreTema(solicitud.UsuarioId, solicitud.TemaId, solicitud.Texto, solicitud.Tipo);

        _notas.Agregar(nota);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return CrearNotaResultado.Creada(nota);
    }
}
