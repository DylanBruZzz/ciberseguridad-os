using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aprendizaje.Aplicacion.Roadmap.Importacion;

public sealed record RoadmapV1Documento
{
    public string? Version { get; init; }
    public JsonElement Fuente { get; init; }
    public PoliticaRoadmapV1? Politica { get; init; }
    public IReadOnlyList<FaseRoadmapV1> Fases { get; init; } = [];
    public IReadOnlyList<RecursoRoadmapV1> Recursos { get; init; } = [];
    public IReadOnlyList<HerramientaRoadmapV1> Herramientas { get; init; } = [];
    public IReadOnlyList<CertificacionRoadmapV1> Certificaciones { get; init; } = [];
    public IReadOnlyList<RelacionCertificacionTemaRoadmapV1> RelacionesCertificacionTema { get; init; } = [];
    public IReadOnlyList<JsonElement> Competencias { get; init; } = [];
    public IReadOnlyList<JsonElement> TemaDependencias { get; init; } = [];
    public DocumentalRoadmapV1? Documental { get; init; }
    public ValidacionesEsperadasRoadmapV1? ValidacionesEsperadas { get; init; }
}

public sealed record PoliticaRoadmapV1
{
    public bool UsuarioIdIncluido { get; init; }
    public bool UsarGuidFijos { get; init; }
    public string? NormalizacionTemas { get; init; }
    public bool ParsearHtmlEnRuntime { get; init; }
    public bool PersistirEvidencePlanificada { get; init; }
    public bool PersistirMercadoLaboral { get; init; }
    public decimal? PesoCertificacionTema { get; init; }
    public bool TemaDependenciaDiferida { get; init; }
    public bool CompetenciasDiferidas { get; init; }
}

public sealed record FaseRoadmapV1
{
    public string? SourceKey { get; init; }
    public string? SourceSection { get; init; }
    public int Orden { get; init; }
    public string? AnioEtiqueta { get; init; }
    public string? Nombre { get; init; }
    public string? Descripcion { get; init; }
    public int? MesInicioRecomendado { get; init; }
    public int? MesFinRecomendado { get; init; }
    public string? CargaSemanalRecomendada { get; init; }
    public IReadOnlyList<string> Objetivos { get; init; } = [];
    public IReadOnlyList<string> CriteriosAvance { get; init; } = [];
    public IReadOnlyList<TemaRoadmapV1> Temas { get; init; } = [];
    public JsonElement MetadataDocumental { get; init; }
}

public sealed record TemaRoadmapV1
{
    public string? SourceKey { get; init; }
    public string? Nombre { get; init; }
    public string? TipoConocimiento { get; init; }
    public IReadOnlyList<string> Objetivos { get; init; } = [];
}

public sealed record RecursoRoadmapV1
{
    public string? SourceKey { get; init; }
    public string? SourceSection { get; init; }
    public string? Titulo { get; init; }
    public string? Tipo { get; init; }
    public string? Url { get; init; }
    public string? Notas { get; init; }
    public IReadOnlyList<string> TemaSourceKeys { get; init; } = [];
}

public sealed record HerramientaRoadmapV1
{
    public string? SourceKey { get; init; }
    public string? Nombre { get; init; }
    public string? Categoria { get; init; }
    public JsonElement MetadataDocumental { get; init; }
}

public sealed record CertificacionRoadmapV1
{
    public string? SourceKey { get; init; }
    public string? Nombre { get; init; }
    public string? Proveedor { get; init; }
    public string? TipoCosto { get; init; }
    public string? Url { get; init; }
    public JsonElement MetadataDocumental { get; init; }
}

public sealed record RelacionCertificacionTemaRoadmapV1
{
    public string? CertificacionSourceKey { get; init; }
    public string? TemaSourceKey { get; init; }
    public decimal? Peso { get; init; }
}

public sealed record DocumentalRoadmapV1
{
    public IReadOnlyList<ProyectoPlanificadoPorFaseRoadmapV1> ProyectosPlanificadosPorFase { get; init; } = [];
    public SeccionDocumentalPersistibleRoadmapV1? HomeLab { get; init; }
    public SeccionDocumentalPersistibleRoadmapV1? PortafolioFuturo { get; init; }
    public MercadoLaboralRoadmapV1? MercadoLaboral { get; init; }
}

public sealed record ProyectoPlanificadoPorFaseRoadmapV1
{
    public string? FaseSourceKey { get; init; }
    public bool PersistirV1 { get; init; }
    public IReadOnlyList<string> Items { get; init; } = [];
}

public sealed record SeccionDocumentalPersistibleRoadmapV1
{
    public bool PersistirV1 { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Metadata { get; init; }
}

public sealed record MercadoLaboralRoadmapV1
{
    public bool CopiadoAlJsonV1 { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Metadata { get; init; }
}

public sealed record ValidacionesEsperadasRoadmapV1
{
    public int? Fases { get; init; }
    public IReadOnlyList<int> OrdenFases { get; init; } = [];
    public int? EvidencePersistiblePlanificada { get; init; }
    public int? CertificacionTemaPesoNoNull { get; init; }
    public int? RelacionesCertificacionTemaV1 { get; init; }
    public int? CompetenciasV1 { get; init; }
    public int? TemaDependenciasV1 { get; init; }
}
