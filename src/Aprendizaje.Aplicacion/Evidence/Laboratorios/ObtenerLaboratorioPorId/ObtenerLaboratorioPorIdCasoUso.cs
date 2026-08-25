using Aprendizaje.Dominio.Evidence.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.Laboratorios.ObtenerLaboratorioPorId;

public sealed class ObtenerLaboratorioPorIdCasoUso
{
    private readonly ILaboratorioRepository _laboratorios;

    public ObtenerLaboratorioPorIdCasoUso(ILaboratorioRepository laboratorios)
    {
        _laboratorios = laboratorios;
    }

    public async Task<ObtenerLaboratorioPorIdResultado> EjecutarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El id debe ser un Guid válido.", nameof(id));

        var laboratorio = await _laboratorios.ObtenerPorIdAsync(id, cancellationToken);

        if (laboratorio is null)
            return ObtenerLaboratorioPorIdResultado.NoEncontrado();

        return ObtenerLaboratorioPorIdResultado.EncontradoCon(new LaboratorioDetalle(
            laboratorio.Id,
            laboratorio.UsuarioId,
            laboratorio.Nombre,
            laboratorio.Objetivo,
            laboratorio.EntornoVms,
            laboratorio.Hallazgos,
            laboratorio.TiempoInvertidoMinutos,
            laboratorio.EstadoMadurez,
            laboratorio.Fecha));
    }
}
