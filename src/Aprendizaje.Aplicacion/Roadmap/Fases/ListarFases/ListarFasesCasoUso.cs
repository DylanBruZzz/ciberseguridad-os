using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Aplicacion.Roadmap.Fases.ListarFases;

public sealed class ListarFasesCasoUso
{
    private readonly IFaseRepository _fases;

    public ListarFasesCasoUso(IFaseRepository fases)
    {
        _fases = fases;
    }

    public async Task<ListarFasesResultado> EjecutarAsync(
        ListarFasesSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        var fases = await _fases.ListarPorUsuarioAsync(solicitud.UsuarioId, cancellationToken);

        var resumenes = fases
            .Select(f => new FaseResumen(
                f.Id,
                f.UsuarioId,
                f.Nombre,
                f.Orden,
                f.Color,
                f.Descripcion,
                f.Objetivos,
                f.CriteriosAvance,
                f.MesInicioRecomendado,
                f.MesFinRecomendado,
                f.CargaSemanalRecomendada))
            .ToArray();

        return new ListarFasesResultado(resumenes);
    }
}
