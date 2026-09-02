using Aprendizaje.Dominio.Nucleo.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.Vistas.EvidenceListaV1;

public sealed class ObtenerEvidenceListaV1CasoUso
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IConsultaEvidenceListaV1 _consulta;

    public ObtenerEvidenceListaV1CasoUso(IUsuarioRepository usuarios, IConsultaEvidenceListaV1 consulta)
    {
        _usuarios = usuarios;
        _consulta = consulta;
    }

    public async Task<ObtenerEvidenceListaV1Resultado> EjecutarAsync(
        ObtenerEvidenceListaV1Solicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.TemaId.HasValue && solicitud.TemaId.Value == Guid.Empty)
            throw new ArgumentException("El temaId debe ser un Guid válido.", nameof(solicitud));

        var usuario = await _usuarios.ObtenerPorIdAsync(solicitud.UsuarioId, cancellationToken);

        if (usuario is null)
            return ObtenerEvidenceListaV1Resultado.NoEncontrado();

        var lista = await _consulta.ObtenerAsync(solicitud, cancellationToken);

        return ObtenerEvidenceListaV1Resultado.EncontradoCon(lista);
    }
}
