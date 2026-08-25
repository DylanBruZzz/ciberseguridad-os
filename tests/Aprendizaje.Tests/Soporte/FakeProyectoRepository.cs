using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Evidence.Repositorios;

namespace Aprendizaje.Tests.Soporte;

internal sealed class FakeProyectoRepository : IProyectoRepository
{
    private readonly Dictionary<Guid, Proyecto> _proyectos = [];
    private readonly HashSet<(Guid ProyectoId, Guid TemaId)> _vinculosTema = [];
    private readonly HashSet<(Guid ProyectoId, Guid HerramientaId)> _vinculosHerramienta = [];

    public int AgregarLlamadas { get; private set; }
    public int VincularTemaLlamadas { get; private set; }
    public int VincularHerramientaLlamadas { get; private set; }

    public Task<Proyecto?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_proyectos.GetValueOrDefault(id));

    public Task<IReadOnlyCollection<Proyecto>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<Proyecto>>(
            _proyectos.Values.Where(p => p.UsuarioId == usuarioId).ToArray());

    public Task<bool> ExisteVinculoTemaAsync(
        Guid proyectoId,
        Guid temaId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_vinculosTema.Contains((proyectoId, temaId)));

    public Task<bool> ExisteVinculoHerramientaAsync(
        Guid proyectoId,
        Guid herramientaId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_vinculosHerramienta.Contains((proyectoId, herramientaId)));

    public void Agregar(Proyecto proyecto)
    {
        AgregarLlamadas++;
        _proyectos[proyecto.Id] = proyecto;
    }

    public void VincularTema(Guid proyectoId, Guid temaId)
    {
        VincularTemaLlamadas++;
        _vinculosTema.Add((proyectoId, temaId));
    }

    public void VincularHerramienta(Guid proyectoId, Guid herramientaId)
    {
        VincularHerramientaLlamadas++;
        _vinculosHerramienta.Add((proyectoId, herramientaId));
    }
}
