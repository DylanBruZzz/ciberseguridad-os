namespace Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.VincularArtefactoAHerramienta;

public sealed record VincularArtefactoAHerramientaResultado(VincularArtefactoAHerramientaEstado Estado)
{
    public static VincularArtefactoAHerramientaResultado Actualizado() =>
        new(VincularArtefactoAHerramientaEstado.Actualizado);

    public static VincularArtefactoAHerramientaResultado ArtefactoNoEncontrado() =>
        new(VincularArtefactoAHerramientaEstado.ArtefactoNoEncontrado);

    public static VincularArtefactoAHerramientaResultado HerramientaNoEncontrada() =>
        new(VincularArtefactoAHerramientaEstado.HerramientaNoEncontrada);
}

public enum VincularArtefactoAHerramientaEstado
{
    Actualizado,
    ArtefactoNoEncontrado,
    HerramientaNoEncontrada
}
