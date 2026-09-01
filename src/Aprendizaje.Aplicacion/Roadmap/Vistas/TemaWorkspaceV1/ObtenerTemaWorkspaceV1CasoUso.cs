namespace Aprendizaje.Aplicacion.Roadmap.Vistas.TemaWorkspaceV1;

public sealed class ObtenerTemaWorkspaceV1CasoUso
{
    private readonly IConsultaTemaWorkspaceV1 _consulta;

    public ObtenerTemaWorkspaceV1CasoUso(IConsultaTemaWorkspaceV1 consulta)
    {
        _consulta = consulta;
    }

    public async Task<ObtenerTemaWorkspaceV1Resultado> EjecutarAsync(
        ObtenerTemaWorkspaceV1Solicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.TemaId == Guid.Empty)
            throw new ArgumentException("El temaId debe ser un Guid válido.", nameof(solicitud));

        var ahoraUtc = solicitud.AhoraUtc?.ToUniversalTime() ?? DateTime.UtcNow;
        var workspace = await _consulta.ObtenerAsync(
            solicitud.UsuarioId,
            solicitud.TemaId,
            ahoraUtc,
            cancellationToken);

        return workspace is null
            ? ObtenerTemaWorkspaceV1Resultado.NoEncontrado()
            : ObtenerTemaWorkspaceV1Resultado.EncontradoCon(workspace);
    }
}
