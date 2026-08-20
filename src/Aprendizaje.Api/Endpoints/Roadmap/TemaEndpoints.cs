using Aprendizaje.Aplicacion.Roadmap.Temas.AsignarTemaAFase;
using Aprendizaje.Aplicacion.Roadmap.Temas.AsignarTemaPadre;
using Aprendizaje.Aplicacion.Roadmap.Temas.CrearTema;
using Aprendizaje.Aplicacion.Roadmap.Temas.DefinirCriteriosRelevantes;
using Aprendizaje.Aplicacion.Roadmap.Temas.DesmarcarCriterio;
using Aprendizaje.Aplicacion.Roadmap.Temas.EstablecerObjetivos;
using Aprendizaje.Aplicacion.Roadmap.Temas.ListarTemas;
using Aprendizaje.Aplicacion.Roadmap.Temas.MarcarCriterio;
using Aprendizaje.Aplicacion.Roadmap.Temas.ObtenerTemaPorId;
using Aprendizaje.Dominio.Roadmap;

namespace Aprendizaje.Api.Endpoints.Roadmap;

public static class TemaEndpoints
{
    public static IEndpointRouteBuilder MapTemaEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/temas");

        grupo.MapPost("/", CrearTemaAsync);
        grupo.MapGet("/", ListarTemasAsync);
        grupo.MapGet("/{id:guid}", ObtenerTemaPorIdAsync);
        grupo.MapPut("/{id:guid}/objetivos", EstablecerObjetivosAsync);
        grupo.MapPut("/{temaId:guid}/criterios", DefinirCriteriosAsync);
        grupo.MapPut("/{temaId:guid}/criterios/{tipo}/cumplido", MarcarCriterioAsync);
        grupo.MapDelete("/{temaId:guid}/criterios/{tipo}/cumplido", DesmarcarCriterioAsync);
        grupo.MapPut("/{temaId:guid}/fase", AsignarFaseAsync);
        grupo.MapPut("/{temaId:guid}/padre", AsignarPadreAsync);

        return app;
    }

    private static async Task<IResult> CrearTemaAsync(
        CrearTemaHttpRequest request,
        CrearTemaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new CrearTemaSolicitud(request.UsuarioId, request.Nombre, request.TipoConocimiento),
                cancellationToken);

            return Results.Created($"/api/temas/{resultado.Id}", resultado);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ListarTemasAsync(
        Guid usuarioId,
        ListarTemasCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new ListarTemasSolicitud(usuarioId),
                cancellationToken);

            return Results.Ok(resultado.Temas);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ObtenerTemaPorIdAsync(
        Guid id,
        ObtenerTemaPorIdCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        var resultado = await casoUso.EjecutarAsync(id, cancellationToken);

        return resultado.Encontrado
            ? Results.Ok(resultado.Tema)
            : Results.NotFound();
    }

    private static async Task<IResult> EstablecerObjetivosAsync(
        Guid id,
        EstablecerObjetivosTemaHttpRequest request,
        EstablecerObjetivosTemaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        if (request.Objetivos is null)
            return Results.BadRequest(new { error = "La lista de objetivos no puede ser null." });

        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new EstablecerObjetivosTemaSolicitud(id, request.Objetivos),
                cancellationToken);

            return resultado.Encontrado
                ? Results.NoContent()
                : Results.NotFound();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> AsignarFaseAsync(
        Guid temaId,
        AsignarTemaAFaseHttpRequest request,
        AsignarTemaAFaseCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new AsignarTemaAFaseSolicitud(temaId, request.FaseId),
                cancellationToken);

            return resultado.Estado switch
            {
                AsignarTemaAFaseEstado.Actualizado => Results.NoContent(),
                AsignarTemaAFaseEstado.TemaNoEncontrado => Results.NotFound(),
                AsignarTemaAFaseEstado.FaseNoEncontrada => Results.NotFound(),
                AsignarTemaAFaseEstado.UsuarioNoCoincide => Results.Conflict(new
                {
                    error = "El Tema y la Fase deben pertenecer al mismo Usuario."
                }),
                _ => Results.Problem("Estado de asignación de Fase no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> DefinirCriteriosAsync(
        Guid temaId,
        DefinirCriteriosRelevantesTemaHttpRequest request,
        DefinirCriteriosRelevantesTemaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        if (request.Criterios is null)
            return Results.BadRequest(new { error = "La lista de criterios no puede ser null." });

        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new DefinirCriteriosRelevantesTemaSolicitud(temaId, request.Criterios),
                cancellationToken);

            return resultado.Estado switch
            {
                DefinirCriteriosRelevantesTemaEstado.Actualizado => Results.NoContent(),
                DefinirCriteriosRelevantesTemaEstado.TemaNoEncontrado => Results.NotFound(),
                DefinirCriteriosRelevantesTemaEstado.ProgresoRegistrado => Results.Conflict(new
                {
                    error = "No se pueden redefinir los criterios relevantes mientras exista progreso registrado."
                }),
                _ => Results.Problem("Estado de definición de criterios no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> MarcarCriterioAsync(
        Guid temaId,
        TipoCriterio tipo,
        MarcarCriterioTemaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new MarcarCriterioTemaSolicitud(temaId, tipo),
                cancellationToken);

            return resultado.Estado switch
            {
                MarcarCriterioTemaEstado.Actualizado => Results.NoContent(),
                MarcarCriterioTemaEstado.TemaNoEncontrado => Results.NotFound(),
                MarcarCriterioTemaEstado.CriterioNoDefinido => Results.Conflict(new
                {
                    error = $"{tipo} no es un criterio relevante para este Tema."
                }),
                _ => Results.Problem("Estado de marcado de criterio no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> DesmarcarCriterioAsync(
        Guid temaId,
        TipoCriterio tipo,
        DesmarcarCriterioTemaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new DesmarcarCriterioTemaSolicitud(temaId, tipo),
                cancellationToken);

            return resultado.Estado switch
            {
                DesmarcarCriterioTemaEstado.Actualizado => Results.NoContent(),
                DesmarcarCriterioTemaEstado.TemaNoEncontrado => Results.NotFound(),
                DesmarcarCriterioTemaEstado.CriterioNoDefinido => Results.Conflict(new
                {
                    error = $"{tipo} no es un criterio relevante para este Tema."
                }),
                _ => Results.Problem("Estado de desmarcado de criterio no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> AsignarPadreAsync(
        Guid temaId,
        AsignarTemaPadreHttpRequest request,
        AsignarTemaPadreCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new AsignarTemaPadreSolicitud(temaId, request.TemaPadreId),
                cancellationToken);

            return resultado.Estado switch
            {
                AsignarTemaPadreEstado.Actualizado => Results.NoContent(),
                AsignarTemaPadreEstado.TemaNoEncontrado => Results.NotFound(),
                AsignarTemaPadreEstado.TemaPadreNoEncontrado => Results.NotFound(),
                AsignarTemaPadreEstado.UsuarioNoCoincide => Results.Conflict(new
                {
                    error = "El Tema y el Tema padre deben pertenecer al mismo Usuario."
                }),
                AsignarTemaPadreEstado.ConflictoJerarquia => Results.Conflict(new
                {
                    error = "La asignación crearía un ciclo directo en la jerarquía de Temas."
                }),
                _ => Results.Problem("Estado de asignación de Tema padre no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private sealed record CrearTemaHttpRequest(
        Guid UsuarioId,
        string Nombre,
        TipoConocimiento TipoConocimiento);

    private sealed record EstablecerObjetivosTemaHttpRequest(
        IReadOnlyCollection<string>? Objetivos);

    private sealed record DefinirCriteriosRelevantesTemaHttpRequest(
        IReadOnlyCollection<TipoCriterio>? Criterios);

    private sealed record AsignarTemaAFaseHttpRequest(Guid FaseId);

    private sealed record AsignarTemaPadreHttpRequest(Guid TemaPadreId);
}
