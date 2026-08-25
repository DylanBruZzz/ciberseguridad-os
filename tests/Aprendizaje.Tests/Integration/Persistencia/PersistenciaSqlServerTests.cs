using Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.VincularArtefactoAHerramienta;
using Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.VincularArtefactoATema;
using Aprendizaje.Aplicacion.Evidence.Laboratorios.VincularLaboratorioAHerramienta;
using Aprendizaje.Aplicacion.Evidence.Laboratorios.VincularLaboratorioATema;
using Aprendizaje.Aplicacion.Evidence.Proyectos.VincularProyectoAHerramienta;
using Aprendizaje.Aplicacion.Evidence.Proyectos.VincularProyectoATema;
using Aprendizaje.Aplicacion.Evidence.Writeups.VincularWriteupATema;
using Aprendizaje.Aplicacion.Resource.Recursos.VincularRecursoATema;
using Aprendizaje.Aplicacion.Study.SesionesEstudio.VincularHerramientaASesionEstudio;
using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Resource;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Study;
using Aprendizaje.Infraestructura.Persistencia.Repositorios;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Aprendizaje.Tests.Integration.Persistencia;

[Collection(ColeccionPersistenciaSqlServer.Nombre)]
public sealed class PersistenciaSqlServerTests
{
    [Fact]
    public async Task Tema_ObjetivosVacios_PersistenYMaterializanCorrectamente()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Dylan Tests", EmailUnico());
        var tema = Tema.Crear(usuario.Id, "Fundamentos de redes", TipoConocimiento.Conceptual);

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Temas.Add(tema);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var objetivosFisicos = await contexto.Database
                .SqlQueryRaw<string?>(
                    "SELECT [Objetivos] AS [Value] FROM [roadmap].[Tema] WHERE [Id] = {0}",
                    tema.Id)
                .SingleAsync(CancellationToken);

            Assert.Null(objetivosFisicos);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var rematerializado = await contexto.Temas.SingleAsync(t => t.Id == tema.Id, CancellationToken);

            Assert.NotNull(rematerializado.Objetivos);
            Assert.Empty(rematerializado.Objetivos);
        }
    }

    [Fact]
    public async Task SesionEstudio_RowVersion_CambiaTrasUpdate()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Dylan Tests", EmailUnico());
        var tema = Tema.Crear(usuario.Id, "Modelo OSI", TipoConocimiento.Conceptual);
        var sesion = SesionEstudio.Registrar(
            usuario.Id,
            tema.Id,
            new DateOnly(2026, 8, 24),
            45,
            TipoSesion.Teoria,
            "Estudio inicial");

        byte[] versionInicial;
        byte[] versionPosterior;

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Temas.Add(tema);
            contexto.SesionesEstudio.Add(sesion);
            await contexto.GuardarCambiosAsync(CancellationToken);

            versionInicial = Assert.IsType<byte[]>(sesion.VersionFila);
            Assert.NotEmpty(versionInicial);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var persistida = await contexto.SesionesEstudio.SingleAsync(s => s.Id == sesion.Id, CancellationToken);

            persistida.CorregirDuracion(60);
            await contexto.GuardarCambiosAsync(CancellationToken);

            versionPosterior = Assert.IsType<byte[]>(persistida.VersionFila);
            Assert.NotEmpty(versionPosterior);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var rematerializada = await contexto.SesionesEstudio
                .AsNoTracking()
                .SingleAsync(s => s.Id == sesion.Id, CancellationToken);

            Assert.Equal(60, rematerializada.DuracionMinutos);
            Assert.NotEqual(versionInicial, versionPosterior);
        }
    }

    [Fact]
    public async Task RecursoTema_VinculoNoSeDuplica()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Dylan Tests", EmailUnico());
        var tema = Tema.Crear(usuario.Id, "Modelo OSI", TipoConocimiento.Conceptual);
        var recurso = Recurso.Guardar(usuario.Id, TipoRecurso.Documentacion, "Documentación modelo OSI");

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Temas.Add(tema);
            contexto.Recursos.Add(recurso);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var casoUso = new VincularRecursoATemaCasoUso(
                new RecursoRepository(contexto),
                new TemaRepository(contexto),
                contexto);

            var primerResultado = await casoUso.EjecutarAsync(
                new VincularRecursoATemaSolicitud(recurso.Id, tema.Id),
                CancellationToken);
            var segundoResultado = await casoUso.EjecutarAsync(
                new VincularRecursoATemaSolicitud(recurso.Id, tema.Id),
                CancellationToken);

            Assert.Equal(VincularRecursoATemaEstado.Actualizado, primerResultado.Estado);
            Assert.Equal(VincularRecursoATemaEstado.Actualizado, segundoResultado.Estado);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var filas = await contexto.Database
                .SqlQueryRaw<int>(
                    "SELECT COUNT(*) AS [Value] FROM [resource].[RecursoTema] WHERE [RecursoId] = {0} AND [TemaId] = {1}",
                    recurso.Id,
                    tema.Id)
                .SingleAsync(CancellationToken);

            Assert.Equal(1, filas);
        }
    }

    [Fact]
    public async Task SesionHerramienta_VinculoNoSeDuplica()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Dylan Tests", EmailUnico());
        var tema = Tema.Crear(usuario.Id, "Modelo OSI", TipoConocimiento.Conceptual);
        var sesion = SesionEstudio.Registrar(
            usuario.Id,
            tema.Id,
            new DateOnly(2026, 8, 24),
            45,
            TipoSesion.Teoria,
            "Estudio inicial");
        var herramienta = Herramienta.Crear($"Wireshark Integration {Guid.CreateVersion7():N}");

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Temas.Add(tema);
            contexto.SesionesEstudio.Add(sesion);
            contexto.Herramientas.Add(herramienta);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var casoUso = new VincularHerramientaASesionEstudioCasoUso(
                new SesionEstudioRepository(contexto),
                new HerramientaRepository(contexto),
                contexto);

            var primerResultado = await casoUso.EjecutarAsync(
                new VincularHerramientaASesionEstudioSolicitud(sesion.Id, herramienta.Id),
                CancellationToken);
            var segundoResultado = await casoUso.EjecutarAsync(
                new VincularHerramientaASesionEstudioSolicitud(sesion.Id, herramienta.Id),
                CancellationToken);

            Assert.Equal(VincularHerramientaASesionEstudioEstado.Actualizado, primerResultado.Estado);
            Assert.Equal(VincularHerramientaASesionEstudioEstado.Actualizado, segundoResultado.Estado);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var filas = await contexto.Database
                .SqlQueryRaw<int>(
                    "SELECT COUNT(*) AS [Value] FROM [study].[SesionHerramienta] WHERE [SesionId] = {0} AND [HerramientaId] = {1}",
                    sesion.Id,
                    herramienta.Id)
                .SingleAsync(CancellationToken);

            Assert.Equal(1, filas);
        }
    }

    [Fact]
    public async Task LaboratorioTema_VinculoNoSeDuplica()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Dylan Tests", EmailUnico());
        var tema = Tema.Crear(usuario.Id, "Modelo OSI", TipoConocimiento.Conceptual);
        var laboratorio = Laboratorio.Crear(usuario.Id, "Análisis de tráfico OSI");

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Temas.Add(tema);
            contexto.Laboratorios.Add(laboratorio);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var casoUso = new VincularLaboratorioATemaCasoUso(
                new LaboratorioRepository(contexto),
                new TemaRepository(contexto),
                contexto);

            var primerResultado = await casoUso.EjecutarAsync(
                new VincularLaboratorioATemaSolicitud(laboratorio.Id, tema.Id),
                CancellationToken);
            var segundoResultado = await casoUso.EjecutarAsync(
                new VincularLaboratorioATemaSolicitud(laboratorio.Id, tema.Id),
                CancellationToken);

            Assert.Equal(VincularLaboratorioATemaEstado.Actualizado, primerResultado.Estado);
            Assert.Equal(VincularLaboratorioATemaEstado.Actualizado, segundoResultado.Estado);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var filas = await contexto.Database
                .SqlQueryRaw<int>(
                    "SELECT COUNT(*) AS [Value] FROM [evidence].[LaboratorioTema] WHERE [LaboratorioId] = {0} AND [TemaId] = {1}",
                    laboratorio.Id,
                    tema.Id)
                .SingleAsync(CancellationToken);

            Assert.Equal(1, filas);
        }
    }

    [Fact]
    public async Task LaboratorioHerramienta_VinculoNoSeDuplica()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Dylan Tests", EmailUnico());
        var laboratorio = Laboratorio.Crear(usuario.Id, "Análisis de tráfico OSI");
        var herramienta = Herramienta.Crear($"Wireshark Integration {Guid.CreateVersion7():N}");

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Laboratorios.Add(laboratorio);
            contexto.Herramientas.Add(herramienta);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var casoUso = new VincularLaboratorioAHerramientaCasoUso(
                new LaboratorioRepository(contexto),
                new HerramientaRepository(contexto),
                contexto);

            var primerResultado = await casoUso.EjecutarAsync(
                new VincularLaboratorioAHerramientaSolicitud(laboratorio.Id, herramienta.Id),
                CancellationToken);
            var segundoResultado = await casoUso.EjecutarAsync(
                new VincularLaboratorioAHerramientaSolicitud(laboratorio.Id, herramienta.Id),
                CancellationToken);

            Assert.Equal(VincularLaboratorioAHerramientaEstado.Actualizado, primerResultado.Estado);
            Assert.Equal(VincularLaboratorioAHerramientaEstado.Actualizado, segundoResultado.Estado);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var filas = await contexto.Database
                .SqlQueryRaw<int>(
                    "SELECT COUNT(*) AS [Value] FROM [evidence].[LaboratorioHerramienta] WHERE [LaboratorioId] = {0} AND [HerramientaId] = {1}",
                    laboratorio.Id,
                    herramienta.Id)
                .SingleAsync(CancellationToken);

            Assert.Equal(1, filas);
        }
    }

    [Fact]
    public async Task ProyectoTema_VinculoNoSeDuplica()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Dylan Tests", EmailUnico());
        var tema = Tema.Crear(usuario.Id, "Modelo OSI", TipoConocimiento.Conceptual);
        var proyecto = Proyecto.Crear(usuario.Id, "Analizador de tráfico OSI");

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Temas.Add(tema);
            contexto.Proyectos.Add(proyecto);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var casoUso = new VincularProyectoATemaCasoUso(
                new ProyectoRepository(contexto),
                new TemaRepository(contexto),
                contexto);

            var primerResultado = await casoUso.EjecutarAsync(
                new VincularProyectoATemaSolicitud(proyecto.Id, tema.Id),
                CancellationToken);
            var segundoResultado = await casoUso.EjecutarAsync(
                new VincularProyectoATemaSolicitud(proyecto.Id, tema.Id),
                CancellationToken);

            Assert.Equal(VincularProyectoATemaEstado.Actualizado, primerResultado.Estado);
            Assert.Equal(VincularProyectoATemaEstado.Actualizado, segundoResultado.Estado);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var filas = await contexto.Database
                .SqlQueryRaw<int>(
                    "SELECT COUNT(*) AS [Value] FROM [evidence].[ProyectoTema] WHERE [ProyectoId] = {0} AND [TemaId] = {1}",
                    proyecto.Id,
                    tema.Id)
                .SingleAsync(CancellationToken);

            Assert.Equal(1, filas);
        }
    }

    [Fact]
    public async Task ProyectoHerramienta_VinculoNoSeDuplica()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Dylan Tests", EmailUnico());
        var proyecto = Proyecto.Crear(usuario.Id, "Analizador de tráfico OSI");
        var herramienta = Herramienta.Crear($"Wireshark Integration {Guid.CreateVersion7():N}");

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Proyectos.Add(proyecto);
            contexto.Herramientas.Add(herramienta);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var casoUso = new VincularProyectoAHerramientaCasoUso(
                new ProyectoRepository(contexto),
                new HerramientaRepository(contexto),
                contexto);

            var primerResultado = await casoUso.EjecutarAsync(
                new VincularProyectoAHerramientaSolicitud(proyecto.Id, herramienta.Id),
                CancellationToken);
            var segundoResultado = await casoUso.EjecutarAsync(
                new VincularProyectoAHerramientaSolicitud(proyecto.Id, herramienta.Id),
                CancellationToken);

            Assert.Equal(VincularProyectoAHerramientaEstado.Actualizado, primerResultado.Estado);
            Assert.Equal(VincularProyectoAHerramientaEstado.Actualizado, segundoResultado.Estado);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var filas = await contexto.Database
                .SqlQueryRaw<int>(
                    "SELECT COUNT(*) AS [Value] FROM [evidence].[ProyectoHerramienta] WHERE [ProyectoId] = {0} AND [HerramientaId] = {1}",
                    proyecto.Id,
                    herramienta.Id)
                .SingleAsync(CancellationToken);

            Assert.Equal(1, filas);
        }
    }

    [Fact]
    public async Task Proyecto_RowVersion_SePueblaAlInsertar()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Dylan Tests", EmailUnico());
        var proyecto = Proyecto.Crear(usuario.Id, "Analizador de tráfico OSI");

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Proyectos.Add(proyecto);
            await contexto.GuardarCambiosAsync(CancellationToken);

            var versionInicial = Assert.IsType<byte[]>(proyecto.VersionFila);
            Assert.NotEmpty(versionInicial);
        }
    }

    [Fact]
    public async Task Proyecto_RowVersion_NoCambiaAlVincularTemaOHerramienta()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Dylan Tests", EmailUnico());
        var tema = Tema.Crear(usuario.Id, "Modelo OSI", TipoConocimiento.Conceptual);
        var proyecto = Proyecto.Crear(usuario.Id, "Analizador de tráfico OSI");
        var herramienta = Herramienta.Crear($"Wireshark Integration {Guid.CreateVersion7():N}");
        byte[] versionAntes;
        byte[] versionDespues;

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Temas.Add(tema);
            contexto.Proyectos.Add(proyecto);
            contexto.Herramientas.Add(herramienta);
            await contexto.GuardarCambiosAsync(CancellationToken);

            versionAntes = Assert.IsType<byte[]>(proyecto.VersionFila);
            Assert.NotEmpty(versionAntes);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var vincularTema = new VincularProyectoATemaCasoUso(
                new ProyectoRepository(contexto),
                new TemaRepository(contexto),
                contexto);
            var vincularHerramienta = new VincularProyectoAHerramientaCasoUso(
                new ProyectoRepository(contexto),
                new HerramientaRepository(contexto),
                contexto);

            await vincularTema.EjecutarAsync(
                new VincularProyectoATemaSolicitud(proyecto.Id, tema.Id),
                CancellationToken);
            await vincularHerramienta.EjecutarAsync(
                new VincularProyectoAHerramientaSolicitud(proyecto.Id, herramienta.Id),
                CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var rematerializado = await contexto.Proyectos
                .AsNoTracking()
                .SingleAsync(p => p.Id == proyecto.Id, CancellationToken);

            versionDespues = Assert.IsType<byte[]>(rematerializado.VersionFila);
            Assert.NotEmpty(versionDespues);
        }

        Assert.Equal(versionAntes, versionDespues);
    }

    [Fact]
    public async Task ArtefactoTema_VinculoNoSeDuplica()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Dylan Tests", EmailUnico());
        var tema = Tema.Crear(usuario.Id, "Modelo OSI", TipoConocimiento.Conceptual);
        var artefacto = ArtefactoTecnico.Crear(
            usuario.Id,
            TipoArtefacto.Cheatsheet,
            "Filtros Wireshark para análisis OSI");

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Temas.Add(tema);
            contexto.ArtefactosTecnicos.Add(artefacto);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var casoUso = new VincularArtefactoATemaCasoUso(
                new ArtefactoTecnicoRepository(contexto),
                new TemaRepository(contexto),
                contexto);

            var primerResultado = await casoUso.EjecutarAsync(
                new VincularArtefactoATemaSolicitud(artefacto.Id, tema.Id),
                CancellationToken);
            var segundoResultado = await casoUso.EjecutarAsync(
                new VincularArtefactoATemaSolicitud(artefacto.Id, tema.Id),
                CancellationToken);

            Assert.Equal(VincularArtefactoATemaEstado.Actualizado, primerResultado.Estado);
            Assert.Equal(VincularArtefactoATemaEstado.Actualizado, segundoResultado.Estado);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var filas = await contexto.Database
                .SqlQueryRaw<int>(
                    "SELECT COUNT(*) AS [Value] FROM [evidence].[ArtefactoTema] WHERE [ArtefactoTecnicoId] = {0} AND [TemaId] = {1}",
                    artefacto.Id,
                    tema.Id)
                .SingleAsync(CancellationToken);

            Assert.Equal(1, filas);
        }
    }

    [Fact]
    public async Task ArtefactoHerramienta_VinculoNoSeDuplica()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Dylan Tests", EmailUnico());
        var artefacto = ArtefactoTecnico.Crear(
            usuario.Id,
            TipoArtefacto.Cheatsheet,
            "Filtros Wireshark para análisis OSI");
        var herramienta = Herramienta.Crear($"Wireshark Integration {Guid.CreateVersion7():N}");

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.ArtefactosTecnicos.Add(artefacto);
            contexto.Herramientas.Add(herramienta);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var casoUso = new VincularArtefactoAHerramientaCasoUso(
                new ArtefactoTecnicoRepository(contexto),
                new HerramientaRepository(contexto),
                contexto);

            var primerResultado = await casoUso.EjecutarAsync(
                new VincularArtefactoAHerramientaSolicitud(artefacto.Id, herramienta.Id),
                CancellationToken);
            var segundoResultado = await casoUso.EjecutarAsync(
                new VincularArtefactoAHerramientaSolicitud(artefacto.Id, herramienta.Id),
                CancellationToken);

            Assert.Equal(VincularArtefactoAHerramientaEstado.Actualizado, primerResultado.Estado);
            Assert.Equal(VincularArtefactoAHerramientaEstado.Actualizado, segundoResultado.Estado);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var filas = await contexto.Database
                .SqlQueryRaw<int>(
                    "SELECT COUNT(*) AS [Value] FROM [evidence].[ArtefactoHerramienta] WHERE [ArtefactoTecnicoId] = {0} AND [HerramientaId] = {1}",
                    artefacto.Id,
                    herramienta.Id)
                .SingleAsync(CancellationToken);

            Assert.Equal(1, filas);
        }
    }

    [Fact]
    public async Task WriteupTema_VinculoNoSeDuplica()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Dylan Tests", EmailUnico());
        var tema = Tema.Crear(usuario.Id, "Modelo OSI", TipoConocimiento.Conceptual);
        var writeup = Writeup.Crear(usuario.Id, "Análisis del modelo OSI con Wireshark");

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Temas.Add(tema);
            contexto.Writeups.Add(writeup);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var casoUso = new VincularWriteupATemaCasoUso(
                new WriteupRepository(contexto),
                new TemaRepository(contexto),
                contexto);

            var primerResultado = await casoUso.EjecutarAsync(
                new VincularWriteupATemaSolicitud(writeup.Id, tema.Id),
                CancellationToken);
            var segundoResultado = await casoUso.EjecutarAsync(
                new VincularWriteupATemaSolicitud(writeup.Id, tema.Id),
                CancellationToken);

            Assert.Equal(VincularWriteupATemaEstado.Actualizado, primerResultado.Estado);
            Assert.Equal(VincularWriteupATemaEstado.Actualizado, segundoResultado.Estado);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var filas = await contexto.Database
                .SqlQueryRaw<int>(
                    "SELECT COUNT(*) AS [Value] FROM [evidence].[WriteupTema] WHERE [WriteupId] = {0} AND [TemaId] = {1}",
                    writeup.Id,
                    tema.Id)
                .SingleAsync(CancellationToken);

            Assert.Equal(1, filas);
        }
    }

    [Fact]
    public async Task CertificacionObtenida_FK_RechazaCertificacionInexistente()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Dylan Tests", EmailUnico());
        var certificacionObtenida = CertificacionObtenida.Registrar(
            usuario.Id,
            Guid.CreateVersion7(),
            new DateOnly(2026, 8, 25));

        await using var contexto = ambiente.CrearNuevoContexto();
        contexto.Usuarios.Add(usuario);
        contexto.CertificacionesObtenidas.Add(certificacionObtenida);

        await Assert.ThrowsAsync<DbUpdateException>(() => contexto.GuardarCambiosAsync(CancellationToken));
    }

    [Fact]
    public async Task CertificacionObtenida_PersisteValoresIniciales()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Dylan Tests", EmailUnico());
        var certificacion = Certificacion.Crear($"CompTIA Network+ Integration {Guid.CreateVersion7():N}", TipoCosto.Pago);
        var fechaObtencion = new DateOnly(2026, 8, 25);
        var certificacionObtenida = CertificacionObtenida.Registrar(usuario.Id, certificacion.Id, fechaObtencion);

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Certificaciones.Add(certificacion);
            contexto.CertificacionesObtenidas.Add(certificacionObtenida);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var persistida = await contexto.CertificacionesObtenidas
                .AsNoTracking()
                .SingleAsync(c => c.Id == certificacionObtenida.Id, CancellationToken);
            var estadoMadurez = await contexto.Database
                .SqlQueryRaw<string>(
                    "SELECT [EstadoMadurez] AS [Value] FROM [evidence].[CertificacionObtenida] WHERE [Id] = {0}",
                    certificacionObtenida.Id)
                .SingleAsync(CancellationToken);

            Assert.Equal(usuario.Id, persistida.UsuarioId);
            Assert.Equal(certificacion.Id, persistida.CertificacionId);
            Assert.Equal(fechaObtencion, persistida.FechaObtencion);
            Assert.Equal(EstadoMadurez.Documentado, persistida.EstadoMadurez);
            Assert.Equal(nameof(EstadoMadurez.Documentado), estadoMadurez);
            Assert.Null(persistida.EvidenciaUrl);
        }
    }

    [Fact]
    public async Task CertificacionObtenida_QueryFilter_OcultaEliminadoLogicamente()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Dylan Tests", EmailUnico());
        var certificacion = Certificacion.Crear($"CompTIA Network+ Integration {Guid.CreateVersion7():N}", TipoCosto.Pago);
        var certificacionObtenida = CertificacionObtenida.Registrar(
            usuario.Id,
            certificacion.Id,
            new DateOnly(2026, 8, 25));

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Certificaciones.Add(certificacion);
            contexto.CertificacionesObtenidas.Add(certificacionObtenida);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var persistida = await contexto.CertificacionesObtenidas
                .SingleAsync(c => c.Id == certificacionObtenida.Id, CancellationToken);

            persistida.MarcarComoEliminado();
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var consultaNormal = await contexto.CertificacionesObtenidas
                .FirstOrDefaultAsync(c => c.Id == certificacionObtenida.Id, CancellationToken);
            var consultaSinFiltro = await contexto.CertificacionesObtenidas
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == certificacionObtenida.Id, CancellationToken);

            Assert.Null(consultaNormal);
            Assert.NotNull(consultaSinFiltro);
            Assert.NotNull(consultaSinFiltro.FechaEliminacionUtc);
        }
    }

    [Fact]
    public async Task Tema_QueryFilter_OcultaEliminadoLogicamente()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Dylan Tests", EmailUnico());
        var tema = Tema.Crear(usuario.Id, "Modelo OSI", TipoConocimiento.Conceptual);

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Temas.Add(tema);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var activo = await contexto.Temas.SingleOrDefaultAsync(t => t.Id == tema.Id, CancellationToken);

            Assert.NotNull(activo);
            activo.MarcarComoEliminado();
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var consultaNormal = await contexto.Temas.SingleOrDefaultAsync(t => t.Id == tema.Id, CancellationToken);
            var consultaSinFiltro = await contexto.Temas
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(t => t.Id == tema.Id, CancellationToken);

            Assert.Null(consultaNormal);
            Assert.NotNull(consultaSinFiltro);
            Assert.NotNull(consultaSinFiltro.FechaEliminacionUtc);
        }
    }

    [Fact]
    public async Task SesionEstudio_FkTemaInvalida_EsRechazadaPorSqlServer()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Dylan Tests", EmailUnico());
        var sesion = SesionEstudio.Registrar(
            usuario.Id,
            Guid.CreateVersion7(),
            new DateOnly(2026, 8, 24),
            45,
            TipoSesion.Teoria);

        await using var contexto = ambiente.CrearNuevoContexto();
        contexto.Usuarios.Add(usuario);
        contexto.SesionesEstudio.Add(sesion);

        await Assert.ThrowsAsync<DbUpdateException>(() => contexto.GuardarCambiosAsync(CancellationToken));
    }

    private static string EmailUnico() => $"integracion-{Guid.CreateVersion7():N}@local.test";

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
