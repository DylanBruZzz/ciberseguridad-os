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
- [x] CrearTema implementado.
- [x] ObtenerTemaPorId implementado.
- [x] EstablecerObjetivosTema implementado.
- [x] ListarTemas implementado.
- [x] CrearFase implementado.
- [ ] Nuevos casos de uso por necesidad real.
- [ ] Manejo de errores transversal evaluado.

## 8. API

- [x] ASP.NET Core Minimal APIs.
- [x] JsonStringEnumConverter configurado para HTTP.
- [x] POST /api/usuarios.
- [x] POST /api/temas.
- [x] GET /api/temas/{id}.
- [x] PUT /api/temas/{id}/objetivos.
- [x] GET /api/temas?usuarioId={id}.
- [x] POST /api/fases.
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
- [x] Listar temas por Usuario -> 200.
- [x] Listar temas para Usuario sin temas -> 200 con [].
- [x] Listar temas con Guid.Empty -> 400.
- [x] Crear Fase -> 201.
- [ ] Nuevos flujos E2E por vertical slice.

## 10. Tests

- [ ] Proyecto de tests.
- [ ] Tests de dominio.
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
- [ ] Gobernanza Herramienta/Certificacion.
- [ ] Read side de vw_TemaEstado.
- [ ] Politica de autenticacion/autorizacion.
- [ ] Estrategia de observabilidad.
