using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Aplicacion.Evidence.Notas.Comun;
using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Evidence.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.Notas.CrearNotaParaArtefactoTecnico;

public sealed class CrearNotaParaArtefactoTecnicoCasoUso
{
    private readonly INotaRepository _notas;
    private readonly IArtefactoTecnicoRepository _artefactosTecnicos;
    private readonly IUnitOfWork _unitOfWork;

    public CrearNotaParaArtefactoTecnicoCasoUso(
        INotaRepository notas,
        IArtefactoTecnicoRepository artefactosTecnicos,
        IUnitOfWork unitOfWork)
    {
        _notas = notas;
        _artefactosTecnicos = artefactosTecnicos;
        _unitOfWork = unitOfWork;
    }

    public async Task<CrearNotaResultado> EjecutarAsync(
        CrearNotaParaArtefactoTecnicoSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.ArtefactoTecnicoId == Guid.Empty)
            throw new ArgumentException("El artefactoTecnicoId debe ser un Guid válido.", nameof(solicitud));

        var artefactoTecnico = await _artefactosTecnicos.ObtenerPorIdAsync(
            solicitud.ArtefactoTecnicoId,
            cancellationToken);

        if (artefactoTecnico is null)
            return CrearNotaResultado.PadreNoEncontrado();

        if (artefactoTecnico.UsuarioId != solicitud.UsuarioId)
            return CrearNotaResultado.UsuarioNoCoincide();

        var nota = Nota.SobreArtefactoTecnico(
            solicitud.UsuarioId,
            solicitud.ArtefactoTecnicoId,
            solicitud.Texto,
            solicitud.Tipo);

        _notas.Agregar(nota);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return CrearNotaResultado.Creada(nota);
    }
}
