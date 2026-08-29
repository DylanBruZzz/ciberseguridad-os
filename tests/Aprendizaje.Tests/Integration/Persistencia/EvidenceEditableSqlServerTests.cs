using Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.ActualizarArtefactoTecnico;
using Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.EliminarArtefactoTecnico;
using Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.VincularArtefactoAHerramienta;
using Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.VincularArtefactoATema;
using Aprendizaje.Aplicacion.Evidence.CertificacionesObtenidas.ActualizarCertificacionObtenida;
using Aprendizaje.Aplicacion.Evidence.CertificacionesObtenidas.EliminarCertificacionObtenida;
using Aprendizaje.Aplicacion.Evidence.Laboratorios.ActualizarLaboratorio;
using Aprendizaje.Aplicacion.Evidence.Laboratorios.EliminarLaboratorio;
using Aprendizaje.Aplicacion.Evidence.Laboratorios.VincularLaboratorioAHerramienta;
using Aprendizaje.Aplicacion.Evidence.Laboratorios.VincularLaboratorioATema;
using Aprendizaje.Aplicacion.Evidence.Proyectos.ActualizarProyecto;
using Aprendizaje.Aplicacion.Evidence.Proyectos.EliminarProyecto;
using Aprendizaje.Aplicacion.Evidence.Proyectos.VincularProyectoAHerramienta;
using Aprendizaje.Aplicacion.Evidence.Proyectos.VincularProyectoATema;
using Aprendizaje.Aplicacion.Evidence.Writeups.ActualizarWriteup;
using Aprendizaje.Aplicacion.Evidence.Writeups.EliminarWriteup;
using Aprendizaje.Aplicacion.Evidence.Writeups.VincularWriteupATema;
using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Study;
using Aprendizaje.Infraestructura.Persistencia;
using Aprendizaje.Infraestructura.Persistencia.Repositorios;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Aprendizaje.Tests.Integration.Persistencia;

[Collection(ColeccionPersistenciaSqlServer.Nombre)]
public sealed class EvidenceEditableSqlServerTests
{
    [Fact]
    public async Task ProyectoEditable_ActualizacionMadurezSoftDeleteYPreservaRelaciones()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Usuario Proyecto", EmailUnico());
        var tema = Tema.Crear(usuario.Id, "Modelo OSI / TCP-IP", TipoConocimiento.Conceptual);
        var herramienta = Herramienta.Crear($"Wireshark Evidence {Guid.CreateVersion7():N}");
        var proyecto = Proyecto.Crear(usuario.Id, "Proyecto original");

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Temas.Add(tema);
            contexto.Herramientas.Add(herramienta);
            contexto.Proyectos.Add(proyecto);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            await new VincularProyectoATemaCasoUso(new ProyectoRepository(contexto), new TemaRepository(contexto), contexto)
                .EjecutarAsync(new VincularProyectoATemaSolicitud(proyecto.Id, tema.Id), CancellationToken);
            await new VincularProyectoAHerramientaCasoUso(
                    new ProyectoRepository(contexto),
                    new HerramientaRepository(contexto),
                    contexto)
                .EjecutarAsync(new VincularProyectoAHerramientaSolicitud(proyecto.Id, herramienta.Id), CancellationToken);

            var resultado = await CrearActualizarProyecto(contexto).EjecutarAsync(new ActualizarProyectoSolicitud(
                proyecto.Id,
                usuario.Id,
                " Proyecto actualizado ",
                "Descripción actualizada",
                EstadoProyecto.Documentado,
                EstadoMadurez.ListoPortafolio,
                "https://example.local/proyecto",
                new DateOnly(2026, 8, 1),
                new DateOnly(2026, 8, 20)), CancellationToken);

            Assert.Equal(ActualizarProyectoEstado.Actualizado, resultado.Estado);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var persistido = await contexto.Proyectos.AsNoTracking().SingleAsync(p => p.Id == proyecto.Id, CancellationToken);

            Assert.Equal("Proyecto actualizado", persistido.Nombre);
            Assert.Equal("Descripción actualizada", persistido.Descripcion);
            Assert.Equal(EstadoProyecto.Documentado, persistido.Estado);
            Assert.Equal(EstadoMadurez.ListoPortafolio, persistido.EstadoMadurez);
            Assert.Equal("https://example.local/proyecto", persistido.RepositorioUrl);
            Assert.Equal(new DateOnly(2026, 8, 1), persistido.FechaInicio);
            Assert.Equal(new DateOnly(2026, 8, 20), persistido.FechaFin);
            Assert.Equal(1, await ContarVinculoAsync(contexto, "evidence", "ProyectoTema", "ProyectoId", proyecto.Id, "TemaId", tema.Id));
            Assert.Equal(1, await ContarVinculoAsync(
                contexto,
                "evidence",
                "ProyectoHerramienta",
                "ProyectoId",
                proyecto.Id,
                "HerramientaId",
                herramienta.Id));
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var eliminado = await CrearEliminarProyecto(contexto).EjecutarAsync(
                new EliminarProyectoSolicitud(proyecto.Id, usuario.Id),
                CancellationToken);

            Assert.Equal(EliminarProyectoEstado.Eliminado, eliminado.Estado);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            Assert.Null(await contexto.Proyectos.SingleOrDefaultAsync(p => p.Id == proyecto.Id, CancellationToken));
            var fisico = await contexto.Proyectos
                .IgnoreQueryFilters()
                .SingleAsync(p => p.Id == proyecto.Id, CancellationToken);

            Assert.NotNull(fisico.FechaEliminacionUtc);
            Assert.Equal(1, await ContarVinculoAsync(contexto, "evidence", "ProyectoTema", "ProyectoId", proyecto.Id, "TemaId", tema.Id));
        }
    }

    [Fact]
    public async Task LaboratorioEditable_ActualizacionMadurezSoftDeleteYPreservaRelaciones()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Usuario Laboratorio", EmailUnico());
        var tema = Tema.Crear(usuario.Id, "DNS, DHCP, HTTP/S", TipoConocimiento.Conceptual);
        var herramienta = Herramienta.Crear($"Nmap Evidence {Guid.CreateVersion7():N}");
        var laboratorio = Laboratorio.Crear(usuario.Id, "Laboratorio original");

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Temas.Add(tema);
            contexto.Herramientas.Add(herramienta);
            contexto.Laboratorios.Add(laboratorio);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            await new VincularLaboratorioATemaCasoUso(
                    new LaboratorioRepository(contexto),
                    new TemaRepository(contexto),
                    contexto)
                .EjecutarAsync(new VincularLaboratorioATemaSolicitud(laboratorio.Id, tema.Id), CancellationToken);
            await new VincularLaboratorioAHerramientaCasoUso(
                    new LaboratorioRepository(contexto),
                    new HerramientaRepository(contexto),
                    contexto)
                .EjecutarAsync(new VincularLaboratorioAHerramientaSolicitud(laboratorio.Id, herramienta.Id), CancellationToken);

            var resultado = await CrearActualizarLaboratorio(contexto).EjecutarAsync(new ActualizarLaboratorioSolicitud(
                laboratorio.Id,
                usuario.Id,
                " Laboratorio actualizado ",
                "Objetivo actualizado",
                "Kali + Windows",
                "Hallazgos actualizados",
                95,
                new DateOnly(2026, 8, 25),
                EstadoMadurez.ListoPortafolio), CancellationToken);

            Assert.Equal(ActualizarLaboratorioEstado.Actualizado, resultado.Estado);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var persistido = await contexto.Laboratorios.AsNoTracking()
                .SingleAsync(l => l.Id == laboratorio.Id, CancellationToken);

            Assert.Equal("Laboratorio actualizado", persistido.Nombre);
            Assert.Equal("Objetivo actualizado", persistido.Objetivo);
            Assert.Equal("Kali + Windows", persistido.EntornoVms);
            Assert.Equal("Hallazgos actualizados", persistido.Hallazgos);
            Assert.Equal(95, persistido.TiempoInvertidoMinutos);
            Assert.Equal(new DateOnly(2026, 8, 25), persistido.Fecha);
            Assert.Equal(EstadoMadurez.ListoPortafolio, persistido.EstadoMadurez);
            Assert.Equal(1, await ContarVinculoAsync(
                contexto,
                "evidence",
                "LaboratorioHerramienta",
                "LaboratorioId",
                laboratorio.Id,
                "HerramientaId",
                herramienta.Id));
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var eliminado = await CrearEliminarLaboratorio(contexto).EjecutarAsync(
                new EliminarLaboratorioSolicitud(laboratorio.Id, usuario.Id),
                CancellationToken);

            Assert.Equal(EliminarLaboratorioEstado.Eliminado, eliminado.Estado);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            Assert.Null(await contexto.Laboratorios.SingleOrDefaultAsync(l => l.Id == laboratorio.Id, CancellationToken));
            var fisico = await contexto.Laboratorios
                .IgnoreQueryFilters()
                .SingleAsync(l => l.Id == laboratorio.Id, CancellationToken);

            Assert.NotNull(fisico.FechaEliminacionUtc);
            Assert.Equal(1, await ContarVinculoAsync(
                contexto,
                "evidence",
                "LaboratorioTema",
                "LaboratorioId",
                laboratorio.Id,
                "TemaId",
                tema.Id));
        }
    }

    [Fact]
    public async Task WriteupEditable_ActualizacionMadurezSoftDeleteYPreservaTema()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Usuario Writeup", EmailUnico());
        var tema = Tema.Crear(usuario.Id, "Splunk / Wazuh", TipoConocimiento.Herramienta);
        var writeup = Writeup.Crear(usuario.Id, "Writeup original");

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Temas.Add(tema);
            contexto.Writeups.Add(writeup);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            await new VincularWriteupATemaCasoUso(new WriteupRepository(contexto), new TemaRepository(contexto), contexto)
                .EjecutarAsync(new VincularWriteupATemaSolicitud(writeup.Id, tema.Id), CancellationToken);

            var resultado = await CrearActualizarWriteup(contexto).EjecutarAsync(new ActualizarWriteupSolicitud(
                writeup.Id,
                usuario.Id,
                " Writeup actualizado ",
                "Hack The Box",
                "https://example.local/writeup",
                new DateOnly(2026, 8, 26),
                EstadoMadurez.ListoPortafolio), CancellationToken);

            Assert.Equal(ActualizarWriteupEstado.Actualizado, resultado.Estado);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var persistido = await contexto.Writeups.AsNoTracking().SingleAsync(w => w.Id == writeup.Id, CancellationToken);

            Assert.Equal("Writeup actualizado", persistido.Titulo);
            Assert.Equal("Hack The Box", persistido.PlataformaOrigen);
            Assert.Equal("https://example.local/writeup", persistido.Url);
            Assert.Equal(new DateOnly(2026, 8, 26), persistido.Fecha);
            Assert.Equal(EstadoMadurez.ListoPortafolio, persistido.EstadoMadurez);
            Assert.Equal(1, await ContarVinculoAsync(contexto, "evidence", "WriteupTema", "WriteupId", writeup.Id, "TemaId", tema.Id));
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var eliminado = await CrearEliminarWriteup(contexto).EjecutarAsync(
                new EliminarWriteupSolicitud(writeup.Id, usuario.Id),
                CancellationToken);

            Assert.Equal(EliminarWriteupEstado.Eliminado, eliminado.Estado);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            Assert.Null(await contexto.Writeups.SingleOrDefaultAsync(w => w.Id == writeup.Id, CancellationToken));
            var fisico = await contexto.Writeups
                .IgnoreQueryFilters()
                .SingleAsync(w => w.Id == writeup.Id, CancellationToken);

            Assert.NotNull(fisico.FechaEliminacionUtc);
        }
    }

    [Fact]
    public async Task ArtefactoTecnicoEditable_ActualizacionMadurezSoftDeleteYPreservaRelaciones()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Usuario Artefacto", EmailUnico());
        var tema = Tema.Crear(usuario.Id, "SQLi / XSS / SSRF", TipoConocimiento.Conceptual);
        var herramienta = Herramienta.Crear($"Burp Evidence {Guid.CreateVersion7():N}");
        var artefacto = ArtefactoTecnico.Crear(usuario.Id, TipoArtefacto.Cheatsheet, "Artefacto original");

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Temas.Add(tema);
            contexto.Herramientas.Add(herramienta);
            contexto.ArtefactosTecnicos.Add(artefacto);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            await new VincularArtefactoATemaCasoUso(
                    new ArtefactoTecnicoRepository(contexto),
                    new TemaRepository(contexto),
                    contexto)
                .EjecutarAsync(new VincularArtefactoATemaSolicitud(artefacto.Id, tema.Id), CancellationToken);
            await new VincularArtefactoAHerramientaCasoUso(
                    new ArtefactoTecnicoRepository(contexto),
                    new HerramientaRepository(contexto),
                    contexto)
                .EjecutarAsync(new VincularArtefactoAHerramientaSolicitud(artefacto.Id, herramienta.Id), CancellationToken);

            var resultado = await CrearActualizarArtefacto(contexto).EjecutarAsync(new ActualizarArtefactoTecnicoSolicitud(
                artefacto.Id,
                usuario.Id,
                TipoArtefacto.Script,
                " Artefacto actualizado ",
                "https://example.local/artefacto",
                "PowerShell",
                EstadoMadurez.ListoPortafolio), CancellationToken);

            Assert.Equal(ActualizarArtefactoTecnicoEstado.Actualizado, resultado.Estado);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var persistido = await contexto.ArtefactosTecnicos.AsNoTracking()
                .SingleAsync(a => a.Id == artefacto.Id, CancellationToken);

            Assert.Equal(TipoArtefacto.Script, persistido.TipoArtefacto);
            Assert.Equal("Artefacto actualizado", persistido.Nombre);
            Assert.Equal("https://example.local/artefacto", persistido.ContenidoOUrl);
            Assert.Equal("PowerShell", persistido.LenguajeTecnologia);
            Assert.Equal(EstadoMadurez.ListoPortafolio, persistido.EstadoMadurez);
            Assert.Equal(1, await ContarVinculoAsync(contexto, "evidence", "ArtefactoTema", "ArtefactoTecnicoId", artefacto.Id, "TemaId", tema.Id));
            Assert.Equal(1, await ContarVinculoAsync(
                contexto,
                "evidence",
                "ArtefactoHerramienta",
                "ArtefactoTecnicoId",
                artefacto.Id,
                "HerramientaId",
                herramienta.Id));
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var eliminado = await CrearEliminarArtefacto(contexto).EjecutarAsync(
                new EliminarArtefactoTecnicoSolicitud(artefacto.Id, usuario.Id),
                CancellationToken);

            Assert.Equal(EliminarArtefactoTecnicoEstado.Eliminado, eliminado.Estado);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            Assert.Null(await contexto.ArtefactosTecnicos.SingleOrDefaultAsync(a => a.Id == artefacto.Id, CancellationToken));
            var fisico = await contexto.ArtefactosTecnicos
                .IgnoreQueryFilters()
                .SingleAsync(a => a.Id == artefacto.Id, CancellationToken);

            Assert.NotNull(fisico.FechaEliminacionUtc);
        }
    }

    [Fact]
    public async Task CertificacionObtenidaEditable_ActualizacionMadurezSoftDeleteYPreservaCatalogo()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Usuario Certificación", EmailUnico());
        var certificacion = Certificacion.Crear($"CompTIA Security+ Evidence {Guid.CreateVersion7():N}", TipoCosto.Pago);
        var obtenida = CertificacionObtenida.Registrar(usuario.Id, certificacion.Id, new DateOnly(2026, 8, 27));

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Certificaciones.Add(certificacion);
            contexto.CertificacionesObtenidas.Add(obtenida);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var resultado = await CrearActualizarCertificacionObtenida(contexto).EjecutarAsync(
                new ActualizarCertificacionObtenidaSolicitud(
                    obtenida.Id,
                    usuario.Id,
                    "https://example.local/certificado",
                    EstadoMadurez.ListoPortafolio),
                CancellationToken);

            Assert.Equal(ActualizarCertificacionObtenidaEstado.Actualizada, resultado.Estado);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var persistida = await contexto.CertificacionesObtenidas.AsNoTracking()
                .SingleAsync(c => c.Id == obtenida.Id, CancellationToken);

            Assert.Equal(certificacion.Id, persistida.CertificacionId);
            Assert.Equal(new DateOnly(2026, 8, 27), persistida.FechaObtencion);
            Assert.Equal("https://example.local/certificado", persistida.EvidenciaUrl);
            Assert.Equal(EstadoMadurez.ListoPortafolio, persistida.EstadoMadurez);
            Assert.Equal(1, await contexto.Certificaciones.CountAsync(c => c.Id == certificacion.Id, CancellationToken));
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var eliminado = await CrearEliminarCertificacionObtenida(contexto).EjecutarAsync(
                new EliminarCertificacionObtenidaSolicitud(obtenida.Id, usuario.Id),
                CancellationToken);

            Assert.Equal(EliminarCertificacionObtenidaEstado.Eliminada, eliminado.Estado);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            Assert.Null(await contexto.CertificacionesObtenidas.SingleOrDefaultAsync(c => c.Id == obtenida.Id, CancellationToken));
            var fisica = await contexto.CertificacionesObtenidas
                .IgnoreQueryFilters()
                .SingleAsync(c => c.Id == obtenida.Id, CancellationToken);

            Assert.NotNull(fisica.FechaEliminacionUtc);
        }
    }

    [Fact]
    public async Task EvidenceEditable_OwnershipIncorrectoNoActualizaNiElimina()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuarioA = Usuario.Registrar("Usuario A", EmailUnico());
        var usuarioB = Usuario.Registrar("Usuario B", EmailUnico());
        var certificacion = Certificacion.Crear($"Certificación Ownership {Guid.CreateVersion7():N}", TipoCosto.Pago);
        var proyecto = Proyecto.Crear(usuarioA.Id, "Proyecto original");
        var laboratorio = Laboratorio.Crear(usuarioA.Id, "Laboratorio original");
        var writeup = Writeup.Crear(usuarioA.Id, "Writeup original");
        var artefacto = ArtefactoTecnico.Crear(usuarioA.Id, TipoArtefacto.Cheatsheet, "Artefacto original");
        var obtenida = CertificacionObtenida.Registrar(usuarioA.Id, certificacion.Id, new DateOnly(2026, 8, 27));

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.AddRange(usuarioA, usuarioB);
            contexto.Certificaciones.Add(certificacion);
            contexto.Proyectos.Add(proyecto);
            contexto.Laboratorios.Add(laboratorio);
            contexto.Writeups.Add(writeup);
            contexto.ArtefactosTecnicos.Add(artefacto);
            contexto.CertificacionesObtenidas.Add(obtenida);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            Assert.Equal(ActualizarProyectoEstado.UsuarioNoCoincide, (await CrearActualizarProyecto(contexto)
                .EjecutarAsync(new ActualizarProyectoSolicitud(
                    proyecto.Id,
                    usuarioB.Id,
                    "No persistir",
                    null,
                    EstadoProyecto.Publicado,
                    EstadoMadurez.Publicado,
                    null,
                    null,
                    null), CancellationToken)).Estado);
            Assert.Equal(EliminarLaboratorioEstado.UsuarioNoCoincide, (await CrearEliminarLaboratorio(contexto)
                .EjecutarAsync(new EliminarLaboratorioSolicitud(laboratorio.Id, usuarioB.Id), CancellationToken)).Estado);
            Assert.Equal(ActualizarWriteupEstado.UsuarioNoCoincide, (await CrearActualizarWriteup(contexto)
                .EjecutarAsync(new ActualizarWriteupSolicitud(
                    writeup.Id,
                    usuarioB.Id,
                    "No persistir",
                    null,
                    null,
                    null,
                    EstadoMadurez.Publicado), CancellationToken)).Estado);
            Assert.Equal(EliminarArtefactoTecnicoEstado.UsuarioNoCoincide, (await CrearEliminarArtefacto(contexto)
                .EjecutarAsync(new EliminarArtefactoTecnicoSolicitud(artefacto.Id, usuarioB.Id), CancellationToken)).Estado);
            Assert.Equal(ActualizarCertificacionObtenidaEstado.UsuarioNoCoincide, (await CrearActualizarCertificacionObtenida(contexto)
                .EjecutarAsync(new ActualizarCertificacionObtenidaSolicitud(
                    obtenida.Id,
                    usuarioB.Id,
                    "https://example.local/no",
                    EstadoMadurez.Publicado), CancellationToken)).Estado);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var proyectoPersistido = await contexto.Proyectos.AsNoTracking()
                .SingleAsync(p => p.Id == proyecto.Id, CancellationToken);
            var laboratorioPersistido = await contexto.Laboratorios.AsNoTracking()
                .SingleAsync(l => l.Id == laboratorio.Id, CancellationToken);
            var writeupPersistido = await contexto.Writeups.AsNoTracking()
                .SingleAsync(w => w.Id == writeup.Id, CancellationToken);
            var artefactoPersistido = await contexto.ArtefactosTecnicos.AsNoTracking()
                .SingleAsync(a => a.Id == artefacto.Id, CancellationToken);
            var obtenidaPersistida = await contexto.CertificacionesObtenidas.AsNoTracking()
                .SingleAsync(c => c.Id == obtenida.Id, CancellationToken);

            Assert.Equal("Proyecto original", proyectoPersistido.Nombre);
            Assert.Equal(EstadoMadurez.Borrador, proyectoPersistido.EstadoMadurez);
            Assert.Null(laboratorioPersistido.FechaEliminacionUtc);
            Assert.Equal("Writeup original", writeupPersistido.Titulo);
            Assert.Equal(EstadoMadurez.Borrador, writeupPersistido.EstadoMadurez);
            Assert.Null(artefactoPersistido.FechaEliminacionUtc);
            Assert.Null(obtenidaPersistida.EvidenciaUrl);
            Assert.Equal(EstadoMadurez.Documentado, obtenidaPersistida.EstadoMadurez);
        }
    }

    private static ActualizarProyectoCasoUso CrearActualizarProyecto(AprendizajeDbContext contexto) =>
        new(new ProyectoRepository(contexto), new UsuarioRepository(contexto), contexto);

    private static EliminarProyectoCasoUso CrearEliminarProyecto(AprendizajeDbContext contexto) =>
        new(new ProyectoRepository(contexto), new UsuarioRepository(contexto), contexto);

    private static ActualizarLaboratorioCasoUso CrearActualizarLaboratorio(AprendizajeDbContext contexto) =>
        new(new LaboratorioRepository(contexto), new UsuarioRepository(contexto), contexto);

    private static EliminarLaboratorioCasoUso CrearEliminarLaboratorio(AprendizajeDbContext contexto) =>
        new(new LaboratorioRepository(contexto), new UsuarioRepository(contexto), contexto);

    private static ActualizarWriteupCasoUso CrearActualizarWriteup(AprendizajeDbContext contexto) =>
        new(new WriteupRepository(contexto), new UsuarioRepository(contexto), contexto);

    private static EliminarWriteupCasoUso CrearEliminarWriteup(AprendizajeDbContext contexto) =>
        new(new WriteupRepository(contexto), new UsuarioRepository(contexto), contexto);

    private static ActualizarArtefactoTecnicoCasoUso CrearActualizarArtefacto(AprendizajeDbContext contexto) =>
        new(new ArtefactoTecnicoRepository(contexto), new UsuarioRepository(contexto), contexto);

    private static EliminarArtefactoTecnicoCasoUso CrearEliminarArtefacto(AprendizajeDbContext contexto) =>
        new(new ArtefactoTecnicoRepository(contexto), new UsuarioRepository(contexto), contexto);

    private static ActualizarCertificacionObtenidaCasoUso CrearActualizarCertificacionObtenida(
        AprendizajeDbContext contexto) =>
        new(new CertificacionObtenidaRepository(contexto), new UsuarioRepository(contexto), contexto);

    private static EliminarCertificacionObtenidaCasoUso CrearEliminarCertificacionObtenida(
        AprendizajeDbContext contexto) =>
        new(new CertificacionObtenidaRepository(contexto), new UsuarioRepository(contexto), contexto);

    private static async Task<int> ContarVinculoAsync(
        AprendizajeDbContext contexto,
        string schema,
        string tabla,
        string columnaA,
        Guid valorA,
        string columnaB,
        Guid valorB)
    {
        var sql = (schema, tabla, columnaA, columnaB) switch
        {
            ("evidence", "ProyectoTema", "ProyectoId", "TemaId") =>
                "SELECT COUNT(*) AS [Value] FROM [evidence].[ProyectoTema] WHERE [ProyectoId] = {0} AND [TemaId] = {1}",
            ("evidence", "ProyectoHerramienta", "ProyectoId", "HerramientaId") =>
                "SELECT COUNT(*) AS [Value] FROM [evidence].[ProyectoHerramienta] WHERE [ProyectoId] = {0} AND [HerramientaId] = {1}",
            ("evidence", "LaboratorioTema", "LaboratorioId", "TemaId") =>
                "SELECT COUNT(*) AS [Value] FROM [evidence].[LaboratorioTema] WHERE [LaboratorioId] = {0} AND [TemaId] = {1}",
            ("evidence", "LaboratorioHerramienta", "LaboratorioId", "HerramientaId") =>
                "SELECT COUNT(*) AS [Value] FROM [evidence].[LaboratorioHerramienta] WHERE [LaboratorioId] = {0} AND [HerramientaId] = {1}",
            ("evidence", "WriteupTema", "WriteupId", "TemaId") =>
                "SELECT COUNT(*) AS [Value] FROM [evidence].[WriteupTema] WHERE [WriteupId] = {0} AND [TemaId] = {1}",
            ("evidence", "ArtefactoTema", "ArtefactoTecnicoId", "TemaId") =>
                "SELECT COUNT(*) AS [Value] FROM [evidence].[ArtefactoTema] WHERE [ArtefactoTecnicoId] = {0} AND [TemaId] = {1}",
            ("evidence", "ArtefactoHerramienta", "ArtefactoTecnicoId", "HerramientaId") =>
                "SELECT COUNT(*) AS [Value] FROM [evidence].[ArtefactoHerramienta] WHERE [ArtefactoTecnicoId] = {0} AND [HerramientaId] = {1}",
            _ => throw new InvalidOperationException("Consulta de vínculo Evidence no reconocida.")
        };

        return await contexto.Database
            .SqlQueryRaw<int>(sql, valorA, valorB)
            .SingleAsync(CancellationToken);
    }

    private static string EmailUnico() => $"evidence-editable-{Guid.CreateVersion7():N}@local.test";

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
