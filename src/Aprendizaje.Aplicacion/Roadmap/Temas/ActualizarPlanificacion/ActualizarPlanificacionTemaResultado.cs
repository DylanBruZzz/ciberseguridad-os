namespace Aprendizaje.Aplicacion.Roadmap.Temas.ActualizarPlanificacion;

public sealed record ActualizarPlanificacionTemaResultado(bool Encontrado)
{
    public static ActualizarPlanificacionTemaResultado Actualizado() => new(true);

    public static ActualizarPlanificacionTemaResultado NoEncontrado() => new(false);
}
