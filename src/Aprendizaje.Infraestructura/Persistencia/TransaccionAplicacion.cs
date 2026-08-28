using Aprendizaje.Aplicacion.Comun;

namespace Aprendizaje.Infraestructura.Persistencia;

public sealed class TransaccionAplicacion : ITransaccionAplicacion
{
    private readonly AprendizajeDbContext _context;

    public TransaccionAplicacion(AprendizajeDbContext context)
    {
        _context = context;
    }

    public async Task<T> EjecutarAsync<T>(
        Func<CancellationToken, Task<T>> operacion,
        CancellationToken cancellationToken = default)
    {
        await using var transaccion = await _context.Database.BeginTransactionAsync(cancellationToken);

        var resultado = await operacion(cancellationToken);

        await transaccion.CommitAsync(cancellationToken);

        return resultado;
    }
}
