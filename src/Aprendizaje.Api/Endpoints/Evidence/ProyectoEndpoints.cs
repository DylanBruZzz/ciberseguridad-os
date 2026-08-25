using Aprendizaje.Aplicacion.Evidence.Proyectos.CrearProyecto;
using Aprendizaje.Aplicacion.Evidence.Proyectos.ListarProyectos;
using Aprendizaje.Aplicacion.Evidence.Proyectos.ObtenerProyectoPorId;
using Aprendizaje.Aplicacion.Evidence.Proyectos.VincularProyectoAHerramienta;
using Aprendizaje.Aplicacion.Evidence.Proyectos.VincularProyectoATema;

namespace Aprendizaje.Api.Endpoints.Evidence;

public static class ProyectoEndpoints
{
    public static IEndpointRouteBuilder MapProyectoEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/proyectos");

        grupo.MapPost("/", CrearProyectoAsync);
        grupo.MapGet("/", ListarProyectosAsync);
        grupo.MapGet("/{id:guid}", ObtenerProyectoPorIdAsync);
        grupo.MapPut("/{proyectoId:guid}/temas/{temaId:guid}", VincularTemaAsync);
        grupo.MapPut("/{proyectoId:guid}/herramientas/{herramientaId:guid}", VincularHerramientaAsync);

        return app;
    }

    private static async Task<IResult> CrearProyectoAsync(
        CrearProyectoHttpRequest request,
        CrearProyectoCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new CrearProyectoSolicitud(request.UsuarioId, request.Nombre),
                cancellationToken);

            return Results.Created($"/api/proyectos/{resultado.Id}", resultado);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ObtenerProyectoPorIdAsync(
        Guid id,
        ObtenerProyectoPorIdCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(id, cancellationToken);

            return resultado.Encontrado
                ? Results.Ok(resultado.Proyecto)
                : Results.NotFound();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ListarProyectosAsync(
        Guid usuarioId,
        ListarProyectosCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new ListarProyectosSolicitud(usuarioId),
                cancellationToken);

            return Results.Ok(resultado.Proyectos);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> VincularTemaAsync(
        Guid proyectoId,
        Guid temaId,
        VincularProyectoATemaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new VincularProyectoATemaSolicitud(proyectoId, temaId),
                cancellationToken);

            return resultado.Estado switch
            {
                VincularProyectoATemaEstado.Actualizado => Results.NoContent(),
                VincularProyectoATemaEstado.ProyectoNoEncontrado => Results.NotFound(),
                VincularProyectoATemaEstado.TemaNoEncontrado => Results.NotFound(),
                VincularProyectoATemaEstado.UsuarioNoCoincide => Results.Conflict(new
                {
                    error = "El Proyecto y el Tema deben pertenecer al mismo Usuario."
                }),
                _ => Results.Problem("Estado de vínculo Proyecto-Tema no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> VincularHerramientaAsync(
        Guid proyectoId,
        Guid herramientaId,
        VincularProyectoAHerramientaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new VincularProyectoAHerramientaSolicitud(proyectoId, herramientaId),
                cancellationToken);

            return resultado.Estado switch
            {
                VincularProyectoAHerramientaEstado.Actualizado => Results.NoContent(),
                VincularProyectoAHerramientaEstado.ProyectoNoEncontrado => Results.NotFound(),
                VincularProyectoAHerramientaEstado.HerramientaNoEncontrada => Results.NotFound(),
                _ => Results.Problem("Estado de vínculo Proyecto-Herramienta no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private sealed record CrearProyectoHttpRequest(Guid UsuarioId, string Nombre);
}
