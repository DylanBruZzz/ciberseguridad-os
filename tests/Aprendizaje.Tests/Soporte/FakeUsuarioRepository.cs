using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Nucleo.Repositorios;

namespace Aprendizaje.Tests.Soporte;

internal sealed class FakeUsuarioRepository : IUsuarioRepository
{
    private readonly Dictionary<Guid, Usuario> _usuarios = [];

    public Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_usuarios.GetValueOrDefault(id));

    public void Agregar(Usuario usuario) => _usuarios[usuario.Id] = usuario;
}
