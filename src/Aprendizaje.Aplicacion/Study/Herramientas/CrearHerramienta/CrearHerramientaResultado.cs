using Aprendizaje.Dominio.Study;

namespace Aprendizaje.Aplicacion.Study.Herramientas.CrearHerramienta;

public sealed record CrearHerramientaResultado(Guid Id, string Nombre, string? Categoria)
{
    public static CrearHerramientaResultado DesdeDominio(Herramienta herramienta) =>
        new(herramienta.Id, herramienta.Nombre, herramienta.Categoria);
}
