using Aprendizaje.Dominio.Comun;

namespace Aprendizaje.Dominio.Study.Eventos;

public sealed record SesionRegistradaEvento(Guid SesionId, Guid TemaId, DateTime OcurrioEnUtc) : IEventoDominio;
