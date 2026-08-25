namespace Aprendizaje.Aplicacion.Evidence.Laboratorios.VincularLaboratorioAHerramienta;

public enum VincularLaboratorioAHerramientaEstado
{
    Actualizado,
    LaboratorioNoEncontrado,
    HerramientaNoEncontrada
}

public sealed record VincularLaboratorioAHerramientaResultado(VincularLaboratorioAHerramientaEstado Estado)
{
    public static VincularLaboratorioAHerramientaResultado Actualizado() =>
        new(VincularLaboratorioAHerramientaEstado.Actualizado);

    public static VincularLaboratorioAHerramientaResultado LaboratorioNoEncontrado() =>
        new(VincularLaboratorioAHerramientaEstado.LaboratorioNoEncontrado);

    public static VincularLaboratorioAHerramientaResultado HerramientaNoEncontrada() =>
        new(VincularLaboratorioAHerramientaEstado.HerramientaNoEncontrada);
}
