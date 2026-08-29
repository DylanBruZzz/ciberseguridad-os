using Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.VincularArtefactoAHerramienta;
using Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.VincularArtefactoATema;
using Aprendizaje.Aplicacion.Evidence.Laboratorios.VincularLaboratorioAHerramienta;
using Aprendizaje.Aplicacion.Evidence.Laboratorios.VincularLaboratorioATema;
using Aprendizaje.Aplicacion.Evidence.Proyectos.VincularProyectoAHerramienta;
using Aprendizaje.Aplicacion.Evidence.Proyectos.VincularProyectoATema;
using Aprendizaje.Aplicacion.Evidence.Writeups.VincularWriteupATema;
using Aprendizaje.Aplicacion.Portafolio;
using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Study;
using Aprendizaje.Infraestructura.Persistencia;
using Aprendizaje.Infraestructura.Persistencia.Consultas;
using Aprendizaje.Infraestructura.Persistencia.Repositorios;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Aprendizaje.Tests.Integration.Persistencia;

[Collection(ColeccionPersistenciaSqlServer.Nombre)]
public sealed class PortafolioSqlServerTests
{
    [Fact]
    public async Task Portafolio_IncluyeSoloEvidenceElegibleYAislaUsuarioConEnriquecimiento()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var datos = await CrearDatasetPortafolioAsync(ambiente);

        await using var contexto = ambiente.CrearNuevoContexto();
        var consulta = new ConsultaPortafolio(contexto);

        var portafolio = await consulta.ObtenerAsync(
            new ObtenerPortafolioSolicitud(datos.UsuarioA.Id),
            CancellationToken);

        Assert.Equal(datos.UsuarioA.Id, portafolio.UsuarioId);
        Assert.Equal(6, portafolio.Resumen.Total);
        Assert.Equal(1, portafolio.Resumen.Proyectos);
        Assert.Equal(1, portafolio.Resumen.Laboratorios);
        Assert.Equal(2, portafolio.Resumen.Writeups);
        Assert.Equal(1, portafolio.Resumen.ArtefactosTecnicos);
        Assert.Equal(1, portafolio.Resumen.CertificacionesObtenidas);
        Assert.Equal(4, portafolio.Resumen.ListosPortafolio);
        Assert.Equal(2, portafolio.Resumen.Publicados);

        var proyecto = Assert.Single(portafolio.Proyectos);
        Assert.Equal(datos.ProyectoElegible.Id, proyecto.ProyectoId);
        Assert.Equal("Proyecto listo", proyecto.Nombre);
        Assert.Equal(EstadoMadurez.ListoPortafolio, proyecto.EstadoMadurez);
        Assert.Equal(datos.TemaRedes.Id, Assert.Single(proyecto.Temas).TemaId);
        Assert.Equal(datos.HerramientaWireshark.Id, Assert.Single(proyecto.Herramientas).HerramientaId);

        var laboratorio = Assert.Single(portafolio.Laboratorios);
        Assert.Equal(datos.LaboratorioPublicado.Id, laboratorio.LaboratorioId);
        Assert.Equal(EstadoMadurez.Publicado, laboratorio.EstadoMadurez);
        Assert.Equal(datos.TemaWeb.Id, Assert.Single(laboratorio.Temas).TemaId);
        Assert.Equal(datos.HerramientaBurp.Id, Assert.Single(laboratorio.Herramientas).HerramientaId);

        Assert.Equal(
            ["Writeup reciente", "Writeup antiguo"],
            portafolio.Writeups.Select(w => w.Titulo).ToArray());
        Assert.All(portafolio.Writeups, w => Assert.NotEmpty(w.Temas));

        var artefacto = Assert.Single(portafolio.ArtefactosTecnicos);
        Assert.Equal(datos.ArtefactoPublicado.Id, artefacto.ArtefactoTecnicoId);
        Assert.Equal(datos.TemaRedes.Id, Assert.Single(artefacto.Temas).TemaId);
        Assert.Equal(datos.HerramientaWireshark.Id, Assert.Single(artefacto.Herramientas).HerramientaId);

        var certificacion = Assert.Single(portafolio.CertificacionesObtenidas);
        Assert.Equal(datos.CertificacionObtenidaLista.Id, certificacion.CertificacionObtenidaId);
        Assert.Equal(datos.Certificacion.Id, certificacion.CertificacionId);
        Assert.Equal("CompTIA Security+ Portfolio", certificacion.Nombre);
        Assert.Equal("CompTIA", certificacion.Proveedor);
        Assert.Equal(TipoCosto.Pago, certificacion.TipoCosto);
        Assert.Equal("https://example.local/certificacion", certificacion.EvidenciaUrl);

        Assert.DoesNotContain(portafolio.Proyectos, p => p.ProyectoId == datos.ProyectoBorrador.Id);
        Assert.DoesNotContain(portafolio.Laboratorios, l => l.LaboratorioId == datos.LaboratorioDocumentado.Id);
        Assert.DoesNotContain(portafolio.ArtefactosTecnicos, a => a.ArtefactoTecnicoId == datos.ArtefactoEliminado.Id);
        Assert.DoesNotContain(portafolio.Proyectos, p => p.ProyectoId == datos.ProyectoOtroUsuario.Id);
    }

    [Fact]
    public async Task Portafolio_FiltraPorTipoEvidenceYEstadoMadurezElegible()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var datos = await CrearDatasetPortafolioAsync(ambiente);

        await using var contexto = ambiente.CrearNuevoContexto();
        var consulta = new ConsultaPortafolio(contexto);

        var soloWriteups = await consulta.ObtenerAsync(
            new ObtenerPortafolioSolicitud(datos.UsuarioA.Id, TipoEvidencePortafolio.Writeup),
            CancellationToken);
        var soloPublicados = await consulta.ObtenerAsync(
            new ObtenerPortafolioSolicitud(datos.UsuarioA.Id, EstadoMadurez: EstadoMadurez.Publicado),
            CancellationToken);

        Assert.Equal(2, soloWriteups.Resumen.Total);
        Assert.Equal(2, soloWriteups.Resumen.Writeups);
        Assert.Empty(soloWriteups.Proyectos);
        Assert.Empty(soloWriteups.Laboratorios);
        Assert.Empty(soloWriteups.ArtefactosTecnicos);
        Assert.Empty(soloWriteups.CertificacionesObtenidas);

        Assert.Equal(2, soloPublicados.Resumen.Total);
        Assert.Equal(1, soloPublicados.Resumen.Laboratorios);
        Assert.Equal(1, soloPublicados.Resumen.ArtefactosTecnicos);
        Assert.Equal(0, soloPublicados.Resumen.ListosPortafolio);
        Assert.Equal(2, soloPublicados.Resumen.Publicados);
    }

    [Fact]
    public async Task Portafolio_UsuarioSinEvidenceElegible_RetornaVacio()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Usuario sin portfolio", EmailUnico());
        var proyectoBorrador = Proyecto.Crear(usuario.Id, "Proyecto borrador");

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Proyectos.Add(proyectoBorrador);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var portafolio = await new ConsultaPortafolio(contexto).ObtenerAsync(
                new ObtenerPortafolioSolicitud(usuario.Id),
                CancellationToken);

            Assert.Equal(usuario.Id, portafolio.UsuarioId);
            Assert.Equal(0, portafolio.Resumen.Total);
            Assert.Empty(portafolio.Proyectos);
            Assert.Empty(portafolio.Laboratorios);
            Assert.Empty(portafolio.Writeups);
            Assert.Empty(portafolio.ArtefactosTecnicos);
            Assert.Empty(portafolio.CertificacionesObtenidas);
        }
    }

    private static async Task<DatosPortafolio> CrearDatasetPortafolioAsync(AmbientePersistenciaSqlServer ambiente)
    {
        var usuarioA = Usuario.Registrar("Usuario A", EmailUnico());
        var usuarioB = Usuario.Registrar("Usuario B", EmailUnico());
        var temaRedes = Tema.Crear(usuarioA.Id, "Modelo OSI / TCP-IP", TipoConocimiento.Conceptual);
        var temaWeb = Tema.Crear(usuarioA.Id, "SQLi / XSS / SSRF", TipoConocimiento.Conceptual);
        var temaOtroUsuario = Tema.Crear(usuarioB.Id, "Linux", TipoConocimiento.Procedimental);
        var herramientaWireshark = Herramienta.Crear($"Wireshark Portfolio {Guid.CreateVersion7():N}");
        herramientaWireshark.ActualizarCategoria("Análisis de red");
        var herramientaBurp = Herramienta.Crear($"Burp Suite Portfolio {Guid.CreateVersion7():N}");
        herramientaBurp.ActualizarCategoria("Web");
        var certificacion = Certificacion.Crear("CompTIA Security+ Portfolio", TipoCosto.Pago);
        certificacion.ActualizarProveedor("CompTIA");

        var proyectoElegible = Proyecto.Crear(usuarioA.Id, "Proyecto listo");
        proyectoElegible.ActualizarDescripcion("Proyecto presentable");
        proyectoElegible.ActualizarRepositorioUrl("https://example.local/proyecto");
        proyectoElegible.ActualizarFechas(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 15));
        proyectoElegible.AvanzarEstado(EstadoProyecto.Documentado);
        proyectoElegible.AvanzarMadurez(EstadoMadurez.ListoPortafolio);

        var proyectoBorrador = Proyecto.Crear(usuarioA.Id, "Proyecto borrador");
        var proyectoOtroUsuario = Proyecto.Crear(usuarioB.Id, "Proyecto de otro usuario");
        proyectoOtroUsuario.AvanzarMadurez(EstadoMadurez.Publicado);

        var laboratorioPublicado = Laboratorio.Crear(usuarioA.Id, "Laboratorio publicado");
        laboratorioPublicado.ActualizarObjetivo("Validar explotación controlada");
        laboratorioPublicado.ActualizarEntornoVms("Kali + DVWA");
        laboratorioPublicado.RegistrarHallazgos("Hallazgo documentado");
        laboratorioPublicado.ActualizarTiempoInvertido(120);
        laboratorioPublicado.ActualizarFecha(new DateOnly(2026, 8, 20));
        laboratorioPublicado.AvanzarMadurez(EstadoMadurez.Publicado);

        var laboratorioDocumentado = Laboratorio.Crear(usuarioA.Id, "Laboratorio documentado");
        laboratorioDocumentado.AvanzarMadurez(EstadoMadurez.Documentado);

        var writeupAntiguo = Writeup.Crear(usuarioA.Id, "Writeup antiguo");
        writeupAntiguo.ActualizarPlataformaOrigen("Hack The Box");
        writeupAntiguo.ActualizarUrl("https://example.local/writeup-antiguo");
        writeupAntiguo.ActualizarFecha(new DateOnly(2026, 8, 10));
        writeupAntiguo.AvanzarMadurez(EstadoMadurez.ListoPortafolio);

        var writeupReciente = Writeup.Crear(usuarioA.Id, "Writeup reciente");
        writeupReciente.ActualizarPlataformaOrigen("TryHackMe");
        writeupReciente.ActualizarUrl("https://example.local/writeup-reciente");
        writeupReciente.ActualizarFecha(new DateOnly(2026, 8, 25));
        writeupReciente.AvanzarMadurez(EstadoMadurez.ListoPortafolio);

        var artefactoPublicado = ArtefactoTecnico.Crear(usuarioA.Id, TipoArtefacto.Script, "Script publicado");
        artefactoPublicado.ActualizarContenidoOUrl("https://example.local/script");
        artefactoPublicado.ActualizarLenguajeTecnologia("PowerShell");
        artefactoPublicado.AvanzarMadurez(EstadoMadurez.Publicado);

        var artefactoEliminado = ArtefactoTecnico.Crear(usuarioA.Id, TipoArtefacto.Cheatsheet, "Artefacto eliminado");
        artefactoEliminado.AvanzarMadurez(EstadoMadurez.Publicado);

        var certificacionObtenidaLista = CertificacionObtenida.Registrar(
            usuarioA.Id,
            certificacion.Id,
            new DateOnly(2026, 8, 28));
        certificacionObtenidaLista.ActualizarEvidenciaUrl("https://example.local/certificacion");
        certificacionObtenidaLista.AvanzarMadurez(EstadoMadurez.ListoPortafolio);

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.AddRange(usuarioA, usuarioB);
            contexto.Temas.AddRange(temaRedes, temaWeb, temaOtroUsuario);
            contexto.Herramientas.AddRange(herramientaWireshark, herramientaBurp);
            contexto.Certificaciones.Add(certificacion);
            contexto.Proyectos.AddRange(proyectoElegible, proyectoBorrador, proyectoOtroUsuario);
            contexto.Laboratorios.AddRange(laboratorioPublicado, laboratorioDocumentado);
            contexto.Writeups.AddRange(writeupAntiguo, writeupReciente);
            contexto.ArtefactosTecnicos.AddRange(artefactoPublicado, artefactoEliminado);
            contexto.CertificacionesObtenidas.Add(certificacionObtenidaLista);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            await new VincularProyectoATemaCasoUso(new ProyectoRepository(contexto), new TemaRepository(contexto), contexto)
                .EjecutarAsync(new VincularProyectoATemaSolicitud(proyectoElegible.Id, temaRedes.Id), CancellationToken);
            await new VincularProyectoAHerramientaCasoUso(
                    new ProyectoRepository(contexto),
                    new HerramientaRepository(contexto),
                    contexto)
                .EjecutarAsync(new VincularProyectoAHerramientaSolicitud(proyectoElegible.Id, herramientaWireshark.Id), CancellationToken);
            await new VincularProyectoATemaCasoUso(new ProyectoRepository(contexto), new TemaRepository(contexto), contexto)
                .EjecutarAsync(new VincularProyectoATemaSolicitud(proyectoOtroUsuario.Id, temaOtroUsuario.Id), CancellationToken);

            await new VincularLaboratorioATemaCasoUso(
                    new LaboratorioRepository(contexto),
                    new TemaRepository(contexto),
                    contexto)
                .EjecutarAsync(new VincularLaboratorioATemaSolicitud(laboratorioPublicado.Id, temaWeb.Id), CancellationToken);
            await new VincularLaboratorioAHerramientaCasoUso(
                    new LaboratorioRepository(contexto),
                    new HerramientaRepository(contexto),
                    contexto)
                .EjecutarAsync(new VincularLaboratorioAHerramientaSolicitud(laboratorioPublicado.Id, herramientaBurp.Id), CancellationToken);

            await new VincularWriteupATemaCasoUso(new WriteupRepository(contexto), new TemaRepository(contexto), contexto)
                .EjecutarAsync(new VincularWriteupATemaSolicitud(writeupAntiguo.Id, temaRedes.Id), CancellationToken);
            await new VincularWriteupATemaCasoUso(new WriteupRepository(contexto), new TemaRepository(contexto), contexto)
                .EjecutarAsync(new VincularWriteupATemaSolicitud(writeupReciente.Id, temaWeb.Id), CancellationToken);

            await new VincularArtefactoATemaCasoUso(
                    new ArtefactoTecnicoRepository(contexto),
                    new TemaRepository(contexto),
                    contexto)
                .EjecutarAsync(new VincularArtefactoATemaSolicitud(artefactoPublicado.Id, temaRedes.Id), CancellationToken);
            await new VincularArtefactoAHerramientaCasoUso(
                    new ArtefactoTecnicoRepository(contexto),
                    new HerramientaRepository(contexto),
                    contexto)
                .EjecutarAsync(new VincularArtefactoAHerramientaSolicitud(artefactoPublicado.Id, herramientaWireshark.Id), CancellationToken);

            var eliminado = await contexto.ArtefactosTecnicos.SingleAsync(
                a => a.Id == artefactoEliminado.Id,
                CancellationToken);
            eliminado.MarcarComoEliminado();
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        return new DatosPortafolio(
            usuarioA,
            usuarioB,
            temaRedes,
            temaWeb,
            herramientaWireshark,
            herramientaBurp,
            certificacion,
            proyectoElegible,
            proyectoBorrador,
            proyectoOtroUsuario,
            laboratorioPublicado,
            laboratorioDocumentado,
            writeupAntiguo,
            writeupReciente,
            artefactoPublicado,
            artefactoEliminado,
            certificacionObtenidaLista);
    }

    private static string EmailUnico() => $"portafolio-sql-{Guid.CreateVersion7():N}@local.test";

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    private sealed record DatosPortafolio(
        Usuario UsuarioA,
        Usuario UsuarioB,
        Tema TemaRedes,
        Tema TemaWeb,
        Herramienta HerramientaWireshark,
        Herramienta HerramientaBurp,
        Certificacion Certificacion,
        Proyecto ProyectoElegible,
        Proyecto ProyectoBorrador,
        Proyecto ProyectoOtroUsuario,
        Laboratorio LaboratorioPublicado,
        Laboratorio LaboratorioDocumentado,
        Writeup WriteupAntiguo,
        Writeup WriteupReciente,
        ArtefactoTecnico ArtefactoPublicado,
        ArtefactoTecnico ArtefactoEliminado,
        CertificacionObtenida CertificacionObtenidaLista);
}
