namespace Aprendizaje.Aplicacion.Resource.Recursos.ObtenerRecursoPorId;

public sealed record ObtenerRecursoPorIdResultado(bool Encontrado, RecursoDetalle? Recurso)
{
    public static ObtenerRecursoPorIdResultado NoEncontrado() => new(false, null);

    public static ObtenerRecursoPorIdResultado EncontradoCon(RecursoDetalle recurso) => new(true, recurso);
}
