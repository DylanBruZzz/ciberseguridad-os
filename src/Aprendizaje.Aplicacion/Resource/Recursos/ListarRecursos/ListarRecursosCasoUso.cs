using Aprendizaje.Aplicacion.Resource.Recursos;

namespace Aprendizaje.Aplicacion.Resource.Recursos.ListarRecursos;

public sealed class ListarRecursosCasoUso
{
    private readonly IConsultaRecursosV1 _consulta;

    public ListarRecursosCasoUso(IConsultaRecursosV1 consulta)
    {
        _consulta = consulta;
    }

    public async Task<ListarRecursosResultado> EjecutarAsync(
        ListarRecursosSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.TemaId.HasValue && solicitud.TemaId.Value == Guid.Empty)
            throw new ArgumentException("El temaId debe ser un Guid válido.", nameof(solicitud));

        var recursos = await _consulta.ListarAsync(solicitud, cancellationToken);

        return new ListarRecursosResultado(recursos);
    }
}
