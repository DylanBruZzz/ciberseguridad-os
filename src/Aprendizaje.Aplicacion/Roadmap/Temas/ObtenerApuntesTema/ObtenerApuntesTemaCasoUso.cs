using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Aplicacion.Roadmap.Temas.ObtenerApuntesTema;

public sealed class ObtenerApuntesTemaCasoUso
{
    private readonly IApunteTemaRepository _apuntes;
    private readonly ITemaRepository _temas;

    public ObtenerApuntesTemaCasoUso(IApunteTemaRepository apuntes, ITemaRepository temas)
    {
        _apuntes = apuntes;
        _temas = temas;
    }

    public async Task<ObtenerApuntesTemaResultado> EjecutarAsync(
        ObtenerApuntesTemaSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.TemaId == Guid.Empty)
            throw new ArgumentException("El temaId debe ser un Guid válido.", nameof(solicitud));

        var tema = await _temas.ObtenerPorIdAsync(solicitud.TemaId, cancellationToken);

        if (tema is null)
            return ObtenerApuntesTemaResultado.TemaNoEncontrado();

        if (tema.UsuarioId != solicitud.UsuarioId)
            return ObtenerApuntesTemaResultado.UsuarioNoCoincide();

        var apunte = await _apuntes.ObtenerPorTemaIdAsync(solicitud.TemaId, cancellationToken);

        if (apunte is null)
            return ObtenerApuntesTemaResultado.Encontrado(new ApuntesTemaDto(
                solicitud.TemaId,
                string.Empty,
                null));

        if (apunte.UsuarioId != solicitud.UsuarioId)
            return ObtenerApuntesTemaResultado.UsuarioNoCoincide();

        return ObtenerApuntesTemaResultado.Encontrado(new ApuntesTemaDto(
            apunte.TemaId,
            apunte.Contenido,
            apunte.FechaModificacionUtc ?? apunte.FechaCreacionUtc));
    }
}
