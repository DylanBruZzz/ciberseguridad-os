namespace Aprendizaje.Aplicacion.Roadmap.Vistas.RoadmapVistaV1;

public sealed class ObtenerRoadmapVistaV1CasoUso
{
    private readonly IConsultaRoadmapVistaV1 _consulta;

    public ObtenerRoadmapVistaV1CasoUso(IConsultaRoadmapVistaV1 consulta)
    {
        _consulta = consulta;
    }

    public async Task<RoadmapVistaV1Dto> EjecutarAsync(
        ObtenerRoadmapVistaV1Solicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        var ahoraUtc = solicitud.AhoraUtc?.ToUniversalTime() ?? DateTime.UtcNow;

        return await _consulta.ObtenerAsync(solicitud.UsuarioId, ahoraUtc, cancellationToken);
    }
}
