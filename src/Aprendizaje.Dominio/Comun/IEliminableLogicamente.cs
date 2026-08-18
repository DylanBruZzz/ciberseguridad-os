namespace Aprendizaje.Dominio.Comun;

public interface IEliminableLogicamente
{
    DateTime? FechaEliminacionUtc { get; }
    void MarcarComoEliminado();
}
