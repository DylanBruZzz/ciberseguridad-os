using System.Data.Common;
using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Roadmap.ValueObjects;
using Aprendizaje.Dominio.Study;
using Aprendizaje.Infraestructura.Persistencia;
using Aprendizaje.Infraestructura.Persistencia.Consultas.Roadmap;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace Aprendizaje.Tests.Integration.Persistencia;

[Collection(ColeccionPersistenciaSqlServer.Nombre)]
public sealed class RoadmapVistaV1SqlServerTests
{
    private static readonly DateTime AhoraUtc = new(2026, 9, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task RoadmapVista_UsuarioSinRoadmap_RetornaFasesVaciasYFaseActualNull()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Usuario sin roadmap", EmailUnico());

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var consulta = new ConsultaRoadmapVistaV1(contexto);

            var vista = await consulta.ObtenerAsync(usuario.Id, AhoraUtc, CancellationToken);

            Assert.Null(vista.FaseActualId);
            Assert.Equal(0, vista.ProgresoGlobalPorcentaje);
            Assert.Equal(0, vista.TotalTemas);
            Assert.Equal(0, vista.TemasDominados);
            Assert.Empty(vista.Fases);
        }
    }

    [Fact]
    public async Task RoadmapVista_ComponeFasesMetadataTemasCriteriosEstadoProgresoYRepaso()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Usuario Roadmap", EmailUnico());
        usuario.ConfigurarIntervaloRepasoDefecto(30);
        var otroUsuario = Usuario.Registrar("Otro usuario", EmailUnico());
        var faseUno = Fase.Crear(usuario.Id, "Fundamentos", 1);
        faseUno.ActualizarDescripcion("Base conceptual");
        faseUno.ConfigurarMetadataPedagogica(
            ["Entender redes"],
            ["Explicar TCP/IP"],
            1,
            2,
            "6 h/semana");
        var faseDos = Fase.Crear(usuario.Id, "Practica", 2);
        var faseOtroUsuario = Fase.Crear(otroUsuario.Id, "Ajena", 1);

        var temaSinCriterios = Tema.Crear(usuario.Id, "A - Sin criterios", TipoConocimiento.Conceptual);
        temaSinCriterios.AsignarFase(faseUno.Id);
        var temaParcial = Tema.Crear(usuario.Id, "B - Parcial", TipoConocimiento.Procedimental);
        temaParcial.AsignarFase(faseUno.Id);
        temaParcial.AsignarTemaPadre(temaSinCriterios.Id);
        temaParcial.ActualizarDescripcion("Practicar subnetting");
        temaParcial.ActualizarDificultadPercibida(NivelPercepcion.Crear(4));
        temaParcial.ActualizarConfianza(NivelPercepcion.Crear(2));
        temaParcial.ConfigurarIntervaloRepaso(IntervaloRepaso.Crear(15));
        temaParcial.DefinirCriteriosRelevantes([Criterio(TipoCriterio.Teoria), Criterio(TipoCriterio.Practica)]);
        temaParcial.MarcarCriterio(TipoCriterio.Teoria);

        var temaDominado = Tema.Crear(usuario.Id, "C - Dominado", TipoConocimiento.Conceptual);
        temaDominado.AsignarFase(faseDos.Id);
        temaDominado.DefinirCriteriosRelevantes([Criterio(TipoCriterio.Teoria), Criterio(TipoCriterio.Explicacion)]);
        temaDominado.MarcarCriterio(TipoCriterio.Teoria);
        temaDominado.MarcarCriterio(TipoCriterio.Explicacion);

        var temaEnRepaso = Tema.Crear(usuario.Id, "D - Repaso", TipoConocimiento.Herramienta);
        temaEnRepaso.AsignarFase(faseDos.Id);
        temaEnRepaso.ConfigurarIntervaloRepaso(IntervaloRepaso.Crear(10));
        temaEnRepaso.DefinirCriteriosRelevantes([Criterio(TipoCriterio.Teoria), Criterio(TipoCriterio.Laboratorio)]);
        temaEnRepaso.MarcarCriterio(TipoCriterio.Teoria);
        temaEnRepaso.MarcarCriterio(TipoCriterio.Laboratorio);

        var temaEliminado = Tema.Crear(usuario.Id, "No debe aparecer", TipoConocimiento.Conceptual);
        temaEliminado.AsignarFase(faseUno.Id);
        temaEliminado.MarcarComoEliminado();
        var temaAjeno = Tema.Crear(otroUsuario.Id, "Tema ajeno", TipoConocimiento.Conceptual);
        temaAjeno.AsignarFase(faseOtroUsuario.Id);

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.AddRange(usuario, otroUsuario);
            contexto.Fases.AddRange(faseDos, faseUno, faseOtroUsuario);
            contexto.Temas.AddRange(
                temaSinCriterios,
                temaParcial,
                temaDominado,
                temaEnRepaso,
                temaEliminado,
                temaAjeno);
            contexto.SesionesEstudio.AddRange(
                SesionEstudio.Registrar(usuario.Id, temaParcial.Id, new DateOnly(2026, 8, 25), 30, TipoSesion.Practica),
                SesionEstudio.Registrar(usuario.Id, temaEnRepaso.Id, new DateOnly(2026, 8, 1), 45, TipoSesion.Repaso),
                SesionEstudio.Registrar(otroUsuario.Id, temaAjeno.Id, new DateOnly(2026, 8, 1), 999, TipoSesion.Practica));
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var consulta = new ConsultaRoadmapVistaV1(contexto);

            var vista = await consulta.ObtenerAsync(usuario.Id, AhoraUtc, CancellationToken);

            Assert.Equal(faseUno.Id, vista.FaseActualId);
            Assert.Equal(83, vista.ProgresoGlobalPorcentaje);
            Assert.Equal(4, vista.TotalTemas);
            Assert.Equal(2, vista.TemasDominados);
            Assert.Collection(
                vista.Fases,
                fase =>
                {
                    Assert.Equal(faseUno.Id, fase.Id);
                    Assert.Equal(1, fase.Orden);
                    Assert.Equal("Fundamentos", fase.Nombre);
                    Assert.Equal("Base conceptual", fase.Descripcion);
                    Assert.Equal(["Entender redes"], fase.Objetivos);
                    Assert.Equal(["Explicar TCP/IP"], fase.CriteriosAvance);
                    Assert.Equal(1, fase.MesInicioRecomendado);
                    Assert.Equal(2, fase.MesFinRecomendado);
                    Assert.Equal("6 h/semana", fase.CargaSemanalRecomendada);
                    Assert.Equal(2, fase.TotalTemas);
                    Assert.Equal(0, fase.TemasDominados);
                    Assert.Equal(50, fase.ProgresoPorcentaje);
                    Assert.False(fase.EstaCompletada);
                    Assert.True(fase.EsFaseActual);
                },
                fase =>
                {
                    Assert.Equal(faseDos.Id, fase.Id);
                    Assert.Equal(2, fase.TotalTemas);
                    Assert.Equal(2, fase.TemasDominados);
                    Assert.Equal(100, fase.ProgresoPorcentaje);
                    Assert.True(fase.EstaCompletada);
                    Assert.False(fase.EsFaseActual);
                });

            var temasFaseUno = vista.Fases.First().Temas.ToArray();
            Assert.Collection(
                temasFaseUno,
                tema =>
                {
                    Assert.Equal(temaSinCriterios.Id, tema.Id);
                    Assert.Equal(EstadoTema.NoIniciado, tema.Estado);
                    Assert.Equal(0, tema.CriteriosTotal);
                    Assert.Equal(0, tema.CriteriosCumplidos);
                    Assert.Equal(0, tema.ProgresoPorcentaje);
                    Assert.Null(tema.ProximaFechaRepaso);
                    Assert.False(tema.RepasoRecomendado);
                },
                tema =>
                {
                    Assert.Equal(temaParcial.Id, tema.Id);
                    Assert.Equal(temaSinCriterios.Id, tema.TemaPadreId);
                    Assert.Equal(TipoConocimiento.Procedimental, tema.TipoConocimiento);
                    Assert.Equal("Practicar subnetting", tema.Descripcion);
                    Assert.Equal(4, tema.DificultadPercibida);
                    Assert.Equal(2, tema.Confianza);
                    Assert.Equal(15, tema.IntervaloRepasoDias);
                    Assert.Equal(EstadoTema.EnPractica, tema.Estado);
                    Assert.Equal(2, tema.CriteriosTotal);
                    Assert.Equal(1, tema.CriteriosCumplidos);
                    Assert.Equal(50, tema.ProgresoPorcentaje);
                    Assert.Equal(new DateOnly(2026, 8, 25), tema.UltimaSesion);
                    Assert.Equal(new DateOnly(2026, 9, 9), tema.ProximaFechaRepaso);
                    Assert.False(tema.RepasoRecomendado);
                });

            var temasFaseDos = vista.Fases.Last().Temas.ToArray();
            Assert.Collection(
                temasFaseDos,
                tema =>
                {
                    Assert.Equal(temaDominado.Id, tema.Id);
                    Assert.Equal(30, tema.IntervaloRepasoDias);
                    Assert.Equal(EstadoTema.Dominado, tema.Estado);
                    Assert.Equal(100, tema.ProgresoPorcentaje);
                    Assert.Null(tema.ProximaFechaRepaso);
                    Assert.False(tema.RepasoRecomendado);
                },
                tema =>
                {
                    Assert.Equal(temaEnRepaso.Id, tema.Id);
                    Assert.Equal(10, tema.IntervaloRepasoDias);
                    Assert.Equal(EstadoTema.EnRepaso, tema.Estado);
                    Assert.Equal(100, tema.ProgresoPorcentaje);
                    Assert.Equal(new DateOnly(2026, 8, 1), tema.UltimaSesion);
                    Assert.Equal(new DateOnly(2026, 8, 11), tema.ProximaFechaRepaso);
                    Assert.True(tema.RepasoRecomendado);
                });
        }
    }

    [Fact]
    public async Task RoadmapVista_FaseActual_UsaPrimeraIncompletaYSiTodasCompletasUsaUltima()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Usuario fases", EmailUnico());
        var faseUno = Fase.Crear(usuario.Id, "Fase 1", 1);
        var faseDos = Fase.Crear(usuario.Id, "Fase 2", 2);
        var faseTres = Fase.Crear(usuario.Id, "Fase 3", 3);
        var temaUno = CrearTemaDominado(usuario.Id, faseUno.Id, "Tema 1");
        var temaDos = CrearTemaDominado(usuario.Id, faseDos.Id, "Tema 2");
        var temaTres = CrearTemaParcial(usuario.Id, faseTres.Id, "Tema 3");

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Fases.AddRange(faseUno, faseDos, faseTres);
            contexto.Temas.AddRange(temaUno, temaDos, temaTres);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var consulta = new ConsultaRoadmapVistaV1(contexto);
            var vista = await consulta.ObtenerAsync(usuario.Id, AhoraUtc, CancellationToken);

            Assert.Equal(faseTres.Id, vista.FaseActualId);
            Assert.Equal([false, false, true], vista.Fases.Select(f => f.EsFaseActual));
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var tema = await contexto.Temas
                .Include(t => t.Criterios)
                .SingleAsync(t => t.Id == temaTres.Id, CancellationToken);
            tema.MarcarCriterio(TipoCriterio.Practica);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var consulta = new ConsultaRoadmapVistaV1(contexto);
            var vista = await consulta.ObtenerAsync(usuario.Id, AhoraUtc, CancellationToken);

            Assert.Equal(faseTres.Id, vista.FaseActualId);
            Assert.True(vista.Fases.All(f => f.EstaCompletada));
            Assert.Equal([false, false, true], vista.Fases.Select(f => f.EsFaseActual));
        }
    }

    [Fact]
    public async Task RoadmapVista_FaseActual_NoRetrocedePorTemaEnRepasoConCriteriosCompletos()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Usuario repaso estructural", EmailUnico());
        var faseAnterior = Fase.Crear(usuario.Id, "Fase anterior", 1);
        var faseActual = Fase.Crear(usuario.Id, "Fase actual", 2);
        var temaEnRepaso = CrearTemaDominado(usuario.Id, faseAnterior.Id, "Tema vencido");
        temaEnRepaso.ConfigurarIntervaloRepaso(IntervaloRepaso.Crear(7));
        var temaParcial = CrearTemaParcial(usuario.Id, faseActual.Id, "Tema parcial");

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Fases.AddRange(faseAnterior, faseActual);
            contexto.Temas.AddRange(temaEnRepaso, temaParcial);
            contexto.SesionesEstudio.Add(
                SesionEstudio.Registrar(
                    usuario.Id,
                    temaEnRepaso.Id,
                    new DateOnly(2026, 8, 1),
                    30,
                    TipoSesion.Repaso));
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var consulta = new ConsultaRoadmapVistaV1(contexto);

            var vista = await consulta.ObtenerAsync(usuario.Id, AhoraUtc, CancellationToken);

            Assert.Equal(faseActual.Id, vista.FaseActualId);
            var faseAnteriorVista = vista.Fases.First();
            var temaVista = Assert.Single(faseAnteriorVista.Temas);
            Assert.Equal(EstadoTema.EnRepaso, temaVista.Estado);
            Assert.Equal(100, temaVista.ProgresoPorcentaje);
            Assert.True(temaVista.RepasoRecomendado);
            Assert.Equal(1, faseAnteriorVista.TemasDominados);
            Assert.Equal(100, faseAnteriorVista.ProgresoPorcentaje);
            Assert.True(faseAnteriorVista.EstaCompletada);
            Assert.False(faseAnteriorVista.EsFaseActual);
        }
    }

    [Fact]
    public async Task RoadmapVista_NodoPadreSinCriterios_NoReduceProgresoDeFaseSiTieneHijosEvaluables()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Usuario jerarquia", EmailUnico());
        var fase = Fase.Crear(usuario.Id, "Fase jerarquica", 1);
        var padre = Tema.Crear(usuario.Id, "Bloque organizativo", TipoConocimiento.Conceptual);
        padre.AsignarFase(fase.Id);
        var hijo = CrearTemaDominado(usuario.Id, fase.Id, "Tema aprendible");
        hijo.AsignarTemaPadre(padre.Id);

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Fases.Add(fase);
            contexto.Temas.AddRange(padre, hijo);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var consulta = new ConsultaRoadmapVistaV1(contexto);

            var vista = await consulta.ObtenerAsync(usuario.Id, AhoraUtc, CancellationToken);

            var faseVista = Assert.Single(vista.Fases);
            Assert.Equal(2, faseVista.TotalTemas);
            Assert.Equal(1, faseVista.TemasDominados);
            Assert.Equal(100, faseVista.ProgresoPorcentaje);
            Assert.True(faseVista.EstaCompletada);
            Assert.Equal(100, vista.ProgresoGlobalPorcentaje);
            Assert.Equal(2, vista.TotalTemas);
            Assert.Equal(1, vista.TemasDominados);

            var padreVista = faseVista.Temas.Single(t => t.Id == padre.Id);
            Assert.Equal(0, padreVista.CriteriosTotal);
            Assert.Equal(0, padreVista.ProgresoPorcentaje);
        }
    }

    [Fact]
    public async Task RoadmapVista_FaseVacia_NoSeConsideraCompletada()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Usuario fase vacia", EmailUnico());
        var fase = Fase.Crear(usuario.Id, "Fase vacia", 1);

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Fases.Add(fase);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var consulta = new ConsultaRoadmapVistaV1(contexto);

            var vista = await consulta.ObtenerAsync(usuario.Id, AhoraUtc, CancellationToken);

            var faseVista = Assert.Single(vista.Fases);
            Assert.Equal(fase.Id, vista.FaseActualId);
            Assert.Equal(0, faseVista.TotalTemas);
            Assert.Equal(0, faseVista.ProgresoPorcentaje);
            Assert.False(faseVista.EstaCompletada);
            Assert.True(faseVista.EsFaseActual);
        }
    }

    [Fact]
    public async Task RoadmapVista_FasesUnoACuatro_OrdenaTemasPorSecuenciaPedagogicaV1()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Usuario orden pedagogico", EmailUnico());
        var faseUno = Fase.Crear(usuario.Id, "Fundamentos de Informática y Redes", 1);
        var faseCuatro = Fase.Crear(usuario.Id, "Red Team · Ethical Hacking · Pentesting", 4);
        var faseCinco = Fase.Crear(usuario.Id, "Fase fuera de ordering V1", 5);
        var temasFaseUno = new[]
        {
            CrearTemaSinCriterios(usuario.Id, faseUno.Id, "Wireshark basics"),
            CrearTemaSinCriterios(usuario.Id, faseUno.Id, "Bash scripting"),
            CrearTemaSinCriterios(usuario.Id, faseUno.Id, "Modelo OSI / TCP-IP"),
            CrearTemaSinCriterios(usuario.Id, faseUno.Id, "Routing & Switching"),
        };
        var temasFaseCuatro = new[]
        {
            CrearTemaSinCriterios(usuario.Id, faseCuatro.Id, "Burp Suite Pro"),
            CrearTemaSinCriterios(usuario.Id, faseCuatro.Id, "SQLi / XSS / SSRF"),
            CrearTemaSinCriterios(usuario.Id, faseCuatro.Id, "Nmap / Nessus avanzado"),
            CrearTemaSinCriterios(usuario.Id, faseCuatro.Id, "OWASP Top 10"),
        };
        var temasFaseCinco = new[]
        {
            CrearTemaSinCriterios(usuario.Id, faseCinco.Id, "Z tema"),
            CrearTemaSinCriterios(usuario.Id, faseCinco.Id, "A tema"),
        };

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Fases.AddRange(faseUno, faseCuatro, faseCinco);
            contexto.Temas.AddRange(temasFaseUno.Concat(temasFaseCuatro).Concat(temasFaseCinco));
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var consulta = new ConsultaRoadmapVistaV1(contexto);

            var vista = await consulta.ObtenerAsync(usuario.Id, AhoraUtc, CancellationToken);

            Assert.Equal(
                [
                    "Modelo OSI / TCP-IP",
                    "Routing & Switching",
                    "Bash scripting",
                    "Wireshark basics",
                ],
                vista.Fases.Single(f => f.Id == faseUno.Id).Temas.Select(t => t.Nombre));
            Assert.Equal(
                [
                    "Nmap / Nessus avanzado",
                    "OWASP Top 10",
                    "SQLi / XSS / SSRF",
                    "Burp Suite Pro",
                ],
                vista.Fases.Single(f => f.Id == faseCuatro.Id).Temas.Select(t => t.Nombre));
            Assert.Equal(
                ["A tema", "Z tema"],
                vista.Fases.Single(f => f.Id == faseCinco.Id).Temas.Select(t => t.Nombre));
        }
    }

    [Fact]
    public async Task RoadmapVista_ConsultaUsaNumeroConstanteDeComandos()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Usuario performance", EmailUnico());
        var fase = Fase.Crear(usuario.Id, "Fase", 1);
        var temas = Enumerable
            .Range(1, 8)
            .Select(i => CrearTemaParcial(usuario.Id, fase.Id, $"Tema {i:00}"))
            .ToArray();

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Fases.Add(fase);
            contexto.Temas.AddRange(temas);
            contexto.SesionesEstudio.AddRange(
                temas.Select(t => SesionEstudio.Registrar(
                    usuario.Id,
                    t.Id,
                    new DateOnly(2026, 8, 20),
                    25,
                    TipoSesion.Practica)));
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        var contador = new ContadorComandosInterceptor();
        await using (var contexto = CrearContextoContado(contador))
        {
            var consulta = new ConsultaRoadmapVistaV1(contexto);

            _ = await consulta.ObtenerAsync(usuario.Id, AhoraUtc, CancellationToken);
        }

        Assert.True(contador.ComandosLectura <= 4, $"Se esperaban como maximo 4 comandos; se ejecutaron {contador.ComandosLectura}.");
    }

    private static Tema CrearTemaDominado(Guid usuarioId, Guid faseId, string nombre)
    {
        var tema = Tema.Crear(usuarioId, nombre, TipoConocimiento.Conceptual);
        tema.AsignarFase(faseId);
        tema.DefinirCriteriosRelevantes([Criterio(TipoCriterio.Teoria), Criterio(TipoCriterio.Practica)]);
        tema.MarcarCriterio(TipoCriterio.Teoria);
        tema.MarcarCriterio(TipoCriterio.Practica);

        return tema;
    }

    private static Tema CrearTemaParcial(Guid usuarioId, Guid faseId, string nombre)
    {
        var tema = Tema.Crear(usuarioId, nombre, TipoConocimiento.Conceptual);
        tema.AsignarFase(faseId);
        tema.DefinirCriteriosRelevantes([Criterio(TipoCriterio.Teoria), Criterio(TipoCriterio.Practica)]);
        tema.MarcarCriterio(TipoCriterio.Teoria);

        return tema;
    }

    private static Tema CrearTemaSinCriterios(Guid usuarioId, Guid faseId, string nombre)
    {
        var tema = Tema.Crear(usuarioId, nombre, TipoConocimiento.Conceptual);
        tema.AsignarFase(faseId);

        return tema;
    }

    private static AprendizajeDbContext CrearContextoContado(ContadorComandosInterceptor contador)
    {
        var opciones = new DbContextOptionsBuilder<AprendizajeDbContext>()
            .UseSqlServer(AmbientePersistenciaSqlServer.ConnectionString)
            .AddInterceptors(contador)
            .Options;

        return new AprendizajeDbContext(opciones);
    }

    private sealed class ContadorComandosInterceptor : DbCommandInterceptor
    {
        public int ComandosLectura { get; private set; }

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            ComandosLectura++;

            return base.ReaderExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            ComandosLectura++;

            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }
    }

    private static string EmailUnico() => $"roadmap-vista-{Guid.CreateVersion7():N}@local.test";

    private static DefinicionCriterioTema Criterio(TipoCriterio tipo) =>
        new(tipo, $"Descripcion {tipo}");

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
