using Aprendizaje.Dominio.Evidence.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.CertificacionesObtenidas.ListarCertificacionesObtenidas;

public sealed class ListarCertificacionesObtenidasCasoUso
{
    private readonly ICertificacionObtenidaRepository _certificacionesObtenidas;

    public ListarCertificacionesObtenidasCasoUso(ICertificacionObtenidaRepository certificacionesObtenidas)
    {
        _certificacionesObtenidas = certificacionesObtenidas;
    }

    public async Task<ListarCertificacionesObtenidasResultado> EjecutarAsync(
        ListarCertificacionesObtenidasSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        var certificacionesObtenidas = await _certificacionesObtenidas.ListarPorUsuarioAsync(
            solicitud.UsuarioId,
            cancellationToken);

        return new ListarCertificacionesObtenidasResultado(
            certificacionesObtenidas
                .Select(c => new CertificacionObtenidaResumen(
                    c.Id,
                    c.UsuarioId,
                    c.CertificacionId,
                    c.FechaObtencion,
                    c.EvidenciaUrl,
                    c.EstadoMadurez))
                .ToArray());
    }
}
