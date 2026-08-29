using Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.ActualizarArtefactoTecnico;
using Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.CrearArtefactoTecnico;
using Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.EliminarArtefactoTecnico;
using Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.ListarArtefactosTecnicos;
using Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.ObtenerArtefactoTecnicoPorId;
using Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.VincularArtefactoAHerramienta;
using Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.VincularArtefactoATema;
using Aprendizaje.Api.Endpoints.Nucleo;
using Aprendizaje.Aplicacion.Nucleo.Usuarios;
using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Api.Endpoints.Evidence;

public static class ArtefactoTecnicoEndpoints
{
    public static IEndpointRouteBuilder MapArtefactoTecnicoEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/artefactos-tecnicos");

        grupo.MapPost("/", CrearArtefactoTecnicoAsync);
        grupo.MapGet("/", ListarArtefactosTecnicosAsync);
        grupo.MapGet("/{id:guid}", ObtenerArtefactoTecnicoPorIdAsync);
        grupo.MapPut("/{id:guid}", ActualizarArtefactoTecnicoAsync);
        grupo.MapDelete("/{id:guid}", EliminarArtefactoTecnicoAsync);
        grupo.MapPut("/{artefactoTecnicoId:guid}/temas/{temaId:guid}", VincularTemaAsync);
        grupo.MapPut("/{artefactoTecnicoId:guid}/herramientas/{herramientaId:guid}", VincularHerramientaAsync);

        return app;
    }

    private static async Task<IResult> CrearArtefactoTecnicoAsync(
        CrearArtefactoTecnicoHttpRequest request,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        CrearArtefactoTecnicoCasoUso casoUso,
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
                new CrearArtefactoTecnicoSolicitud(usuario.UsuarioId, request.TipoArtefacto, request.Nombre),
                cancellationToken);

            return Results.Created($"/api/artefactos-tecnicos/{resultado.Id}", resultado);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ObtenerArtefactoTecnicoPorIdAsync(
        Guid id,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerArtefactoTecnicoPorIdCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(id, cancellationToken);

            if (!resultado.Encontrado)
                return Results.NotFound();

            var validacion = await UsuarioHttpContexto.ValidarPertenenciaPersonalAsync(
                resultado.ArtefactoTecnico!.UsuarioId,
                environment,
                usuarioActual,
                cancellationToken);

            return validacion ?? Results.Ok(resultado.ArtefactoTecnico);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ListarArtefactosTecnicosAsync(
        Guid? usuarioId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ListarArtefactosTecnicosCasoUso casoUso,
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
                new ListarArtefactosTecnicosSolicitud(usuario.UsuarioId),
                cancellationToken);

            return Results.Ok(resultado.ArtefactosTecnicos);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> VincularTemaAsync(
        Guid artefactoTecnicoId,
        Guid temaId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerArtefactoTecnicoPorIdCasoUso obtenerArtefacto,
        VincularArtefactoATemaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var validacion = await ValidarArtefactoPersonalAsync(
                artefactoTecnicoId,
                environment,
                usuarioActual,
                obtenerArtefacto,
                cancellationToken);

            if (validacion is not null)
                return validacion;

            var resultado = await casoUso.EjecutarAsync(
                new VincularArtefactoATemaSolicitud(artefactoTecnicoId, temaId),
                cancellationToken);

            return resultado.Estado switch
            {
                VincularArtefactoATemaEstado.Actualizado => Results.NoContent(),
                VincularArtefactoATemaEstado.ArtefactoNoEncontrado => Results.NotFound(),
                VincularArtefactoATemaEstado.TemaNoEncontrado => Results.NotFound(),
                VincularArtefactoATemaEstado.UsuarioNoCoincide => Results.Conflict(new
                {
                    error = "El Artefacto Técnico y el Tema deben pertenecer al mismo Usuario."
                }),
                _ => Results.Problem("Estado de vínculo Artefacto-Tema no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> VincularHerramientaAsync(
        Guid artefactoTecnicoId,
        Guid herramientaId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerArtefactoTecnicoPorIdCasoUso obtenerArtefacto,
        VincularArtefactoAHerramientaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var validacion = await ValidarArtefactoPersonalAsync(
                artefactoTecnicoId,
                environment,
                usuarioActual,
                obtenerArtefacto,
                cancellationToken);

            if (validacion is not null)
                return validacion;

            var resultado = await casoUso.EjecutarAsync(
                new VincularArtefactoAHerramientaSolicitud(artefactoTecnicoId, herramientaId),
                cancellationToken);

            return resultado.Estado switch
            {
                VincularArtefactoAHerramientaEstado.Actualizado => Results.NoContent(),
                VincularArtefactoAHerramientaEstado.ArtefactoNoEncontrado => Results.NotFound(),
                VincularArtefactoAHerramientaEstado.HerramientaNoEncontrada => Results.NotFound(),
                _ => Results.Problem("Estado de vínculo Artefacto-Herramienta no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ActualizarArtefactoTecnicoAsync(
        Guid id,
        ActualizarArtefactoTecnicoHttpRequest request,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ActualizarArtefactoTecnicoCasoUso casoUso,
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
                new ActualizarArtefactoTecnicoSolicitud(
                    id,
                    usuario.UsuarioId,
                    request.TipoArtefacto,
                    request.Nombre,
                    request.ContenidoOUrl,
                    request.LenguajeTecnologia,
                    request.EstadoMadurez),
                cancellationToken);

            return resultado.Estado switch
            {
                ActualizarArtefactoTecnicoEstado.Actualizado => Results.NoContent(),
                ActualizarArtefactoTecnicoEstado.UsuarioNoEncontrado => Results.NotFound(),
                ActualizarArtefactoTecnicoEstado.ArtefactoTecnicoNoEncontrado => Results.NotFound(),
                ActualizarArtefactoTecnicoEstado.UsuarioNoCoincide => Results.Conflict(new
                {
                    error = "El Artefacto Técnico no pertenece al usuario indicado."
                }),
                _ => Results.Problem("Estado de actualización de Artefacto Técnico no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> EliminarArtefactoTecnicoAsync(
        Guid id,
        Guid? usuarioId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        EliminarArtefactoTecnicoCasoUso casoUso,
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
                new EliminarArtefactoTecnicoSolicitud(id, usuario.UsuarioId),
                cancellationToken);

            return resultado.Estado switch
            {
                EliminarArtefactoTecnicoEstado.Eliminado => Results.NoContent(),
                EliminarArtefactoTecnicoEstado.UsuarioNoEncontrado => Results.NotFound(),
                EliminarArtefactoTecnicoEstado.ArtefactoTecnicoNoEncontrado => Results.NotFound(),
                EliminarArtefactoTecnicoEstado.UsuarioNoCoincide => Results.Conflict(new
                {
                    error = "El Artefacto Técnico no pertenece al usuario indicado."
                }),
                _ => Results.Problem("Estado de eliminación de Artefacto Técnico no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private sealed record CrearArtefactoTecnicoHttpRequest(
        Guid? UsuarioId,
        TipoArtefacto TipoArtefacto,
        string Nombre);

    private sealed record ActualizarArtefactoTecnicoHttpRequest(
        Guid? UsuarioId,
        TipoArtefacto TipoArtefacto,
        string Nombre,
        string? ContenidoOUrl,
        string? LenguajeTecnologia,
        EstadoMadurez EstadoMadurez);

    private static async Task<IResult?> ValidarArtefactoPersonalAsync(
        Guid artefactoTecnicoId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerArtefactoTecnicoPorIdCasoUso obtenerArtefacto,
        CancellationToken cancellationToken)
    {
        if (!environment.IsEnvironment("Personal"))
            return null;

        var artefacto = await obtenerArtefacto.EjecutarAsync(artefactoTecnicoId, cancellationToken);

        if (!artefacto.Encontrado)
            return Results.NotFound();

        return await UsuarioHttpContexto.ValidarPertenenciaPersonalAsync(
            artefacto.ArtefactoTecnico!.UsuarioId,
            environment,
            usuarioActual,
            cancellationToken);
    }
}
