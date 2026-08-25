using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Evidence.CertificacionesObtenidas.CrearCertificacionObtenida;

public sealed record CrearCertificacionObtenidaResultado(
    CrearCertificacionObtenidaEstado Estado,
    Guid? Id = null,
    Guid? UsuarioId = null,
    Guid? CertificacionId = null,
    DateOnly? FechaObtencion = null,
    string? EvidenciaUrl = null,
    EstadoMadurez? EstadoMadurez = null)
{
    public static CrearCertificacionObtenidaResultado CertificacionNoEncontrada() =>
        new(CrearCertificacionObtenidaEstado.CertificacionNoEncontrada);

    public static CrearCertificacionObtenidaResultado Creada(CertificacionObtenida certificacionObtenida) =>
        new(
            CrearCertificacionObtenidaEstado.Creada,
            certificacionObtenida.Id,
            certificacionObtenida.UsuarioId,
            certificacionObtenida.CertificacionId,
            certificacionObtenida.FechaObtencion,
            certificacionObtenida.EvidenciaUrl,
            certificacionObtenida.EstadoMadurez);
}

public enum CrearCertificacionObtenidaEstado
{
    Creada,
    CertificacionNoEncontrada
}
