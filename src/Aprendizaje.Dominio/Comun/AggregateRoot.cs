namespace Aprendizaje.Dominio.Comun;

public abstract class AggregateRoot : Entidad
{
    private readonly List<IEventoDominio> _eventos = new();
    public IReadOnlyCollection<IEventoDominio> EventosDominio => _eventos.AsReadOnly();

    protected AggregateRoot() { }
    protected AggregateRoot(Guid id) : base(id) { }

    protected void RegistrarEvento(IEventoDominio evento) => _eventos.Add(evento);
    public void LimpiarEventos() => _eventos.Clear();
}
