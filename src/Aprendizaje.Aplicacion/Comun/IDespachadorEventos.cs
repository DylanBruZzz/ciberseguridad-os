using Aprendizaje.Dominio.Comun;

namespace Aprendizaje.Aplicacion.Comun;

/// <summary>
/// Contrato de despacho de Domain Events (convención 16). Se define aquí, en Aplicación, e
/// implementa en Infraestructura — el dominio no conoce esta interfaz, solo produce eventos
/// mediante AggregateRoot.RegistrarEvento. La implementación concreta (envoltorio sobre
/// MediatR u otro publicador) queda diferida a la Fase 3, según la convención 16.3.
/// </summary>
public interface IDespachadorEventos
{
    Task PublicarAsync(IEventoDominio evento, CancellationToken cancellationToken = default);
}
