using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Nucleo.Repositorios;
using Aprendizaje.Dominio.Resource;
using Aprendizaje.Dominio.Resource.Repositorios;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Roadmap.Repositorios;
using Aprendizaje.Dominio.Study;
using Aprendizaje.Dominio.Study.Repositorios;

namespace Aprendizaje.Aplicacion.Roadmap.Importacion;

public sealed class ImportarRoadmapV1CasoUso
{
    private static readonly StringComparer Comparador = StringComparer.OrdinalIgnoreCase;

    private readonly IUsuarioRepository _usuarios;
    private readonly IFaseRepository _fases;
    private readonly ITemaRepository _temas;
    private readonly IHerramientaRepository _herramientas;
    private readonly ICertificacionRepository _certificaciones;
    private readonly IRecursoRepository _recursos;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITransaccionAplicacion _transaccion;

    public ImportarRoadmapV1CasoUso(
        IUsuarioRepository usuarios,
        IFaseRepository fases,
        ITemaRepository temas,
        IHerramientaRepository herramientas,
        ICertificacionRepository certificaciones,
        IRecursoRepository recursos,
        IUnitOfWork unitOfWork,
        ITransaccionAplicacion transaccion)
    {
        _usuarios = usuarios;
        _fases = fases;
        _temas = temas;
        _herramientas = herramientas;
        _certificaciones = certificaciones;
        _recursos = recursos;
        _unitOfWork = unitOfWork;
        _transaccion = transaccion;
    }

    public async Task<ImportarRoadmapV1Resultado> EjecutarAsync(
        ImportarRoadmapV1Solicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.UsuarioId == Guid.Empty)
            return ImportarRoadmapV1Resultado.ArgumentosInvalidos(
                solicitud.UsuarioId,
                "UsuarioId no puede ser Guid.Empty.");

        var erroresDocumento = RoadmapV1DocumentoValidador.Validar(solicitud.Documento);

        if (erroresDocumento.Count > 0)
            return ImportarRoadmapV1Resultado.DocumentoInvalido(
                solicitud.UsuarioId,
                solicitud.Documento.Version,
                erroresDocumento);

        var usuario = await _usuarios.ObtenerPorIdAsync(solicitud.UsuarioId, cancellationToken);

        if (usuario is null)
            return ImportarRoadmapV1Resultado.UsuarioNoEncontrado(solicitud.UsuarioId, solicitud.Documento.Version);

        return await _transaccion.EjecutarAsync(
            ct => ImportarDentroDeTransaccionAsync(solicitud.UsuarioId, solicitud.Documento, ct),
            cancellationToken);
    }

    private async Task<ImportarRoadmapV1Resultado> ImportarDentroDeTransaccionAsync(
        Guid usuarioId,
        RoadmapV1Documento documento,
        CancellationToken cancellationToken)
    {
        var advertencias = new List<string>();
        var fasesResultado = new ConteoMutable();
        var temasResultado = new ConteoMutable();
        var herramientasResultado = new ConteoMutable();
        var certificacionesResultado = new ConteoMutable();
        var recursosResultado = new ConteoMutable();
        var relacionesResultado = new RelacionesConteoMutable();

        var fasesExistentes = await _fases.ListarPorUsuarioParaImportacionAsync(usuarioId, cancellationToken);
        var fasesPorOrden = fasesExistentes.ToDictionary(f => f.Orden);
        var fasesPorSourceKey = new Dictionary<string, Fase>(Comparador);

        foreach (var faseDocumento in documento.Fases.OrderBy(f => f.Orden))
        {
            var nombre = NormalizarRequerido(faseDocumento.Nombre);
            var objetivos = NormalizarLista(faseDocumento.Objetivos);
            var criterios = NormalizarLista(faseDocumento.CriteriosAvance);
            var descripcion = NormalizarOpcional(faseDocumento.Descripcion);
            var carga = NormalizarOpcional(faseDocumento.CargaSemanalRecomendada);

            if (!fasesPorOrden.TryGetValue(faseDocumento.Orden, out var fase))
            {
                fase = Fase.Crear(usuarioId, nombre, faseDocumento.Orden);
                fase.ActualizarDescripcion(descripcion);
                fase.ConfigurarMetadataPedagogica(
                    objetivos,
                    criterios,
                    faseDocumento.MesInicioRecomendado,
                    faseDocumento.MesFinRecomendado,
                    carga);

                _fases.Agregar(fase);
                fasesPorOrden[fase.Orden] = fase;
                fasesResultado.Creados++;
            }
            else
            {
                if (!string.Equals(fase.Nombre, nombre, StringComparison.Ordinal))
                {
                    return ImportarRoadmapV1Resultado.Conflicto(
                        usuarioId,
                        documento.Version,
                        [$"Existe una Fase con Orden {fase.Orden} pero nombre distinto: '{fase.Nombre}' != '{nombre}'."],
                        advertencias);
                }

                if (FaseRequiereActualizacion(fase, descripcion, objetivos, criterios, faseDocumento, carga))
                {
                    fase.ActualizarDescripcion(descripcion);
                    fase.ConfigurarMetadataPedagogica(
                        objetivos,
                        criterios,
                        faseDocumento.MesInicioRecomendado,
                        faseDocumento.MesFinRecomendado,
                        carga);
                    fasesResultado.Actualizados++;
                }
                else
                {
                    fasesResultado.Existentes++;
                }
            }

            fasesPorSourceKey[NormalizarRequerido(faseDocumento.SourceKey)] = fase;
        }

        var temasExistentes = await _temas.ListarPorUsuarioParaImportacionAsync(usuarioId, cancellationToken);
        var temasPorSourceKey = new Dictionary<string, Tema>(Comparador);

        foreach (var faseDocumento in documento.Fases.OrderBy(f => f.Orden))
        {
            var fase = fasesPorSourceKey[NormalizarRequerido(faseDocumento.SourceKey)];

            foreach (var temaDocumento in faseDocumento.Temas)
            {
                var nombre = NormalizarRequerido(temaDocumento.Nombre);
                var tipo = ParsearEnum<TipoConocimiento>(temaDocumento.TipoConocimiento);
                var objetivos = NormalizarLista(temaDocumento.Objetivos);
                var candidatos = temasExistentes
                    .Where(t => t.UsuarioId == usuarioId
                        && t.FaseId == fase.Id
                        && t.TemaPadreId is null
                        && string.Equals(t.Nombre, nombre, StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                if (candidatos.Length > 1)
                {
                    return ImportarRoadmapV1Resultado.Conflicto(
                        usuarioId,
                        documento.Version,
                        [$"Clave natural ambigua para Tema '{nombre}' en Fase '{fase.Nombre}'."],
                        advertencias);
                }

                if (candidatos.Length == 0)
                {
                    var tema = Tema.Crear(usuarioId, nombre, tipo);
                    tema.AsignarFase(fase.Id);
                    tema.EstablecerObjetivos(objetivos);

                    _temas.Agregar(tema);
                    temasPorSourceKey[NormalizarRequerido(temaDocumento.SourceKey)] = tema;
                    temasResultado.Creados++;
                    continue;
                }

                var existente = candidatos[0];

                if (existente.TipoConocimiento != tipo)
                {
                    return ImportarRoadmapV1Resultado.Conflicto(
                        usuarioId,
                        documento.Version,
                        [$"Tema '{nombre}' existe con TipoConocimiento '{existente.TipoConocimiento}' pero el dataset trae '{tipo}'."],
                        advertencias);
                }

                if (!existente.Objetivos.SequenceEqual(objetivos))
                {
                    existente.EstablecerObjetivos(objetivos);
                    temasResultado.Actualizados++;
                }
                else
                {
                    temasResultado.Existentes++;
                }

                temasPorSourceKey[NormalizarRequerido(temaDocumento.SourceKey)] = existente;
            }
        }

        var nombresHerramientas = documento.Herramientas
            .Select(h => NormalizarRequerido(h.Nombre))
            .Distinct(Comparador)
            .ToArray();
        var herramientasExistentes = await _herramientas.ListarPorNombresParaImportacionAsync(
            nombresHerramientas,
            cancellationToken);
        var herramientasPorNombre = herramientasExistentes.ToDictionary(h => h.Nombre, Comparador);

        foreach (var herramientaDocumento in documento.Herramientas)
        {
            var nombre = NormalizarRequerido(herramientaDocumento.Nombre);
            var categoria = NormalizarOpcional(herramientaDocumento.Categoria);

            if (!herramientasPorNombre.TryGetValue(nombre, out var herramienta))
            {
                herramienta = Herramienta.Crear(nombre);
                herramienta.ActualizarCategoria(categoria);
                _herramientas.Agregar(herramienta);
                herramientasPorNombre[nombre] = herramienta;
                herramientasResultado.Creados++;
                continue;
            }

            herramientasResultado.Reutilizados++;

            if (string.IsNullOrWhiteSpace(herramienta.Categoria) && categoria is not null)
            {
                herramienta.ActualizarCategoria(categoria);
                herramientasResultado.Actualizados++;
            }
            else if (categoria is not null && !string.Equals(herramienta.Categoria, categoria, StringComparison.Ordinal))
            {
                advertencias.Add($"Herramienta global '{nombre}' conserva categoría existente '{herramienta.Categoria}'. Dataset trae '{categoria}'.");
            }
        }

        var nombresCertificaciones = documento.Certificaciones
            .Select(c => NormalizarRequerido(c.Nombre))
            .Distinct(Comparador)
            .ToArray();
        var certificacionesExistentes = await _certificaciones.ListarPorNombresParaImportacionAsync(
            nombresCertificaciones,
            cancellationToken);
        var certificacionesPorNombre = certificacionesExistentes.ToDictionary(c => c.Nombre, Comparador);
        var certificacionesPorSourceKey = new Dictionary<string, Certificacion>(Comparador);

        foreach (var certificacionDocumento in documento.Certificaciones)
        {
            var nombre = NormalizarRequerido(certificacionDocumento.Nombre);
            var tipoCosto = ParsearEnum<TipoCosto>(certificacionDocumento.TipoCosto);
            var proveedor = NormalizarOpcional(certificacionDocumento.Proveedor);
            var url = NormalizarUrl(certificacionDocumento.Url);

            if (!certificacionesPorNombre.TryGetValue(nombre, out var certificacion))
            {
                certificacion = Certificacion.Crear(nombre, tipoCosto);
                certificacion.ActualizarProveedor(proveedor);
                certificacion.ActualizarUrl(url);
                _certificaciones.Agregar(certificacion);
                certificacionesPorNombre[nombre] = certificacion;
                certificacionesResultado.Creados++;
            }
            else
            {
                certificacionesResultado.Reutilizados++;

                if (certificacion.TipoCosto != tipoCosto)
                {
                    return ImportarRoadmapV1Resultado.Conflicto(
                        usuarioId,
                        documento.Version,
                        [$"Certificación global '{nombre}' existe con TipoCosto '{certificacion.TipoCosto}' pero el dataset trae '{tipoCosto}'."],
                        advertencias);
                }

                var actualizada = false;

                if (string.IsNullOrWhiteSpace(certificacion.Proveedor) && proveedor is not null)
                {
                    certificacion.ActualizarProveedor(proveedor);
                    actualizada = true;
                }
                else if (proveedor is not null && !string.Equals(certificacion.Proveedor, proveedor, StringComparison.Ordinal))
                {
                    advertencias.Add($"Certificación global '{nombre}' conserva proveedor existente '{certificacion.Proveedor}'. Dataset trae '{proveedor}'.");
                }

                if (string.IsNullOrWhiteSpace(certificacion.Url) && url is not null)
                {
                    certificacion.ActualizarUrl(url);
                    actualizada = true;
                }
                else if (url is not null && !string.Equals(certificacion.Url, url, StringComparison.Ordinal))
                {
                    advertencias.Add($"Certificación global '{nombre}' conserva URL existente.");
                }

                if (actualizada)
                    certificacionesResultado.Actualizados++;
            }

            certificacionesPorSourceKey[NormalizarRequerido(certificacionDocumento.SourceKey)] = certificacion;
        }

        var recursosExistentes = await _recursos.ListarPorUsuarioParaImportacionAsync(usuarioId, cancellationToken);
        var recursosPorSourceKey = new Dictionary<string, Recurso>(Comparador);

        foreach (var grupoRecurso in AgruparRecursosPersistibles(documento.Recursos))
        {
            var recursoDocumento = grupoRecurso[0];
            var titulo = NormalizarRequerido(recursoDocumento.Titulo);
            var tipo = ParsearEnum<TipoRecurso>(recursoDocumento.Tipo);
            var url = NormalizarUrl(recursoDocumento.Url);
            var candidatos = recursosExistentes
                .Where(r => r.UsuarioId == usuarioId
                    && r.Tipo == tipo
                    && string.Equals(r.Titulo, titulo, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(NormalizarUrl(r.Url), url, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (candidatos.Length > 1)
            {
                return ImportarRoadmapV1Resultado.Conflicto(
                    usuarioId,
                    documento.Version,
                    [$"Clave natural ambigua para Recurso '{titulo}'."],
                    advertencias);
            }

            if (grupoRecurso.Count > 1)
                advertencias.Add($"Recurso '{titulo}' aparece {grupoRecurso.Count} veces en el dataset y se importa una sola vez por clave natural.");

            if (candidatos.Length == 0)
            {
                var recurso = Recurso.Guardar(usuarioId, tipo, titulo);
                recurso.ActualizarUrl(url);
                recurso.ActualizarNotas(NormalizarOpcional(recursoDocumento.Notas));
                _recursos.Agregar(recurso);

                foreach (var recursoAgrupado in grupoRecurso)
                    recursosPorSourceKey[NormalizarRequerido(recursoAgrupado.SourceKey)] = recurso;

                recursosResultado.Creados++;
            }
            else
            {
                foreach (var recursoAgrupado in grupoRecurso)
                    recursosPorSourceKey[NormalizarRequerido(recursoAgrupado.SourceKey)] = candidatos[0];

                recursosResultado.Existentes++;
            }
        }

        foreach (var recursoDocumento in documento.Recursos)
        {
            if (recursoDocumento.TemaSourceKeys.Count == 0)
                continue;

            var recurso = recursosPorSourceKey[NormalizarRequerido(recursoDocumento.SourceKey)];

            foreach (var temaSourceKey in recursoDocumento.TemaSourceKeys)
            {
                var tema = temasPorSourceKey[NormalizarRequerido(temaSourceKey)];
                var existe = await _recursos.ExisteVinculoTemaAsync(recurso.Id, tema.Id, cancellationToken);

                if (existe)
                {
                    relacionesResultado.RecursoTemaExistentes++;
                    continue;
                }

                _recursos.VincularTema(recurso.Id, tema.Id);
                relacionesResultado.RecursoTemaCreadas++;
            }
        }

        foreach (var relacion in documento.RelacionesCertificacionTema)
        {
            var certificacion = certificacionesPorSourceKey[NormalizarRequerido(relacion.CertificacionSourceKey)];
            var tema = temasPorSourceKey[NormalizarRequerido(relacion.TemaSourceKey)];
            var existe = await _certificaciones.ExisteVinculoTemaAsync(certificacion.Id, tema.Id, cancellationToken);

            if (existe)
            {
                relacionesResultado.CertificacionTemaExistentes++;
                continue;
            }

            _certificaciones.VincularTema(certificacion.Id, tema.Id);
            relacionesResultado.CertificacionTemaCreadas++;
        }

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return ImportarRoadmapV1Resultado.Importado(
            usuarioId,
            documento.Version!,
            fasesResultado.ToConteo(),
            temasResultado.ToConteo(),
            herramientasResultado.ToConteo(),
            certificacionesResultado.ToConteo(),
            recursosResultado.ToConteo(),
            relacionesResultado.ToConteo(),
            advertencias);
    }

    private static bool FaseRequiereActualizacion(
        Fase fase,
        string? descripcion,
        IReadOnlyList<string> objetivos,
        IReadOnlyList<string> criterios,
        FaseRoadmapV1 documento,
        string? carga) =>
        !string.Equals(fase.Descripcion, descripcion, StringComparison.Ordinal)
        || !fase.Objetivos.SequenceEqual(objetivos)
        || !fase.CriteriosAvance.SequenceEqual(criterios)
        || fase.MesInicioRecomendado != documento.MesInicioRecomendado
        || fase.MesFinRecomendado != documento.MesFinRecomendado
        || !string.Equals(fase.CargaSemanalRecomendada, carga, StringComparison.Ordinal);

    private static string NormalizarRequerido(string? valor) => valor!.Trim();

    private static string? NormalizarOpcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static string? NormalizarUrl(string? valor) => NormalizarOpcional(valor);

    private static IReadOnlyList<string> NormalizarLista(IReadOnlyList<string> valores) =>
        valores
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v.Trim())
            .ToArray();

    private static IReadOnlyCollection<IReadOnlyList<RecursoRoadmapV1>> AgruparRecursosPersistibles(
        IReadOnlyList<RecursoRoadmapV1> recursos) =>
        recursos
            .GroupBy(ClaveNaturalRecurso, StringComparer.OrdinalIgnoreCase)
            .Select(g => (IReadOnlyList<RecursoRoadmapV1>)g.ToArray())
            .ToArray();

    private static string ClaveNaturalRecurso(RecursoRoadmapV1 recurso) =>
        string.Join(
            "|",
            NormalizarRequerido(recurso.Titulo),
            NormalizarRequerido(recurso.Tipo),
            NormalizarUrl(recurso.Url) ?? "<null>");

    private static TEnum ParsearEnum<TEnum>(string? valor)
        where TEnum : struct =>
        Enum.Parse<TEnum>(NormalizarRequerido(valor), ignoreCase: false);

    private sealed class ConteoMutable
    {
        public int Creados { get; set; }
        public int Existentes { get; set; }
        public int Actualizados { get; set; }
        public int Reutilizados { get; set; }

        public ImportacionRoadmapConteo ToConteo() => new(Creados, Existentes, Actualizados, Reutilizados);
    }

    private sealed class RelacionesConteoMutable
    {
        public int RecursoTemaCreadas { get; set; }
        public int RecursoTemaExistentes { get; set; }
        public int CertificacionTemaCreadas { get; set; }
        public int CertificacionTemaExistentes { get; set; }

        public ImportacionRoadmapRelacionesConteo ToConteo() =>
            new(RecursoTemaCreadas, RecursoTemaExistentes, CertificacionTemaCreadas, CertificacionTemaExistentes);
    }
}
