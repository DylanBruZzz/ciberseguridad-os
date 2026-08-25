using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Evidence.Repositorios;

namespace Aprendizaje.Tests.Soporte;

internal sealed class FakeCertificacionObtenidaRepository : ICertificacionObtenidaRepository
{
    private readonly Dictionary<Guid, CertificacionObtenida> _certificacionesObtenidas = [];

    public int AgregarLlamadas { get; private set; }

    public Task<CertificacionObtenida?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_certificacionesObtenidas.GetValueOrDefault(id));

    public Task<IReadOnlyCollection<CertificacionObtenida>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<CertificacionObtenida>>(
            _certificacionesObtenidas.Values.Where(c => c.UsuarioId == usuarioId).ToArray());

    public void Agregar(CertificacionObtenida certificacionObtenida)
    {
        AgregarLlamadas++;
        _certificacionesObtenidas[certificacionObtenida.Id] = certificacionObtenida;
    }
}
