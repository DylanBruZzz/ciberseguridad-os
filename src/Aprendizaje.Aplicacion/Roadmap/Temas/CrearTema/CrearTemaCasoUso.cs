using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Aplicacion.Roadmap.Temas.CrearTema;

public sealed class CrearTemaCasoUso
{
    private readonly ITemaRepository _temas;
    private readonly IUnitOfWork _unitOfWork;

    public CrearTemaCasoUso(ITemaRepository temas, IUnitOfWork unitOfWork)
    {
        _temas = temas;
        _unitOfWork = unitOfWork;
    }

    public async Task<CrearTemaResultado> EjecutarAsync(
        CrearTemaSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var tema = Tema.Crear(solicitud.UsuarioId, solicitud.Nombre, solicitud.TipoConocimiento);

        _temas.Agregar(tema);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return new CrearTemaResultado(
            tema.Id,
            tema.UsuarioId,
            tema.Nombre,
            tema.TipoConocimiento);
    }
}
