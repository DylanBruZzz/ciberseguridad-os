# Estado Proyecto

## Checkpoint Git

Branch: main

Ultimo checkpoint:

```text
542a2cc test: establish xunit testing foundation
```

Checkpoint anterior:

```text
73aa27c feat: establish study session flow
0c4ee2d docs: formalize Codex execution modes
a3382e8 feat: establish resource library flow
16aeec2 feat: establish topic mastery criteria flow
9739d1b feat: establish topic hierarchy flow
2d19acd feat: assign topic to phase
2250c9a feat: add phase listing flow
754bbaa feat: establish phase creation flow
3e6043d feat: add topic listing flow
7e9c808 feat: establish topic objectives flow
8bfdcb8 docs: add Codex operating context
1f7dcb7 feat: validate first end-to-end roadmap flow
caf832a feat: establish validated initial persistence
```

## Estado General

- PERSISTENCIA INICIAL VALIDADA
- PRIMER FLUJO FUNCIONAL END-TO-END VALIDADO
- ESTABLECER OBJETIVOS DE TEMA VALIDADO END-TO-END
- LISTAR TEMAS VALIDADO END-TO-END
- CREAR FASE VALIDADO END-TO-END
- LISTAR FASES VALIDADO END-TO-END
- ASIGNAR TEMA A FASE VALIDADO END-TO-END
- ASIGNAR TEMA PADRE VALIDADO END-TO-END
- MOTOR DE CRITERIOS DE TEMA VALIDADO END-TO-END
- RESOURCE / BIBLIOTECA DE RECURSOS VALIDADO END-TO-END
- STUDY / SESIONES DE ESTUDIO VALIDADO END-TO-END
- INFRAESTRUCTURA DE TESTS XUNIT MTP VALIDADA
- TESTS DE DOMINIO FUNDACIONALES VALIDADO

## Migraciones Aplicadas

- 20260818153945_Inicial
- 20260818174900_HacerObjetivosTemaNullable

## Base de Desarrollo

- SQL Server local: .\MSSQLSERVER01
- Base: AprendizajeDb
- Environment: Development
- Secretos: ninguno documentado aqui.

## Datos E2E Actuales

- Usuario: 1
- Tema: 2
- Fase: 1
- Recurso: 1
- SesionEstudio: 1

Usuario E2E:

- Id: 01A015CC-1AC8-7EB0-A2C8-5D2A33894DCC
- Nombre: Dylan
- Email: dylan.e2e@local.test

Tema E2E:

- Id: 01A01604-8436-742A-A59B-B756B8FF07B3
- Nombre: Fundamentos de redes
- TipoConocimiento: Conceptual
- FaseId: 01A016CB-92F1-75B0-B5F6-803F92691273
- TemaPadreId: null
- Objetivos:
  - Comprender el modelo OSI
  - Diferenciar TCP y UDP

Subtema E2E:

- Id: 01A016F7-1517-7C35-BAF3-A1BEB648776C
- Nombre: Modelo OSI
- TipoConocimiento: Conceptual
- TemaPadreId: 01A01604-8436-742A-A59B-B756B8FF07B3
- Criterios:
  - Teoria: cumplido
  - Practica: cumplido

Recurso E2E:

- Id: 01A01DEB-1C4A-7C05-9C43-1F08CADBA7C2
- Titulo: Documentación modelo OSI
- Tipo: Documentacion
- Estado: PorClasificar
- Tema vinculado: 01A016F7-1517-7C35-BAF3-A1BEB648776C

Fase E2E:

- Id: 01A016CB-92F1-75B0-B5F6-803F92691273
- Nombre: Fundamentos
- Orden: 1

SesionEstudio E2E:

- Id: 01A01FD6-072F-7DB6-8513-F75C0C993AF9
- Tema: 01A016F7-1517-7C35-BAF3-A1BEB648776C
- Tipo: Teoria
- Fecha: 2026-08-20
- DuracionMinutos: 45
- Notas: Estudio inicial del modelo OSI
- RowVersion: verificada fisicamente y actualizada tras corregir duracion.

## Flujos Funcionales Actuales

- POST /api/usuarios -> 201
- POST /api/temas -> 201
- GET /api/temas/{id} -> 200
- GET Tema inexistente -> 404
- PUT /api/temas/{id}/objetivos -> 204
- PUT objetivos Tema inexistente -> 404
- GET /api/temas?usuarioId={id} -> 200
- GET /api/temas?usuarioId={id-sin-temas} -> 200 con []
- GET /api/temas?usuarioId={Guid.Empty} -> 400
- POST /api/fases -> 201
- GET /api/fases?usuarioId={id} -> 200
- GET /api/fases?usuarioId={id-sin-fases} -> 200 con []
- GET /api/fases?usuarioId={Guid.Empty} -> 400
- PUT /api/temas/{id}/fase -> 204
- PUT fase Tema inexistente -> 404
- PUT fase Fase inexistente -> 404
- PUT fase con Guid.Empty -> 400
- PUT /api/temas/{id}/padre -> 204
- PUT padre Tema hijo inexistente -> 404
- PUT padre Tema padre inexistente -> 404
- PUT padre self-parent -> 400
- PUT padre ciclo directo -> 409
- PUT /api/temas/{id}/criterios -> 204
- PUT criterios con menos de 2 criterios distintos -> 400
- PUT criterios duplicados -> 204, normalizados por dominio
- PUT criterios con progreso registrado -> 409
- PUT /api/temas/{id}/criterios/{tipo}/cumplido -> 204
- PUT criterio no definido -> 409
- DELETE /api/temas/{id}/criterios/{tipo}/cumplido -> 204
- DELETE criterio no definido -> 409
- Transicion observable a Tema dominado -> validada con todos los criterios cumplidos
- POST /api/recursos -> 201
- GET /api/recursos/{id} -> 200
- GET Recurso inexistente -> 404
- GET /api/recursos?usuarioId={id} -> 200
- GET /api/recursos?usuarioId={id-sin-recursos} -> 200 con []
- GET /api/recursos?usuarioId={Guid.Empty} -> 400
- PUT /api/recursos/{id}/temas/{temaId} -> 204
- PUT vínculo Recurso-Tema repetido -> 204 idempotente
- PUT vínculo con Recurso inexistente -> 404
- PUT vínculo con Tema inexistente -> 404
- PUT vínculo con Guid.Empty -> 400
- POST /api/sesiones-estudio -> 201
- GET /api/sesiones-estudio/{id} -> 200
- GET SesionEstudio inexistente -> 404
- GET /api/sesiones-estudio?usuarioId={id} -> 200
- GET /api/sesiones-estudio?usuarioId={id-sin-sesiones} -> 200 con []
- GET /api/sesiones-estudio?usuarioId={Guid.Empty} -> 400
- PUT /api/sesiones-estudio/{id}/duracion -> 204
- PUT duracion SesionEstudio inexistente -> 404
- PUT duracion con Guid.Empty -> 400
- PUT duracion invalida -> 400
- SesionRegistradaEvento confirmado por codigo; despacho sigue diferido.

## Build

- 0 warnings
- 0 errores

## Tests

- Proyecto: tests/Aprendizaje.Tests
- Framework: xUnit v3 3.2.2
- Runner: Microsoft Testing Platform mediante global.json
- SDK validado: .NET 10.0.400
- Microsoft.NET.Test.Sdk: no requerido con la estrategia MTP actual
- dotnet run del proyecto de tests: validado
- dotnet test por proyecto: validado
- dotnet test por solucion: validado
- Smoke test actual: Tema.Crear expone Objetivos como coleccion no-null y vacia.
- Tests de Dominio Tema: objetivos, fase, jerarquia directa, criterios, dominio y TemaDominadoEvento validados.
- Tests de Dominio SesionEstudio: registro, invariantes, correccion de duracion y SesionRegistradaEvento validados.
- Tests de Application: pendientes.
- Tests de integracion: pendientes.

## Estado Git Esperado

- working tree clean

## Proxima Area

Tests / Dominio.

Continuar con tests de dominio y Application antes de ampliar infraestructura de integracion.

## Pendientes Deliberados

- tests completos de dominio;
- tests de Application;
- tests de integracion;
- IDespachadorEventos concreto;
- registro DespachoEventosInterceptor;
- vw_TemaEstado;
- read side;
- Application dependency policy tests;
- TemaDependencia race;
- deteccion completa de ciclos profundos en jerarquia de Temas;
- prueba automatizada directa de TemaDominadoEvento;
- auth;
- analytics;
- integrations;
- frontend;
- GitHub remote.
