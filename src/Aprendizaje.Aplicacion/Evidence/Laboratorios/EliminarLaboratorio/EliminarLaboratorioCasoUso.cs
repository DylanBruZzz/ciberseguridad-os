using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Evidence.Repositorios;
using Aprendizaje.Dominio.Nucleo.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.Laboratorios.EliminarLaboratorio;

public sealed class EliminarLaboratorioCasoUso
{
    private readonly ILaboratorioRepository _laboratorios;
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _unitOfWork;

    public EliminarLaboratorioCasoUso(
        ILaboratorioRepository laboratorios,
        IUsuarioRepository usuarios,
        IUnitOfWork unitOfWork)
    {
        _laboratorios = laboratorios;
        _usuarios = usuarios;
        _unitOfWork = unitOfWork;
    }

    public async Task<EliminarLaboratorioResultado> EjecutarAsync(
        EliminarLaboratorioSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.LaboratorioId == Guid.Empty)
            throw new ArgumentException("El laboratorioId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        var usuario = await _usuarios.ObtenerPorIdAsync(solicitud.UsuarioId, cancellationToken);
        if (usuario is null)
            return EliminarLaboratorioResultado.UsuarioNoEncontrado();

        var laboratorio = await _laboratorios.ObtenerPorIdAsync(solicitud.LaboratorioId, cancellationToken);
        if (laboratorio is null)
            return EliminarLaboratorioResultado.LaboratorioNoEncontrado();

        if (laboratorio.UsuarioId != solicitud.UsuarioId)
            return EliminarLaboratorioResultado.UsuarioNoCoincide();

        laboratorio.MarcarComoEliminado();

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return EliminarLaboratorioResultado.Eliminado();
    }
}
