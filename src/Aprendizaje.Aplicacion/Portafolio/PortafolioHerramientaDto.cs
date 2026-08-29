namespace Aprendizaje.Aplicacion.Portafolio;

public sealed record PortafolioHerramientaDto(
    Guid HerramientaId,
    string Nombre,
    string? Categoria);
