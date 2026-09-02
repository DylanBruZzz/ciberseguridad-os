# Decisiones Arquitectura

Este documento registra decisiones congeladas. No es historial de conversacion.

## Arquitectura

- Arquitectura: Clean Architecture + DDD + modular monolith.
- Stack: .NET 10, C#, ASP.NET Core Minimal APIs, EF Core 10 y SQL Server.
- Proyectos:
  - Aprendizaje.Dominio
  - Aprendizaje.Aplicacion
  - Aprendizaje.Infraestructura
  - Aprendizaje.Api
- Dependencias permitidas:
  - Dominio -> ninguna capa interna
  - Aplicacion -> Dominio
  - Infraestructura -> Dominio + Aplicacion
  - Api -> Aplicacion + Infraestructura

## Modulos

- Nucleo
- Roadmap
- Study
- Evidence
- Resource
- Analytics
- Integration

## Dominio

- El lenguaje del dominio y de los casos de negocio es español.
- Existen 17 Aggregate Roots:
  - Usuario
  - Fase
  - Certificacion
  - Tema
  - Competencia
  - Herramienta
  - SesionEstudio
  - EntradaBitacora
  - Proyecto
  - Laboratorio
  - Writeup
  - ArtefactoTecnico
  - CertificacionObtenida
  - Nota
  - Recurso
  - SnapshotProgreso
  - Conector
- CriterioTema y LogSincronizacion son entidades internas.
- ApunteTema es entidad de dominio dependiente para el texto personal editable 0..1 de un Tema; no es Aggregate Root nuevo y no reutiliza Nota.
- Las 12 tablas de union puras se representan con modelos tecnicos de Infraestructura, sin entidades de dominio.
- No crear Aggregate Roots, DbSet ni repositorios para tablas de union puras.
- No crear repositorio generico.
- Repositorios especificos y minimos, solo cuando un caso de uso real los requiera.
- AggregateRoot produce Domain Events.
- IDespachadorEventos concreto esta diferido.
- DespachoEventosInterceptor existe, pero no esta registrado todavia.

## Persistencia

- Los Guid se generan en dominio con Guid.CreateVersion7().
- EF configura ValueGeneratedNever() donde corresponde.
- No usar DEFAULT NEWSEQUENTIALID() en SQL.
- AuditoriaInterceptor gestiona FechaCreacionUtc y FechaModificacionUtc antes de SaveChanges.
- IUnitOfWork vive en Aplicacion.
- AprendizajeDbContext implementa IUnitOfWork directamente.
- Los repositorios no llaman SaveChanges.
- Unit of Work controla la persistencia del caso de uso.
- Enums persistidos como string donde ya esta definido.
- Json enums HTTP configurados como strings.
- Separacion de bases para V1 personal:
  - AprendizajeDb = desarrollo/E2E.
  - AprendizajeTestsDb = pruebas automatizadas.
  - AprendizajePersonalDb = uso personal real V1.
- appsettings.Personal.json configura el runtime Personal contra AprendizajePersonalDb; appsettings.Development.json sigue apuntando a AprendizajeDb.

## RowVersion

RowVersion es selectivo, no universal.

Se usa unicamente en:

- Usuario
- Tema
- SesionEstudio
- Proyecto

No mover VersionFila a AggregateRoot. Cualquier nuevo RowVersion requiere caso de uso real y cambios coordinados en Dominio, EF, SQL/migracion y pruebas de concurrencia.

## Soft Delete

Soft delete es selectivo y solo aplica a Aggregate Roots que implementan IEliminableLogicamente.

## SQL y DDL

- SQL maestro sigue siendo referencia estructural.
- Politica de paridad DDL: hibrida.
- Se exige paridad en schemas, tablas, columnas, tipos, nullability, PK, FKs, ON DELETE, CHECK, defaults funcionales, RowVersion, soft delete, indices relevantes, tablas de union e integridad referencial.
- Se aceptan diferencias nominales no bloqueantes: nombres convencionales de FK/PK, UNIQUE constraint SQL representado mediante unique index EF, e indices adicionales convencionales de EF para algunas FKs.
- SQL UNIQUE puede representarse mediante unique index EF.
- Indices convencionales EF adicionales aceptados bajo la politica hibrida.

## Decisiones Diferidas

- vw_TemaEstado esta deliberadamente diferida. RoadmapVistaV1 reutiliza Tema.CalcularEstado() desde una consulta on-demand y no crea vista SQL, entidad keyless ni snapshot persistido.
- No crear entidad keyless para vw_TemaEstado hasta una fase explicita de paridad SQL.
- TemaDependencia tiene condicion de carrera read -> validate -> insert diferida.
- TemaDependencia queda diferida hasta disenar una garantia completa de aciclicidad y concurrencia.
- Gobernanza de catalogos globales Herramienta/Certificacion diferida.
- La semantica operativa de CertificacionTema.Peso queda diferida; no se usa para Analytics hasta definir formula.
- FaseHerramienta, prioridad contextual de herramientas, PlanPortafolio y EntregablePlanificado quedan diferidos.
- Application-to-Application policy tests diferidos.

## API y Aplicacion

- API actual usa Minimal APIs.
- Application coordina casos de uso.
- API no contiene logica de dominio.
- Estrategia actual: vertical slices minimos.
- No introducir MediatR, AutoMapper, FluentValidation ni frameworks preventivos.

## Identidad y Operacion Local V1

- V1 adopta single-user local context para la experiencia personal: abrir la aplicacion y usarla sin pantalla de registro, login ni seleccion manual de Usuario.
- Auth visible, passwords, JWT, cookies, Identity, roles y sesiones quedan diferidos hasta exposicion remota, multiusuario o necesidad real.
- Environment Personal usa AprendizajePersonalDb. Development conserva AprendizajeDb y Testing conserva AprendizajeTestsDb.
- El Usuario actual local se resuelve por cardinalidad de Usuarios visibles en la base Personal: cero Usuarios visibles es error operativo, un Usuario visible es el Usuario actual y multiples Usuarios visibles es error de configuracion.
- No se guarda UsuarioId en appsettings.Personal.json y no se resuelve por Email. El Email permanece como atributo interno del dominio, no como login.
- Application conserva UsuarioId explicito en sus casos de uso para mantener ownership y facilitar una evolucion futura a Auth real sin reescribir dominio.
- API/runtime es responsable de resolver el Usuario actual para endpoints personales futuros. GET /api/usuario-actual expone solo Id y Nombre.
- API Personal V1 resuelve usuarioId en el borde HTTP mediante IUsuarioActual para flujos user-owned de Roadmap, Resource, Study, Evidence, Analytics y Portafolio. Application sigue recibiendo UsuarioId explicito.
- La estrategia de compatibilidad es gradual: los contratos explicit-user legacy se conservan temporalmente para Development/tests. En modo Personal, si se envia un usuarioId explicito que no coincide con el Usuario actual, la API responde conflicto.
- GET por Id de entidades user-owned debe validar ownership contra el Usuario actual en modo Personal; conocer un Guid no debe permitir saltar aislamiento.
- Los catalogos globales Herramienta y Certificacion no reciben ownership artificial por la adaptacion Personal.
- La operacion Personal debe escuchar por loopback/local. CORS se definira junto con Frontend Foundation; no usar AllowAnyOrigin permanente.

## Frontend V1

- Frontend V1 se implementa con Angular 21 standalone en frontend/.
- El cliente frontend no almacena ni envia UsuarioId para ownership en modo Personal; la API resuelve el Usuario actual en el borde HTTP.
- La base de API del frontend es /api. En desarrollo se usa proxy Angular local hacia http://localhost:64021 para evitar CORS y mantener la API limitada a loopback.
- No se configura AllowAnyOrigin. Cualquier CORS futuro debe limitarse al origen local real o eliminarse mediante despliegue same-origin.
- El frontend no contiene connection strings, Email de usuario, secretos ni configuracion SQL.
- La primera pantalla funcional es Roadmap: consume Fases y Temas reales desde AprendizajePersonalDb en modo read-only, sin hardcodear datos del roadmap.
- Study, Resources, Evidence y Portfolio quedan como rutas preparadas; su funcionalidad se implementara por bloques V1 posteriores.
- El diseno visual definitivo no queda congelado por la foundation.

## Portafolio V1 Read Side

- Portafolio V1 es una proyeccion de lectura sobre Evidence existente; no es Aggregate Root, no tiene tabla propia y no duplica datos.
- La elegibilidad V1 se define por Evidence visible del Usuario con EstadoMadurez ListoPortafolio o Publicado.
- Borrador, Documentado y Evidence eliminada logicamente quedan fuera del Portafolio V1.
- ListoPortafolio se incluye para previsualizacion personal; Publicado no implica exposicion publica ni web publicada.
- CertificacionObtenida se enriquece desde Certificacion global relacionada, sin modificar ni duplicar el catalogo.
- Temas y Herramientas se muestran solo mediante relaciones persistidas existentes; no hay inferencia por texto, tagging automatico ni AI summaries.
- El endpoint de lectura participa de API Personal V1: en modo Personal puede omitir usuarioId y resolverlo en la capa HTTP; los contratos explicit-user legacy permanecen temporalmente para Development/tests.
- No se crea Snapshot, vista materializada, cache ni motor de scoring para Portafolio V1.

## RoadmapVistaV1

- RoadmapVistaV1 es una proyeccion de lectura on-demand para la pantalla Roadmap V1. No es entidad, Aggregate Root, snapshot persistido, vista SQL ni motor paralelo de dominio.
- API Personal expone GET /api/roadmap/vista resolviendo Usuario actual en el borde HTTP; Application conserva UsuarioId explicito.
- Progreso de Tema = criterios cumplidos / criterios totales, redondeado a porcentaje entero; si un Tema no tiene criterios definidos, su progreso es 0%. CriterioTema no tiene pesos en V1, por lo que no hay ponderacion.
- EstadoTema se calcula con Tema.CalcularEstado(), usando la ultima SesionEstudio visible del Tema y el intervalo efectivo de repaso: Tema.IntervaloRepaso si existe, o Usuario.IntervaloRepasoDefectoDias.
- Repaso recomendado solo es true cuando EstadoTema resulta EnRepaso. ProximaFechaRepaso se expone si existe ultima sesion; sin ultima sesion no se inventa fecha.
- Progreso de Fase = promedio simple del progreso de sus Temas evaluables asociados. Un Tema padre con hijos y 0 criterios se trata como nodo organizativo y no participa en el denominador de progreso; un Tema sin hijos y 0 criterios sigue siendo un Tema no iniciado. CriteriosAvance de Fase permanecen como metadata pedagogica descriptiva y no alteran el calculo.
- TemasDominados cuenta Temas estructuralmente completos por criterios: CriteriosTotal mayor a 0 y CriteriosCumplidos igual a CriteriosTotal. EnRepaso conserva progreso 100% y cuenta como avance estructural ya recorrido, aunque se expone separadamente como repaso recomendado.
- Fase actual derivada = primera Fase por Orden que no esta completada. Una Fase esta completada solo si tiene al menos un Tema evaluable y todos sus Temas evaluables estan estructuralmente completos por criterios. Si todas las Fases estan completadas, la Fase actual derivada es la ultima. Una Fase sin Temas evaluables no se considera completada.
- ProgresoGlobalPorcentaje se incluye porque deriva del mismo conjunto de datos del Roadmap: promedio simple del progreso de los Temas evaluables asociados a Fases. No implica Dashboard completo ni SnapshotProgreso operativo.
- Los Temas se entregan planos por Fase con TemaPadreId; no se construye arbol recursivo en backend para evitar DTOs recursivos innecesarios.

## TemaWorkspaceV1

- TemaWorkspaceV1 es una proyeccion de lectura on-demand para abrir el Workspace de un Tema. No es entidad, Aggregate Root, snapshot persistido, vista SQL, cache ni God Dashboard.
- API Personal expone GET /api/temas/{temaId}/workspace resolviendo Usuario actual en el borde HTTP; Application conserva UsuarioId explicito.
- Reutiliza la misma semantica de RoadmapVistaV1 mediante una utilidad de read-side compartida: progreso de Tema = criterios cumplidos / criterios totales, 0% si no hay criterios; EstadoTema = Tema.CalcularEstado(); repasoRecomendado = EstadoTema.EnRepaso; proximaFechaRepaso solo existe si hay ultima sesion.
- El endpoint devuelve contexto inicial del Tema: Tema, Fase, objetivos reales del Tema, criterios de dominio, ApunteTema actual, ultima SesionEstudio y resumenes de Resources, Sesiones y Evidence.
- Resources, Sesiones y Evidence se exponen como resumenes contextuales para evitar cargar listas profundas. Evidence se cuenta por los ARs existentes: Proyecto, Laboratorio, Writeup, ArtefactoTecnico y CertificacionObtenida; CertificacionObtenida se relaciona con Tema a traves de CertificacionTema.
- Leer Workspace no crea ApunteTema, no escribe estado de repaso, no persiste progreso y no implementa frontend.

## EvidenceListaV1

- EvidenceListaV1 es una proyeccion de lectura on-demand para la pantalla Evidence V1. No es entidad, Aggregate Root, superclase de dominio, tabla, vista SQL ni repositorio generico.
- API Personal expone GET /api/evidence resolviendo Usuario actual en el borde HTTP; Application conserva UsuarioId explicito. Filtros V1: tipoEvidence, estadoMadurez y temaId.
- Los cinco ARs permanecen como fuentes de verdad: Proyecto, Laboratorio, Writeup, ArtefactoTecnico y CertificacionObtenida. El discriminador `TipoEvidenceV1` vive en Application/read-side y no se persiste.
- EstadoMadurez se devuelve factual con los valores de dominio Borrador, Documentado, ListoPortafolio y Publicado; no se transforma en porcentaje ni estado visual.
- El titulo se normaliza por proyeccion: Proyecto.Nombre, Laboratorio.Nombre, Writeup.Titulo, ArtefactoTecnico.Nombre y Certificacion.Nombre para CertificacionObtenida.
- Fechas factuales: fechaCreacionUtc, fechaModificacionUtc y fechaActividadUtc como FechaModificacionUtc o FechaCreacionUtc; el texto relativo queda para frontend. FechaReferencia usa la fecha propia del AR cuando existe.
- Temas se exponen por relaciones reales: ProyectoTema, LaboratorioTema, WriteupTema, ArtefactoTema y CertificacionTema para CertificacionObtenida via Certificacion. Herramientas se exponen solo para Proyecto, Laboratorio y ArtefactoTecnico.
- Portafolio permanece separado: EvidenceListaV1 incluye todos los EstadosMadurez visibles; Portafolio sigue filtrando ListoPortafolio/Publicado.
- No crea detail unificado ni writes genericos `/api/evidence`; el frontend puede usar tipoEvidence + id para ir al endpoint especifico del AR.

## Tema.Objetivos

- Dominio: Objetivos se expone como coleccion no-null desde la API publica.
- Persistencia: la coleccion vacia se representa actualmente como SQL NULL.
- TemaConfiguration: Objetivos es nullable.
- Tema normaliza el backing field cuando EF materializa NULL.

## ApunteTema V1

- ApunteTema representa apuntes personales permanentes y editables del Tema: conocimiento que el usuario conserva y reemplaza a lo largo del tiempo.
- La cardinalidad es 0..1 por Tema mediante indice unico en roadmap.ApunteTema.TemaId.
- Contenido usa nvarchar(max) para no limitar artificialmente texto potencialmente amplio.
- Se mantiene separado de Tema.Objetivos, Nota append-only, SesionEstudio.Notas y Recurso.Notas.
- No usa RowVersion ni soft delete en V1; RowVersion sigue restringido a Usuario, Tema, SesionEstudio y Proyecto.
- API Personal expone GET/PUT /api/temas/{temaId}/apuntes resolviendo Usuario actual en el borde HTTP; Application conserva UsuarioId explicito.

## Fase.MetadataPedagogica

- Fase incorpora metadata pedagogica descriptiva del roadmap recomendado: Objetivos, CriteriosAvance, MesInicioRecomendado, MesFinRecomendado y CargaSemanalRecomendada.
- Objetivos y CriteriosAvance son listas textuales sin estado; no reutilizan CriterioTema y no representan progreso.
- MesInicioRecomendado y MesFinRecomendado son meses relativos del programa, no fechas reales de estudio.
- CargaSemanalRecomendada es texto para preservar expresiones como "~10 hrs/semana" o "Tiempo completo" sin inventar escala numerica.
- Esta metadata no crea Evidence, no activa Analytics semantico y no modifica Tema.FechaInicio/FechaFin.
- Se mantiene diferido: Evidence planificada, FaseHerramienta, prioridad contextual de herramientas y PlanPortafolio.

## Importador Roadmap V1

- El importador Roadmap V1 es un bootstrap CLI idempotente y controlado, no un seed automatico, no EF HasData, no migracion de datos, no endpoint HTTP y no BackgroundService.
- El importador consume data/roadmap/roadmap-v1.json. El HTML original sigue siendo fuente humana inmutable y no se parsea en runtime.
- SourceKey es identificador del dataset solo durante la ejecucion. No se persiste en entidades, no hay tabla de historial de importacion y V1 no intenta sincronizar renames/deletes futuros.
- La importacion usa una transaccion unica por Usuario destino: si hay conflicto o error de persistencia, no debe quedar un Roadmap parcial.
- El Usuario destino se especifica explicitamente antes de Auth. El importador valida que exista y no crea Usuario automaticamente.
- Fase se resuelve por UsuarioId + Orden; mismo orden con nombre distinto es conflicto. Tema se resuelve por UsuarioId + FaseId + TemaPadreId + Nombre. Recurso se resuelve por UsuarioId + Titulo + Tipo + Url.
- Herramienta y Certificacion permanecen catalogos globales. Se reutilizan por Nombre y no reciben UsuarioId artificial.
- Certificacion con TipoCosto divergente es conflicto. Metadata global divergente no se sobrescribe silenciosamente.
- El importador no crea SesionEstudio, EntradaBitacora, Proyecto, Laboratorio, Writeup, ArtefactoTecnico, CertificacionObtenida, Nota, SnapshotProgreso, Conector ni LogSincronizacion.
- El dataset conserva 31 entradas documentales de Recurso, pero V1 materializa 30 recursos fisicos por deduplicacion de clave natural. No se inventan relaciones RecursoTema cuando el JSON no las expresa.
- La importacion real V1 fue ejecutada sobre AprendizajePersonalDb con Usuario personal creado mediante flujo productivo. La segunda ejecucion confirmo idempotencia fisica sin duplicar Fase, Tema, Recurso, Herramienta ni Certificacion.
