using Aprendizaje.Dominio.Study.Repositorios;

namespace Aprendizaje.Aplicacion.Study.EntradasBitacora.ObtenerEntradaBitacoraPorId;

public sealed class ObtenerEntradaBitacoraPorIdCasoUso
{
    private readonly IEntradaBitacoraRepository _entradas;

    public ObtenerEntradaBitacoraPorIdCasoUso(IEntradaBitacoraRepository entradas)
    {
        _entradas = entradas;
    }

    public async Task<ObtenerEntradaBitacoraPorIdResultado> EjecutarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El id debe ser un Guid válido.", nameof(id));

        var entrada = await _entradas.ObtenerPorIdAsync(id, cancellationToken);

        return entrada is null
            ? ObtenerEntradaBitacoraPorIdResultado.NoEncontrada()
            : ObtenerEntradaBitacoraPorIdResultado.EncontradaCon(EntradaBitacoraDetalle.DesdeDominio(entrada));
    }
}
