using Aprendizaje.Dominio.Resource;
using Aprendizaje.Dominio.Resource.Repositorios;

namespace Aprendizaje.Tests.Soporte;

internal sealed class FakeRecursoRepository : IRecursoRepository
{
    private readonly Dictionary<Guid, Recurso> _recursos = [];
    private readonly HashSet<(Guid RecursoId, Guid TemaId)> _vinculosTema = [];

    public int VincularTemaLlamadas { get; private set; }

    public Task<Recurso?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_recursos.GetValueOrDefault(id));

    public Task<IReadOnlyCollection<Recurso>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<Recurso>>(_recursos.Values.Where(r => r.UsuarioId == usuarioId).ToArray());

    public Task<IReadOnlyCollection<Recurso>> ListarPorUsuarioParaImportacionAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<Recurso>>(_recursos.Values.Where(r => r.UsuarioId == usuarioId).ToArray());

    public Task<bool> ExisteVinculoTemaAsync(
        Guid recursoId,
        Guid temaId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_vinculosTema.Contains((recursoId, temaId)));

    public void Agregar(Recurso recurso) => _recursos[recurso.Id] = recurso;

    public void VincularTema(Guid recursoId, Guid temaId)
    {
        VincularTemaLlamadas++;
        _vinculosTema.Add((recursoId, temaId));
    }
}
