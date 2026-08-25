using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Evidence.Repositorios;

namespace Aprendizaje.Tests.Soporte;

internal sealed class FakeWriteupRepository : IWriteupRepository
{
    private readonly Dictionary<Guid, Writeup> _writeups = [];
    private readonly HashSet<(Guid WriteupId, Guid TemaId)> _vinculosTema = [];

    public int AgregarLlamadas { get; private set; }
    public int VincularTemaLlamadas { get; private set; }

    public Task<Writeup?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_writeups.GetValueOrDefault(id));

    public Task<IReadOnlyCollection<Writeup>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<Writeup>>(
            _writeups.Values.Where(w => w.UsuarioId == usuarioId).ToArray());

    public Task<bool> ExisteVinculoTemaAsync(
        Guid writeupId,
        Guid temaId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_vinculosTema.Contains((writeupId, temaId)));

    public void Agregar(Writeup writeup)
    {
        AgregarLlamadas++;
        _writeups[writeup.Id] = writeup;
    }

    public void VincularTema(Guid writeupId, Guid temaId)
    {
        VincularTemaLlamadas++;
        _vinculosTema.Add((writeupId, temaId));
    }
}
