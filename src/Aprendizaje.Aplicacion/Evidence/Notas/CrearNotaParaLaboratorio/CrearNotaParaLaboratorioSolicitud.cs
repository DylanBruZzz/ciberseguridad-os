using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Evidence.Notas.CrearNotaParaLaboratorio;

public sealed record CrearNotaParaLaboratorioSolicitud(
    Guid UsuarioId,
    Guid LaboratorioId,
    string Texto,
    TipoNota Tipo = TipoNota.Nota);
