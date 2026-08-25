using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Evidence.Notas.ListarNotas;

public sealed record NotaResumen(
    Guid Id,
    Guid UsuarioId,
    Guid? TemaId,
    Guid? ProyectoId,
    Guid? LaboratorioId,
    Guid? WriteupId,
    Guid? ArtefactoTecnicoId,
    DateTime Fecha,
    string Texto,
    TipoNota Tipo);
