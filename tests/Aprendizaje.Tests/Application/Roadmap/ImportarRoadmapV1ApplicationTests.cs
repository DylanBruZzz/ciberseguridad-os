using Aprendizaje.Aplicacion.Roadmap.Importacion;
using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Tests.Soporte;
using Xunit;

namespace Aprendizaje.Tests.Application.Roadmap;

public sealed class ImportarRoadmapV1ApplicationTests
{
    [Fact]
    public async Task Importar_GuidEmpty_RetornaArgumentosInvalidosSinGuardar()
    {
        var contexto = CrearContexto();
        var casoUso = CrearCasoUso(contexto);

        var resultado = await casoUso.EjecutarAsync(
            new ImportarRoadmapV1Solicitud(Guid.Empty, CrearDocumentoValido()),
            CancellationToken);

        Assert.Equal(ImportarRoadmapV1Estado.ArgumentosInvalidos, resultado.Estado);
        Assert.Equal(0, contexto.UnitOfWork.GuardarCambiosLlamadas);
        Assert.Equal(0, contexto.Transaccion.EjecutarLlamadas);
    }

    [Fact]
    public async Task Importar_UsuarioInexistente_RetornaNoEncontradoSinGuardar()
    {
        var contexto = CrearContexto();
        var casoUso = CrearCasoUso(contexto);

        var resultado = await casoUso.EjecutarAsync(
            new ImportarRoadmapV1Solicitud(Guid.CreateVersion7(), CrearDocumentoValido()),
            CancellationToken);

        Assert.Equal(ImportarRoadmapV1Estado.UsuarioNoEncontrado, resultado.Estado);
        Assert.Equal(0, contexto.UnitOfWork.GuardarCambiosLlamadas);
        Assert.Equal(0, contexto.Transaccion.EjecutarLlamadas);
    }

    [Fact]
    public async Task Importar_DocumentoInvalido_RetornaErroresSinGuardar()
    {
        var usuario = Usuario.Registrar("Dylan", "dylan@test.local");
        var contexto = CrearContexto(usuario);
        var casoUso = CrearCasoUso(contexto);
        var documento = CrearDocumentoValido() with { Version = "2" };

        var resultado = await casoUso.EjecutarAsync(
            new ImportarRoadmapV1Solicitud(usuario.Id, documento),
            CancellationToken);

        Assert.Equal(ImportarRoadmapV1Estado.DocumentoInvalido, resultado.Estado);
        Assert.Contains(resultado.Errores, e => e.Contains("versión", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(0, contexto.UnitOfWork.GuardarCambiosLlamadas);
        Assert.Equal(0, contexto.Transaccion.EjecutarLlamadas);
    }

    [Fact]
    public async Task Importar_DocumentoConSourceKeyDuplicado_RetornaDocumentoInvalido()
    {
        var usuario = Usuario.Registrar("Dylan", "dylan@test.local");
        var contexto = CrearContexto(usuario);
        var casoUso = CrearCasoUso(contexto);
        var fases = CrearFasesValidas();
        fases[1] = fases[1] with { SourceKey = fases[0].SourceKey };
        var documento = CrearDocumentoValido(fases);

        var resultado = await casoUso.EjecutarAsync(
            new ImportarRoadmapV1Solicitud(usuario.Id, documento),
            CancellationToken);

        Assert.Equal(ImportarRoadmapV1Estado.DocumentoInvalido, resultado.Estado);
        Assert.Contains(resultado.Errores, e => e.Contains("duplicado", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(0, contexto.UnitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task Importar_DocumentoValido_CreaRegistrosYResultadoFactual()
    {
        var usuario = Usuario.Registrar("Dylan", "dylan@test.local");
        var contexto = CrearContexto(usuario);
        var casoUso = CrearCasoUso(contexto);

        var resultado = await casoUso.EjecutarAsync(
            new ImportarRoadmapV1Solicitud(usuario.Id, CrearDocumentoValido()),
            CancellationToken);

        Assert.Equal(ImportarRoadmapV1Estado.Importado, resultado.Estado);
        Assert.Equal(new ImportacionRoadmapConteo(7, 0, 0, 0), resultado.Fases);
        Assert.Equal(new ImportacionRoadmapConteo(7, 0, 0, 0), resultado.Temas);
        Assert.Equal(new ImportacionRoadmapConteo(1, 0, 0, 0), resultado.Herramientas);
        Assert.Equal(new ImportacionRoadmapConteo(1, 0, 0, 0), resultado.Certificaciones);
        Assert.Equal(new ImportacionRoadmapConteo(1, 0, 0, 0), resultado.Recursos);
        Assert.Equal(0, resultado.EvidenceCreadas);
        Assert.Empty(resultado.Errores);
        Assert.Equal(1, contexto.UnitOfWork.GuardarCambiosLlamadas);
        Assert.Equal(1, contexto.Transaccion.EjecutarLlamadas);
    }

    [Fact]
    public async Task Importar_SegundaEjecucion_NoDuplica()
    {
        var usuario = Usuario.Registrar("Dylan", "dylan@test.local");
        var contexto = CrearContexto(usuario);
        var casoUso = CrearCasoUso(contexto);
        var documento = CrearDocumentoValido();

        await casoUso.EjecutarAsync(new ImportarRoadmapV1Solicitud(usuario.Id, documento), CancellationToken);
        var resultado = await casoUso.EjecutarAsync(new ImportarRoadmapV1Solicitud(usuario.Id, documento), CancellationToken);

        Assert.Equal(ImportarRoadmapV1Estado.Importado, resultado.Estado);
        Assert.Equal(new ImportacionRoadmapConteo(0, 7, 0, 0), resultado.Fases);
        Assert.Equal(new ImportacionRoadmapConteo(0, 7, 0, 0), resultado.Temas);
        Assert.Equal(new ImportacionRoadmapConteo(0, 0, 0, 1), resultado.Herramientas);
        Assert.Equal(new ImportacionRoadmapConteo(0, 0, 0, 1), resultado.Certificaciones);
        Assert.Equal(new ImportacionRoadmapConteo(0, 1, 0, 0), resultado.Recursos);
        Assert.Equal(2, contexto.UnitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task Importar_FaseExistenteConMismoOrdenYNombreDistinto_RetornaConflicto()
    {
        var usuario = Usuario.Registrar("Dylan", "dylan@test.local");
        var contexto = CrearContexto(usuario);
        contexto.Fases.Agregar(Fase.Crear(usuario.Id, "Nombre previo", 1));
        var casoUso = CrearCasoUso(contexto);

        var resultado = await casoUso.EjecutarAsync(
            new ImportarRoadmapV1Solicitud(usuario.Id, CrearDocumentoValido()),
            CancellationToken);

        Assert.Equal(ImportarRoadmapV1Estado.Conflicto, resultado.Estado);
        Assert.Contains(resultado.Errores, e => e.Contains("Orden 1", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(0, contexto.UnitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task Importar_CertificacionExistenteConTipoCostoDistinto_RetornaConflicto()
    {
        var usuario = Usuario.Registrar("Dylan", "dylan@test.local");
        var contexto = CrearContexto(usuario);
        contexto.Certificaciones.Agregar(Certificacion.Crear("Certificación V1", TipoCosto.Gratuita));
        var casoUso = CrearCasoUso(contexto);

        var resultado = await casoUso.EjecutarAsync(
            new ImportarRoadmapV1Solicitud(usuario.Id, CrearDocumentoValido()),
            CancellationToken);

        Assert.Equal(ImportarRoadmapV1Estado.Conflicto, resultado.Estado);
        Assert.Contains(resultado.Errores, e => e.Contains("TipoCosto", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(0, contexto.UnitOfWork.GuardarCambiosLlamadas);
    }

    private static ImportarRoadmapV1CasoUso CrearCasoUso(ContextoFakeImportacion contexto) =>
        new(
            contexto.Usuarios,
            contexto.Fases,
            contexto.Temas,
            contexto.Herramientas,
            contexto.Certificaciones,
            contexto.Recursos,
            contexto.UnitOfWork,
            contexto.Transaccion);

    private static ContextoFakeImportacion CrearContexto(Usuario? usuario = null)
    {
        var contexto = new ContextoFakeImportacion();

        if (usuario is not null)
            contexto.Usuarios.Agregar(usuario);

        return contexto;
    }

    private static RoadmapV1Documento CrearDocumentoValido(IReadOnlyList<FaseRoadmapV1>? fases = null) =>
        new()
        {
            Version = "1",
            Politica = new PoliticaRoadmapV1
            {
                UsuarioIdIncluido = false,
                UsarGuidFijos = false,
                ParsearHtmlEnRuntime = false,
                PersistirEvidencePlanificada = false,
                PersistirMercadoLaboral = false,
                PesoCertificacionTema = null,
                TemaDependenciaDiferida = true,
                CompetenciasDiferidas = true
            },
            Fases = fases ?? CrearFasesValidas(),
            Herramientas =
            [
                new HerramientaRoadmapV1
                {
                    SourceKey = "herramienta-nmap",
                    Nombre = "Nmap",
                    Categoria = "Análisis de Red"
                }
            ],
            Certificaciones =
            [
                new CertificacionRoadmapV1
                {
                    SourceKey = "cert-v1",
                    Nombre = "Certificación V1",
                    Proveedor = "Proveedor V1",
                    TipoCosto = nameof(TipoCosto.Pago),
                    Url = null
                }
            ],
            Recursos =
            [
                new RecursoRoadmapV1
                {
                    SourceKey = "recurso-v1",
                    Titulo = "Recurso V1",
                    Tipo = "Curso",
                    Url = null,
                    Notas = "Referencia documental",
                    TemaSourceKeys = []
                }
            ],
            RelacionesCertificacionTema = [],
            Competencias = [],
            TemaDependencias = [],
            Documental = new DocumentalRoadmapV1
            {
                ProyectosPlanificadosPorFase =
                [
                    new ProyectoPlanificadoPorFaseRoadmapV1
                    {
                        FaseSourceKey = "fase-01",
                        PersistirV1 = false,
                        Items = ["Proyecto futuro no persistible"]
                    }
                ],
                HomeLab = new SeccionDocumentalPersistibleRoadmapV1 { PersistirV1 = false },
                PortafolioFuturo = new SeccionDocumentalPersistibleRoadmapV1 { PersistirV1 = false },
                MercadoLaboral = new MercadoLaboralRoadmapV1 { CopiadoAlJsonV1 = false }
            },
            ValidacionesEsperadas = new ValidacionesEsperadasRoadmapV1
            {
                Fases = 7,
                OrdenFases = [1, 2, 3, 4, 5, 6, 7],
                EvidencePersistiblePlanificada = 0,
                CertificacionTemaPesoNoNull = 0,
                RelacionesCertificacionTemaV1 = 0,
                CompetenciasV1 = 0,
                TemaDependenciasV1 = 0
            }
        };

    private static FaseRoadmapV1[] CrearFasesValidas() =>
        Enumerable.Range(1, 7)
            .Select(i => new FaseRoadmapV1
            {
                SourceKey = $"fase-{i:00}",
                Orden = i,
                Nombre = $"Fase {i}",
                Descripcion = $"Descripción {i}",
                MesInicioRecomendado = i,
                MesFinRecomendado = i,
                CargaSemanalRecomendada = "~10 hrs/semana",
                Objetivos = [$"Objetivo {i}"],
                CriteriosAvance = [$"Criterio {i}"],
                Temas =
                [
                    new TemaRoadmapV1
                    {
                        SourceKey = $"tema-{i:00}",
                        Nombre = $"Tema {i}",
                        TipoConocimiento = nameof(TipoConocimiento.Conceptual),
                        Objetivos = []
                    }
                ]
            })
            .ToArray();

    private sealed class ContextoFakeImportacion
    {
        public FakeUsuarioRepository Usuarios { get; } = new();
        public FakeFaseRepository Fases { get; } = new();
        public FakeTemaRepository Temas { get; } = new();
        public FakeHerramientaRepository Herramientas { get; } = new();
        public FakeCertificacionRepository Certificaciones { get; } = new();
        public FakeRecursoRepository Recursos { get; } = new();
        public FakeUnitOfWork UnitOfWork { get; } = new();
        public FakeTransaccionAplicacion Transaccion { get; } = new();
    }

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
