using Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.CrearArtefactoTecnico;
using Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.ListarArtefactosTecnicos;
using Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.ObtenerArtefactoTecnicoPorId;
using Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.VincularArtefactoAHerramienta;
using Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.VincularArtefactoATema;
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
        grupo.MapPut("/{artefactoTecnicoId:guid}/temas/{temaId:guid}", VincularTemaAsync);
        grupo.MapPut("/{artefactoTecnicoId:guid}/herramientas/{herramientaId:guid}", VincularHerramientaAsync);

        return app;
    }

    private static async Task<IResult> CrearArtefactoTecnicoAsync(
        CrearArtefactoTecnicoHttpRequest request,
        CrearArtefactoTecnicoCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new CrearArtefactoTecnicoSolicitud(request.UsuarioId, request.TipoArtefacto, request.Nombre),
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
        ObtenerArtefactoTecnicoPorIdCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(id, cancellationToken);

            return resultado.Encontrado
                ? Results.Ok(resultado.ArtefactoTecnico)
                : Results.NotFound();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ListarArtefactosTecnicosAsync(
        Guid usuarioId,
        ListarArtefactosTecnicosCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new ListarArtefactosTecnicosSolicitud(usuarioId),
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
        VincularArtefactoATemaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
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
        VincularArtefactoAHerramientaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
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

    private sealed record CrearArtefactoTecnicoHttpRequest(
        Guid UsuarioId,
        TipoArtefacto TipoArtefacto,
        string Nombre);
}
