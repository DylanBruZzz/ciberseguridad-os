namespace Aprendizaje.Aplicacion.Roadmap.Fases.ActualizarMetadataPedagogica;

public enum ActualizarMetadataPedagogicaFaseEstado
{
    Actualizada,
    FaseNoEncontrada,
    UsuarioNoCoincide
}

public sealed record ActualizarMetadataPedagogicaFaseResultado(
    ActualizarMetadataPedagogicaFaseEstado Estado)
{
    public static ActualizarMetadataPedagogicaFaseResultado Actualizada() =>
        new(ActualizarMetadataPedagogicaFaseEstado.Actualizada);

    public static ActualizarMetadataPedagogicaFaseResultado FaseNoEncontrada() =>
        new(ActualizarMetadataPedagogicaFaseEstado.FaseNoEncontrada);

    public static ActualizarMetadataPedagogicaFaseResultado UsuarioNoCoincide() =>
        new(ActualizarMetadataPedagogicaFaseEstado.UsuarioNoCoincide);
}
