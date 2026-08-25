using Aprendizaje.Dominio.Study.Repositorios;

namespace Aprendizaje.Aplicacion.Study.EntradasBitacora.ListarEntradasBitacora;

public sealed class ListarEntradasBitacoraCasoUso
{
    private readonly IEntradaBitacoraRepository _entradas;

    public ListarEntradasBitacoraCasoUso(IEntradaBitacoraRepository entradas)
    {
        _entradas = entradas;
    }

    public async Task<ListarEntradasBitacoraResultado> EjecutarAsync(
        ListarEntradasBitacoraSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        var entradas = await _entradas.ListarPorUsuarioAsync(solicitud.UsuarioId, cancellationToken);

        return new ListarEntradasBitacoraResultado(
            entradas.Select(EntradaBitacoraResumen.DesdeDominio).ToArray());
    }
}
