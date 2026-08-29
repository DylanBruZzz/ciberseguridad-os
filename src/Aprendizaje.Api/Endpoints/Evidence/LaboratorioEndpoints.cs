using Aprendizaje.Aplicacion.Evidence.Laboratorios.ActualizarLaboratorio;
using Aprendizaje.Aplicacion.Evidence.Laboratorios.CrearLaboratorio;
using Aprendizaje.Aplicacion.Evidence.Laboratorios.EliminarLaboratorio;
using Aprendizaje.Aplicacion.Evidence.Laboratorios.ListarLaboratorios;
using Aprendizaje.Aplicacion.Evidence.Laboratorios.ObtenerLaboratorioPorId;
using Aprendizaje.Aplicacion.Evidence.Laboratorios.VincularLaboratorioAHerramienta;
using Aprendizaje.Aplicacion.Evidence.Laboratorios.VincularLaboratorioATema;
using Aprendizaje.Api.Endpoints.Nucleo;
using Aprendizaje.Aplicacion.Nucleo.Usuarios;
using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Api.Endpoints.Evidence;

public static class LaboratorioEndpoints
{
    public static IEndpointRouteBuilder MapLaboratorioEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/laboratorios");

        grupo.MapPost("/", CrearLaboratorioAsync);
        grupo.MapGet("/", ListarLaboratoriosAsync);
        grupo.MapGet("/{id:guid}", ObtenerLaboratorioPorIdAsync);
        grupo.MapPut("/{id:guid}", ActualizarLaboratorioAsync);
        grupo.MapDelete("/{id:guid}", EliminarLaboratorioAsync);
        grupo.MapPut("/{laboratorioId:guid}/temas/{temaId:guid}", VincularTemaAsync);
        grupo.MapPut("/{laboratorioId:guid}/herramientas/{herramientaId:guid}", VincularHerramientaAsync);

        return app;
    }

    private static async Task<IResult> CrearLaboratorioAsync(
        CrearLaboratorioHttpRequest request,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        CrearLaboratorioCasoUso casoUso,
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
                new CrearLaboratorioSolicitud(
                    usuario.UsuarioId,
                    request.Nombre,
                    request.Objetivo,
                    request.EntornoVms,
                    request.Hallazgos,
                    request.TiempoInvertidoMinutos,
                    request.Fecha),
                cancellationToken);

            return Results.Created($"/api/laboratorios/{resultado.Id}", resultado);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ObtenerLaboratorioPorIdAsync(
        Guid id,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerLaboratorioPorIdCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(id, cancellationToken);

            if (!resultado.Encontrado)
                return Results.NotFound();

            var validacion = await UsuarioHttpContexto.ValidarPertenenciaPersonalAsync(
                resultado.Laboratorio!.UsuarioId,
                environment,
                usuarioActual,
                cancellationToken);

            return validacion ?? Results.Ok(resultado.Laboratorio);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ListarLaboratoriosAsync(
        Guid? usuarioId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ListarLaboratoriosCasoUso casoUso,
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
                new ListarLaboratoriosSolicitud(usuario.UsuarioId),
                cancellationToken);

            return Results.Ok(resultado.Laboratorios);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> VincularTemaAsync(
        Guid laboratorioId,
        Guid temaId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerLaboratorioPorIdCasoUso obtenerLaboratorio,
        VincularLaboratorioATemaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var validacion = await ValidarLaboratorioPersonalAsync(
                laboratorioId,
                environment,
                usuarioActual,
                obtenerLaboratorio,
                cancellationToken);

            if (validacion is not null)
                return validacion;

            var resultado = await casoUso.EjecutarAsync(
                new VincularLaboratorioATemaSolicitud(laboratorioId, temaId),
                cancellationToken);

            return resultado.Estado switch
            {
                VincularLaboratorioATemaEstado.Actualizado => Results.NoContent(),
                VincularLaboratorioATemaEstado.LaboratorioNoEncontrado => Results.NotFound(),
                VincularLaboratorioATemaEstado.TemaNoEncontrado => Results.NotFound(),
                VincularLaboratorioATemaEstado.UsuarioNoCoincide => Results.Conflict(new
                {
                    error = "El Laboratorio y el Tema deben pertenecer al mismo Usuario."
                }),
                _ => Results.Problem("Estado de vínculo Laboratorio-Tema no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> VincularHerramientaAsync(
        Guid laboratorioId,
        Guid herramientaId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerLaboratorioPorIdCasoUso obtenerLaboratorio,
        VincularLaboratorioAHerramientaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var validacion = await ValidarLaboratorioPersonalAsync(
                laboratorioId,
                environment,
                usuarioActual,
                obtenerLaboratorio,
                cancellationToken);

            if (validacion is not null)
                return validacion;

            var resultado = await casoUso.EjecutarAsync(
                new VincularLaboratorioAHerramientaSolicitud(laboratorioId, herramientaId),
                cancellationToken);

            return resultado.Estado switch
            {
                VincularLaboratorioAHerramientaEstado.Actualizado => Results.NoContent(),
                VincularLaboratorioAHerramientaEstado.LaboratorioNoEncontrado => Results.NotFound(),
                VincularLaboratorioAHerramientaEstado.HerramientaNoEncontrada => Results.NotFound(),
                _ => Results.Problem("Estado de vínculo Laboratorio-Herramienta no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ActualizarLaboratorioAsync(
        Guid id,
        ActualizarLaboratorioHttpRequest request,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ActualizarLaboratorioCasoUso casoUso,
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
                new ActualizarLaboratorioSolicitud(
                    id,
                    usuario.UsuarioId,
                    request.Nombre,
                    request.Objetivo,
                    request.EntornoVms,
                    request.Hallazgos,
                    request.TiempoInvertidoMinutos,
                    request.Fecha,
                    request.EstadoMadurez),
                cancellationToken);

            return resultado.Estado switch
            {
                ActualizarLaboratorioEstado.Actualizado => Results.NoContent(),
                ActualizarLaboratorioEstado.UsuarioNoEncontrado => Results.NotFound(),
                ActualizarLaboratorioEstado.LaboratorioNoEncontrado => Results.NotFound(),
                ActualizarLaboratorioEstado.UsuarioNoCoincide => Results.Conflict(new
                {
                    error = "El Laboratorio no pertenece al usuario indicado."
                }),
                _ => Results.Problem("Estado de actualización de Laboratorio no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> EliminarLaboratorioAsync(
        Guid id,
        Guid? usuarioId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        EliminarLaboratorioCasoUso casoUso,
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
                new EliminarLaboratorioSolicitud(id, usuario.UsuarioId),
                cancellationToken);

            return resultado.Estado switch
            {
                EliminarLaboratorioEstado.Eliminado => Results.NoContent(),
                EliminarLaboratorioEstado.UsuarioNoEncontrado => Results.NotFound(),
                EliminarLaboratorioEstado.LaboratorioNoEncontrado => Results.NotFound(),
                EliminarLaboratorioEstado.UsuarioNoCoincide => Results.Conflict(new
                {
                    error = "El Laboratorio no pertenece al usuario indicado."
                }),
                _ => Results.Problem("Estado de eliminación de Laboratorio no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult?> ValidarLaboratorioPersonalAsync(
        Guid laboratorioId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerLaboratorioPorIdCasoUso obtenerLaboratorio,
        CancellationToken cancellationToken)
    {
        if (!environment.IsEnvironment("Personal"))
            return null;

        var laboratorio = await obtenerLaboratorio.EjecutarAsync(laboratorioId, cancellationToken);

        if (!laboratorio.Encontrado)
            return Results.NotFound();

        return await UsuarioHttpContexto.ValidarPertenenciaPersonalAsync(
            laboratorio.Laboratorio!.UsuarioId,
            environment,
            usuarioActual,
            cancellationToken);
    }

    private sealed record CrearLaboratorioHttpRequest(
        Guid? UsuarioId,
        string Nombre,
        string? Objetivo,
        string? EntornoVms,
        string? Hallazgos,
        int? TiempoInvertidoMinutos,
        DateOnly? Fecha);

    private sealed record ActualizarLaboratorioHttpRequest(
        Guid? UsuarioId,
        string Nombre,
        string? Objetivo,
        string? EntornoVms,
        string? Hallazgos,
        int? TiempoInvertidoMinutos,
        DateOnly? Fecha,
        EstadoMadurez EstadoMadurez);
}
