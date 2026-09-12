using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Tests.Soporte;

internal sealed class FakeTemaRepository : ITemaRepository
{
    private readonly Dictionary<Guid, Tema> _temas = [];
    private readonly HashSet<(Guid TemaId, Guid HerramientaId)> _vinculosHerramienta = [];

    public int VincularHerramientaLlamadas { get; private set; }
    public int DesvincularHerramientaLlamadas { get; private set; }

    public Task<Tema?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_temas.GetValueOrDefault(id));

    public Task<IReadOnlyCollection<Tema>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<Tema>>(_temas.Values.Where(t => t.UsuarioId == usuarioId).ToArray());

    public Task<IReadOnlyCollection<Tema>> ListarPorUsuarioParaImportacionAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<Tema>>(_temas.Values.Where(t => t.UsuarioId == usuarioId).ToArray());

    public Task<bool> ExisteVinculoHerramientaAsync(
        Guid temaId,
        Guid herramientaId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_vinculosHerramienta.Contains((temaId, herramientaId)));

    public void Agregar(Tema tema) => _temas[tema.Id] = tema;

    public void VincularHerramienta(Guid temaId, Guid herramientaId)
    {
        VincularHerramientaLlamadas++;
        _vinculosHerramienta.Add((temaId, herramientaId));
    }

    public Task<bool> DesvincularHerramientaAsync(
        Guid temaId,
        Guid herramientaId,
        CancellationToken cancellationToken = default)
    {
        DesvincularHerramientaLlamadas++;

        return Task.FromResult(_vinculosHerramienta.Remove((temaId, herramientaId)));
    }
}
