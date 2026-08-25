using Aprendizaje.Dominio.Evidence.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.Laboratorios.ListarLaboratorios;

public sealed class ListarLaboratoriosCasoUso
{
    private readonly ILaboratorioRepository _laboratorios;

    public ListarLaboratoriosCasoUso(ILaboratorioRepository laboratorios)
    {
        _laboratorios = laboratorios;
    }

    public async Task<ListarLaboratoriosResultado> EjecutarAsync(
        ListarLaboratoriosSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        var laboratorios = await _laboratorios.ListarPorUsuarioAsync(solicitud.UsuarioId, cancellationToken);

        return new ListarLaboratoriosResultado(laboratorios
            .Select(l => new LaboratorioResumen(
                l.Id,
                l.UsuarioId,
                l.Nombre,
                l.Objetivo,
                l.TiempoInvertidoMinutos,
                l.EstadoMadurez,
                l.Fecha))
            .ToArray());
    }
}
