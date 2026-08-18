namespace Aprendizaje.Aplicacion.Roadmap.Temas.AsignarTemaAFase;

public enum AsignarTemaAFaseEstado
{
    Actualizado,
    TemaNoEncontrado,
    FaseNoEncontrada,
    UsuarioNoCoincide
}

public sealed record AsignarTemaAFaseResultado(AsignarTemaAFaseEstado Estado)
{
    public static AsignarTemaAFaseResultado Actualizado() => new(AsignarTemaAFaseEstado.Actualizado);

    public static AsignarTemaAFaseResultado TemaNoEncontrado() => new(AsignarTemaAFaseEstado.TemaNoEncontrado);

    public static AsignarTemaAFaseResultado FaseNoEncontrada() => new(AsignarTemaAFaseEstado.FaseNoEncontrada);

    public static AsignarTemaAFaseResultado UsuarioNoCoincide() => new(AsignarTemaAFaseEstado.UsuarioNoCoincide);
}
