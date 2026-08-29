using Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.ActualizarArtefactoTecnico;
using Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.CrearArtefactoTecnico;
using Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.EliminarArtefactoTecnico;
using Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.ListarArtefactosTecnicos;
using Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.ObtenerArtefactoTecnicoPorId;
using Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.VincularArtefactoAHerramienta;
using Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.VincularArtefactoATema;
using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Study;
using Aprendizaje.Tests.Soporte;
using Xunit;

namespace Aprendizaje.Tests.Application.Evidence;

public sealed class ArtefactosTecnicosApplicationTests
{
    [Fact]
    public async Task CrearArtefactoTecnico_DebeAgregarArtefactoYGuardar()
    {
        var artefactos = new FakeArtefactoTecnicoRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new CrearArtefactoTecnicoCasoUso(artefactos, unitOfWork);
        var usuarioId = Guid.CreateVersion7();

        var resultado = await casoUso.EjecutarAsync(
            new CrearArtefactoTecnicoSolicitud(
                usuarioId,
                TipoArtefacto.Cheatsheet,
                " Filtros Wireshark para análisis OSI "),
            CancellationToken);

        Assert.NotEqual(Guid.Empty, resultado.Id);
        Assert.Equal(usuarioId, resultado.UsuarioId);
        Assert.Equal(TipoArtefacto.Cheatsheet, resultado.TipoArtefacto);
        Assert.Equal("Filtros Wireshark para análisis OSI", resultado.Nombre);
        Assert.Null(resultado.ContenidoOUrl);
        Assert.Null(resultado.LenguajeTecnologia);
        Assert.Equal(EstadoMadurez.Borrador, resultado.EstadoMadurez);
        Assert.NotNull(await artefactos.ObtenerPorIdAsync(resultado.Id, CancellationToken));
        Assert.Equal(1, artefactos.AgregarLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task CrearArtefactoTecnico_DebeRechazarUsuarioVacioSinGuardar()
    {
        var artefactos = new FakeArtefactoTecnicoRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new CrearArtefactoTecnicoCasoUso(artefactos, unitOfWork);

        await Assert.ThrowsAsync<ArgumentException>(() => casoUso.EjecutarAsync(
            new CrearArtefactoTecnicoSolicitud(
                Guid.Empty,
                TipoArtefacto.Cheatsheet,
                "Filtros Wireshark para análisis OSI"),
            CancellationToken));

        Assert.Equal(0, artefactos.AgregarLlamadas);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ObtenerArtefactoTecnicoPorId_DebeRetornarArtefactoExistente()
    {
        var artefactos = new FakeArtefactoTecnicoRepository();
        var artefacto = CrearArtefactoTecnico();
        artefactos.Agregar(artefacto);
        var casoUso = new ObtenerArtefactoTecnicoPorIdCasoUso(artefactos);

        var resultado = await casoUso.EjecutarAsync(artefacto.Id, CancellationToken);

        Assert.True(resultado.Encontrado);
        Assert.NotNull(resultado.ArtefactoTecnico);
        Assert.Equal(artefacto.Id, resultado.ArtefactoTecnico.Id);
        Assert.Equal(artefacto.Nombre, resultado.ArtefactoTecnico.Nombre);
    }

    [Fact]
    public async Task ObtenerArtefactoTecnicoPorId_DebeRetornarNoEncontrado()
    {
        var artefactos = new FakeArtefactoTecnicoRepository();
        var casoUso = new ObtenerArtefactoTecnicoPorIdCasoUso(artefactos);

        var resultado = await casoUso.EjecutarAsync(Guid.CreateVersion7(), CancellationToken);

        Assert.False(resultado.Encontrado);
        Assert.Null(resultado.ArtefactoTecnico);
    }

    [Fact]
    public async Task ListarArtefactosTecnicos_DebeMapearColeccionDelRepository()
    {
        var artefactos = new FakeArtefactoTecnicoRepository();
        var usuarioId = Guid.CreateVersion7();
        var artefacto = CrearArtefactoTecnico(usuarioId);
        artefactos.Agregar(artefacto);
        artefactos.Agregar(CrearArtefactoTecnico(Guid.CreateVersion7()));
        var casoUso = new ListarArtefactosTecnicosCasoUso(artefactos);

        var resultado = await casoUso.EjecutarAsync(
            new ListarArtefactosTecnicosSolicitud(usuarioId),
            CancellationToken);

        var resumen = Assert.Single(resultado.ArtefactosTecnicos);
        Assert.Equal(artefacto.Id, resumen.Id);
        Assert.Equal(usuarioId, resumen.UsuarioId);
        Assert.Equal(artefacto.Nombre, resumen.Nombre);
        Assert.Equal(artefacto.TipoArtefacto, resumen.TipoArtefacto);
    }

    [Fact]
    public async Task VincularArtefactoATema_DebeRetornarArtefactoNoEncontradoSinGuardar()
    {
        var artefactos = new FakeArtefactoTecnicoRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new VincularArtefactoATemaCasoUso(artefactos, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularArtefactoATemaSolicitud(Guid.CreateVersion7(), Guid.CreateVersion7()),
            CancellationToken);

        Assert.Equal(VincularArtefactoATemaEstado.ArtefactoNoEncontrado, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularArtefactoATema_DebeRetornarTemaNoEncontradoSinGuardar()
    {
        var artefactos = new FakeArtefactoTecnicoRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var artefacto = CrearArtefactoTecnico();
        artefactos.Agregar(artefacto);
        var casoUso = new VincularArtefactoATemaCasoUso(artefactos, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularArtefactoATemaSolicitud(artefacto.Id, Guid.CreateVersion7()),
            CancellationToken);

        Assert.Equal(VincularArtefactoATemaEstado.TemaNoEncontrado, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularArtefactoATema_DebeRetornarUsuarioNoCoincideSinGuardar()
    {
        var artefactos = new FakeArtefactoTecnicoRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var artefacto = CrearArtefactoTecnico();
        var tema = Tema.Crear(Guid.CreateVersion7(), "Modelo OSI", TipoConocimiento.Conceptual);
        artefactos.Agregar(artefacto);
        temas.Agregar(tema);
        var casoUso = new VincularArtefactoATemaCasoUso(artefactos, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularArtefactoATemaSolicitud(artefacto.Id, tema.Id),
            CancellationToken);

        Assert.Equal(VincularArtefactoATemaEstado.UsuarioNoCoincide, resultado.Estado);
        Assert.Equal(0, artefactos.VincularTemaLlamadas);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularArtefactoATema_DebeVincularYGuardar()
    {
        var artefactos = new FakeArtefactoTecnicoRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuarioId = Guid.CreateVersion7();
        var artefacto = CrearArtefactoTecnico(usuarioId);
        var tema = Tema.Crear(usuarioId, "Modelo OSI", TipoConocimiento.Conceptual);
        artefactos.Agregar(artefacto);
        temas.Agregar(tema);
        var casoUso = new VincularArtefactoATemaCasoUso(artefactos, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularArtefactoATemaSolicitud(artefacto.Id, tema.Id),
            CancellationToken);

        Assert.Equal(VincularArtefactoATemaEstado.Actualizado, resultado.Estado);
        Assert.True(await artefactos.ExisteVinculoTemaAsync(artefacto.Id, tema.Id, CancellationToken));
        Assert.Equal(1, artefactos.VincularTemaLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularArtefactoATema_DebeSerIdempotenteSinGuardarDeNuevo()
    {
        var artefactos = new FakeArtefactoTecnicoRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuarioId = Guid.CreateVersion7();
        var artefacto = CrearArtefactoTecnico(usuarioId);
        var tema = Tema.Crear(usuarioId, "Modelo OSI", TipoConocimiento.Conceptual);
        artefactos.Agregar(artefacto);
        temas.Agregar(tema);
        var casoUso = new VincularArtefactoATemaCasoUso(artefactos, temas, unitOfWork);
        await casoUso.EjecutarAsync(new VincularArtefactoATemaSolicitud(artefacto.Id, tema.Id), CancellationToken);

        var resultado = await casoUso.EjecutarAsync(
            new VincularArtefactoATemaSolicitud(artefacto.Id, tema.Id),
            CancellationToken);

        Assert.Equal(VincularArtefactoATemaEstado.Actualizado, resultado.Estado);
        Assert.Equal(1, artefactos.VincularTemaLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularArtefactoAHerramienta_DebeRetornarArtefactoNoEncontradoSinGuardar()
    {
        var artefactos = new FakeArtefactoTecnicoRepository();
        var herramientas = new FakeHerramientaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new VincularArtefactoAHerramientaCasoUso(artefactos, herramientas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularArtefactoAHerramientaSolicitud(Guid.CreateVersion7(), Guid.CreateVersion7()),
            CancellationToken);

        Assert.Equal(VincularArtefactoAHerramientaEstado.ArtefactoNoEncontrado, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularArtefactoAHerramienta_DebeRetornarHerramientaNoEncontradaSinGuardar()
    {
        var artefactos = new FakeArtefactoTecnicoRepository();
        var herramientas = new FakeHerramientaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var artefacto = CrearArtefactoTecnico();
        artefactos.Agregar(artefacto);
        var casoUso = new VincularArtefactoAHerramientaCasoUso(artefactos, herramientas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularArtefactoAHerramientaSolicitud(artefacto.Id, Guid.CreateVersion7()),
            CancellationToken);

        Assert.Equal(VincularArtefactoAHerramientaEstado.HerramientaNoEncontrada, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularArtefactoAHerramienta_DebeVincularYGuardar()
    {
        var artefactos = new FakeArtefactoTecnicoRepository();
        var herramientas = new FakeHerramientaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var artefacto = CrearArtefactoTecnico();
        var herramienta = Herramienta.Crear("Wireshark");
        artefactos.Agregar(artefacto);
        herramientas.Agregar(herramienta);
        var casoUso = new VincularArtefactoAHerramientaCasoUso(artefactos, herramientas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularArtefactoAHerramientaSolicitud(artefacto.Id, herramienta.Id),
            CancellationToken);

        Assert.Equal(VincularArtefactoAHerramientaEstado.Actualizado, resultado.Estado);
        Assert.True(await artefactos.ExisteVinculoHerramientaAsync(artefacto.Id, herramienta.Id, CancellationToken));
        Assert.Equal(1, artefactos.VincularHerramientaLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularArtefactoAHerramienta_DebeSerIdempotenteSinGuardarDeNuevo()
    {
        var artefactos = new FakeArtefactoTecnicoRepository();
        var herramientas = new FakeHerramientaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var artefacto = CrearArtefactoTecnico();
        var herramienta = Herramienta.Crear("Wireshark");
        artefactos.Agregar(artefacto);
        herramientas.Agregar(herramienta);
        var casoUso = new VincularArtefactoAHerramientaCasoUso(artefactos, herramientas, unitOfWork);
        await casoUso.EjecutarAsync(
            new VincularArtefactoAHerramientaSolicitud(artefacto.Id, herramienta.Id),
            CancellationToken);

        var resultado = await casoUso.EjecutarAsync(
            new VincularArtefactoAHerramientaSolicitud(artefacto.Id, herramienta.Id),
            CancellationToken);

        Assert.Equal(VincularArtefactoAHerramientaEstado.Actualizado, resultado.Estado);
        Assert.Equal(1, artefactos.VincularHerramientaLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ActualizarArtefactoTecnico_DebeActualizarCamposMadurezYGuardar()
    {
        var artefactos = new FakeArtefactoTecnicoRepository();
        var usuarios = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuario = Usuario.Registrar("Usuario Artefacto", EmailUnico());
        var artefacto = CrearArtefactoTecnico(usuario.Id);
        usuarios.Agregar(usuario);
        artefactos.Agregar(artefacto);
        var casoUso = new ActualizarArtefactoTecnicoCasoUso(artefactos, usuarios, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(new ActualizarArtefactoTecnicoSolicitud(
            artefacto.Id,
            usuario.Id,
            TipoArtefacto.Script,
            " Artefacto corregido ",
            "https://example.local/artefacto",
            "PowerShell",
            EstadoMadurez.ListoPortafolio), CancellationToken);

        Assert.Equal(ActualizarArtefactoTecnicoEstado.Actualizado, resultado.Estado);
        Assert.Equal(TipoArtefacto.Script, artefacto.TipoArtefacto);
        Assert.Equal("Artefacto corregido", artefacto.Nombre);
        Assert.Equal("https://example.local/artefacto", artefacto.ContenidoOUrl);
        Assert.Equal("PowerShell", artefacto.LenguajeTecnologia);
        Assert.Equal(EstadoMadurez.ListoPortafolio, artefacto.EstadoMadurez);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ActualizarArtefactoTecnico_DebeRetornarNoEncontradoSinGuardar()
    {
        var usuarios = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuario = Usuario.Registrar("Usuario Artefacto", EmailUnico());
        usuarios.Agregar(usuario);
        var casoUso = new ActualizarArtefactoTecnicoCasoUso(
            new FakeArtefactoTecnicoRepository(),
            usuarios,
            unitOfWork);

        var resultado = await casoUso.EjecutarAsync(new ActualizarArtefactoTecnicoSolicitud(
            Guid.CreateVersion7(),
            usuario.Id,
            TipoArtefacto.Cheatsheet,
            "Artefacto",
            null,
            null,
            EstadoMadurez.Borrador), CancellationToken);

        Assert.Equal(ActualizarArtefactoTecnicoEstado.ArtefactoTecnicoNoEncontrado, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ActualizarArtefactoTecnico_DebeRetornarUsuarioNoEncontradoSinGuardar()
    {
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new ActualizarArtefactoTecnicoCasoUso(
            new FakeArtefactoTecnicoRepository(),
            new FakeUsuarioRepository(),
            unitOfWork);

        var resultado = await casoUso.EjecutarAsync(new ActualizarArtefactoTecnicoSolicitud(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            TipoArtefacto.Cheatsheet,
            "Artefacto",
            null,
            null,
            EstadoMadurez.Borrador), CancellationToken);

        Assert.Equal(ActualizarArtefactoTecnicoEstado.UsuarioNoEncontrado, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ActualizarArtefactoTecnico_DebeRetornarUsuarioNoCoincideSinGuardar()
    {
        var artefactos = new FakeArtefactoTecnicoRepository();
        var usuarios = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuarioA = Usuario.Registrar("Usuario A", EmailUnico());
        var usuarioB = Usuario.Registrar("Usuario B", EmailUnico());
        var artefacto = CrearArtefactoTecnico(usuarioA.Id);
        usuarios.Agregar(usuarioA);
        usuarios.Agregar(usuarioB);
        artefactos.Agregar(artefacto);
        var casoUso = new ActualizarArtefactoTecnicoCasoUso(artefactos, usuarios, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(new ActualizarArtefactoTecnicoSolicitud(
            artefacto.Id,
            usuarioB.Id,
            TipoArtefacto.Script,
            "No persistir",
            null,
            null,
            EstadoMadurez.Publicado), CancellationToken);

        Assert.Equal(ActualizarArtefactoTecnicoEstado.UsuarioNoCoincide, resultado.Estado);
        Assert.Equal(TipoArtefacto.Cheatsheet, artefacto.TipoArtefacto);
        Assert.Equal("Filtros Wireshark para análisis OSI", artefacto.Nombre);
        Assert.Equal(EstadoMadurez.Borrador, artefacto.EstadoMadurez);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task EliminarArtefactoTecnico_DebeMarcarComoEliminadoYGuardar()
    {
        var artefactos = new FakeArtefactoTecnicoRepository();
        var usuarios = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuario = Usuario.Registrar("Usuario Artefacto", EmailUnico());
        var artefacto = CrearArtefactoTecnico(usuario.Id);
        usuarios.Agregar(usuario);
        artefactos.Agregar(artefacto);
        var casoUso = new EliminarArtefactoTecnicoCasoUso(artefactos, usuarios, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new EliminarArtefactoTecnicoSolicitud(artefacto.Id, usuario.Id),
            CancellationToken);

        Assert.Equal(EliminarArtefactoTecnicoEstado.Eliminado, resultado.Estado);
        Assert.NotNull(artefacto.FechaEliminacionUtc);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task EliminarArtefactoTecnico_DebeRetornarArtefactoNoEncontradoSinGuardar()
    {
        var usuarios = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuario = Usuario.Registrar("Usuario Artefacto", EmailUnico());
        usuarios.Agregar(usuario);
        var casoUso = new EliminarArtefactoTecnicoCasoUso(
            new FakeArtefactoTecnicoRepository(),
            usuarios,
            unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new EliminarArtefactoTecnicoSolicitud(Guid.CreateVersion7(), usuario.Id),
            CancellationToken);

        Assert.Equal(EliminarArtefactoTecnicoEstado.ArtefactoTecnicoNoEncontrado, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task EliminarArtefactoTecnico_DebeRetornarUsuarioNoCoincideSinGuardar()
    {
        var artefactos = new FakeArtefactoTecnicoRepository();
        var usuarios = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuarioA = Usuario.Registrar("Usuario A", EmailUnico());
        var usuarioB = Usuario.Registrar("Usuario B", EmailUnico());
        var artefacto = CrearArtefactoTecnico(usuarioA.Id);
        usuarios.Agregar(usuarioA);
        usuarios.Agregar(usuarioB);
        artefactos.Agregar(artefacto);
        var casoUso = new EliminarArtefactoTecnicoCasoUso(artefactos, usuarios, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new EliminarArtefactoTecnicoSolicitud(artefacto.Id, usuarioB.Id),
            CancellationToken);

        Assert.Equal(EliminarArtefactoTecnicoEstado.UsuarioNoCoincide, resultado.Estado);
        Assert.Null(artefacto.FechaEliminacionUtc);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    private static ArtefactoTecnico CrearArtefactoTecnico(Guid? usuarioId = null) =>
        ArtefactoTecnico.Crear(
            usuarioId ?? Guid.CreateVersion7(),
            TipoArtefacto.Cheatsheet,
            "Filtros Wireshark para análisis OSI");

    private static string EmailUnico() => $"artefacto-editable-{Guid.CreateVersion7():N}@local.test";

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
