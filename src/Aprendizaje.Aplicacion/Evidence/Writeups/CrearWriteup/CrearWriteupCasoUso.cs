using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Evidence.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.Writeups.CrearWriteup;

public sealed class CrearWriteupCasoUso
{
    private readonly IWriteupRepository _writeups;
    private readonly IUnitOfWork _unitOfWork;

    public CrearWriteupCasoUso(IWriteupRepository writeups, IUnitOfWork unitOfWork)
    {
        _writeups = writeups;
        _unitOfWork = unitOfWork;
    }

    public async Task<CrearWriteupResultado> EjecutarAsync(
        CrearWriteupSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var writeup = Writeup.Crear(solicitud.UsuarioId, solicitud.Titulo);

        _writeups.Agregar(writeup);

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return new CrearWriteupResultado(
            writeup.Id,
            writeup.UsuarioId,
            writeup.Titulo,
            writeup.PlataformaOrigen,
            writeup.Url,
            writeup.EstadoMadurez,
            writeup.Fecha);
    }
}
