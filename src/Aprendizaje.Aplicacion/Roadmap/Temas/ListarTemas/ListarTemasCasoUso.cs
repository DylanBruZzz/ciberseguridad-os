using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Aplicacion.Roadmap.Temas.ListarTemas;

public sealed class ListarTemasCasoUso
{
    private readonly ITemaRepository _temas;

    public ListarTemasCasoUso(ITemaRepository temas)
    {
        _temas = temas;
    }

    public async Task<ListarTemasResultado> EjecutarAsync(
        ListarTemasSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        var temas = await _temas.ListarPorUsuarioAsync(solicitud.UsuarioId, cancellationToken);

        var resumenes = temas
            .Select(t => new TemaResumen(
                t.Id,
                t.UsuarioId,
                t.FaseId,
                t.TemaPadreId,
                t.Nombre,
                t.TipoConocimiento,
                t.Descripcion))
            .ToArray();

        return new ListarTemasResultado(resumenes);
    }
}
