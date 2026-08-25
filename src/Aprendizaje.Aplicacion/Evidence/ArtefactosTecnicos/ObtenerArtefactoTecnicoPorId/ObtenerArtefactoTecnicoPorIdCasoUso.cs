using Aprendizaje.Dominio.Evidence.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.ObtenerArtefactoTecnicoPorId;

public sealed class ObtenerArtefactoTecnicoPorIdCasoUso
{
    private readonly IArtefactoTecnicoRepository _artefactosTecnicos;

    public ObtenerArtefactoTecnicoPorIdCasoUso(IArtefactoTecnicoRepository artefactosTecnicos)
    {
        _artefactosTecnicos = artefactosTecnicos;
    }

    public async Task<ObtenerArtefactoTecnicoPorIdResultado> EjecutarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El id debe ser un Guid válido.", nameof(id));

        var artefactoTecnico = await _artefactosTecnicos.ObtenerPorIdAsync(id, cancellationToken);

        if (artefactoTecnico is null)
            return ObtenerArtefactoTecnicoPorIdResultado.NoEncontrado();

        return ObtenerArtefactoTecnicoPorIdResultado.EncontradoCon(new ArtefactoTecnicoDetalle(
            artefactoTecnico.Id,
            artefactoTecnico.UsuarioId,
            artefactoTecnico.TipoArtefacto,
            artefactoTecnico.Nombre,
            artefactoTecnico.ContenidoOUrl,
            artefactoTecnico.LenguajeTecnologia,
            artefactoTecnico.EstadoMadurez));
    }
}
