namespace Aprendizaje.Aplicacion.Evidence.Proyectos.VincularProyectoAHerramienta;

public enum VincularProyectoAHerramientaEstado
{
    Actualizado,
    ProyectoNoEncontrado,
    HerramientaNoEncontrada
}

public sealed record VincularProyectoAHerramientaResultado(VincularProyectoAHerramientaEstado Estado)
{
    public static VincularProyectoAHerramientaResultado Actualizado() =>
        new(VincularProyectoAHerramientaEstado.Actualizado);

    public static VincularProyectoAHerramientaResultado ProyectoNoEncontrado() =>
        new(VincularProyectoAHerramientaEstado.ProyectoNoEncontrado);

    public static VincularProyectoAHerramientaResultado HerramientaNoEncontrada() =>
        new(VincularProyectoAHerramientaEstado.HerramientaNoEncontrada);
}
