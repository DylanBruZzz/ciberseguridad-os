using Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.VincularArtefactoATema;
using Aprendizaje.Aplicacion.Evidence.Laboratorios.VincularLaboratorioATema;
using Aprendizaje.Aplicacion.Evidence.Proyectos.VincularProyectoATema;
using Aprendizaje.Aplicacion.Evidence.Writeups.VincularWriteupATema;
using Aprendizaje.Aplicacion.Resource.Recursos.VincularRecursoATema;
using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Resource;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Roadmap.ValueObjects;
using Aprendizaje.Dominio.Study;
using Aprendizaje.Infraestructura.Persistencia.Consultas;
using Aprendizaje.Infraestructura.Persistencia.Repositorios;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Aprendizaje.Tests.Integration.Persistencia;

[Collection(ColeccionPersistenciaSqlServer.Nombre)]
public sealed class ResumenTemaSqlServerTests
{
    [Fact]
    public async Task ResumenTema_TemaConDatos_AgregaDatosDirectosYAislaOtrosUsuarios()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuarioA = Usuario.Registrar("Usuario A", EmailUnico());
        var usuarioB = Usuario.Registrar("Usuario B", EmailUnico());
        var temaA = Tema.Crear(usuarioA.Id, "Modelo OSI", TipoConocimiento.Conceptual);
        var temaB = Tema.Crear(usuarioB.Id, "Linux", TipoConocimiento.Procedimental);
        var fechaInicio = new DateOnly(2026, 8, 25);
        var fechaFin = new DateOnly(2026, 10, 15);
        temaA.ActualizarDificultadPercibida(NivelPercepcion.Crear(4));
        temaA.ActualizarConfianza(NivelPercepcion.Crear(5));
        temaA.IniciarEstudio(fechaInicio);
        temaA.FinalizarEstudio(fechaFin);
        temaA.ConfigurarIntervaloRepaso(IntervaloRepaso.Crear(21));
        temaA.DefinirCriteriosRelevantes([TipoCriterio.Teoria, TipoCriterio.Practica, TipoCriterio.Laboratorio]);
        temaA.MarcarCriterio(TipoCriterio.Teoria);
        temaA.MarcarCriterio(TipoCriterio.Practica);

        var recursoVisible = Recurso.Guardar(usuarioA.Id, TipoRecurso.Documentacion, "Documentación modelo OSI");
        var recursoEliminado = Recurso.Guardar(usuarioA.Id, TipoRecurso.Cheatsheet, "Recurso eliminado");
        var recursoOtroUsuario = Recurso.Guardar(usuarioB.Id, TipoRecurso.Documentacion, "Recurso otro usuario");
        var laboratorioVisible = Laboratorio.Crear(usuarioA.Id, "Laboratorio OSI");
        var laboratorioEliminado = Laboratorio.Crear(usuarioA.Id, "Laboratorio eliminado");
        var laboratorioOtroUsuario = Laboratorio.Crear(usuarioB.Id, "Laboratorio otro usuario");
        var proyecto = Proyecto.Crear(usuarioA.Id, "Analizador OSI");
        var artefacto = ArtefactoTecnico.Crear(usuarioA.Id, TipoArtefacto.Cheatsheet, "Filtros Wireshark");
        var writeup = Writeup.Crear(usuarioA.Id, "Writeup OSI");
        var notaDirecta = Nota.SobreTema(usuarioA.Id, temaA.Id, "Nota directa del Tema");
        var notaOtroUsuario = Nota.SobreTema(usuarioB.Id, temaB.Id, "Nota otro usuario");

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.AddRange(usuarioA, usuarioB);
            contexto.Temas.AddRange(temaA, temaB);
            contexto.Recursos.AddRange(recursoVisible, recursoEliminado, recursoOtroUsuario);
            contexto.Laboratorios.AddRange(laboratorioVisible, laboratorioEliminado, laboratorioOtroUsuario);
            contexto.Proyectos.Add(proyecto);
            contexto.ArtefactosTecnicos.Add(artefacto);
            contexto.Writeups.Add(writeup);
            contexto.Notas.AddRange(notaDirecta, notaOtroUsuario);
            contexto.SesionesEstudio.AddRange(
                SesionEstudio.Registrar(usuarioA.Id, temaA.Id, new DateOnly(2026, 8, 20), 30, TipoSesion.Teoria),
                SesionEstudio.Registrar(usuarioA.Id, temaA.Id, new DateOnly(2026, 8, 22), 60, TipoSesion.Practica),
                SesionEstudio.Registrar(usuarioB.Id, temaB.Id, new DateOnly(2026, 8, 23), 999, TipoSesion.Laboratorio));
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            await VincularDatosAsync(
                contexto,
                temaA.Id,
                recursoVisible.Id,
                recursoEliminado.Id,
                laboratorioVisible.Id,
                laboratorioEliminado.Id,
                proyecto.Id,
                artefacto.Id,
                writeup.Id);

            await VincularDatosDeOtroUsuarioAsync(contexto, temaB.Id, recursoOtroUsuario.Id, laboratorioOtroUsuario.Id);

            var recurso = await contexto.Recursos.SingleAsync(r => r.Id == recursoEliminado.Id, CancellationToken);
            var laboratorio = await contexto.Laboratorios.SingleAsync(l => l.Id == laboratorioEliminado.Id, CancellationToken);
            recurso.MarcarComoEliminado();
            laboratorio.MarcarComoEliminado();
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var consulta = new ConsultaResumenTema(contexto);

            var resumen = await consulta.ObtenerAsync(usuarioA.Id, temaA.Id, CancellationToken);

            Assert.NotNull(resumen);
            Assert.Equal(temaA.Id, resumen.TemaId);
            Assert.Equal("Modelo OSI", resumen.Nombre);
            Assert.Equal(4, resumen.DificultadPercibida);
            Assert.Equal(5, resumen.Confianza);
            Assert.Equal(fechaInicio, resumen.FechaInicio);
            Assert.Equal(fechaFin, resumen.FechaFin);
            Assert.Equal(21, resumen.IntervaloRepasoDias);
            Assert.Equal(2, resumen.SesionesTotales);
            Assert.Equal(90, resumen.MinutosTotales);
            Assert.Equal(new DateOnly(2026, 8, 22), resumen.UltimaSesion);
            Assert.Equal(3, resumen.CriteriosTotales);
            Assert.Equal(2, resumen.CriteriosCumplidos);
            Assert.Equal(1, resumen.RecursosVinculados);
            Assert.Equal(1, resumen.Laboratorios);
            Assert.Equal(1, resumen.Proyectos);
            Assert.Equal(1, resumen.ArtefactosTecnicos);
            Assert.Equal(1, resumen.Writeups);
            Assert.Equal(1, resumen.Notas);
        }
    }

    [Fact]
    public async Task ResumenTema_TemaSinDatos_RetornaConteosVacios()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Usuario sin datos", EmailUnico());
        var tema = Tema.Crear(usuario.Id, "Modelo OSI", TipoConocimiento.Conceptual);

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Temas.Add(tema);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var consulta = new ConsultaResumenTema(contexto);

            var resumen = await consulta.ObtenerAsync(usuario.Id, tema.Id, CancellationToken);

            Assert.NotNull(resumen);
            Assert.Equal(tema.Id, resumen.TemaId);
            Assert.Equal("Modelo OSI", resumen.Nombre);
            Assert.Null(resumen.DificultadPercibida);
            Assert.Null(resumen.Confianza);
            Assert.Null(resumen.FechaInicio);
            Assert.Null(resumen.FechaFin);
            Assert.Null(resumen.IntervaloRepasoDias);
            Assert.Equal(0, resumen.SesionesTotales);
            Assert.Equal(0, resumen.MinutosTotales);
            Assert.Null(resumen.UltimaSesion);
            Assert.Equal(0, resumen.CriteriosTotales);
            Assert.Equal(0, resumen.CriteriosCumplidos);
            Assert.Equal(0, resumen.RecursosVinculados);
            Assert.Equal(0, resumen.Laboratorios);
            Assert.Equal(0, resumen.Proyectos);
            Assert.Equal(0, resumen.ArtefactosTecnicos);
            Assert.Equal(0, resumen.Writeups);
            Assert.Equal(0, resumen.Notas);
        }
    }

    [Fact]
    public async Task ResumenTema_TemaDeOtroUsuario_RetornaNull()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuarioA = Usuario.Registrar("Usuario A", EmailUnico());
        var usuarioB = Usuario.Registrar("Usuario B", EmailUnico());
        var temaB = Tema.Crear(usuarioB.Id, "Linux", TipoConocimiento.Procedimental);

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.AddRange(usuarioA, usuarioB);
            contexto.Temas.Add(temaB);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var consulta = new ConsultaResumenTema(contexto);

            var resumen = await consulta.ObtenerAsync(usuarioA.Id, temaB.Id, CancellationToken);

            Assert.Null(resumen);
        }
    }

    private static async Task VincularDatosAsync(
        Aprendizaje.Infraestructura.Persistencia.AprendizajeDbContext contexto,
        Guid temaId,
        Guid recursoVisibleId,
        Guid recursoEliminadoId,
        Guid laboratorioVisibleId,
        Guid laboratorioEliminadoId,
        Guid proyectoId,
        Guid artefactoId,
        Guid writeupId)
    {
        await new VincularRecursoATemaCasoUso(new RecursoRepository(contexto), new TemaRepository(contexto), contexto)
            .EjecutarAsync(new VincularRecursoATemaSolicitud(recursoVisibleId, temaId), CancellationToken);
        await new VincularRecursoATemaCasoUso(new RecursoRepository(contexto), new TemaRepository(contexto), contexto)
            .EjecutarAsync(new VincularRecursoATemaSolicitud(recursoEliminadoId, temaId), CancellationToken);
        await new VincularLaboratorioATemaCasoUso(new LaboratorioRepository(contexto), new TemaRepository(contexto), contexto)
            .EjecutarAsync(new VincularLaboratorioATemaSolicitud(laboratorioVisibleId, temaId), CancellationToken);
        await new VincularLaboratorioATemaCasoUso(new LaboratorioRepository(contexto), new TemaRepository(contexto), contexto)
            .EjecutarAsync(new VincularLaboratorioATemaSolicitud(laboratorioEliminadoId, temaId), CancellationToken);
        await new VincularProyectoATemaCasoUso(new ProyectoRepository(contexto), new TemaRepository(contexto), contexto)
            .EjecutarAsync(new VincularProyectoATemaSolicitud(proyectoId, temaId), CancellationToken);
        await new VincularArtefactoATemaCasoUso(new ArtefactoTecnicoRepository(contexto), new TemaRepository(contexto), contexto)
            .EjecutarAsync(new VincularArtefactoATemaSolicitud(artefactoId, temaId), CancellationToken);
        await new VincularWriteupATemaCasoUso(new WriteupRepository(contexto), new TemaRepository(contexto), contexto)
            .EjecutarAsync(new VincularWriteupATemaSolicitud(writeupId, temaId), CancellationToken);
    }

    private static async Task VincularDatosDeOtroUsuarioAsync(
        Aprendizaje.Infraestructura.Persistencia.AprendizajeDbContext contexto,
        Guid temaId,
        Guid recursoId,
        Guid laboratorioId)
    {
        await new VincularRecursoATemaCasoUso(new RecursoRepository(contexto), new TemaRepository(contexto), contexto)
            .EjecutarAsync(new VincularRecursoATemaSolicitud(recursoId, temaId), CancellationToken);
        await new VincularLaboratorioATemaCasoUso(new LaboratorioRepository(contexto), new TemaRepository(contexto), contexto)
            .EjecutarAsync(new VincularLaboratorioATemaSolicitud(laboratorioId, temaId), CancellationToken);
    }

    private static string EmailUnico() => $"analytics-tema-{Guid.CreateVersion7():N}@local.test";

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
