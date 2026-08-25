using Aprendizaje.Aplicacion.Evidence.Laboratorios.CrearLaboratorio;
using Aprendizaje.Aplicacion.Evidence.Laboratorios.ListarLaboratorios;
using Aprendizaje.Aplicacion.Evidence.Laboratorios.ObtenerLaboratorioPorId;
using Aprendizaje.Aplicacion.Evidence.Laboratorios.VincularLaboratorioAHerramienta;
using Aprendizaje.Aplicacion.Evidence.Laboratorios.VincularLaboratorioATema;

namespace Aprendizaje.Api.Endpoints.Evidence;

public static class LaboratorioEndpoints
{
    public static IEndpointRouteBuilder MapLaboratorioEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/laboratorios");

        grupo.MapPost("/", CrearLaboratorioAsync);
        grupo.MapGet("/", ListarLaboratoriosAsync);
        grupo.MapGet("/{id:guid}", ObtenerLaboratorioPorIdAsync);
        grupo.MapPut("/{laboratorioId:guid}/temas/{temaId:guid}", VincularTemaAsync);
        grupo.MapPut("/{laboratorioId:guid}/herramientas/{herramientaId:guid}", VincularHerramientaAsync);

        return app;
    }

    private static async Task<IResult> CrearLaboratorioAsync(
        CrearLaboratorioHttpRequest request,
        CrearLaboratorioCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new CrearLaboratorioSolicitud(
                    request.UsuarioId,
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
        ObtenerLaboratorioPorIdCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(id, cancellationToken);

            return resultado.Encontrado
                ? Results.Ok(resultado.Laboratorio)
                : Results.NotFound();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ListarLaboratoriosAsync(
        Guid usuarioId,
        ListarLaboratoriosCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new ListarLaboratoriosSolicitud(usuarioId),
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
        VincularLaboratorioATemaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
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
        VincularLaboratorioAHerramientaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
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

    private sealed record CrearLaboratorioHttpRequest(
        Guid UsuarioId,
        string Nombre,
        string? Objetivo,
        string? EntornoVms,
        string? Hallazgos,
        int? TiempoInvertidoMinutos,
        DateOnly? Fecha);
}
