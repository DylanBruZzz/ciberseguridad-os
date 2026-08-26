# Estado Proyecto

## Checkpoint Git

Branch: main

Ultimo checkpoint:

```text
a6d52a3 feat: establish certification topic roadmap flow
```

Checkpoint anterior:

```text
8ec5c93 feat: establish competency roadmap flow
8940e65 feat: establish advanced topic planning flow
96277a0 feat: complete minimal evidence module
603e62d feat: establish certification evidence flow
f9e452a feat: establish writeup evidence flow
b1a4d6c feat: establish technical artifact evidence flow
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
- CERTIFICACION SOPORTE MINIMO VALIDADO END-TO-END
- EVIDENCE CERTIFICACIONOBTENIDA VALIDADO END-TO-END
- FK CERTIFICACIONOBTENIDA-CERTIFICACION VALIDADA SOBRE SQL SERVER REAL
- EVIDENCE NOTA VALIDADO END-TO-END
- NOTA CON 5 TIPOS DE PADRE VALIDADA
- OWNERSHIP NOTA-PADRE VALIDADO EN APPLICATION
- CK_NOTA_UNSOLOPADRE VALIDADO SOBRE SQL SERVER REAL
- HITO EVIDENCE MINIMO FUNCIONAL CERRADO
- ROADMAP AVANZADO TEMA PLANIFICACION/PERCEPCION VALIDADO END-TO-END
- INTERVALO REPASO DE TEMA CONFIGURABLE END-TO-END
- ROWVERSION DE TEMA VERIFICADA TRAS UPDATE REAL
- ROADMAP AVANZADO COMPETENCIA VALIDADO END-TO-END
- COMPETENCIA-TEMA VALIDADO END-TO-END
- OWNERSHIP COMPETENCIA-TEMA VALIDADO EN APPLICATION
- ROADMAP AVANZADO CERTIFICACION-TEMA VALIDADO END-TO-END
- CERTIFICACION-TEMA VALIDADO COMO VINCULO GLOBAL SIN OWNERSHIP ARTIFICIAL
- PESO DE CERTIFICACION-TEMA PERMANECE NULL Y SEMANTICAMENTE DIFERIDO
- HITO ROADMAP AVANZADO MINIMO FUNCIONAL CERRADO
- TEMADEPENDENCIA DIFERIDA POR ACICLICIDAD Y CONCURRENCIA NO RESUELTAS
- ANALYTICS MINIMO INICIADO
- RESUMEN DE ESTUDIO POR USUARIO VALIDADO END-TO-END
- ANALYTICS RESUMEN ESTUDIO FUNCIONA ON-DEMAND SIN SNAPSHOT NI VISTA

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
- Certificacion: 1
- CertificacionTema: 1
- CertificacionObtenida: 1
- Nota: 1
- Competencia: 1
- CompetenciaTema: 1

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
- FechaInicio: 2026-08-25
- FechaFin: 2026-10-15
- DificultadPercibida: 4
- Confianza: 5
- IntervaloRepasoDias: 21
- RowVersion: 0x0000000000014053
- Criterios:
  - Teoria: cumplido
  - Practica: cumplido

Competencia E2E:

- Id: 01A03AE8-7EEF-7981-B925-D1A8A66B0A53
- Usuario: 01A015CC-1AC8-7EB0-A2C8-5D2A33894DCC
- Nombre: Comprensión de fundamentos de redes E2E Roadmap
- Descripcion: null
- Tema vinculado: 01A016F7-1517-7C35-BAF3-A1BEB648776C

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

Certificacion E2E:

- Id: 01A0396F-2D99-7805-AEF4-D5A85D587BED
- Nombre: CompTIA Network+ E2E Evidence
- Proveedor: null
- TipoCosto: Pago
- Url: null
- Catalogo global: si
- Tema vinculado: 01A016F7-1517-7C35-BAF3-A1BEB648776C
- CertificacionTema.Peso: null

CertificacionObtenida E2E:

- Id: 01A0396F-3B45-7C8C-84AE-D870B729E04D
- Usuario: 01A015CC-1AC8-7EB0-A2C8-5D2A33894DCC
- Certificacion: 01A0396F-2D99-7805-AEF4-D5A85D587BED
- FechaObtencion: 2026-08-25
- EvidenciaUrl: null
- EstadoMadurez: Documentado

Nota E2E:

- Id: 01A0398C-3C84-7B1B-87D6-5A5D773A26C4
- Usuario: 01A015CC-1AC8-7EB0-A2C8-5D2A33894DCC
- Proyecto: 01A036BB-D0BC-71EA-A339-1178A34A7F0A
- Tema: null
- Laboratorio: null
- Writeup: null
- ArtefactoTecnico: null
- Texto: Observación de cierre del proyecto OSI
- Tipo: Nota
- Padres informados: 1
- CertificacionObtenida no es padre de Nota.

## Flujos Funcionales Actuales

- POST /api/usuarios -> 201
- POST /api/temas -> 201
- GET /api/temas/{id} -> 200
- GET Tema inexistente -> 404
- PUT /api/temas/{id}/objetivos -> 204
- PUT objetivos Tema inexistente -> 404
- PUT /api/temas/{id}/percepcion -> 204
- PUT percepcion Tema inexistente -> 404
- PUT percepcion con Guid.Empty -> 400
- PUT percepcion con NivelPercepcion fuera de rango -> 400
- PUT /api/temas/{id}/planificacion -> 204
- PUT planificacion Tema inexistente -> 404
- PUT planificacion con Guid.Empty -> 400
- PUT planificacion con fechas invalidas -> 400
- PUT /api/temas/{id}/intervalo-repaso -> 204
- PUT intervalo-repaso Tema inexistente -> 404
- PUT intervalo-repaso con Guid.Empty -> 400
- PUT intervalo-repaso con dias invalidos -> 400
- GET /api/temas/{id} devuelve dificultad, confianza, fechas e IntervaloRepasoDias configurados.
- GET /api/temas?usuarioId={id} -> 200
- GET /api/temas?usuarioId={id-sin-temas} -> 200 con []
- GET /api/temas?usuarioId={Guid.Empty} -> 400
- POST /api/competencias -> 201
- GET /api/competencias/{id} -> 200
- GET /api/competencias?usuarioId={id} -> 200
- GET /api/competencias?usuarioId={id-sin-competencias} -> 200 con []
- GET /api/competencias?usuarioId={Guid.Empty} -> 400
- PUT /api/competencias/{id}/temas/{temaId} -> 204
- PUT vinculo Competencia-Tema repetido -> 204 idempotente
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
- POST /api/certificaciones -> 201
- GET /api/certificaciones/{id} -> 200
- GET /api/certificaciones -> 200
- PUT /api/certificaciones/{id}/temas/{temaId} -> 204
- PUT vinculo Certificacion-Tema repetido -> 204 idempotente
- POST /api/certificaciones-obtenidas -> 201
- GET /api/certificaciones-obtenidas/{id} -> 200
- GET /api/certificaciones-obtenidas?usuarioId={id} -> 200
- GET /api/certificaciones-obtenidas?usuarioId={id-sin-certificaciones} -> 200 con []
- GET /api/certificaciones-obtenidas?usuarioId={Guid.Empty} -> 400
- POST /api/temas/{temaId}/notas -> 201
- POST /api/proyectos/{proyectoId}/notas -> 201
- POST /api/laboratorios/{laboratorioId}/notas -> 201
- POST /api/writeups/{writeupId}/notas -> 201
- POST /api/artefactos-tecnicos/{artefactoTecnicoId}/notas -> 201
- POST Nota con padre inexistente -> 404
- POST Nota con padre de otro Usuario -> 409
- GET /api/notas/{id} -> 200
- GET /api/notas?usuarioId={id} -> 200
- GET /api/notas?usuarioId={Guid.Empty} -> 400
- GET /api/analytics/estudio?usuarioId={id} -> 200
- GET /api/analytics/estudio?usuarioId={id-sin-sesiones} -> 200 con resumen vacio
- GET /api/analytics/estudio?usuarioId={Guid.Empty} -> 400

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
- dotnet test por solucion: validado con 304 tests correctos
- Smoke test actual: Tema.Crear expone Objetivos como coleccion no-null y vacia.
- Tests de Dominio Tema: objetivos, fase, jerarquia directa, criterios, planificacion, percepcion, IntervaloRepaso, dominio y TemaDominadoEvento validados.
- Tests de Dominio Competencia validados.
- Tests de Dominio SesionEstudio: registro, invariantes, correccion de duracion y SesionRegistradaEvento validados.
- Tests de Dominio EntradaBitacora y Herramienta validados.
- Tests de Dominio Laboratorio validados.
- Tests de Dominio Proyecto validados.
- Tests de Dominio ArtefactoTecnico validados.
- Tests de Dominio Writeup validados.
- Tests de Dominio Certificacion y CertificacionObtenida validados.
- Tests de Dominio Nota validados.
- Tests de Application Roadmap: flujos criticos de Tema, planificacion, percepcion, IntervaloRepaso y Competencia/CompetenciaTema cubiertos con fakes minimos.
- Tests de Application Resource: crear, obtener, listar y vincular cubiertos con fakes minimos.
- Tests de Application Study: SesionEstudio, EntradaBitacora, Herramienta y SesionHerramienta cubiertos con fakes minimos.
- Tests de Application Evidence: Laboratorio crear, obtener, listar, vincular a Tema y vincular a Herramienta cubiertos con fakes minimos.
- Tests de Application Evidence: Proyecto crear, obtener, listar, vincular a Tema y vincular a Herramienta cubiertos con fakes minimos.
- Tests de Application Evidence: ArtefactoTecnico crear, obtener, listar, vincular a Tema y vincular a Herramienta cubiertos con fakes minimos.
- Tests de Application Evidence: Writeup crear, obtener, listar y vincular a Tema cubiertos con fakes minimos.
- Tests de Application Roadmap: Certificacion crear, obtener, listar y vincular a Tema cubiertos con fake minimo.
- Tests de Application Evidence: CertificacionObtenida crear, obtener y listar cubiertos con fakes minimos.
- Tests de Application Evidence: Nota crear sobre Tema/Proyecto/Laboratorio/Writeup/ArtefactoTecnico, obtener y listar cubiertos con fakes minimos.
- Tests de Application Analytics: ResumenEstudio valida Guid.Empty y contrato del caso de uso.
- Tests de integracion/persistencia: SQL Server real .\MSSQLSERVER01 con base exclusiva AprendizajeTestsDb.
- Guard rail de integracion: rechaza AprendizajeDb, database vacio y cualquier base distinta a AprendizajeTestsDb antes de recrear.
- Tests de persistencia cubren: Tema.Objetivos vacios como SQL NULL y rematerializacion no-null, planificacion/percepcion/IntervaloRepaso de Tema, RowVersion de Tema tras update, RowVersion de SesionEstudio tras update, idempotencia fisica RecursoTema, idempotencia fisica CompetenciaTema, idempotencia fisica CertificacionTema con Peso NULL, idempotencia fisica SesionHerramienta, idempotencia fisica LaboratorioTema, idempotencia fisica LaboratorioHerramienta, idempotencia fisica ProyectoTema, idempotencia fisica ProyectoHerramienta, idempotencia fisica ArtefactoTema, idempotencia fisica ArtefactoHerramienta, idempotencia fisica WriteupTema, RowVersion de Proyecto poblada al insertar y estable al vincular joins, FK real CertificacionObtenida -> Certificacion, valores iniciales de CertificacionObtenida, query filter de CertificacionObtenida, Nota con un padre valido, CHECK CK_Nota_UnSoloPadre para cero y dos padres, query filter de Nota, query filter de soft delete en Tema y FK real SesionEstudio -> Tema.
- Frameworks de mocking: no utilizados.
- Tests de integracion fundacionales: validados.
- Tests de integracion Analytics: ResumenEstudio agrega sesiones visibles por usuario, aisla otros usuarios, devuelve resumen vacio y excluye sesiones eliminadas logicamente.

## Estado Git Esperado

- working tree clean

## Proxima Area

Roadmap avanzado minimo funcional cerrado: Fase, jerarquia Tema padre/hijo, objetivos, criterios de dominio, dificultad percibida, confianza, FechaInicio/FechaFin, IntervaloRepaso, Competencia/CompetenciaTema y Certificacion/CertificacionTema estan disponibles y validados.

TemaDependencia queda diferida conscientemente: aporta prerequisitos transversales utiles, pero no es necesaria para importacion inicial ni para Analytics minimo. Exponerla ahora dejaria incompleta la garantia de ausencia de ciclos profundos y no resolveria la carrera read -> validate -> insert bajo concurrencia.

Proxima area sugerida: continuar Analytics/read side minimo con metricas directas por Tema/Evidence, sin usar Peso como formula y sin depender de TemaDependencia.

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
- aciclicidad completa de TemaDependencia;
- estrategia concurrente de TemaDependencia;
- semantica operativa de CertificacionTema.Peso;
- prueba automatizada directa de TemaDominadoEvento;
- concurrencia HTTP/ETag/If-Match para Tema y Proyecto;
- updates de Proyecto;
- auth;
- SnapshotProgreso operativo;
- integrations;
- frontend;
- GitHub remote.
