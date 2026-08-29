using Aprendizaje.Aplicacion.Roadmap.Competencias.CrearCompetencia;
using Aprendizaje.Aplicacion.Roadmap.Competencias.ListarCompetencias;
using Aprendizaje.Aplicacion.Roadmap.Competencias.ObtenerCompetenciaPorId;
using Aprendizaje.Aplicacion.Roadmap.Competencias.VincularCompetenciaATema;
using Aprendizaje.Api.Endpoints.Nucleo;
using Aprendizaje.Aplicacion.Nucleo.Usuarios;

namespace Aprendizaje.Api.Endpoints.Roadmap;

public static class CompetenciaEndpoints
{
    public static IEndpointRouteBuilder MapCompetenciaEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/competencias");

        grupo.MapPost("/", CrearCompetenciaAsync);
        grupo.MapGet("/", ListarCompetenciasAsync);
        grupo.MapGet("/{id:guid}", ObtenerCompetenciaPorIdAsync);
        grupo.MapPut("/{competenciaId:guid}/temas/{temaId:guid}", VincularTemaAsync);

        return app;
    }

    private static async Task<IResult> CrearCompetenciaAsync(
        CrearCompetenciaHttpRequest request,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        CrearCompetenciaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var usuario = await UsuarioHttpContexto.ResolverAsync(
                request.UsuarioId,
                environment,
                usuarioActual,
                cancellationToken);

            if (!usuario.Exitosa)
                return usuario.Error!;

            var resultado = await casoUso.EjecutarAsync(
                new CrearCompetenciaSolicitud(usuario.UsuarioId, request.Nombre),
                cancellationToken);

            return Results.Created($"/api/competencias/{resultado.Id}", resultado);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ObtenerCompetenciaPorIdAsync(
        Guid id,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerCompetenciaPorIdCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(id, cancellationToken);

            if (!resultado.Encontrada)
                return Results.NotFound();

            var validacion = await UsuarioHttpContexto.ValidarPertenenciaPersonalAsync(
                resultado.Competencia!.UsuarioId,
                environment,
                usuarioActual,
                cancellationToken);

            return validacion ?? Results.Ok(resultado.Competencia);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ListarCompetenciasAsync(
        Guid? usuarioId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ListarCompetenciasCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var usuario = await UsuarioHttpContexto.ResolverAsync(
                usuarioId,
                environment,
                usuarioActual,
                cancellationToken);

            if (!usuario.Exitosa)
                return usuario.Error!;

            var resultado = await casoUso.EjecutarAsync(
                new ListarCompetenciasSolicitud(usuario.UsuarioId),
                cancellationToken);

            return Results.Ok(resultado.Competencias);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> VincularTemaAsync(
        Guid competenciaId,
        Guid temaId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerCompetenciaPorIdCasoUso obtenerCompetencia,
        VincularCompetenciaATemaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var validacion = await ValidarCompetenciaPersonalAsync(
                competenciaId,
                environment,
                usuarioActual,
                obtenerCompetencia,
                cancellationToken);

            if (validacion is not null)
                return validacion;

            var resultado = await casoUso.EjecutarAsync(
                new VincularCompetenciaATemaSolicitud(competenciaId, temaId),
                cancellationToken);

            return resultado.Estado switch
            {
                VincularCompetenciaATemaEstado.Actualizado => Results.NoContent(),
                VincularCompetenciaATemaEstado.CompetenciaNoEncontrada => Results.NotFound(),
                VincularCompetenciaATemaEstado.TemaNoEncontrado => Results.NotFound(),
                VincularCompetenciaATemaEstado.UsuarioNoCoincide => Results.Conflict(new
                {
                    error = "La Competencia y el Tema deben pertenecer al mismo Usuario."
                }),
                _ => Results.Problem("Estado de vínculo Competencia-Tema no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult?> ValidarCompetenciaPersonalAsync(
        Guid competenciaId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerCompetenciaPorIdCasoUso obtenerCompetencia,
        CancellationToken cancellationToken)
    {
        if (!environment.IsEnvironment("Personal"))
            return null;

        var competencia = await obtenerCompetencia.EjecutarAsync(competenciaId, cancellationToken);

        if (!competencia.Encontrada)
            return Results.NotFound();

        return await UsuarioHttpContexto.ValidarPertenenciaPersonalAsync(
            competencia.Competencia!.UsuarioId,
            environment,
            usuarioActual,
            cancellationToken);
    }

    private sealed record CrearCompetenciaHttpRequest(Guid? UsuarioId, string Nombre);
}
