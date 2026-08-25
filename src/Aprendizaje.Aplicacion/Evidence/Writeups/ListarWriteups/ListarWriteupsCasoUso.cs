using Aprendizaje.Dominio.Evidence.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.Writeups.ListarWriteups;

public sealed class ListarWriteupsCasoUso
{
    private readonly IWriteupRepository _writeups;

    public ListarWriteupsCasoUso(IWriteupRepository writeups)
    {
        _writeups = writeups;
    }

    public async Task<ListarWriteupsResultado> EjecutarAsync(
        ListarWriteupsSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        var writeups = await _writeups.ListarPorUsuarioAsync(solicitud.UsuarioId, cancellationToken);

        return new ListarWriteupsResultado(
            writeups
                .Select(w => new WriteupResumen(
                    w.Id,
                    w.UsuarioId,
                    w.Titulo,
                    w.PlataformaOrigen,
                    w.EstadoMadurez,
                    w.Fecha))
                .ToArray());
    }
}
