using Aprendizaje.Dominio.Study;

namespace Aprendizaje.Aplicacion.Study.EntradasBitacora.ListarEntradasBitacora;

public sealed record EntradaBitacoraResumen(Guid Id, Guid UsuarioId, Guid? TemaId, DateTime Fecha, string Texto)
{
    public static EntradaBitacoraResumen DesdeDominio(EntradaBitacora entrada) =>
        new(entrada.Id, entrada.UsuarioId, entrada.TemaId, entrada.Fecha, entrada.Texto);
}
