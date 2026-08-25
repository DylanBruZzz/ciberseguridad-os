using Aprendizaje.Dominio.Study;
using Aprendizaje.Dominio.Study.Repositorios;

namespace Aprendizaje.Tests.Soporte;

internal sealed class FakeHerramientaRepository : IHerramientaRepository
{
    private readonly Dictionary<Guid, Herramienta> _herramientas = [];

    public int AgregarLlamadas { get; private set; }

    public Task<Herramienta?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_herramientas.GetValueOrDefault(id));

    public Task<IReadOnlyCollection<Herramienta>> ListarAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<Herramienta>>(
            _herramientas.Values
                .OrderBy(h => h.Nombre)
                .ThenBy(h => h.Id)
                .ToArray());

    public void Agregar(Herramienta herramienta)
    {
        AgregarLlamadas++;
        _herramientas[herramienta.Id] = herramienta;
    }
}
