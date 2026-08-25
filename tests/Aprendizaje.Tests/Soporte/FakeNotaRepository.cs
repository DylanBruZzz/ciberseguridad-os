using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Evidence.Repositorios;

namespace Aprendizaje.Tests.Soporte;

internal sealed class FakeNotaRepository : INotaRepository
{
    private readonly Dictionary<Guid, Nota> _notas = [];

    public int AgregarLlamadas { get; private set; }
    public Nota? UltimaNotaAgregada { get; private set; }

    public Task<Nota?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_notas.GetValueOrDefault(id));

    public Task<IReadOnlyCollection<Nota>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<Nota>>(
            _notas.Values.Where(n => n.UsuarioId == usuarioId).ToArray());

    public void Agregar(Nota nota)
    {
        AgregarLlamadas++;
        UltimaNotaAgregada = nota;
        _notas[nota.Id] = nota;
    }
}
