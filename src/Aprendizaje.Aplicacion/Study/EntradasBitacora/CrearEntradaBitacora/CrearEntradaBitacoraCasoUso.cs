using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Roadmap.Repositorios;
using Aprendizaje.Dominio.Study;
using Aprendizaje.Dominio.Study.Repositorios;

namespace Aprendizaje.Aplicacion.Study.EntradasBitacora.CrearEntradaBitacora;

public sealed class CrearEntradaBitacoraCasoUso
{
    private readonly IEntradaBitacoraRepository _entradas;
    private readonly ITemaRepository _temas;
    private readonly IUnitOfWork _unitOfWork;

    public CrearEntradaBitacoraCasoUso(
        IEntradaBitacoraRepository entradas,
        ITemaRepository temas,
        IUnitOfWork unitOfWork)
    {
        _entradas = entradas;
        _temas = temas;
        _unitOfWork = unitOfWork;
    }

    public async Task<CrearEntradaBitacoraResultado> EjecutarAsync(
        CrearEntradaBitacoraSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.TemaId == Guid.Empty)
            throw new ArgumentException("El temaId debe ser null o un Guid válido.", nameof(solicitud));

        if (solicitud.TemaId.HasValue)
        {
            var tema = await _temas.ObtenerPorIdAsync(solicitud.TemaId.Value, cancellationToken);

            if (tema is null)
                return CrearEntradaBitacoraResultado.TemaNoEncontrado();

            if (tema.UsuarioId != solicitud.UsuarioId)
                return CrearEntradaBitacoraResultado.UsuarioNoCoincide();
        }

        var entrada = EntradaBitacora.Escribir(solicitud.UsuarioId, solicitud.Texto, solicitud.TemaId);

        _entradas.Agregar(entrada);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return CrearEntradaBitacoraResultado.Creada(entrada);
    }
}
