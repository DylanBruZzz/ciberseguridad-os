using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Aplicacion.Roadmap.Temas.ObtenerTemaPorId;

public sealed class ObtenerTemaPorIdCasoUso
{
    private readonly ITemaRepository _temas;

    public ObtenerTemaPorIdCasoUso(ITemaRepository temas)
    {
        _temas = temas;
    }

    public async Task<ObtenerTemaPorIdResultado> EjecutarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var tema = await _temas.ObtenerPorIdAsync(id, cancellationToken);

        if (tema is null)
            return ObtenerTemaPorIdResultado.NoEncontrado();

        var detalle = new TemaDetalle(
            tema.Id,
            tema.UsuarioId,
            tema.FaseId,
            tema.TemaPadreId,
            tema.Nombre,
            tema.Descripcion,
            tema.Objetivos.ToArray(),
            tema.TipoConocimiento,
            tema.FechaInicio,
            tema.FechaFin,
            tema.DificultadPercibida?.Valor,
            tema.Confianza?.Valor,
            tema.IntervaloRepaso?.Dias,
            tema.Criterios
                .Select(c => new CriterioTemaDetalle(c.Tipo, c.Descripcion, c.Cumplido, c.FechaCumplido))
                .ToArray());

        return ObtenerTemaPorIdResultado.EncontradoCon(detalle);
    }
}
