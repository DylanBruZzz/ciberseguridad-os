using Aprendizaje.Dominio.Study;

namespace Aprendizaje.Aplicacion.Analytics.Estudio;

public sealed record ResumenEstudioDto(
    Guid UsuarioId,
    int SesionesTotales,
    int MinutosTotales,
    int TemasEstudiados,
    DateOnly? UltimaSesion,
    IReadOnlyCollection<SesionPorTipoDto> SesionesPorTipo);

public sealed record SesionPorTipoDto(
    TipoSesion Tipo,
    int Sesiones,
    int Minutos);
