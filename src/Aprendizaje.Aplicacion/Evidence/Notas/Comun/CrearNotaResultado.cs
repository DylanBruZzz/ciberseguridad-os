using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Evidence.Notas.Comun;

public sealed record CrearNotaResultado(
    CrearNotaEstado Estado,
    Guid? Id = null,
    Guid? UsuarioId = null,
    Guid? TemaId = null,
    Guid? ProyectoId = null,
    Guid? LaboratorioId = null,
    Guid? WriteupId = null,
    Guid? ArtefactoTecnicoId = null,
    DateTime? Fecha = null,
    string? Texto = null,
    TipoNota? Tipo = null)
{
    public static CrearNotaResultado PadreNoEncontrado() => new(CrearNotaEstado.PadreNoEncontrado);

    public static CrearNotaResultado UsuarioNoCoincide() => new(CrearNotaEstado.UsuarioNoCoincide);

    public static CrearNotaResultado Creada(Nota nota) =>
        new(
            CrearNotaEstado.Creada,
            nota.Id,
            nota.UsuarioId,
            nota.TemaId,
            nota.ProyectoId,
            nota.LaboratorioId,
            nota.WriteupId,
            nota.ArtefactoTecnicoId,
            nota.Fecha,
            nota.Texto,
            nota.Tipo);
}

public enum CrearNotaEstado
{
    Creada,
    PadreNoEncontrado,
    UsuarioNoCoincide
}
