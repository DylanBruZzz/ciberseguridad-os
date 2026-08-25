using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Aplicacion.Roadmap.Certificaciones.ListarCertificaciones;

public sealed class ListarCertificacionesCasoUso
{
    private readonly ICertificacionRepository _certificaciones;

    public ListarCertificacionesCasoUso(ICertificacionRepository certificaciones)
    {
        _certificaciones = certificaciones;
    }

    public async Task<ListarCertificacionesResultado> EjecutarAsync(CancellationToken cancellationToken = default)
    {
        var certificaciones = await _certificaciones.ListarAsync(cancellationToken);

        return new ListarCertificacionesResultado(
            certificaciones
                .Select(c => new CertificacionResumen(
                    c.Id,
                    c.Nombre,
                    c.Proveedor,
                    c.TipoCosto,
                    c.Url))
                .ToArray());
    }
}
