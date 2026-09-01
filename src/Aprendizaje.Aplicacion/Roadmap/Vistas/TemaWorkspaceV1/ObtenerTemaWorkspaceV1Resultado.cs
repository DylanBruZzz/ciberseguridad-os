namespace Aprendizaje.Aplicacion.Roadmap.Vistas.TemaWorkspaceV1;

public sealed record ObtenerTemaWorkspaceV1Resultado(bool Encontrado, TemaWorkspaceV1Dto? Workspace)
{
    public static ObtenerTemaWorkspaceV1Resultado NoEncontrado() => new(false, null);

    public static ObtenerTemaWorkspaceV1Resultado EncontradoCon(TemaWorkspaceV1Dto workspace) => new(true, workspace);
}
