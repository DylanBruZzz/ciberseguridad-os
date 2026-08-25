namespace Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.ObtenerArtefactoTecnicoPorId;

public sealed record ObtenerArtefactoTecnicoPorIdResultado(bool Encontrado, ArtefactoTecnicoDetalle? ArtefactoTecnico)
{
    public static ObtenerArtefactoTecnicoPorIdResultado NoEncontrado() => new(false, null);

    public static ObtenerArtefactoTecnicoPorIdResultado EncontradoCon(ArtefactoTecnicoDetalle artefactoTecnico) =>
        new(true, artefactoTecnico);
}
