using Aprendizaje.Aplicacion.Study.EntradasBitacora.CrearEntradaBitacora;
using Aprendizaje.Aplicacion.Study.EntradasBitacora.ListarEntradasBitacora;
using Aprendizaje.Aplicacion.Study.EntradasBitacora.ObtenerEntradaBitacoraPorId;
using Aprendizaje.Api.Endpoints.Nucleo;
using Aprendizaje.Aplicacion.Nucleo.Usuarios;

namespace Aprendizaje.Api.Endpoints.Study;

public static class EntradaBitacoraEndpoints
{
    public static IEndpointRouteBuilder MapEntradaBitacoraEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/entradas-bitacora");

        grupo.MapPost("/", CrearEntradaBitacoraAsync);
        grupo.MapGet("/", ListarEntradasBitacoraAsync);
        grupo.MapGet("/{id:guid}", ObtenerEntradaBitacoraPorIdAsync);

        return app;
    }

    private static async Task<IResult> CrearEntradaBitacoraAsync(
        CrearEntradaBitacoraHttpRequest request,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        CrearEntradaBitacoraCasoUso casoUso,
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
                new CrearEntradaBitacoraSolicitud(usuario.UsuarioId, request.Texto, request.TemaId),
                cancellationToken);

            return resultado.Estado switch
            {
                CrearEntradaBitacoraEstado.Creada => Results.Created(
                    $"/api/entradas-bitacora/{resultado.Id}",
                    resultado),
                CrearEntradaBitacoraEstado.TemaNoEncontrado => Results.NotFound(),
                CrearEntradaBitacoraEstado.UsuarioNoCoincide => Results.Conflict(new
                {
                    error = "La entrada de bitácora y el Tema deben pertenecer al mismo Usuario."
                }),
                _ => Results.Problem("Estado de creación de entrada de bitácora no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ObtenerEntradaBitacoraPorIdAsync(
        Guid id,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerEntradaBitacoraPorIdCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(id, cancellationToken);

            if (!resultado.Encontrada)
                return Results.NotFound();

            var validacion = await UsuarioHttpContexto.ValidarPertenenciaPersonalAsync(
                resultado.Entrada!.UsuarioId,
                environment,
                usuarioActual,
                cancellationToken);

            return validacion ?? Results.Ok(resultado.Entrada);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ListarEntradasBitacoraAsync(
        Guid? usuarioId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ListarEntradasBitacoraCasoUso casoUso,
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
                new ListarEntradasBitacoraSolicitud(usuario.UsuarioId),
                cancellationToken);

            return Results.Ok(resultado.Entradas);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private sealed record CrearEntradaBitacoraHttpRequest(Guid? UsuarioId, string Texto, Guid? TemaId);
}
