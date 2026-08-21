using Aprendizaje.Aplicacion.Comun;

namespace Aprendizaje.Tests.Soporte;

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int GuardarCambiosLlamadas { get; private set; }

    public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
    {
        GuardarCambiosLlamadas++;

        return Task.FromResult(1);
    }
}
