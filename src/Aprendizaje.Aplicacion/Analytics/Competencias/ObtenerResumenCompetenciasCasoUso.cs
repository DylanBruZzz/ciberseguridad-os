namespace Aprendizaje.Aplicacion.Analytics.Competencias;

public sealed class ObtenerResumenCompetenciasCasoUso
{
    private readonly IConsultaResumenCompetencias _consulta;

    public ObtenerResumenCompetenciasCasoUso(IConsultaResumenCompetencias consulta)
    {
        _consulta = consulta;
    }

    public Task<IReadOnlyCollection<ResumenCompetenciaDto>> EjecutarAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(usuarioId));

        return _consulta.ListarAsync(usuarioId, cancellationToken);
    }
}
