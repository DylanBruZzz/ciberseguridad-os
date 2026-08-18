namespace Aprendizaje.Aplicacion.Roadmap.Fases.CrearFase;

public sealed record CrearFaseSolicitud(
    Guid UsuarioId,
    string Nombre,
    int Orden);
