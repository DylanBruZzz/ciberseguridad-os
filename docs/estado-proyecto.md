# Estado Proyecto

## Checkpoint Funcional Documentado

Branch: main

Checkpoint funcional base:

```text
352e285 feat: establish competency certification analytics read models
```

Politica:

```text
Este documento registra el checkpoint funcional base conocido, no el hash del commit que lo contiene.
Asi se evita el desfase autorreferencial producido por el flujo documentar -> commit.
```

Historial previo:

```text
f1e404e feat: establish study analytics read model
625d09f docs: close minimal advanced roadmap milestone
a6d52a3 feat: establish certification topic roadmap flow
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
- RESUMEN DIRECTO POR TEMA VALIDADO END-TO-END
- ANALYTICS RESUMEN TEMA EXPONE METRICAS FACTUALES SIN ESTADO CALCULADO
- RESUMEN ESTRUCTURAL DE COMPETENCIAS VALIDADO END-TO-END
- RESUMEN ESTRUCTURAL DE CERTIFICACIONES VALIDADO END-TO-END
- ANALYTICS DIRECTO MINIMO FUNCIONAL CERRADO
- METADATA PEDAGOGICA DE FASE VALIDADA END-TO-END
- ESPECIFICACION NORMALIZADA ROADMAP V1 VERSIONADA
- IMPORTADOR CLI ROADMAP V1 VALIDADO END-TO-END
- IMPORTADOR ROADMAP V1 ES BOOTSTRAP IDEMPOTENTE, NO MOTOR DE SINCRONIZACION
- IMPORTADOR ROADMAP V1 NO CREA EVIDENCE NI SESIONES
- APRENDIZAJEPERSONALDB CREADA Y MIGRADA PARA USO PERSONAL V1
- USUARIO PERSONAL CREADO MEDIANTE FLUJO PRODUCTIVO
- ROADMAP REAL V1 IMPORTADO EN APRENDIZAJEPERSONALDB
- IMPORTACION REAL ROADMAP V1 VALIDADA CON IDEMPOTENCIA FISICA
- RESOURCE EDITABLE V1 VALIDADO END-TO-END
- RECURSO ACTUALIZABLE CON OWNERSHIP EXPLICITO
- SOFT DELETE DE RECURSO EXPUESTO Y VALIDADO SOBRE SQL SERVER REAL
- STUDY CORRECTIONS V1 VALIDADO END-TO-END
- SESIONESTUDIO CORREGIBLE CON OWNERSHIP EXPLICITO
- SOFT DELETE DE SESIONESTUDIO EXPUESTO Y VALIDADO SOBRE SQL SERVER REAL
- EVIDENCE EDITABLE + MATURITY V1 VALIDADO END-TO-END
- PROYECTO, LABORATORIO, WRITEUP, ARTEFACTOTECNICO Y CERTIFICACIONOBTENIDA CORREGIBLES CON OWNERSHIP EXPLICITO
- ESTADOMADUREZ OPERATIVO PARA EVIDENCE Y LISTO PARA PORTAFOLIO READ SIDE
- SOFT DELETE DE EVIDENCE EXPUESTO Y VALIDADO SOBRE SQL SERVER REAL
- PORTAFOLIO V1 READ SIDE VALIDADO END-TO-END
- PORTAFOLIO ES PROYECCION SOBRE EVIDENCE EXISTENTE, SIN AGGREGATE ROOT NI TABLA PROPIA
- ELEGIBILIDAD PORTAFOLIO DEFINIDA POR ESTADOMADUREZ LISTOPORTAFOLIO/PUBLICADO
- CONTEXTO SINGLE-USER LOCAL V1 VALIDADO END-TO-END
- RUNTIME PERSONAL CONFIGURADO PARA APRENDIZAJEPERSONALDB
- USUARIO ACTUAL LOCAL RESUELTO POR CARDINALIDAD VISIBLE
- GET /api/usuario-actual VALIDADO EN RUNTIME PERSONAL
- API PERSONAL V1 VALIDADA END-TO-END
- FRONTEND V1 YA NO REQUIERE ENVIAR USUARIOID PARA FLUJOS DIARIOS
- FRONTEND FOUNDATION V1 VALIDADO END-TO-END
- ROADMAP REAL VISIBLE DESDE FRONTEND SIN USUARIOID
- APUNTES PERMANENTES DE TEMA V1 BACKEND VALIDADO END-TO-END
- APUNTETEMA SOPORTA CONTENIDO PERSONAL EDITABLE SIN REUTILIZAR NOTA APPEND-ONLY
- API PERSONAL GET/PUT /api/temas/{temaId}/apuntes SIN USUARIOID VALIDADA
- ROADMAPVISTAV1 READ-SIDE BACKEND VALIDADO END-TO-END
- API PERSONAL GET /api/roadmap/vista SIN USUARIOID VALIDADA
- PROGRESO DE ROADMAP, FASE ACTUAL Y REPASO DERIVADOS SIN PERSISTENCIA NUEVA
- TEMAWORKSPACEV1 READ-SIDE BACKEND VALIDADO END-TO-END
- API PERSONAL GET /api/temas/{temaId}/workspace SIN USUARIOID VALIDADA
- TEMAWORKSPACEV1 REUTILIZA SEMANTICA ROADMAPVISTAV1 PARA PROGRESO, ESTADO Y REPASO

## Migraciones Aplicadas

- 20260818153945_Inicial
- 20260818174900_HacerObjetivosTemaNullable
- 20260827073704_AgregarMetadataPedagogicaFase

## Migraciones Disponibles Pendientes De Aplicacion Operativa

- 20260901003645_AgregarApuntesPermanentesTema: crea roadmap.ApunteTema y fue validada por tests contra AprendizajeTestsDb. No fue aplicada a AprendizajePersonalDb.

## Base de Desarrollo

- SQL Server local: .\MSSQLSERVER01
- Base: AprendizajeDb
- Environment: Development
- Secretos: ninguno documentado aqui.

## Bases Operativas

- AprendizajeDb: base de desarrollo/E2E.
- AprendizajeTestsDb: base exclusiva de pruebas automatizadas.
- AprendizajePersonalDb: base personal real V1.
- Environment Personal: configurado para usar AprendizajePersonalDb mediante appsettings.Personal.json.
- El contexto single-user local resuelve el unico Usuario visible en modo Personal. Cero o multiples usuarios visibles son error operativo.
- API Personal resuelve usuarioId en el borde HTTP mediante IUsuarioActual. El frontend V1 puede omitir usuarioId en flujos diarios de Roadmap, Resource, Study, Evidence, Analytics y Portafolio.
- Los contratos explicit-user legacy siguen disponibles temporalmente para Development/tests; en modo Personal un usuarioId explicito que no coincide con el Usuario actual se rechaza como conflicto.

## Frontend V1

- Stack: Angular 21 standalone.
- Ubicacion: frontend/
- Routing: habilitado.
- Estilos: CSS nativo.
- Base API frontend: /api.
- Desarrollo frontend: Angular dev server con proxy local hacia http://localhost:64021.
- No se almacena UsuarioId, Email, connection string ni secretos en el frontend.
- Usuario actual: GET /api/usuario-actual devuelve Id y Nombre; el Id es informativo y no se usa para construir requests de ownership.
- Roadmap inicial: GET /api/fases y GET /api/temas se consumen sin usuarioId desde frontend.
- Runtime Personal validado en modo read-only:
  - UsuarioActual: Dylan
  - Fases: 7
  - Temas: 63
- Navegacion base preparada: Roadmap, Study, Resources, Evidence y Portfolio.
- Implementacion funcional profunda actual: Roadmap.
- Study, Resources, Evidence y Portfolio quedan como rutas placeholder limpias hasta sus bloques V1.
- CORS no se agrego en este bloque; el desarrollo usa proxy Angular especifico. No existe AllowAnyOrigin.
- Diseno visual definitivo: pendiente.

## Datos Roadmap V1 Personal

- Base: AprendizajePersonalDb
- Usuario personal:
  - Id: 01A046D5-9BF3-7CEC-AA05-10C93459FA16
  - Nombre: Dylan
  - Email: dylan@aprendizaje.local
  - Email de uso local interno; no representa cuenta operativa ni habilita envio de correo.
- Migrations aplicadas:
  - 20260818153945_Inicial
  - 20260818174900_HacerObjetivosTemaNullable
  - 20260827073704_AgregarMetadataPedagogicaFase
- Roadmap importado:
  - Fase: 7
  - Tema: 63
  - Recurso: 30 recursos fisicos unicos
  - Herramienta: 63
  - Certificacion: 12
  - Competencia: 0
  - TemaDependencia: 0
  - CertificacionTema: 0
  - Evidence: 0
  - SesionEstudio: 0
  - EntradaBitacora: 0
  - SnapshotProgreso: 0
  - Conector: 0
  - LogSincronizacion: 0
- Idempotencia real confirmada: segunda ejecucion del importador no creo nuevas Fases, Temas, Recursos, Herramientas ni Certificaciones.
- Dataset: 31 entradas documentales de Recurso -> 30 recursos fisicos unicos por deduplicacion de clave natural UsuarioId + Titulo + Tipo + Url.
- Backup inicial verificado: C:\Program Files\Microsoft SQL Server\MSSQL17.MSSQLSERVER01\MSSQL\Backup\AprendizajePersonalDb_20260828_002927.bak

## Datos E2E Actuales

- Usuario: 1
- Tema: 2
- Fase: 2
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

Fase Metadata Pedagogica E2E:

- Id: 01A0422E-2CBA-7D1B-949C-B75FE9CDDA00
- Nombre: Metadata pedagógica Fase E2E
- Orden: 2
- Objetivos:
  - Validar metadata pedagógica de fase actualizada
- CriteriosAvance:
  - Confirmar actualización idempotente para importación
- MesInicioRecomendado: 49
- MesFinRecomendado: 52
- CargaSemanalRecomendada: Tiempo controlado E2E

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

- En environment Personal, los endpoints user-owned V1 aceptan omitir usuarioId en query/body y resuelven el Usuario actual por IUsuarioActual en la capa API. Application conserva UsuarioId explicito internamente.
- Los endpoints legacy con usuarioId explicito permanecen para compatibilidad de Development/tests durante la transicion.
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
- GET /api/temas/{temaId}/apuntes -> 200 con contenido vacio si aun no existen apuntes.
- PUT /api/temas/{temaId}/apuntes -> 204 como upsert logico de apuntes permanentes.
- GET/PUT apuntes de Tema en Personal resuelven Usuario actual sin usuarioId explicito.
- GET/PUT apuntes de Tema ajeno en Personal -> 404.
- GET /api/temas/{temaId}/workspace -> 200 con cabecera de Tema/Fase, objetivos, criterios, apuntes, ultima sesion, repaso y resumenes contextuales.
- GET /api/temas/{temaId}/workspace en Personal resuelve Usuario actual sin usuarioId explicito.
- GET /api/temas/{temaId}/workspace para Tema inexistente o ajeno -> 404.
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
- PUT /api/recursos/{id} -> 204
- PUT Recurso inexistente -> 404
- PUT Recurso con Usuario inexistente -> 404
- PUT Recurso de otro Usuario -> 409
- PUT Recurso con Guid.Empty -> 400
- PUT Recurso con Rating fuera de rango -> 400
- DELETE /api/recursos/{id}?usuarioId={id} -> 204
- DELETE Recurso inexistente -> 404
- DELETE Recurso de otro Usuario -> 409
- DELETE Recurso ya eliminado logicamente -> 404 por query filter
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
- PUT /api/sesiones-estudio/{id} -> 204
- PUT SesionEstudio con Usuario inexistente -> 404
- PUT SesionEstudio inexistente -> 404
- PUT SesionEstudio con Tema inexistente -> 404
- PUT SesionEstudio de otro Usuario -> 409
- PUT SesionEstudio hacia Tema de otro Usuario -> 409
- PUT SesionEstudio con Guid.Empty -> 400
- PUT SesionEstudio con duracion invalida -> 400
- DELETE /api/sesiones-estudio/{id}?usuarioId={id} -> 204
- DELETE SesionEstudio inexistente -> 404
- DELETE SesionEstudio de otro Usuario -> 409
- DELETE SesionEstudio ya eliminada logicamente -> 404 por query filter
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
- GET /api/analytics/temas/{temaId}?usuarioId={id} -> 200
- GET /api/analytics/temas/{temaId}?usuarioId={Guid.Empty} -> 400
- GET /api/analytics/temas/{Guid.Empty}?usuarioId={id} -> 400
- GET /api/analytics/temas/{tema-inexistente}?usuarioId={id} -> 404
- GET /api/analytics/competencias?usuarioId={id} -> 200
- GET /api/analytics/competencias?usuarioId={Guid.Empty} -> 400
- GET /api/analytics/certificaciones?usuarioId={id} -> 200
- GET /api/analytics/certificaciones?usuarioId={Guid.Empty} -> 400
- GET /api/portafolio?usuarioId={id} -> 200
- GET /api/portafolio?usuarioId={id}&tipoEvidence={tipo} -> 200
- GET /api/portafolio?usuarioId={id}&estadoMadurez={ListoPortafolio|Publicado} -> 200
- GET /api/portafolio?usuarioId={Guid.Empty} -> 400
- GET /api/portafolio con EstadoMadurez no elegible -> 400
- GET /api/portafolio con Usuario inexistente -> 404
- GET /api/usuario-actual en environment Personal -> 200 con Id y Nombre.
- GET /api/usuario-actual no expone Email ni datos de configuracion.
- GET /api/usuario-actual con cero Usuarios visibles -> 409.
- GET /api/usuario-actual con multiples Usuarios visibles -> 409.
- GET /api/usuario-actual fuera de environment Personal -> 409.

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
- dotnet test por solucion: validado con 511 tests correctos
- Frontend: Angular build validado.
- Frontend: 10 tests unitarios correctos.
- Smoke test actual: Tema.Crear expone Objetivos como coleccion no-null y vacia.
- Tests de Dominio Tema: objetivos, fase, jerarquia directa, criterios, planificacion, percepcion, IntervaloRepaso, dominio y TemaDominadoEvento validados.
- Tests de Dominio Competencia validados.
- Tests de Dominio SesionEstudio: registro, invariantes, correccion de duracion, cambio de Tema/Fecha/Tipo/Notas, soft delete y SesionRegistradaEvento validados.
- Tests de Dominio EntradaBitacora y Herramienta validados.
- Tests de Dominio Recurso: edicion, campos opcionales, rating y soft delete validados.
- Tests de Dominio Laboratorio validados, incluyendo editabilidad minima, EstadoMadurez y soft delete.
- Tests de Dominio Proyecto validados, incluyendo editabilidad minima, EstadoMadurez, fechas opcionales y soft delete.
- Tests de Dominio ArtefactoTecnico validados, incluyendo editabilidad minima, EstadoMadurez y soft delete.
- Tests de Dominio Writeup validados, incluyendo editabilidad minima, EstadoMadurez, fecha opcional y soft delete.
- Tests de Dominio Certificacion y CertificacionObtenida validados, incluyendo evidencia URL, EstadoMadurez y soft delete.
- Tests de Dominio Nota validados.
- Tests de Application Roadmap: flujos criticos de Tema, planificacion, percepcion, IntervaloRepaso y Competencia/CompetenciaTema cubiertos con fakes minimos.
- Tests de Application Resource: crear, obtener, listar, actualizar, eliminar logicamente y vincular cubiertos con fakes minimos.
- Tests de Application Study: SesionEstudio crear/obtener/listar/corregir duracion/actualizar/eliminar, EntradaBitacora, Herramienta y SesionHerramienta cubiertos con fakes minimos.
- Tests de Application Evidence: Laboratorio crear, obtener, listar, actualizar, eliminar logicamente, vincular a Tema y vincular a Herramienta cubiertos con fakes minimos.
- Tests de Application Evidence: Proyecto crear, obtener, listar, actualizar, eliminar logicamente, vincular a Tema y vincular a Herramienta cubiertos con fakes minimos.
- Tests de Application Evidence: ArtefactoTecnico crear, obtener, listar, actualizar, eliminar logicamente, vincular a Tema y vincular a Herramienta cubiertos con fakes minimos.
- Tests de Application Evidence: Writeup crear, obtener, listar, actualizar, eliminar logicamente y vincular a Tema cubiertos con fakes minimos.
- Tests de Application Roadmap: Certificacion crear, obtener, listar y vincular a Tema cubiertos con fake minimo.
- Tests de Application Evidence: CertificacionObtenida crear, obtener, listar, actualizar evidencia/madurez y eliminar logicamente cubiertos con fakes minimos.
- Tests de Application Evidence: Nota crear sobre Tema/Proyecto/Laboratorio/Writeup/ArtefactoTecnico, obtener y listar cubiertos con fakes minimos.
- Tests de Application Analytics: ResumenEstudio valida Guid.Empty y contrato del caso de uso.
- Tests de Application Analytics: ResumenTema valida ids vacios, no encontrado y contrato del caso de uso.
- Tests de Application Analytics: ResumenCompetencia y ResumenCertificacion validan Guid.Empty, lista vacia y contrato de respuesta.
- Tests de Application Portafolio: validan usuarioId vacio, Usuario inexistente, filtros de madurez, filtros de tipo y contrato de portafolio vacio.
- Tests de Application UsuarioActualLocal: validan cero, uno y multiples Usuarios visibles, ademas de Usuario eliminado logicamente.
- Tests de API Personal V1: validan resolucion HTTP de usuarioId opcional en Personal, rechazo de usuarioId divergente, 400 fuera de Personal sin usuarioId y proteccion de lecturas por ownership.
- Tests de Apuntes Permanentes de Tema V1: dominio, aplicacion, API Personal y persistencia SQL validados.
- Tests de Application Roadmap: ObtenerApuntesTema y GuardarApuntesTema cubren Tema inexistente, ownership incorrecto, contenido vacio y upsert sin duplicados.
- Tests de API Personal ApuntesTema: GET/PUT sin usuarioId explicito, Tema ajeno no visible y actualizacion posterior visible.
- Tests de integracion ApuntesTema: persistencia real, update sin duplicar, unique TemaId y Tema soft-deleted oculto por query filter del Tema.
- Tests de RoadmapVistaV1: Application valida contrato de consulta, API Personal valida GET /api/roadmap/vista sin usuarioId y contexto invalido, e integracion SQL valida roadmap vacio, orden de Fases, metadata pedagogica, TemaPadreId, criterios total/cumplidos, progreso, EstadoTema, repaso, fase actual, ownership, soft delete y ausencia de N+1 obvio con maximo 4 comandos de lectura.
- Tests de TemaWorkspaceV1: Application valida contrato de consulta y not found, API Personal valida GET /api/temas/{temaId}/workspace sin usuarioId y contexto invalido, e integracion SQL valida composicion de Tema/Fase, objetivos, criterios, apuntes, ultima sesion, repaso, Resources/Sesiones/Evidence resumidos, ownership, estado sin relaciones, paridad semantica con RoadmapVistaV1 y ausencia de N+1 obvio con maximo 8 comandos de lectura.
- Tests de integracion/persistencia: SQL Server real .\MSSQLSERVER01 con base exclusiva AprendizajeTestsDb.
- Guard rail de integracion: rechaza AprendizajeDb, database vacio y cualquier base distinta a AprendizajeTestsDb antes de recrear.
- Tests de persistencia cubren: metadata pedagogica de Fase, listas vacias de Fase materializadas no-null, checks SQL de meses recomendados de Fase, Tema.Objetivos vacios como SQL NULL y rematerializacion no-null, planificacion/percepcion/IntervaloRepaso de Tema, RowVersion de Tema tras update, RowVersion de SesionEstudio tras update, idempotencia fisica RecursoTema, idempotencia fisica CompetenciaTema, idempotencia fisica CertificacionTema con Peso NULL, idempotencia fisica SesionHerramienta, idempotencia fisica LaboratorioTema, idempotencia fisica LaboratorioHerramienta, idempotencia fisica ProyectoTema, idempotencia fisica ProyectoHerramienta, idempotencia fisica ArtefactoTema, idempotencia fisica ArtefactoHerramienta, idempotencia fisica WriteupTema, RowVersion de Proyecto poblada al insertar y estable al vincular joins, FK real CertificacionObtenida -> Certificacion, valores iniciales de CertificacionObtenida, query filter de CertificacionObtenida, Nota con un padre valido, CHECK CK_Nota_UnSoloPadre para cero y dos padres, query filter de Nota, query filter de soft delete en Tema y FK real SesionEstudio -> Tema.
- Frameworks de mocking: no utilizados.
- Tests de integracion fundacionales: validados.
- Tests de integracion Analytics: ResumenEstudio agrega sesiones visibles por usuario, aisla otros usuarios, devuelve resumen vacio y excluye sesiones eliminadas logicamente.
- Tests de integracion Analytics: ResumenTema agrega datos directos por tema, criterios cumplidos/total, recursos/evidencias visibles, aislamiento por usuario y exclusion de soft delete.
- Tests de integracion Analytics: ResumenCompetencia cuenta temas visibles por usuario y excluye temas eliminados logicamente.
- Tests de integracion Analytics: ResumenCertificacion cuenta temas visibles por usuario, respeta catalogo global, ignora Peso y cuenta CertificacionObtenida visible sin asumir unicidad.
- Tests de Application Importacion Roadmap V1: validan argumentos, Usuario inexistente, dataset invalido, sourceKey duplicado, idempotencia logica y conflictos de Fase/Certificacion.
- Tests de integracion Importacion Roadmap V1: importan roadmap-v1.json real en AprendizajeTestsDb, validan idempotencia SQL, rollback ante conflicto, reutilizacion de catalogos globales, metadata de Fase, no Evidence creada y ejecucion CLI controlada.
- Tests de integracion Resource Editable V1: actualizacion persistida, RecursoTema preservado, ownership incorrecto sin mutacion y soft delete oculto por query filter con fila fisica preservada.
- Tests de integracion Study Corrections V1: actualizacion de SesionEstudio persistida, cambio de Tema con ownership, SesionHerramienta preservada, ownership incorrecto sin mutacion, soft delete oculto por query filter con fila fisica preservada y ResumenEstudio excluyendo sesiones eliminadas.
- Tests de integracion Evidence Editable + Maturity V1: update, EstadoMadurez, ownership, soft delete, fila fisica preservada y relaciones representativas preservadas para Proyecto, Laboratorio, Writeup, ArtefactoTecnico y CertificacionObtenida.
- Tests de integracion Portafolio V1: validan inclusion de Evidence elegible, exclusion de Borrador/Documentado/soft-delete, aislamiento por Usuario, enriquecimiento con Tema/Herramienta/Certificacion, filtros y orden estable.
- Tests de integracion UsuarioActualLocal: validan cardinalidad sobre SQL Server real, query filter de Usuario eliminado y resolucion de 1 visible + 1 eliminado.

## Estado Git Esperado

- working tree clean

## Proxima Area

Roadmap avanzado minimo funcional cerrado: Fase, jerarquia Tema padre/hijo, objetivos, criterios de dominio, dificultad percibida, confianza, FechaInicio/FechaFin, IntervaloRepaso, Competencia/CompetenciaTema y Certificacion/CertificacionTema estan disponibles y validados.

TemaDependencia queda diferida conscientemente: aporta prerequisitos transversales utiles, pero no es necesaria para importacion inicial ni para Analytics minimo. Exponerla ahora dejaria incompleta la garantia de ausencia de ciclos profundos y no resolveria la carrera read -> validate -> insert bajo concurrencia.

Analytics directo minimo funcional cerrado: ResumenEstudio, ResumenTema, ResumenCompetencia y ResumenCertificacion estan disponibles como consultas on-demand factuales. RoadmapVistaV1 agrega una proyeccion de lectura especifica para Roadmap con progreso de Tema/Fase/Global, fase actual derivada y repaso recomendado calculados on-demand. TemaWorkspaceV1 agrega una proyeccion contextual de lectura para abrir un Tema sin ensamblar multiples endpoints iniciales: reutiliza la misma semantica de progreso/EstadoTema/repaso, incluye ApunteTema solo en lectura y resume Resources, Sesiones y Evidence sin listas profundas. Esto no equivale a Dashboard completo: readiness, SnapshotProgreso operativo y vw_TemaEstado siguen diferidos.

Metadata pedagogica de Fase validada: Fase ahora puede describir objetivos, criterios descriptivos de avance, meses relativos recomendados y carga semanal recomendada como texto. Esta metadata pertenece al roadmap recomendado; no representa progreso real del usuario, no reemplaza Tema.FechaInicio/FechaFin y no crea evidencia.

Importacion Roadmap original: la especificacion normalizada versionada existe en data/roadmap/roadmap-v1.json, el importador CLI V1 esta validado y la importacion real fue ejecutada sobre AprendizajePersonalDb con idempotencia fisica confirmada. AprendizajeDb permanece como desarrollo/E2E y AprendizajeTestsDb como testing.

Importador Roadmap V1: consume data/roadmap/roadmap-v1.json, no parsea HTML, no expone endpoint HTTP, no usa SnapshotProgreso/vw_TemaEstado/eventos y no crea Evidence planificada. SourceKey se usa solo en memoria. La importacion es transaccional e idempotente para el mismo dataset. El dataset contiene 31 entradas documentales de Recurso, que se materializan como 30 recursos fisicos por deduplicacion de clave natural UsuarioId + Titulo + Tipo + Url.

Portafolio read side V1 validado: GET /api/portafolio devuelve una proyeccion factual sobre Evidence visible del Usuario con EstadoMadurez ListoPortafolio o Publicado. No crea tabla, no duplica Evidence, no implementa Portafolio como Aggregate Root, no publica web y no modifica Evidence write-side.

Contexto single-user local V1 validado: Environment Personal usa AprendizajePersonalDb, no introduce Auth visible y resuelve el Usuario actual por cardinalidad de Usuarios visibles. GET /api/usuario-actual devuelve Id y Nombre, no expone Email ni datos de configuracion. Application conserva UsuarioId explicito internamente.

API Personal V1 validada: en modo Personal, la capa HTTP resuelve usuarioId mediante IUsuarioActual para flujos diarios de Roadmap, Resource, Study, Evidence, Analytics y Portafolio. No introduce Auth, JWT, Identity, migraciones ni paquetes. Los endpoints explicit-user legacy se conservan temporalmente para Development/tests y compatibilidad; si en Personal se envia un usuarioId explicito divergente, la API responde conflicto. GET /api/fases y GET /api/portafolio fueron validados en runtime Personal contra AprendizajePersonalDb en modo read-only.

Frontend Foundation V1 validado: Angular standalone vive en frontend/, consume la API Personal via /api y proxy local, obtiene UsuarioActual y muestra las 7 Fases reales y 63 Temas sin que el cliente envie usuarioId. No hay Auth, CORS global, dashboard avanzado ni escritura sobre AprendizajePersonalDb.

Apuntes permanentes de Tema V1 backend validado: `roadmap.ApunteTema` modela el contenido personal editable 0..1 asociado a un Tema. No reemplaza Tema.Objetivos, Nota append-only, SesionEstudio.Notas ni Recurso.Notas. GET/PUT `/api/temas/{temaId}/apuntes` participan de API Personal y resuelven Usuario actual en el borde HTTP. La migracion `20260901003645_AgregarApuntesPermanentesTema` fue creada y validada sobre AprendizajeTestsDb; AprendizajePersonalDb no fue modificada en este bloque.

RoadmapVistaV1 backend validado: GET `/api/roadmap/vista` participa de API Personal y resuelve Usuario actual en el borde HTTP. La respuesta entrega Fases ordenadas con metadata pedagogica, Temas planos por Fase con TemaPadreId, criterios total/cumplidos, progreso de Tema, EstadoTema, proxima fecha de repaso si existe ultima sesion, repaso recomendado, progreso de Fase y progreso global. Progreso de Tema = criterios cumplidos / criterios totales, 0% si no hay criterios; no hay pesos. Progreso de Fase y Global = promedio de progreso de Temas evaluables asociados a Fases; un Tema padre con hijos y 0 criterios se trata como nodo organizativo para no degradar el avance de sus hijos. Fase actual = primera Fase por Orden no completada; una Fase se completa solo si tiene Temas evaluables y todos estan estructuralmente completos por criterios, aunque alguno este en EstadoTema.EnRepaso; si todas estan completas se devuelve la ultima. CriteriosAvance de Fase permanecen como metadata descriptiva, no progreso. No crea migraciones, SnapshotProgreso, vw_TemaEstado ni Dashboard.

TemaWorkspaceV1 backend validado: GET `/api/temas/{temaId}/workspace` participa de API Personal y resuelve Usuario actual en el borde HTTP. La respuesta entrega Tema, Fase, objetivos reales del Tema, criterios de dominio, ApunteTema actual o contenido vacio, ultima SesionEstudio factual, proxima fecha de repaso, repaso recomendado y resumenes contextuales de Resources, Sesiones y Evidence. Recursos se exponen como conteo total; Sesiones como total, minutos totales y ultima sesion; Evidence como conteo total y desglose por Proyecto, Laboratorio, Writeup, ArtefactoTecnico y CertificacionObtenida. Reutiliza `SemanticaTemaReadSide` compartida con RoadmapVistaV1 para progreso de Tema, EstadoTema, intervalo efectivo y repaso. No crea migraciones, no escribe ApunteTema durante lectura, no crea God Dashboard, no implementa EvidenceListaV1 ni frontend Workspace.

Proxima area sugerida: checkpoint TemaWorkspaceV1 y luego pantalla frontend de Workspace del Tema consumiendo `/api/temas/{temaId}/workspace`. No implementar Dashboard completo, Evidence planificada, EvidenceListaV1 ni PlanPortafolio.

## Pendientes Deliberados

- tests completos de dominio;
- tests de integracion;
- IDespachadorEventos concreto;
- registro DespachoEventosInterceptor;
- vw_TemaEstado;
- Application dependency policy tests;
- TemaDependencia race;
- deteccion completa de ciclos profundos en jerarquia de Temas;
- aciclicidad completa de TemaDependencia;
- estrategia concurrente de TemaDependencia;
- semantica operativa de CertificacionTema.Peso;
- FaseHerramienta y metadata contextual de prioridad de herramientas;
- PlanPortafolio / EntregablePlanificado;
- Evidence planificada excluida de la importacion;
- prueba automatizada directa de TemaDominadoEvento;
- concurrencia HTTP/ETag/If-Match para Tema y Proyecto;
- auth;
- SnapshotProgreso operativo;
- integrations;
- pantallas frontend V1 completas;
- formularios frontend Resource/Study/Evidence;
- Portafolio frontend completo;
- filtros Resource;
- filtros Study;
- edicion EntradaBitacora;
- TipoRecurso editable;
- GitHub remote.
