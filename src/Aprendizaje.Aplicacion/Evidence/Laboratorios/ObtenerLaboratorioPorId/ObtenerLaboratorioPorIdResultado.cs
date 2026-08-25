namespace Aprendizaje.Aplicacion.Evidence.Laboratorios.ObtenerLaboratorioPorId;

public sealed record ObtenerLaboratorioPorIdResultado(bool Encontrado, LaboratorioDetalle? Laboratorio)
{
    public static ObtenerLaboratorioPorIdResultado NoEncontrado() => new(false, null);

    public static ObtenerLaboratorioPorIdResultado EncontradoCon(LaboratorioDetalle laboratorio) =>
        new(true, laboratorio);
}
