using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Tests.Soporte;

internal sealed class FakeApunteTemaRepository : IApunteTemaRepository
{
    private readonly Dictionary<Guid, ApunteTema> _apuntesPorTema = [];

    public int AgregarLlamadas { get; private set; }

    public ApunteTema? UltimoApunteAgregado { get; private set; }

    public Task<ApunteTema?> ObtenerPorTemaIdAsync(Guid temaId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_apuntesPorTema.GetValueOrDefault(temaId));

    public void Agregar(ApunteTema apunte)
    {
        AgregarLlamadas++;
        UltimoApunteAgregado = apunte;
        _apuntesPorTema[apunte.TemaId] = apunte;
    }
}
