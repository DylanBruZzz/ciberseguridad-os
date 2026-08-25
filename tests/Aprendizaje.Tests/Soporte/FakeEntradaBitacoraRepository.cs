using Aprendizaje.Dominio.Study;
using Aprendizaje.Dominio.Study.Repositorios;

namespace Aprendizaje.Tests.Soporte;

internal sealed class FakeEntradaBitacoraRepository : IEntradaBitacoraRepository
{
    private readonly Dictionary<Guid, EntradaBitacora> _entradas = [];

    public int AgregarLlamadas { get; private set; }

    public Task<EntradaBitacora?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_entradas.GetValueOrDefault(id));

    public Task<IReadOnlyCollection<EntradaBitacora>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<EntradaBitacora>>(
            _entradas.Values
                .Where(e => e.UsuarioId == usuarioId)
                .OrderByDescending(e => e.Fecha)
                .ThenBy(e => e.Id)
                .ToArray());

    public void Agregar(EntradaBitacora entrada)
    {
        AgregarLlamadas++;
        _entradas[entrada.Id] = entrada;
    }
}
