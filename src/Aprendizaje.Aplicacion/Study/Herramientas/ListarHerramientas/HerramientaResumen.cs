using Aprendizaje.Dominio.Study;

namespace Aprendizaje.Aplicacion.Study.Herramientas.ListarHerramientas;

public sealed record HerramientaResumen(Guid Id, string Nombre, string? Categoria)
{
    public static HerramientaResumen DesdeDominio(Herramienta herramienta) =>
        new(herramienta.Id, herramienta.Nombre, herramienta.Categoria);
}
