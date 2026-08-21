using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Tests.Soporte;

internal sealed class FakeTemaRepository : ITemaRepository
{
    private readonly Dictionary<Guid, Tema> _temas = [];

    public Task<Tema?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_temas.GetValueOrDefault(id));

    public Task<IReadOnlyCollection<Tema>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<Tema>>(_temas.Values.Where(t => t.UsuarioId == usuarioId).ToArray());

    public void Agregar(Tema tema) => _temas[tema.Id] = tema;
}
