using Aprendizaje.Dominio.Resource.Repositorios;

namespace Aprendizaje.Aplicacion.Resource.Recursos.ListarRecursos;

public sealed class ListarRecursosCasoUso
{
    private readonly IRecursoRepository _recursos;

    public ListarRecursosCasoUso(IRecursoRepository recursos)
    {
        _recursos = recursos;
    }

    public async Task<ListarRecursosResultado> EjecutarAsync(
        ListarRecursosSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        var recursos = await _recursos.ListarPorUsuarioAsync(solicitud.UsuarioId, cancellationToken);

        return new ListarRecursosResultado(recursos
            .Select(r => new RecursoResumen(r.Id, r.UsuarioId, r.Tipo, r.Titulo, r.Url, r.Estado))
            .ToArray());
    }
}
