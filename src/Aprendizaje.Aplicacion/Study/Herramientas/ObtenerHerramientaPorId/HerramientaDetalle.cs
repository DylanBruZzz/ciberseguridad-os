using Aprendizaje.Dominio.Study;

namespace Aprendizaje.Aplicacion.Study.Herramientas.ObtenerHerramientaPorId;

public sealed record HerramientaDetalle(Guid Id, string Nombre, string? Categoria)
{
    public static HerramientaDetalle DesdeDominio(Herramienta herramienta) =>
        new(herramienta.Id, herramienta.Nombre, herramienta.Categoria);
}
