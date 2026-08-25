using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Evidence.Repositorios;

namespace Aprendizaje.Tests.Soporte;

internal sealed class FakeLaboratorioRepository : ILaboratorioRepository
{
    private readonly Dictionary<Guid, Laboratorio> _laboratorios = [];
    private readonly HashSet<(Guid LaboratorioId, Guid TemaId)> _vinculosTema = [];
    private readonly HashSet<(Guid LaboratorioId, Guid HerramientaId)> _vinculosHerramienta = [];

    public int AgregarLlamadas { get; private set; }
    public int VincularTemaLlamadas { get; private set; }
    public int VincularHerramientaLlamadas { get; private set; }

    public Task<Laboratorio?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_laboratorios.GetValueOrDefault(id));

    public Task<IReadOnlyCollection<Laboratorio>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<Laboratorio>>(
            _laboratorios.Values.Where(l => l.UsuarioId == usuarioId).ToArray());

    public Task<bool> ExisteVinculoTemaAsync(
        Guid laboratorioId,
        Guid temaId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_vinculosTema.Contains((laboratorioId, temaId)));

    public Task<bool> ExisteVinculoHerramientaAsync(
        Guid laboratorioId,
        Guid herramientaId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_vinculosHerramienta.Contains((laboratorioId, herramientaId)));

    public void Agregar(Laboratorio laboratorio)
    {
        AgregarLlamadas++;
        _laboratorios[laboratorio.Id] = laboratorio;
    }

    public void VincularTema(Guid laboratorioId, Guid temaId)
    {
        VincularTemaLlamadas++;
        _vinculosTema.Add((laboratorioId, temaId));
    }

    public void VincularHerramienta(Guid laboratorioId, Guid herramientaId)
    {
        VincularHerramientaLlamadas++;
        _vinculosHerramienta.Add((laboratorioId, herramientaId));
    }
}
