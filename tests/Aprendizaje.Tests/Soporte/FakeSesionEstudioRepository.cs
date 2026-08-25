using Aprendizaje.Dominio.Study;
using Aprendizaje.Dominio.Study.Repositorios;

namespace Aprendizaje.Tests.Soporte;

internal sealed class FakeSesionEstudioRepository : ISesionEstudioRepository
{
    private readonly Dictionary<Guid, SesionEstudio> _sesiones = [];
    private readonly HashSet<(Guid SesionId, Guid HerramientaId)> _herramientas = [];

    public int AgregarLlamadas { get; private set; }
    public int VincularHerramientaLlamadas { get; private set; }

    public Task<SesionEstudio?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_sesiones.GetValueOrDefault(id));

    public Task<IReadOnlyCollection<SesionEstudio>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<SesionEstudio>>(_sesiones.Values.Where(s => s.UsuarioId == usuarioId).ToArray());

    public Task<bool> ExisteVinculoHerramientaAsync(
        Guid sesionId,
        Guid herramientaId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_herramientas.Contains((sesionId, herramientaId)));

    public void Agregar(SesionEstudio sesion)
    {
        AgregarLlamadas++;
        _sesiones[sesion.Id] = sesion;
    }

    public void VincularHerramienta(Guid sesionId, Guid herramientaId)
    {
        VincularHerramientaLlamadas++;
        _herramientas.Add((sesionId, herramientaId));
    }
}
