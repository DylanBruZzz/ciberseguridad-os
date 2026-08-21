using Aprendizaje.Dominio.Roadmap;
using Xunit;

namespace Aprendizaje.Tests.Dominio.Roadmap;

public sealed class TemaTests
{
    [Fact]
    public void Crear_DebeExponerObjetivosComoColeccionVacia()
    {
        var tema = Tema.Crear(Guid.CreateVersion7(), "Modelo OSI", TipoConocimiento.Conceptual);

        Assert.NotNull(tema.Objetivos);
        Assert.Empty(tema.Objetivos);
    }
}
