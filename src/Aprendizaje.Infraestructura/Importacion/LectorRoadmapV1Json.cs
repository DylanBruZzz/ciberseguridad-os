using System.Text.Json;
using System.Text.Json.Serialization;
using Aprendizaje.Aplicacion.Roadmap.Importacion;

namespace Aprendizaje.Infraestructura.Importacion;

public sealed class LectorRoadmapV1Json
{
    private static readonly JsonSerializerOptions OpcionesJson = new()
    {
        PropertyNameCaseInsensitive = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    public async Task<RoadmapV1Documento> LeerAsync(
        string ruta,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ruta))
            throw new ArgumentException("La ruta del Roadmap V1 es requerida.", nameof(ruta));

        await using var stream = File.OpenRead(ruta);

        return await JsonSerializer.DeserializeAsync<RoadmapV1Documento>(
            stream,
            OpcionesJson,
            cancellationToken)
            ?? throw new InvalidOperationException("No se pudo deserializar roadmap-v1.json.");
    }
}
