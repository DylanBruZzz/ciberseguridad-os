using Aprendizaje.Dominio.Evidence.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.Notas.ObtenerNotaPorId;

public sealed class ObtenerNotaPorIdCasoUso
{
    private readonly INotaRepository _notas;

    public ObtenerNotaPorIdCasoUso(INotaRepository notas)
    {
        _notas = notas;
    }

    public async Task<ObtenerNotaPorIdResultado> EjecutarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El id debe ser un Guid válido.", nameof(id));

        var nota = await _notas.ObtenerPorIdAsync(id, cancellationToken);

        if (nota is null)
            return ObtenerNotaPorIdResultado.NoEncontrada();

        return ObtenerNotaPorIdResultado.EncontradaCon(new NotaDetalle(
            nota.Id,
            nota.UsuarioId,
            nota.TemaId,
            nota.ProyectoId,
            nota.LaboratorioId,
            nota.WriteupId,
            nota.ArtefactoTecnicoId,
            nota.Fecha,
            nota.Texto,
            nota.Tipo));
    }
}
