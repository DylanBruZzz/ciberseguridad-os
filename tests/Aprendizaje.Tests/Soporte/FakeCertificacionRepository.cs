using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Tests.Soporte;

internal sealed class FakeCertificacionRepository : ICertificacionRepository
{
    private readonly Dictionary<Guid, Certificacion> _certificaciones = [];
    private readonly HashSet<(Guid CertificacionId, Guid TemaId)> _vinculosTema = [];

    public int AgregarLlamadas { get; private set; }
    public int VincularTemaLlamadas { get; private set; }

    public Task<Certificacion?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_certificaciones.GetValueOrDefault(id));

    public Task<IReadOnlyCollection<Certificacion>> ListarAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<Certificacion>>(_certificaciones.Values.ToArray());

    public Task<bool> ExisteVinculoTemaAsync(
        Guid certificacionId,
        Guid temaId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_vinculosTema.Contains((certificacionId, temaId)));

    public void Agregar(Certificacion certificacion)
    {
        AgregarLlamadas++;
        _certificaciones[certificacion.Id] = certificacion;
    }

    public void VincularTema(Guid certificacionId, Guid temaId)
    {
        VincularTemaLlamadas++;
        _vinculosTema.Add((certificacionId, temaId));
    }
}
