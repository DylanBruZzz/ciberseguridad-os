using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Tests.Soporte;

internal sealed class FakeFaseRepository : IFaseRepository
{
    private readonly Dictionary<Guid, Fase> _fases = [];

    public int AgregarLlamadas { get; private set; }

    public Task<Fase?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_fases.GetValueOrDefault(id));

    public Task<IReadOnlyCollection<Fase>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<Fase>>(_fases.Values.Where(f => f.UsuarioId == usuarioId).ToArray());

    public void Agregar(Fase fase)
    {
        AgregarLlamadas++;
        _fases[fase.Id] = fase;
    }
}
