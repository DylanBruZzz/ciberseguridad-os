using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Nucleo.Repositorios;

namespace Aprendizaje.Aplicacion.Portafolio;

public sealed class ObtenerPortafolioCasoUso
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IConsultaPortafolio _consulta;

    public ObtenerPortafolioCasoUso(IUsuarioRepository usuarios, IConsultaPortafolio consulta)
    {
        _usuarios = usuarios;
        _consulta = consulta;
    }

    public async Task<ObtenerPortafolioResultado> EjecutarAsync(
        ObtenerPortafolioSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.EstadoMadurez.HasValue && !EsMadurezElegible(solicitud.EstadoMadurez.Value))
            throw new ArgumentException(
                "El filtro de EstadoMadurez de Portafolio sólo admite ListoPortafolio o Publicado.",
                nameof(solicitud));

        var usuario = await _usuarios.ObtenerPorIdAsync(solicitud.UsuarioId, cancellationToken);

        if (usuario is null)
            return ObtenerPortafolioResultado.NoEncontrado();

        var portafolio = await _consulta.ObtenerAsync(solicitud, cancellationToken);

        return ObtenerPortafolioResultado.EncontradoCon(portafolio);
    }

    private static bool EsMadurezElegible(EstadoMadurez estadoMadurez) =>
        estadoMadurez is EstadoMadurez.ListoPortafolio or EstadoMadurez.Publicado;
}
