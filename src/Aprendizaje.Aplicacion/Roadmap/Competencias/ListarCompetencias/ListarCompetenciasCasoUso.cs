using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Aplicacion.Roadmap.Competencias.ListarCompetencias;

public sealed class ListarCompetenciasCasoUso
{
    private readonly ICompetenciaRepository _competencias;

    public ListarCompetenciasCasoUso(ICompetenciaRepository competencias)
    {
        _competencias = competencias;
    }

    public async Task<ListarCompetenciasResultado> EjecutarAsync(
        ListarCompetenciasSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        var competencias = await _competencias.ListarPorUsuarioAsync(solicitud.UsuarioId, cancellationToken);

        return new ListarCompetenciasResultado(
            competencias
                .Select(c => new CompetenciaResumen(
                    c.Id,
                    c.UsuarioId,
                    c.Nombre,
                    c.Descripcion))
                .ToArray());
    }
}
