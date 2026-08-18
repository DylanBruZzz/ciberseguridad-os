namespace Aprendizaje.Aplicacion.Roadmap.Fases.ListarFases;

public sealed record FaseResumen(
    Guid Id,
    Guid UsuarioId,
    string Nombre,
    int Orden,
    string? Color,
    string? Descripcion);
