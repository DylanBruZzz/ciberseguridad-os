using Aprendizaje.Dominio.Evidence.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.Writeups.ObtenerWriteupPorId;

public sealed class ObtenerWriteupPorIdCasoUso
{
    private readonly IWriteupRepository _writeups;

    public ObtenerWriteupPorIdCasoUso(IWriteupRepository writeups)
    {
        _writeups = writeups;
    }

    public async Task<ObtenerWriteupPorIdResultado> EjecutarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El id debe ser un Guid válido.", nameof(id));

        var writeup = await _writeups.ObtenerPorIdAsync(id, cancellationToken);

        if (writeup is null)
            return ObtenerWriteupPorIdResultado.NoEncontrado();

        return ObtenerWriteupPorIdResultado.EncontradoCon(new WriteupDetalle(
            writeup.Id,
            writeup.UsuarioId,
            writeup.Titulo,
            writeup.PlataformaOrigen,
            writeup.Url,
            writeup.EstadoMadurez,
            writeup.Fecha));
    }
}
