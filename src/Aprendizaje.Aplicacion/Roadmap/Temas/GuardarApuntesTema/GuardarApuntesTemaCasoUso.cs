using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Aplicacion.Roadmap.Temas.GuardarApuntesTema;

public sealed class GuardarApuntesTemaCasoUso
{
    private readonly IApunteTemaRepository _apuntes;
    private readonly ITemaRepository _temas;
    private readonly IUnitOfWork _unitOfWork;

    public GuardarApuntesTemaCasoUso(
        IApunteTemaRepository apuntes,
        ITemaRepository temas,
        IUnitOfWork unitOfWork)
    {
        _apuntes = apuntes;
        _temas = temas;
        _unitOfWork = unitOfWork;
    }

    public async Task<GuardarApuntesTemaResultado> EjecutarAsync(
        GuardarApuntesTemaSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.TemaId == Guid.Empty)
            throw new ArgumentException("El temaId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.Contenido is null)
            throw new ArgumentException("El contenido de los apuntes no puede ser null.", nameof(solicitud));

        var tema = await _temas.ObtenerPorIdAsync(solicitud.TemaId, cancellationToken);

        if (tema is null)
            return GuardarApuntesTemaResultado.TemaNoEncontrado();

        if (tema.UsuarioId != solicitud.UsuarioId)
            return GuardarApuntesTemaResultado.UsuarioNoCoincide();

        var apunte = await _apuntes.ObtenerPorTemaIdAsync(solicitud.TemaId, cancellationToken);

        if (apunte is null)
        {
            _apuntes.Agregar(ApunteTema.Crear(solicitud.UsuarioId, tema.Id, solicitud.Contenido));
        }
        else
        {
            if (apunte.UsuarioId != solicitud.UsuarioId)
                return GuardarApuntesTemaResultado.UsuarioNoCoincide();

            apunte.ReemplazarContenido(solicitud.Contenido);
        }

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return GuardarApuntesTemaResultado.Guardado();
    }
}
