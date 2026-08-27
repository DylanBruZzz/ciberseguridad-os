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
