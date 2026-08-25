using Aprendizaje.Dominio.Evidence.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.ListarArtefactosTecnicos;

public sealed class ListarArtefactosTecnicosCasoUso
{
    private readonly IArtefactoTecnicoRepository _artefactosTecnicos;

    public ListarArtefactosTecnicosCasoUso(IArtefactoTecnicoRepository artefactosTecnicos)
    {
        _artefactosTecnicos = artefactosTecnicos;
    }

    public async Task<ListarArtefactosTecnicosResultado> EjecutarAsync(
        ListarArtefactosTecnicosSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        var artefactosTecnicos = await _artefactosTecnicos.ListarPorUsuarioAsync(
            solicitud.UsuarioId,
            cancellationToken);

        return new ListarArtefactosTecnicosResultado(
            artefactosTecnicos
                .Select(a => new ArtefactoTecnicoResumen(
                    a.Id,
                    a.UsuarioId,
                    a.TipoArtefacto,
                    a.Nombre,
                    a.LenguajeTecnologia,
                    a.EstadoMadurez))
                .ToArray());
    }
}
