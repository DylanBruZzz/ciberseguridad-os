using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Nucleo.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace Aprendizaje.Infraestructura.Persistencia.Repositorios;

public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly AprendizajeDbContext _context;

    public UsuarioRepository(AprendizajeDbContext context)
    {
        _context = context;
    }

    public Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Usuario>> ListarVisiblesAsync(CancellationToken cancellationToken = default) =>
        await _context.Usuarios
            .OrderBy(u => u.FechaRegistro)
            .ThenBy(u => u.Id)
            .ToListAsync(cancellationToken);

    public void Agregar(Usuario usuario) => _context.Usuarios.Add(usuario);
}
