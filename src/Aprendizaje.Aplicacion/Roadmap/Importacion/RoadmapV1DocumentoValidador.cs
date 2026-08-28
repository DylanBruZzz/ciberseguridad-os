using Aprendizaje.Dominio.Resource;
using Aprendizaje.Dominio.Roadmap;

namespace Aprendizaje.Aplicacion.Roadmap.Importacion;

public static class RoadmapV1DocumentoValidador
{
    private static readonly HashSet<string> TiposConocimientoValidos =
        Enum.GetNames<TipoConocimiento>().ToHashSet(StringComparer.Ordinal);

    private static readonly HashSet<string> TiposRecursoValidos =
        Enum.GetNames<TipoRecurso>().ToHashSet(StringComparer.Ordinal);

    private static readonly HashSet<string> TiposCostoValidos =
        Enum.GetNames<TipoCosto>().ToHashSet(StringComparer.Ordinal);

    public static IReadOnlyCollection<string> Validar(RoadmapV1Documento? documento)
    {
        var errores = new List<string>();

        if (documento is null)
            return ["El documento Roadmap V1 es requerido."];

        if (documento.Version != "1")
            errores.Add("La versión del dataset debe ser '1'.");

        ValidarPolitica(documento.Politica, errores);
        ValidarFases(documento, errores);
        ValidarRecursos(documento, errores);
        ValidarHerramientas(documento, errores);
        ValidarCertificaciones(documento, errores);
        ValidarRelacionesCertificacionTema(documento, errores);
        ValidarColeccionesDiferidas(documento, errores);
        ValidarDocumental(documento.Documental, errores);
        ValidarValidacionesEsperadas(documento, errores);
        ValidarSourceKeysGlobales(documento, errores);

        return errores;
    }

    private static void ValidarPolitica(PoliticaRoadmapV1? politica, List<string> errores)
    {
        if (politica is null)
        {
            errores.Add("La política del dataset es requerida.");
            return;
        }

        if (politica.UsuarioIdIncluido)
            errores.Add("El dataset no debe incluir UsuarioId.");

        if (politica.UsarGuidFijos)
            errores.Add("El dataset no debe usar GUIDs fijos.");

        if (politica.ParsearHtmlEnRuntime)
            errores.Add("El runtime no debe parsear HTML.");

        if (politica.PersistirEvidencePlanificada)
            errores.Add("La Evidence planificada no debe persistirse en V1.");

        if (politica.PersistirMercadoLaboral)
            errores.Add("El mercado laboral no debe persistirse en V1.");

        if (politica.PesoCertificacionTema is not null)
            errores.Add("CertificacionTema.Peso debe permanecer null.");

        if (!politica.TemaDependenciaDiferida)
            errores.Add("TemaDependencia debe permanecer diferida en V1.");

        if (!politica.CompetenciasDiferidas)
            errores.Add("Competencias debe permanecer diferida en V1.");
    }

    private static void ValidarFases(RoadmapV1Documento documento, List<string> errores)
    {
        if (documento.Fases.Count != 7)
            errores.Add("Roadmap V1 debe contener exactamente 7 fases.");

        var ordenes = documento.Fases.Select(f => f.Orden).Order().ToArray();

        if (!ordenes.SequenceEqual([1, 2, 3, 4, 5, 6, 7]))
            errores.Add("Las fases deben usar orden 1..7 sin duplicados.");

        var sourceKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var clavesNaturales = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var temaSourceKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var fase in documento.Fases)
        {
            ValidarSourceKey(fase.SourceKey, $"Fase orden {fase.Orden}", errores);

            if (!string.IsNullOrWhiteSpace(fase.SourceKey) && !sourceKeys.Add(fase.SourceKey.Trim()))
                errores.Add($"SourceKey de Fase duplicado: {fase.SourceKey}.");

            if (string.IsNullOrWhiteSpace(fase.Nombre))
                errores.Add($"La Fase {fase.SourceKey ?? fase.Orden.ToString()} debe tener nombre.");

            if (!clavesNaturales.Add(fase.Orden.ToString()))
                errores.Add($"Orden de Fase duplicado: {fase.Orden}.");

            ValidarMeses(fase, errores);

            foreach (var tema in fase.Temas)
            {
                ValidarSourceKey(tema.SourceKey, $"Tema en {fase.SourceKey}", errores);

                if (!string.IsNullOrWhiteSpace(tema.SourceKey) && !temaSourceKeys.Add(tema.SourceKey.Trim()))
                    errores.Add($"SourceKey de Tema duplicado: {tema.SourceKey}.");

                if (string.IsNullOrWhiteSpace(tema.Nombre))
                    errores.Add($"El Tema {tema.SourceKey ?? "(sin sourceKey)"} debe tener nombre.");

                if (string.IsNullOrWhiteSpace(tema.TipoConocimiento)
                    || !TiposConocimientoValidos.Contains(tema.TipoConocimiento.Trim()))
                {
                    errores.Add($"El Tema {tema.SourceKey ?? tema.Nombre} tiene TipoConocimiento inválido.");
                }
            }

            var temasDuplicados = fase.Temas
                .Where(t => !string.IsNullOrWhiteSpace(t.Nombre))
                .GroupBy(t => t.Nombre!.Trim(), StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            foreach (var temaDuplicado in temasDuplicados)
                errores.Add($"Tema duplicado dentro de Fase {fase.SourceKey}: {temaDuplicado}.");
        }
    }

    private static void ValidarMeses(FaseRoadmapV1 fase, List<string> errores)
    {
        if (fase.MesInicioRecomendado.HasValue && fase.MesInicioRecomendado.Value < 1)
            errores.Add($"La Fase {fase.SourceKey} tiene mes de inicio recomendado inválido.");

        if (fase.MesFinRecomendado.HasValue && fase.MesFinRecomendado.Value < 1)
            errores.Add($"La Fase {fase.SourceKey} tiene mes de fin recomendado inválido.");

        if (fase.MesInicioRecomendado.HasValue
            && fase.MesFinRecomendado.HasValue
            && fase.MesFinRecomendado.Value < fase.MesInicioRecomendado.Value)
        {
            errores.Add($"La Fase {fase.SourceKey} tiene rango de meses recomendado incoherente.");
        }
    }

    private static void ValidarRecursos(RoadmapV1Documento documento, List<string> errores)
    {
        var sourceKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var temaSourceKeys = documento.Fases
            .SelectMany(f => f.Temas)
            .Select(t => t.SourceKey)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s!.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var recurso in documento.Recursos)
        {
            ValidarSourceKey(recurso.SourceKey, $"Recurso {recurso.Titulo}", errores);

            if (!string.IsNullOrWhiteSpace(recurso.SourceKey) && !sourceKeys.Add(recurso.SourceKey.Trim()))
                errores.Add($"SourceKey de Recurso duplicado: {recurso.SourceKey}.");

            if (string.IsNullOrWhiteSpace(recurso.Titulo))
                errores.Add($"El Recurso {recurso.SourceKey ?? "(sin sourceKey)"} debe tener título.");

            if (string.IsNullOrWhiteSpace(recurso.Tipo) || !TiposRecursoValidos.Contains(recurso.Tipo.Trim()))
                errores.Add($"El Recurso {recurso.SourceKey ?? recurso.Titulo} tiene Tipo inválido.");

            foreach (var temaSourceKey in recurso.TemaSourceKeys.Where(t => !temaSourceKeys.Contains(t.Trim())))
                errores.Add($"El Recurso {recurso.SourceKey} referencia un Tema inexistente: {temaSourceKey}.");
        }
    }

    private static void ValidarHerramientas(RoadmapV1Documento documento, List<string> errores)
    {
        var sourceKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var nombres = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var herramienta in documento.Herramientas)
        {
            ValidarSourceKey(herramienta.SourceKey, $"Herramienta {herramienta.Nombre}", errores);

            if (!string.IsNullOrWhiteSpace(herramienta.SourceKey) && !sourceKeys.Add(herramienta.SourceKey.Trim()))
                errores.Add($"SourceKey de Herramienta duplicado: {herramienta.SourceKey}.");

            if (string.IsNullOrWhiteSpace(herramienta.Nombre))
            {
                errores.Add($"La Herramienta {herramienta.SourceKey ?? "(sin sourceKey)"} debe tener nombre.");
                continue;
            }

            if (!nombres.Add(herramienta.Nombre.Trim()))
                errores.Add($"Nombre de Herramienta duplicado en dataset: {herramienta.Nombre}.");
        }
    }

    private static void ValidarCertificaciones(RoadmapV1Documento documento, List<string> errores)
    {
        var sourceKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var nombres = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var certificacion in documento.Certificaciones)
        {
            ValidarSourceKey(certificacion.SourceKey, $"Certificación {certificacion.Nombre}", errores);

            if (!string.IsNullOrWhiteSpace(certificacion.SourceKey) && !sourceKeys.Add(certificacion.SourceKey.Trim()))
                errores.Add($"SourceKey de Certificación duplicado: {certificacion.SourceKey}.");

            if (string.IsNullOrWhiteSpace(certificacion.Nombre))
            {
                errores.Add($"La Certificación {certificacion.SourceKey ?? "(sin sourceKey)"} debe tener nombre.");
                continue;
            }

            if (!nombres.Add(certificacion.Nombre.Trim()))
                errores.Add($"Nombre de Certificación duplicado en dataset: {certificacion.Nombre}.");

            if (string.IsNullOrWhiteSpace(certificacion.TipoCosto)
                || !TiposCostoValidos.Contains(certificacion.TipoCosto.Trim()))
            {
                errores.Add($"La Certificación {certificacion.SourceKey ?? certificacion.Nombre} tiene TipoCosto inválido.");
            }
        }
    }

    private static void ValidarRelacionesCertificacionTema(RoadmapV1Documento documento, List<string> errores)
    {
        if (documento.RelacionesCertificacionTema.Count == 0)
            return;

        var certificaciones = documento.Certificaciones
            .Select(c => c.SourceKey)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s!.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var temas = documento.Fases
            .SelectMany(f => f.Temas)
            .Select(t => t.SourceKey)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s!.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var relacion in documento.RelacionesCertificacionTema)
        {
            if (relacion.Peso is not null)
                errores.Add("CertificacionTema.Peso debe permanecer null.");

            if (string.IsNullOrWhiteSpace(relacion.CertificacionSourceKey)
                || !certificaciones.Contains(relacion.CertificacionSourceKey.Trim()))
            {
                errores.Add("Existe una relación CertificacionTema con CertificacionSourceKey inválido.");
            }

            if (string.IsNullOrWhiteSpace(relacion.TemaSourceKey) || !temas.Contains(relacion.TemaSourceKey.Trim()))
                errores.Add("Existe una relación CertificacionTema con TemaSourceKey inválido.");
        }
    }

    private static void ValidarColeccionesDiferidas(RoadmapV1Documento documento, List<string> errores)
    {
        if (documento.Competencias.Count != 0)
            errores.Add("Competencias debe permanecer vacío en Roadmap V1.");

        if (documento.TemaDependencias.Count != 0)
            errores.Add("TemaDependencias debe permanecer vacío en Roadmap V1.");
    }

    private static void ValidarDocumental(DocumentalRoadmapV1? documental, List<string> errores)
    {
        if (documental is null)
            return;

        foreach (var plan in documental.ProyectosPlanificadosPorFase.Where(p => p.PersistirV1))
            errores.Add($"Proyecto/lab planificado marcado como persistible: {plan.FaseSourceKey}.");

        if (documental.HomeLab?.PersistirV1 == true)
            errores.Add("HomeLab no debe persistirse como Evidence en V1.");

        if (documental.PortafolioFuturo?.PersistirV1 == true)
            errores.Add("Portafolio futuro no debe persistirse como Evidence en V1.");

        if (documental.MercadoLaboral?.CopiadoAlJsonV1 == true)
            errores.Add("Mercado laboral no debe convertirse en dataset persistible V1.");
    }

    private static void ValidarValidacionesEsperadas(RoadmapV1Documento documento, List<string> errores)
    {
        var esperadas = documento.ValidacionesEsperadas;

        if (esperadas is null)
            return;

        if (esperadas.Fases.HasValue && esperadas.Fases.Value != documento.Fases.Count)
            errores.Add("El conteo esperado de Fases no coincide con el documento.");

        if (esperadas.OrdenFases.Count > 0
            && !esperadas.OrdenFases.Order().SequenceEqual(documento.Fases.Select(f => f.Orden).Order()))
        {
            errores.Add("El orden esperado de Fases no coincide con el documento.");
        }

        if (esperadas.EvidencePersistiblePlanificada is not null and not 0)
            errores.Add("El documento declara Evidence planificada persistible.");

        if (esperadas.CertificacionTemaPesoNoNull is not null and not 0)
            errores.Add("El documento declara pesos de CertificacionTema no nulos.");

        if (esperadas.RelacionesCertificacionTemaV1.HasValue
            && esperadas.RelacionesCertificacionTemaV1.Value != documento.RelacionesCertificacionTema.Count)
        {
            errores.Add("El conteo esperado de relaciones CertificacionTema no coincide con el documento.");
        }

        if (esperadas.CompetenciasV1.HasValue && esperadas.CompetenciasV1.Value != documento.Competencias.Count)
            errores.Add("El conteo esperado de Competencias no coincide con el documento.");

        if (esperadas.TemaDependenciasV1.HasValue
            && esperadas.TemaDependenciasV1.Value != documento.TemaDependencias.Count)
        {
            errores.Add("El conteo esperado de TemaDependencias no coincide con el documento.");
        }
    }

    private static void ValidarSourceKeysGlobales(RoadmapV1Documento documento, List<string> errores)
    {
        var sourceKeys = documento.Fases.Select(f => f.SourceKey)
            .Concat(documento.Fases.SelectMany(f => f.Temas).Select(t => t.SourceKey))
            .Concat(documento.Recursos.Select(r => r.SourceKey))
            .Concat(documento.Herramientas.Select(h => h.SourceKey))
            .Concat(documento.Certificaciones.Select(c => c.SourceKey))
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s!.Trim());

        var duplicados = sourceKeys
            .GroupBy(s => s, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        foreach (var duplicado in duplicados)
            errores.Add($"SourceKey duplicado global: {duplicado}.");
    }

    private static void ValidarSourceKey(string? sourceKey, string contexto, List<string> errores)
    {
        if (string.IsNullOrWhiteSpace(sourceKey))
        {
            errores.Add($"{contexto} debe tener sourceKey.");
            return;
        }

        if (Guid.TryParse(sourceKey, out _))
            errores.Add($"{contexto} usa un GUID como sourceKey.");
    }
}
