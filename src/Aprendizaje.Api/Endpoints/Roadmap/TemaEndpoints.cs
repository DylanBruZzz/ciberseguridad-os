using Aprendizaje.Aplicacion.Roadmap.Temas.ActualizarPercepcion;
using Aprendizaje.Aplicacion.Roadmap.Temas.ActualizarPlanificacion;
using Aprendizaje.Aplicacion.Roadmap.Temas.AsignarTemaAFase;
using Aprendizaje.Aplicacion.Roadmap.Temas.AsignarTemaPadre;
using Aprendizaje.Aplicacion.Roadmap.Temas.ConfigurarIntervaloRepaso;
using Aprendizaje.Aplicacion.Roadmap.Temas.CrearTema;
using Aprendizaje.Aplicacion.Roadmap.Temas.DefinirCriteriosRelevantes;
using Aprendizaje.Aplicacion.Roadmap.Temas.DesmarcarCriterio;
using Aprendizaje.Aplicacion.Roadmap.Temas.DesvincularHerramientaDeTema;
using Aprendizaje.Aplicacion.Roadmap.Temas.EstablecerObjetivos;
using Aprendizaje.Aplicacion.Roadmap.Temas.ListarTemas;
using Aprendizaje.Aplicacion.Roadmap.Temas.MarcarCriterio;
using Aprendizaje.Aplicacion.Roadmap.Temas.ObtenerTemaPorId;
using Aprendizaje.Aplicacion.Roadmap.Temas.ObtenerApuntesTema;
using Aprendizaje.Aplicacion.Roadmap.Temas.GuardarApuntesTema;
using Aprendizaje.Aplicacion.Roadmap.Temas.VincularHerramientaATema;
using Aprendizaje.Api.Endpoints.Nucleo;
using Aprendizaje.Aplicacion.Nucleo.Usuarios;
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
        grupo.MapGet("/{temaId:guid}/apuntes", ObtenerApuntesAsync);
        grupo.MapPut("/{temaId:guid}/apuntes", GuardarApuntesAsync);
        grupo.MapPut("/{id:guid}/objetivos", EstablecerObjetivosAsync);
        grupo.MapPut("/{temaId:guid}/percepcion", ActualizarPercepcionAsync);
        grupo.MapPut("/{temaId:guid}/planificacion", ActualizarPlanificacionAsync);
        grupo.MapPut("/{temaId:guid}/intervalo-repaso", ConfigurarIntervaloRepasoAsync);
        grupo.MapPut("/{temaId:guid}/criterios", DefinirCriteriosAsync);
        grupo.MapPut("/{temaId:guid}/criterios/{tipo}/cumplido", MarcarCriterioAsync);
        grupo.MapDelete("/{temaId:guid}/criterios/{tipo}/cumplido", DesmarcarCriterioAsync);
        grupo.MapPut("/{temaId:guid}/fase", AsignarFaseAsync);
        grupo.MapPut("/{temaId:guid}/padre", AsignarPadreAsync);

        return app;
    }

    private static async Task<IResult> CrearTemaAsync(
        CrearTemaHttpRequest request,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        CrearTemaCasoUso casoUso,
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
                new CrearTemaSolicitud(usuario.UsuarioId, request.Nombre, request.TipoConocimiento),
                cancellationToken);

            return Results.Created($"/api/temas/{resultado.Id}", resultado);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ListarTemasAsync(
        Guid? usuarioId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ListarTemasCasoUso casoUso,
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
                new ListarTemasSolicitud(usuario.UsuarioId),
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
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerTemaPorIdCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        var resultado = await casoUso.EjecutarAsync(id, cancellationToken);

        if (!resultado.Encontrado)
            return Results.NotFound();

        var validacion = await UsuarioHttpContexto.ValidarPertenenciaPersonalAsync(
            resultado.Tema!.UsuarioId,
            environment,
            usuarioActual,
            cancellationToken);

        return validacion ?? Results.Ok(resultado.Tema);
    }

    private static async Task<IResult> ObtenerApuntesAsync(
        Guid temaId,
        Guid? usuarioId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerApuntesTemaCasoUso casoUso,
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
                new ObtenerApuntesTemaSolicitud(usuario.UsuarioId, temaId),
                cancellationToken);

            return resultado.Estado switch
            {
                ObtenerApuntesTemaEstado.Encontrado => Results.Ok(resultado.Apuntes),
                ObtenerApuntesTemaEstado.TemaNoEncontrado => Results.NotFound(),
                ObtenerApuntesTemaEstado.UsuarioNoCoincide => ResultadoOwnershipNoCoincide(environment),
                _ => Results.Problem("Estado de obtención de apuntes de Tema no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> GuardarApuntesAsync(
        Guid temaId,
        GuardarApuntesTemaHttpRequest request,
        Guid? usuarioId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        GuardarApuntesTemaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        if (request.Contenido is null)
            return Results.BadRequest(new { error = "El contenido de los apuntes no puede ser null." });

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
                new GuardarApuntesTemaSolicitud(usuario.UsuarioId, temaId, request.Contenido),
                cancellationToken);

            return resultado.Estado switch
            {
                GuardarApuntesTemaEstado.Guardado => Results.NoContent(),
                GuardarApuntesTemaEstado.TemaNoEncontrado => Results.NotFound(),
                GuardarApuntesTemaEstado.UsuarioNoCoincide => ResultadoOwnershipNoCoincide(environment),
                _ => Results.Problem("Estado de guardado de apuntes de Tema no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> EstablecerObjetivosAsync(
        Guid id,
        EstablecerObjetivosTemaHttpRequest request,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerTemaPorIdCasoUso obtenerTema,
        EstablecerObjetivosTemaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        if (request.Objetivos is null)
            return Results.BadRequest(new { error = "La lista de objetivos no puede ser null." });

        try
        {
            var validacion = await ValidarTemaPersonalAsync(id, environment, usuarioActual, obtenerTema, cancellationToken);
            if (validacion is not null)
                return validacion;

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

    private static async Task<IResult> ActualizarPercepcionAsync(
        Guid temaId,
        ActualizarPercepcionTemaHttpRequest request,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerTemaPorIdCasoUso obtenerTema,
        ActualizarPercepcionTemaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var validacion = await ValidarTemaPersonalAsync(temaId, environment, usuarioActual, obtenerTema, cancellationToken);
            if (validacion is not null)
                return validacion;

            var resultado = await casoUso.EjecutarAsync(
                new ActualizarPercepcionTemaSolicitud(temaId, request.DificultadPercibida, request.Confianza),
                cancellationToken);

            return resultado.Encontrado
                ? Results.NoContent()
                : Results.NotFound();
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ActualizarPlanificacionAsync(
        Guid temaId,
        ActualizarPlanificacionTemaHttpRequest request,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerTemaPorIdCasoUso obtenerTema,
        ActualizarPlanificacionTemaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var validacion = await ValidarTemaPersonalAsync(temaId, environment, usuarioActual, obtenerTema, cancellationToken);
            if (validacion is not null)
                return validacion;

            var resultado = await casoUso.EjecutarAsync(
                new ActualizarPlanificacionTemaSolicitud(temaId, request.FechaInicio, request.FechaFin),
                cancellationToken);

            return resultado.Encontrado
                ? Results.NoContent()
                : Results.NotFound();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ConfigurarIntervaloRepasoAsync(
        Guid temaId,
        ConfigurarIntervaloRepasoTemaHttpRequest request,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerTemaPorIdCasoUso obtenerTema,
        ConfigurarIntervaloRepasoTemaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var validacion = await ValidarTemaPersonalAsync(temaId, environment, usuarioActual, obtenerTema, cancellationToken);
            if (validacion is not null)
                return validacion;

            var resultado = await casoUso.EjecutarAsync(
                new ConfigurarIntervaloRepasoTemaSolicitud(temaId, request.Dias),
                cancellationToken);

            return resultado.Encontrado
                ? Results.NoContent()
                : Results.NotFound();
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> AsignarFaseAsync(
        Guid temaId,
        AsignarTemaAFaseHttpRequest request,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerTemaPorIdCasoUso obtenerTema,
        AsignarTemaAFaseCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var validacion = await ValidarTemaPersonalAsync(temaId, environment, usuarioActual, obtenerTema, cancellationToken);
            if (validacion is not null)
                return validacion;

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
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerTemaPorIdCasoUso obtenerTema,
        DefinirCriteriosRelevantesTemaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        if (request.Criterios is null)
            return Results.BadRequest(new { error = "La lista de criterios no puede ser null." });

        try
        {
            var validacion = await ValidarTemaPersonalAsync(temaId, environment, usuarioActual, obtenerTema, cancellationToken);
            if (validacion is not null)
                return validacion;

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
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerTemaPorIdCasoUso obtenerTema,
        MarcarCriterioTemaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var validacion = await ValidarTemaPersonalAsync(temaId, environment, usuarioActual, obtenerTema, cancellationToken);
            if (validacion is not null)
                return validacion;

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
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerTemaPorIdCasoUso obtenerTema,
        DesmarcarCriterioTemaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var validacion = await ValidarTemaPersonalAsync(temaId, environment, usuarioActual, obtenerTema, cancellationToken);
            if (validacion is not null)
                return validacion;

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
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerTemaPorIdCasoUso obtenerTema,
        AsignarTemaPadreCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var validacion = await ValidarTemaPersonalAsync(temaId, environment, usuarioActual, obtenerTema, cancellationToken);
            if (validacion is not null)
                return validacion;

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

    private static async Task<IResult?> ValidarTemaPersonalAsync(
        Guid temaId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerTemaPorIdCasoUso obtenerTema,
        CancellationToken cancellationToken)
    {
        if (!environment.IsEnvironment("Personal"))
            return null;

        var tema = await obtenerTema.EjecutarAsync(temaId, cancellationToken);

        if (!tema.Encontrado)
            return Results.NotFound();

        return await UsuarioHttpContexto.ValidarPertenenciaPersonalAsync(
            tema.Tema!.UsuarioId,
            environment,
            usuarioActual,
            cancellationToken);
    }

    private static IResult ResultadoOwnershipNoCoincide(IHostEnvironment environment) =>
        environment.IsEnvironment("Personal")
            ? Results.NotFound()
            : Results.Conflict(new { error = "El Tema debe pertenecer al Usuario indicado." });

    private sealed record CrearTemaHttpRequest(
        Guid? UsuarioId,
        string Nombre,
        TipoConocimiento TipoConocimiento);

    private sealed record EstablecerObjetivosTemaHttpRequest(
        IReadOnlyCollection<string>? Objetivos);

    private sealed record ActualizarPercepcionTemaHttpRequest(
        int? DificultadPercibida,
        int? Confianza);

    private sealed record ActualizarPlanificacionTemaHttpRequest(
        DateOnly? FechaInicio,
        DateOnly? FechaFin);

    private sealed record ConfigurarIntervaloRepasoTemaHttpRequest(int? Dias);

    private sealed record GuardarApuntesTemaHttpRequest(string? Contenido);

    private sealed record DefinirCriteriosRelevantesTemaHttpRequest(
        IReadOnlyCollection<DefinicionCriterioTemaSolicitud>? Criterios);

    private sealed record AsignarTemaAFaseHttpRequest(Guid FaseId);

    private sealed record AsignarTemaPadreHttpRequest(Guid TemaPadreId);
}
