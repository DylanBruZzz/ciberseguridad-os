namespace Aprendizaje.Aplicacion.Roadmap.Fases.CrearFase;

public sealed record CrearFaseResultado(
    Guid Id,
    Guid UsuarioId,
    string Nombre,
    int Orden);
