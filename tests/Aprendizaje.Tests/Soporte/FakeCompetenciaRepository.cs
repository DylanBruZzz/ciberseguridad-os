using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Tests.Soporte;

internal sealed class FakeCompetenciaRepository : ICompetenciaRepository
{
    private readonly Dictionary<Guid, Competencia> _competencias = [];
    private readonly HashSet<(Guid CompetenciaId, Guid TemaId)> _vinculosTema = [];

    public int AgregarLlamadas { get; private set; }
    public int VincularTemaLlamadas { get; private set; }

    public Task<Competencia?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_competencias.GetValueOrDefault(id));

    public Task<IReadOnlyCollection<Competencia>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<Competencia>>(
            _competencias.Values.Where(c => c.UsuarioId == usuarioId).ToArray());

    public Task<bool> ExisteVinculoTemaAsync(
        Guid competenciaId,
        Guid temaId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_vinculosTema.Contains((competenciaId, temaId)));

    public void Agregar(Competencia competencia)
    {
        AgregarLlamadas++;
        _competencias[competencia.Id] = competencia;
    }

    public void VincularTema(Guid competenciaId, Guid temaId)
    {
        VincularTemaLlamadas++;
        _vinculosTema.Add((competenciaId, temaId));
    }
}
