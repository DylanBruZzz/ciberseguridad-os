# Lista Maestra

## 1. Arquitectura base

- [x] Clean Architecture definida.
- [x] DDD adoptado.
- [x] Modular monolith definido.
- [x] Proyectos fisicos separados por capa.
- [x] Direcciones de dependencias validadas.
- [ ] Application dependency policy tests.

## 2. Dominio

- [x] 17 Aggregate Roots definidos.
- [x] CriterioTema como entidad interna.
- [x] LogSincronizacion como entidad interna.
- [x] Guid.CreateVersion7() usado en dominio.
- [x] RowVersion selectivo definido.
- [x] Soft delete selectivo definido.
- [x] Tema.Objetivos robustecido ante NULL materializado por EF.
- [ ] Nuevos comportamientos de dominio por caso de uso real.

## 3. Persistencia EF Core

- [x] AprendizajeDbContext configurado.
- [x] 17 DbSet raiz.
- [x] Configuraciones IEntityTypeConfiguration aplicadas por assembly.
- [x] AuditoriaInterceptor registrado.
- [x] FKs directas pre-migracion corregidas.
- [x] 12 tablas de union puras representadas con modelos tecnicos de Infraestructura.
- [x] Tema.Objetivos nullable en EF.
- [ ] IDespachadorEventos concreto.
- [ ] Registro de DespachoEventosInterceptor.

## 4. SQL Server / Migraciones

- [x] SQL maestro congelado como referencia estructural.
- [x] Politica DDL hibrida aceptada.
- [x] Primera migracion Inicial generada.
- [x] Primera migracion aplicada a AprendizajeDb.
- [x] Persistencia fisica inicial validada.
- [x] Migracion HacerObjetivosTemaNullable aplicada.
- [ ] Crear migraciones futuras solo con auditoria previa.

## 5. Tooling / entorno

- [x] dotnet-ef local 10.0.9 configurado.
- [x] Microsoft.EntityFrameworkCore.Design 10.0.9 disponible.
- [x] appsettings.Development.json configurado para SQL Server local.
- [x] DbContext design-time validado.

## 6. Git

- [x] .gitignore creado.
- [x] Git inicializado.
- [x] Branch main.
- [x] Checkpoint persistencia inicial creado.
- [x] Checkpoint primer flujo funcional creado.
- [ ] Remote GitHub configurado.
- [ ] Push inicial.

## 7. Application

- [x] IUnitOfWork definido.
- [x] CrearUsuario implementado.
- [x] CrearRecurso implementado.
- [x] ObtenerRecursoPorId implementado.
- [x] ListarRecursos implementado.
- [x] VincularRecursoATema implementado.
- [x] RegistrarSesionEstudio implementado.
- [x] ObtenerSesionEstudio implementado.
- [x] ListarSesionesEstudio implementado.
- [x] CorregirDuracionSesionEstudio implementado.
- [x] CrearTema implementado.
- [x] ObtenerTemaPorId implementado.
- [x] EstablecerObjetivosTema implementado.
- [x] AsignarTemaAFase implementado.
- [x] AsignarTemaPadre implementado.
- [x] DefinirCriteriosRelevantes implementado.
- [x] MarcarCriterio implementado.
- [x] DesmarcarCriterio implementado.
- [x] ListarTemas implementado.
- [x] CrearFase implementado.
- [x] ListarFases implementado.
- [ ] Nuevos casos de uso por necesidad real.
- [ ] Manejo de errores transversal evaluado.

## 8. API

- [x] ASP.NET Core Minimal APIs.
- [x] JsonStringEnumConverter configurado para HTTP.
- [x] POST /api/usuarios.
- [x] POST /api/recursos.
- [x] GET /api/recursos/{id}.
- [x] GET /api/recursos?usuarioId={id}.
- [x] PUT /api/recursos/{id}/temas/{temaId}.
- [x] POST /api/sesiones-estudio.
- [x] GET /api/sesiones-estudio/{id}.
- [x] GET /api/sesiones-estudio?usuarioId={id}.
- [x] PUT /api/sesiones-estudio/{id}/duracion.
- [x] POST /api/temas.
- [x] GET /api/temas/{id}.
- [x] PUT /api/temas/{id}/objetivos.
- [x] PUT /api/temas/{id}/criterios.
- [x] PUT /api/temas/{id}/criterios/{tipo}/cumplido.
- [x] DELETE /api/temas/{id}/criterios/{tipo}/cumplido.
- [x] PUT /api/temas/{id}/fase.
- [x] PUT /api/temas/{id}/padre.
- [x] GET /api/temas?usuarioId={id}.
- [x] POST /api/fases.
- [x] GET /api/fases?usuarioId={id}.
- [ ] Endpoints adicionales por caso de uso real.
- [ ] Auth.

## 9. Flujos E2E

- [x] Primer flujo funcional E2E Usuario -> Tema.
- [x] POST /api/usuarios -> 201.
- [x] POST /api/temas -> 201.
- [x] GET /api/temas/{id} -> 200.
- [x] GET Tema inexistente -> 404.
- [x] Establecer objetivos de Tema -> 204.
- [x] Establecer objetivos de Tema inexistente -> 404.
- [x] Asignar Tema a Fase -> 204.
- [x] Asignar Fase a Tema inexistente -> 404.
- [x] Asignar Fase inexistente a Tema -> 404.
- [x] Asignar Fase con Guid.Empty -> 400.
- [x] Asignar Tema padre -> 204.
- [x] Asignar Tema padre con hijo inexistente -> 404.
- [x] Asignar Tema padre inexistente -> 404.
- [x] Asignar Tema padre self-parent -> 400.
- [x] Asignar Tema padre con ciclo directo -> 409.
- [x] Definir criterios relevantes de Tema -> 204.
- [x] Definir criterios con menos de 2 criterios distintos -> 400.
- [x] Definir criterios duplicados -> normalizados por dominio.
- [x] Redefinir criterios sin progreso -> 204.
- [x] Redefinir criterios con progreso -> 409.
- [x] Marcar criterio de Tema -> 204.
- [x] Marcar criterio de Tema es idempotente.
- [x] Marcar criterio no definido -> 409.
- [x] Desmarcar criterio de Tema -> 204.
- [x] Desmarcar criterio no definido -> 409.
- [x] Transicion observable a Tema dominado.
- [x] TemaDominadoEvento confirmado por codigo en la transicion no dominado -> dominado.
- [x] Crear Recurso -> 201.
- [x] Obtener Recurso por Id -> 200.
- [x] Obtener Recurso inexistente -> 404.
- [x] Listar recursos por Usuario -> 200.
- [x] Listar recursos para Usuario sin recursos -> 200 con [].
- [x] Listar recursos con Guid.Empty -> 400.
- [x] Vincular Recurso a Tema -> 204.
- [x] Vincular Recurso a Tema es idempotente.
- [x] Vincular Recurso inexistente a Tema -> 404.
- [x] Vincular Recurso a Tema inexistente -> 404.
- [x] Vincular Recurso/Tema con Guid.Empty -> 400.
- [x] Registrar SesionEstudio -> 201.
- [x] Obtener SesionEstudio por Id -> 200.
- [x] Obtener SesionEstudio inexistente -> 404.
- [x] Listar sesiones por Usuario -> 200.
- [x] Listar sesiones para Usuario sin sesiones -> 200 con [].
- [x] Listar sesiones con Guid.Empty -> 400.
- [x] Corregir duracion de SesionEstudio -> 204.
- [x] Corregir duracion de SesionEstudio inexistente -> 404.
- [x] Corregir duracion con Guid.Empty -> 400.
- [x] Corregir duracion invalida -> 400.
- [x] RowVersion de SesionEstudio verificada fisicamente.
- [x] SesionRegistradaEvento confirmado por codigo.
- [x] Listar temas por Usuario -> 200.
- [x] Listar temas para Usuario sin temas -> 200 con [].
- [x] Listar temas con Guid.Empty -> 400.
- [x] Crear Fase -> 201.
- [x] Listar fases por Usuario -> 200.
- [x] Listar fases para Usuario sin fases -> 200 con [].
- [x] Listar fases con Guid.Empty -> 400.
- [ ] Nuevos flujos E2E por vertical slice.

## 10. Tests

- [x] Proyecto de tests Aprendizaje.Tests creado.
- [x] xUnit v3 configurado.
- [x] Microsoft Testing Platform configurado como runner de dotnet test.
- [x] Smoke test de Tema.Objetivos ejecutado correctamente.
- [x] dotnet test por proyecto validado.
- [x] dotnet test por solucion validado.
- [x] Tests fundacionales de dominio para Tema.
- [x] Tests fundacionales de dominio para SesionEstudio.
- [x] Test automatizado directo de TemaDominadoEvento.
- [x] Test automatizado directo de SesionRegistradaEvento.
- [ ] Tests de Application.
- [ ] Tests de persistencia.
- [ ] Tests E2E automatizados.
- [ ] Tests de concurrencia donde aplique RowVersion.

## 11. Read side / Analytics

- [ ] vw_TemaEstado como read side.
- [ ] Paridad Tema.CalcularEstado() vs vw_TemaEstado.
- [ ] Consultas de lectura.
- [ ] Analytics operativos.

## 12. Integraciones

- [ ] IDespachadorEventos concreto.
- [ ] DespachoEventosInterceptor registrado.
- [ ] Integraciones externas.
- [ ] Conectores operativos.

## 13. Frontend

- [ ] Frontend definido.
- [ ] Experiencia de usuario inicial.
- [ ] Integracion con API.

## 14. Importacion Roadmap original

- [ ] Importar Roadmap original al modelo persistente.
- [ ] Validar datos importados.
- [ ] Auditar trazabilidad de importacion.

## 15. Pendientes arquitectonicos

- [ ] TemaDependencia race.
- [ ] Deteccion completa de ciclos profundos en jerarquia de Temas.
- [ ] Gobernanza Herramienta/Certificacion.
- [ ] Read side de vw_TemaEstado.
- [ ] Politica de autenticacion/autorizacion.
- [ ] Estrategia de observabilidad.
