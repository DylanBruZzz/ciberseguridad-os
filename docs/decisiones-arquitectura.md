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

- vw_TemaEstado esta deliberadamente diferida al read side.
- No crear entidad keyless para vw_TemaEstado hasta fase de lectura.
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

## Portafolio V1 Read Side

- Portafolio V1 es una proyeccion de lectura sobre Evidence existente; no es Aggregate Root, no tiene tabla propia y no duplica datos.
- La elegibilidad V1 se define por Evidence visible del Usuario con EstadoMadurez ListoPortafolio o Publicado.
- Borrador, Documentado y Evidence eliminada logicamente quedan fuera del Portafolio V1.
- ListoPortafolio se incluye para previsualizacion personal; Publicado no implica exposicion publica ni web publicada.
- CertificacionObtenida se enriquece desde Certificacion global relacionada, sin modificar ni duplicar el catalogo.
- Temas y Herramientas se muestran solo mediante relaciones persistidas existentes; no hay inferencia por texto, tagging automatico ni AI summaries.
- El endpoint de lectura participa de API Personal V1: en modo Personal puede omitir usuarioId y resolverlo en la capa HTTP; los contratos explicit-user legacy permanecen temporalmente para Development/tests.
- No se crea Snapshot, vista materializada, cache ni motor de scoring para Portafolio V1.

## Tema.Objetivos

- Dominio: Objetivos se expone como coleccion no-null desde la API publica.
- Persistencia: la coleccion vacia se representa actualmente como SQL NULL.
- TemaConfiguration: Objetivos es nullable.
- Tema normaliza el backing field cuando EF materializa NULL.

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
