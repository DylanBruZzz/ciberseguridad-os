using Aprendizaje.Aplicacion.Comun;

namespace Aprendizaje.Tests.Soporte;

internal sealed class FakeTransaccionAplicacion : ITransaccionAplicacion
{
    public int EjecutarLlamadas { get; private set; }

    public async Task<T> EjecutarAsync<T>(
        Func<CancellationToken, Task<T>> operacion,
        CancellationToken cancellationToken = default)
    {
        EjecutarLlamadas++;

        return await operacion(cancellationToken);
    }
}
