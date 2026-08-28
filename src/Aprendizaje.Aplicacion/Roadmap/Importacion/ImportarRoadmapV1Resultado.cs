namespace Aprendizaje.Aplicacion.Roadmap.Importacion;

public sealed record ImportarRoadmapV1Resultado(
    ImportarRoadmapV1Estado Estado,
    Guid UsuarioId,
    string? VersionDataset,
    ImportacionRoadmapConteo Fases,
    ImportacionRoadmapConteo Temas,
    ImportacionRoadmapConteo Herramientas,
    ImportacionRoadmapConteo Certificaciones,
    ImportacionRoadmapConteo Recursos,
    ImportacionRoadmapRelacionesConteo Relaciones,
    int EvidenceCreadas,
    IReadOnlyCollection<string> Errores,
    IReadOnlyCollection<string> Advertencias)
{
    public static ImportarRoadmapV1Resultado ArgumentosInvalidos(Guid usuarioId, params string[] errores) =>
        Fallo(ImportarRoadmapV1Estado.ArgumentosInvalidos, usuarioId, null, errores);

    public static ImportarRoadmapV1Resultado DocumentoInvalido(
        Guid usuarioId,
        string? versionDataset,
        IReadOnlyCollection<string> errores) =>
        Fallo(ImportarRoadmapV1Estado.DocumentoInvalido, usuarioId, versionDataset, errores);

    public static ImportarRoadmapV1Resultado UsuarioNoEncontrado(Guid usuarioId, string? versionDataset) =>
        Fallo(
            ImportarRoadmapV1Estado.UsuarioNoEncontrado,
            usuarioId,
            versionDataset,
            ["El Usuario destino no existe o no está visible."]);

    public static ImportarRoadmapV1Resultado Conflicto(
        Guid usuarioId,
        string? versionDataset,
        IReadOnlyCollection<string> errores,
        IReadOnlyCollection<string>? advertencias = null) =>
        new(
            ImportarRoadmapV1Estado.Conflicto,
            usuarioId,
            versionDataset,
            ImportacionRoadmapConteo.Vacio(),
            ImportacionRoadmapConteo.Vacio(),
            ImportacionRoadmapConteo.Vacio(),
            ImportacionRoadmapConteo.Vacio(),
            ImportacionRoadmapConteo.Vacio(),
            ImportacionRoadmapRelacionesConteo.Vacio(),
            0,
            errores,
            advertencias ?? []);

    public static ImportarRoadmapV1Resultado Importado(
        Guid usuarioId,
        string versionDataset,
        ImportacionRoadmapConteo fases,
        ImportacionRoadmapConteo temas,
        ImportacionRoadmapConteo herramientas,
        ImportacionRoadmapConteo certificaciones,
        ImportacionRoadmapConteo recursos,
        ImportacionRoadmapRelacionesConteo relaciones,
        IReadOnlyCollection<string> advertencias) =>
        new(
            ImportarRoadmapV1Estado.Importado,
            usuarioId,
            versionDataset,
            fases,
            temas,
            herramientas,
            certificaciones,
            recursos,
            relaciones,
            0,
            [],
            advertencias);

    private static ImportarRoadmapV1Resultado Fallo(
        ImportarRoadmapV1Estado estado,
        Guid usuarioId,
        string? versionDataset,
        IReadOnlyCollection<string> errores) =>
        new(
            estado,
            usuarioId,
            versionDataset,
            ImportacionRoadmapConteo.Vacio(),
            ImportacionRoadmapConteo.Vacio(),
            ImportacionRoadmapConteo.Vacio(),
            ImportacionRoadmapConteo.Vacio(),
            ImportacionRoadmapConteo.Vacio(),
            ImportacionRoadmapRelacionesConteo.Vacio(),
            0,
            errores,
            []);
}

public enum ImportarRoadmapV1Estado
{
    Importado,
    ArgumentosInvalidos,
    DocumentoInvalido,
    UsuarioNoEncontrado,
    Conflicto
}

public sealed record ImportacionRoadmapConteo(
    int Creados,
    int Existentes,
    int Actualizados,
    int Reutilizados)
{
    public static ImportacionRoadmapConteo Vacio() => new(0, 0, 0, 0);
}

public sealed record ImportacionRoadmapRelacionesConteo(
    int RecursoTemaCreadas,
    int RecursoTemaExistentes,
    int CertificacionTemaCreadas,
    int CertificacionTemaExistentes)
{
    public static ImportacionRoadmapRelacionesConteo Vacio() => new(0, 0, 0, 0);
}
