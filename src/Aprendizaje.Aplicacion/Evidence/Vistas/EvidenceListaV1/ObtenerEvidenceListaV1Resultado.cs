namespace Aprendizaje.Aplicacion.Evidence.Vistas.EvidenceListaV1;

public sealed record ObtenerEvidenceListaV1Resultado(
    bool Encontrado,
    EvidenceListaV1Dto? Evidence)
{
    public static ObtenerEvidenceListaV1Resultado NoEncontrado() => new(false, null);

    public static ObtenerEvidenceListaV1Resultado EncontradoCon(EvidenceListaV1Dto evidence) => new(true, evidence);
}
