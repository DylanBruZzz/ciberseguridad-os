using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Aplicacion.Roadmap.Competencias.ObtenerCompetenciaPorId;

public sealed class ObtenerCompetenciaPorIdCasoUso
{
    private readonly ICompetenciaRepository _competencias;

    public ObtenerCompetenciaPorIdCasoUso(ICompetenciaRepository competencias)
    {
        _competencias = competencias;
    }

    public async Task<ObtenerCompetenciaPorIdResultado> EjecutarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El id debe ser un Guid válido.", nameof(id));

        var competencia = await _competencias.ObtenerPorIdAsync(id, cancellationToken);

        if (competencia is null)
            return ObtenerCompetenciaPorIdResultado.NoEncontrada();

        return ObtenerCompetenciaPorIdResultado.EncontradaCon(new CompetenciaDetalle(
            competencia.Id,
            competencia.UsuarioId,
            competencia.Nombre,
            competencia.Descripcion));
    }
}
