using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Aplicacion.Roadmap.Certificaciones.ObtenerCertificacionPorId;

public sealed class ObtenerCertificacionPorIdCasoUso
{
    private readonly ICertificacionRepository _certificaciones;

    public ObtenerCertificacionPorIdCasoUso(ICertificacionRepository certificaciones)
    {
        _certificaciones = certificaciones;
    }

    public async Task<ObtenerCertificacionPorIdResultado> EjecutarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El id debe ser un Guid válido.", nameof(id));

        var certificacion = await _certificaciones.ObtenerPorIdAsync(id, cancellationToken);

        if (certificacion is null)
            return ObtenerCertificacionPorIdResultado.NoEncontrada();

        return ObtenerCertificacionPorIdResultado.EncontradaCon(new CertificacionDetalle(
            certificacion.Id,
            certificacion.Nombre,
            certificacion.Proveedor,
            certificacion.TipoCosto,
            certificacion.Url));
    }
}
