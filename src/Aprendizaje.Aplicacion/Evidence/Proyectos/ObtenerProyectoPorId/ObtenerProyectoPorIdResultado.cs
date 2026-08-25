namespace Aprendizaje.Aplicacion.Evidence.Proyectos.ObtenerProyectoPorId;

public sealed record ObtenerProyectoPorIdResultado(bool Encontrado, ProyectoDetalle? Proyecto)
{
    public static ObtenerProyectoPorIdResultado NoEncontrado() => new(false, null);

    public static ObtenerProyectoPorIdResultado EncontradoCon(ProyectoDetalle proyecto) => new(true, proyecto);
}
