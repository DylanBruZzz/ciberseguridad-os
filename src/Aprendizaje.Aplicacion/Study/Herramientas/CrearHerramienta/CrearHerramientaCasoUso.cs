using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Study;
using Aprendizaje.Dominio.Study.Repositorios;

namespace Aprendizaje.Aplicacion.Study.Herramientas.CrearHerramienta;

public sealed class CrearHerramientaCasoUso
{
    private readonly IHerramientaRepository _herramientas;
    private readonly IUnitOfWork _unitOfWork;

    public CrearHerramientaCasoUso(IHerramientaRepository herramientas, IUnitOfWork unitOfWork)
    {
        _herramientas = herramientas;
        _unitOfWork = unitOfWork;
    }

    public async Task<CrearHerramientaResultado> EjecutarAsync(
        CrearHerramientaSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var herramienta = Herramienta.Crear(solicitud.Nombre);
        herramienta.ActualizarCategoria(solicitud.Categoria);

        _herramientas.Agregar(herramienta);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return CrearHerramientaResultado.DesdeDominio(herramienta);
    }
}
