namespace Aprendizaje.Dominio.Nucleo.Repositorios;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Agregar(Usuario usuario);
}
