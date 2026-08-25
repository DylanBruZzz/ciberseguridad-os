# Estado Proyecto

## Checkpoint Git

Branch: main

Ultimo checkpoint:

```text
b1a4d6c feat: establish technical artifact evidence flow
```

Checkpoint anterior:

```text
6805d19 feat: establish project evidence flow
430e4bf feat: establish laboratory evidence flow
85f077d feat: complete minimal study module
c0d4fc3 test: cover critical persistence behavior
c4e5a59 test: cover core application flows
b05cc6f test: cover core domain behavior
542a2cc test: establish xunit testing foundation
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
- TESTS APPLICATION FUNDACIONALES VALIDADO
- TESTS DE INTEGRACION/PERSISTENCIA FUNDACIONALES VALIDADO SOBRE SQL SERVER REAL
- ENTRADA BITACORA VALIDADO END-TO-END
- HERRAMIENTA SOPORTE MINIMO VALIDADO END-TO-END
- SESION-HERRAMIENTA VALIDADO END-TO-END
- STUDY MINIMO CERRADO
- EVIDENCE LABORATORIO VALIDADO END-TO-END
- LABORATORIO-TEMA VALIDADO END-TO-END
- LABORATORIO-HERRAMIENTA VALIDADO END-TO-END
- EVIDENCE PROYECTO VALIDADO END-TO-END
- PROYECTO-TEMA VALIDADO END-TO-END
- PROYECTO-HERRAMIENTA VALIDADO END-TO-END
- ROWVERSION DE PROYECTO PROTEGIDA POR TESTS DE INTEGRACION
- EVIDENCE ARTEFACTOTECNICO VALIDADO END-TO-END
- ARTEFACTO-TEMA VALIDADO END-TO-END
- ARTEFACTO-HERRAMIENTA VALIDADO END-TO-END
- EVIDENCE WRITEUP VALIDADO END-TO-END
- WRITEUP-TEMA VALIDADO END-TO-END

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
- EntradaBitacora: 1
- Herramienta: 1
- SesionHerramienta: 1
- Laboratorio: 1
- LaboratorioTema: 1
- LaboratorioHerramienta: 1
- Proyecto: 1
- ProyectoTema: 1
- ProyectoHerramienta: 1
- ArtefactoTecnico: 1
- ArtefactoTema: 1
- ArtefactoHerramienta: 1
- Writeup: 1
- WriteupTema: 1

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

EntradaBitacora E2E:

- Id: 01A0364E-1D06-7644-9F33-84D1597A3942
- Usuario: 01A015CC-1AC8-7EB0-A2C8-5D2A33894DCC
- Tema: 01A016F7-1517-7C35-BAF3-A1BEB648776C
- Texto: Repaso inicial del modelo OSI

Herramienta E2E:

- Id: 01A0364E-1F1D-7418-92F8-BE5DBC14C7FD
- Nombre: Wireshark E2E Study
- Categoria: Redes
- Vinculada a SesionEstudio: 01A01FD6-072F-7DB6-8513-F75C0C993AF9
- Vinculada a Laboratorio: 01A0366B-9775-7BFD-8F16-408B94E557CA

Laboratorio E2E:

- Id: 01A0366B-9775-7BFD-8F16-408B94E557CA
- Usuario: 01A015CC-1AC8-7EB0-A2C8-5D2A33894DCC
- Nombre: Análisis de tráfico OSI con Wireshark
- Objetivo: Identificar capas del modelo OSI en tráfico capturado
- EntornoVms: Wireshark
- Hallazgos: Tráfico de prueba clasificado por capas
- TiempoInvertidoMinutos: 45
- Fecha: 2026-08-24
- EstadoMadurez: Borrador
- Tema vinculado: 01A016F7-1517-7C35-BAF3-A1BEB648776C
- Herramienta vinculada: 01A0364E-1F1D-7418-92F8-BE5DBC14C7FD

Proyecto E2E:

- Id: 01A036BB-D0BC-71EA-A339-1178A34A7F0A
- Usuario: 01A015CC-1AC8-7EB0-A2C8-5D2A33894DCC
- Nombre: Analizador de tráfico OSI
- Estado: Idea
- EstadoMadurez: Borrador
- RowVersion: 0x00000000000101d1
- Tema vinculado: 01A016F7-1517-7C35-BAF3-A1BEB648776C
- Herramienta vinculada: 01A0364E-1F1D-7418-92F8-BE5DBC14C7FD

ArtefactoTecnico E2E:

- Id: 01A03704-78C5-73A5-B683-0AB4E0C213FA
- Usuario: 01A015CC-1AC8-7EB0-A2C8-5D2A33894DCC
- TipoArtefacto: Cheatsheet
- Nombre: Filtros Wireshark para análisis OSI
- EstadoMadurez: Borrador
- Tema vinculado: 01A016F7-1517-7C35-BAF3-A1BEB648776C
- Herramienta vinculada: 01A0364E-1F1D-7418-92F8-BE5DBC14C7FD

Writeup E2E:

- Id: 01A03716-FBAF-706E-830A-13D1B5985137
- Usuario: 01A015CC-1AC8-7EB0-A2C8-5D2A33894DCC
- Titulo: Análisis del modelo OSI con Wireshark
- PlataformaOrigen: null
- Url: null
- EstadoMadurez: Borrador
- Tema vinculado: 01A016F7-1517-7C35-BAF3-A1BEB648776C

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
- POST /api/entradas-bitacora -> 201
- GET /api/entradas-bitacora/{id} -> 200
- GET /api/entradas-bitacora?usuarioId={id} -> 200
- GET /api/entradas-bitacora?usuarioId={id-sin-entradas} -> 200 con []
- GET /api/entradas-bitacora?usuarioId={Guid.Empty} -> 400
- POST /api/herramientas -> 201
- GET /api/herramientas/{id} -> 200
- GET /api/herramientas -> 200
- PUT /api/sesiones-estudio/{id}/herramientas/{herramientaId} -> 204
- PUT vinculo SesionEstudio-Herramienta repetido -> 204 idempotente
- POST /api/laboratorios -> 201
- GET /api/laboratorios/{id} -> 200
- GET /api/laboratorios?usuarioId={id} -> 200
- GET /api/laboratorios?usuarioId={id-sin-laboratorios} -> 200 con []
- GET /api/laboratorios?usuarioId={Guid.Empty} -> 400
- PUT /api/laboratorios/{id}/temas/{temaId} -> 204
- PUT vinculo Laboratorio-Tema repetido -> 204 idempotente
- PUT /api/laboratorios/{id}/herramientas/{herramientaId} -> 204
- PUT vinculo Laboratorio-Herramienta repetido -> 204 idempotente
- POST /api/proyectos -> 201
- GET /api/proyectos/{id} -> 200
- GET /api/proyectos?usuarioId={id} -> 200
- GET /api/proyectos?usuarioId={Guid.Empty} -> 400
- PUT /api/proyectos/{id}/temas/{temaId} -> 204
- PUT vinculo Proyecto-Tema repetido -> 204 idempotente
- PUT /api/proyectos/{id}/herramientas/{herramientaId} -> 204
- PUT vinculo Proyecto-Herramienta repetido -> 204 idempotente
- POST /api/artefactos-tecnicos -> 201
- GET /api/artefactos-tecnicos/{id} -> 200
- GET /api/artefactos-tecnicos?usuarioId={id} -> 200
- GET /api/artefactos-tecnicos?usuarioId={Guid.Empty} -> 400
- PUT /api/artefactos-tecnicos/{id}/temas/{temaId} -> 204
- PUT vinculo Artefacto-Tema repetido -> 204 idempotente
- PUT /api/artefactos-tecnicos/{id}/herramientas/{herramientaId} -> 204
- PUT vinculo Artefacto-Herramienta repetido -> 204 idempotente
- POST /api/writeups -> 201
- GET /api/writeups/{id} -> 200
- GET /api/writeups?usuarioId={id} -> 200
- GET /api/writeups?usuarioId={Guid.Empty} -> 400
- PUT /api/writeups/{id}/temas/{temaId} -> 204
- PUT vinculo Writeup-Tema repetido -> 204 idempotente

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
- dotnet test por solucion: validado con 191 tests correctos
- Smoke test actual: Tema.Crear expone Objetivos como coleccion no-null y vacia.
- Tests de Dominio Tema: objetivos, fase, jerarquia directa, criterios, dominio y TemaDominadoEvento validados.
- Tests de Dominio SesionEstudio: registro, invariantes, correccion de duracion y SesionRegistradaEvento validados.
- Tests de Dominio EntradaBitacora y Herramienta validados.
- Tests de Dominio Laboratorio validados.
- Tests de Dominio Proyecto validados.
- Tests de Dominio ArtefactoTecnico validados.
- Tests de Dominio Writeup validados.
- Tests de Application Roadmap: flujos criticos de Tema cubiertos con fakes minimos.
- Tests de Application Resource: crear, obtener, listar y vincular cubiertos con fakes minimos.
- Tests de Application Study: SesionEstudio, EntradaBitacora, Herramienta y SesionHerramienta cubiertos con fakes minimos.
- Tests de Application Evidence: Laboratorio crear, obtener, listar, vincular a Tema y vincular a Herramienta cubiertos con fakes minimos.
- Tests de Application Evidence: Proyecto crear, obtener, listar, vincular a Tema y vincular a Herramienta cubiertos con fakes minimos.
- Tests de Application Evidence: ArtefactoTecnico crear, obtener, listar, vincular a Tema y vincular a Herramienta cubiertos con fakes minimos.
- Tests de Application Evidence: Writeup crear, obtener, listar y vincular a Tema cubiertos con fakes minimos.
- Tests de integracion/persistencia: SQL Server real .\MSSQLSERVER01 con base exclusiva AprendizajeTestsDb.
- Guard rail de integracion: rechaza AprendizajeDb, database vacio y cualquier base distinta a AprendizajeTestsDb antes de recrear.
- Tests de persistencia cubren: Tema.Objetivos vacios como SQL NULL y rematerializacion no-null, RowVersion de SesionEstudio tras update, idempotencia fisica RecursoTema, idempotencia fisica SesionHerramienta, idempotencia fisica LaboratorioTema, idempotencia fisica LaboratorioHerramienta, idempotencia fisica ProyectoTema, idempotencia fisica ProyectoHerramienta, idempotencia fisica ArtefactoTema, idempotencia fisica ArtefactoHerramienta, idempotencia fisica WriteupTema, RowVersion de Proyecto poblada al insertar y estable al vincular joins, query filter de soft delete en Tema y FK real SesionEstudio -> Tema.
- Frameworks de mocking: no utilizados.
- Tests de integracion fundacionales: validados.

## Estado Git Esperado

- working tree clean

## Proxima Area

Slices Evidence funcionales cerrados: Laboratorio, Proyecto, ArtefactoTecnico y Writeup con crear/obtener/listar y vinculos reales validados.

Proxima area sugerida: continuar Evidence con CertificacionObtenida, Nota o el siguiente slice aprobado.

## Pendientes Deliberados

- tests completos de dominio;
- tests de integracion;
- IDespachadorEventos concreto;
- registro DespachoEventosInterceptor;
- vw_TemaEstado;
- read side;
- Application dependency policy tests;
- TemaDependencia race;
- deteccion completa de ciclos profundos en jerarquia de Temas;
- prueba automatizada directa de TemaDominadoEvento;
- concurrencia HTTP/ETag/If-Match para Proyecto;
- updates de Proyecto;
- Nota/CertificacionObtenida;
- auth;
- analytics;
- integrations;
- frontend;
- GitHub remote.
