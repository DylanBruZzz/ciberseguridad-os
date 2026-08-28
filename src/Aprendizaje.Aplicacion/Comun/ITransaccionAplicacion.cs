namespace Aprendizaje.Aplicacion.Comun;

public interface ITransaccionAplicacion
{
    Task<T> EjecutarAsync<T>(
        Func<CancellationToken, Task<T>> operacion,
        CancellationToken cancellationToken = default);
}
