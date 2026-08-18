using Aprendizaje.Dominio.Comun;

namespace Aprendizaje.Dominio.Roadmap.Eventos;

public sealed record TemaDominadoEvento(Guid TemaId, DateTime OcurrioEnUtc) : IEventoDominio;
