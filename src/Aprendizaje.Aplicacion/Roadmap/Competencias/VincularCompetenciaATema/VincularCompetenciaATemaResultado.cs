namespace Aprendizaje.Aplicacion.Roadmap.Competencias.VincularCompetenciaATema;

public enum VincularCompetenciaATemaEstado
{
    Actualizado,
    CompetenciaNoEncontrada,
    TemaNoEncontrado,
    UsuarioNoCoincide
}

public sealed record VincularCompetenciaATemaResultado(VincularCompetenciaATemaEstado Estado)
{
    public static VincularCompetenciaATemaResultado Actualizado() =>
        new(VincularCompetenciaATemaEstado.Actualizado);

    public static VincularCompetenciaATemaResultado CompetenciaNoEncontrada() =>
        new(VincularCompetenciaATemaEstado.CompetenciaNoEncontrada);

    public static VincularCompetenciaATemaResultado TemaNoEncontrado() =>
        new(VincularCompetenciaATemaEstado.TemaNoEncontrado);

    public static VincularCompetenciaATemaResultado UsuarioNoCoincide() =>
        new(VincularCompetenciaATemaEstado.UsuarioNoCoincide);
}
