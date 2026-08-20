using Aprendizaje.Aplicacion.Resource.Recursos.CrearRecurso;
using Aprendizaje.Aplicacion.Resource.Recursos.ListarRecursos;
using Aprendizaje.Aplicacion.Resource.Recursos.ObtenerRecursoPorId;
using Aprendizaje.Aplicacion.Resource.Recursos.VincularRecursoATema;
using Aprendizaje.Dominio.Resource;

namespace Aprendizaje.Api.Endpoints.Resource;

public static class RecursoEndpoints
{
    public static IEndpointRouteBuilder MapRecursoEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/recursos");

        grupo.MapPost("/", CrearRecursoAsync);
        grupo.MapGet("/", ListarRecursosAsync);
        grupo.MapGet("/{id:guid}", ObtenerRecursoPorIdAsync);
        grupo.MapPut("/{recursoId:guid}/temas/{temaId:guid}", VincularTemaAsync);

        return app;
    }

    private static async Task<IResult> CrearRecursoAsync(
        CrearRecursoHttpRequest request,
        CrearRecursoCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new CrearRecursoSolicitud(request.UsuarioId, request.Tipo, request.Titulo, request.Url),
                cancellationToken);

            return Results.Created($"/api/recursos/{resultado.Id}", resultado);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ObtenerRecursoPorIdAsync(
        Guid id,
        ObtenerRecursoPorIdCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        var resultado = await casoUso.EjecutarAsync(id, cancellationToken);

        return resultado.Encontrado
            ? Results.Ok(resultado.Recurso)
            : Results.NotFound();
    }

    private static async Task<IResult> ListarRecursosAsync(
        Guid usuarioId,
        ListarRecursosCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new ListarRecursosSolicitud(usuarioId),
                cancellationToken);

            return Results.Ok(resultado.Recursos);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> VincularTemaAsync(
        Guid recursoId,
        Guid temaId,
        VincularRecursoATemaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new VincularRecursoATemaSolicitud(recursoId, temaId),
                cancellationToken);

            return resultado.Estado switch
            {
                VincularRecursoATemaEstado.Actualizado => Results.NoContent(),
                VincularRecursoATemaEstado.RecursoNoEncontrado => Results.NotFound(),
                VincularRecursoATemaEstado.TemaNoEncontrado => Results.NotFound(),
                VincularRecursoATemaEstado.UsuarioNoCoincide => Results.Conflict(new
                {
                    error = "El Recurso y el Tema deben pertenecer al mismo Usuario."
                }),
                _ => Results.Problem("Estado de vínculo Recurso-Tema no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private sealed record CrearRecursoHttpRequest(
        Guid UsuarioId,
        TipoRecurso Tipo,
        string Titulo,
        string? Url);
}
