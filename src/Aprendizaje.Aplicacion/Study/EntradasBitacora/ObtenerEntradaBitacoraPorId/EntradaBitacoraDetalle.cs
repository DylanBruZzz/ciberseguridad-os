using Aprendizaje.Dominio.Study;

namespace Aprendizaje.Aplicacion.Study.EntradasBitacora.ObtenerEntradaBitacoraPorId;

public sealed record EntradaBitacoraDetalle(Guid Id, Guid UsuarioId, Guid? TemaId, DateTime Fecha, string Texto)
{
    public static EntradaBitacoraDetalle DesdeDominio(EntradaBitacora entrada) =>
        new(entrada.Id, entrada.UsuarioId, entrada.TemaId, entrada.Fecha, entrada.Texto);
}
