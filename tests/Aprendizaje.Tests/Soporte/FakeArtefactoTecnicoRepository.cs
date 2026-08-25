using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Evidence.Repositorios;

namespace Aprendizaje.Tests.Soporte;

internal sealed class FakeArtefactoTecnicoRepository : IArtefactoTecnicoRepository
{
    private readonly Dictionary<Guid, ArtefactoTecnico> _artefactosTecnicos = [];
    private readonly HashSet<(Guid ArtefactoTecnicoId, Guid TemaId)> _vinculosTema = [];
    private readonly HashSet<(Guid ArtefactoTecnicoId, Guid HerramientaId)> _vinculosHerramienta = [];

    public int AgregarLlamadas { get; private set; }
    public int VincularTemaLlamadas { get; private set; }
    public int VincularHerramientaLlamadas { get; private set; }

    public Task<ArtefactoTecnico?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_artefactosTecnicos.GetValueOrDefault(id));

    public Task<IReadOnlyCollection<ArtefactoTecnico>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<ArtefactoTecnico>>(
            _artefactosTecnicos.Values.Where(a => a.UsuarioId == usuarioId).ToArray());

    public Task<bool> ExisteVinculoTemaAsync(
        Guid artefactoTecnicoId,
        Guid temaId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_vinculosTema.Contains((artefactoTecnicoId, temaId)));

    public Task<bool> ExisteVinculoHerramientaAsync(
        Guid artefactoTecnicoId,
        Guid herramientaId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_vinculosHerramienta.Contains((artefactoTecnicoId, herramientaId)));

    public void Agregar(ArtefactoTecnico artefactoTecnico)
    {
        AgregarLlamadas++;
        _artefactosTecnicos[artefactoTecnico.Id] = artefactoTecnico;
    }

    public void VincularTema(Guid artefactoTecnicoId, Guid temaId)
    {
        VincularTemaLlamadas++;
        _vinculosTema.Add((artefactoTecnicoId, temaId));
    }

    public void VincularHerramienta(Guid artefactoTecnicoId, Guid herramientaId)
    {
        VincularHerramientaLlamadas++;
        _vinculosHerramienta.Add((artefactoTecnicoId, herramientaId));
    }
}
