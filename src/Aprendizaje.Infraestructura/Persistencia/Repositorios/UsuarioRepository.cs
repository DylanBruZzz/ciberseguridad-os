using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Nucleo.Repositorios;

namespace Aprendizaje.Infraestructura.Persistencia.Repositorios;

public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly AprendizajeDbContext _context;

    public UsuarioRepository(AprendizajeDbContext context)
    {
        _context = context;
    }

    public void Agregar(Usuario usuario) => _context.Usuarios.Add(usuario);
}
