using Aprendizaje.Dominio.Evidence.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.Notas.ListarNotas;

public sealed class ListarNotasCasoUso
{
    private readonly INotaRepository _notas;

    public ListarNotasCasoUso(INotaRepository notas)
    {
        _notas = notas;
    }

    public async Task<ListarNotasResultado> EjecutarAsync(
        ListarNotasSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        var notas = await _notas.ListarPorUsuarioAsync(solicitud.UsuarioId, cancellationToken);

        return new ListarNotasResultado(
            notas
                .Select(n => new NotaResumen(
                    n.Id,
                    n.UsuarioId,
                    n.TemaId,
                    n.ProyectoId,
                    n.LaboratorioId,
                    n.WriteupId,
                    n.ArtefactoTecnicoId,
                    n.Fecha,
                    n.Texto,
                    n.Tipo))
                .ToArray());
    }
}
