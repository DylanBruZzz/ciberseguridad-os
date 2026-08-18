# Arquitectura física de la solución

Clean Architecture + DDD sobre un monolito modular. Este documento cierra la estructura
antes de escribir la primera entidad — todo lo que sigue traduce las 15 convenciones ya
congeladas a proyectos, carpetas y reglas de dependencia concretas.

---

## 1. Estructura de la solución (.sln)

### Proyectos de producción

| Proyecto | Responsabilidad | Referencia a |
|---|---|---|
| `Aprendizaje.Dominio` | Aggregate Roots, entidades internas, Value Objects, Domain Events, interfaces de repositorio, servicios de dominio puros | *(nada — solo BCL)* |
| `Aprendizaje.Aplicacion` | Casos de uso (comandos/consultas), DTOs, validadores, interfaces de orquestación (`IUnitOfWork`, `IDespachadorEventos`) | `Dominio` |
| `Aprendizaje.Infraestructura` | EF Core (`DbContext`, configuraciones, migraciones), implementación de repositorios, interceptor de eventos, integraciones externas, autenticación | `Dominio`, `Aplicacion` |
| `Aprendizaje.Api` | Endpoints HTTP, middleware, composición de dependencias (`Program.cs`) | `Aplicacion`, `Infraestructura` |

### Proyectos de tests

| Proyecto | Qué prueba |
|---|---|
| `Aprendizaje.Dominio.Tests` | Lógica de agregados y Value Objects en aislamiento — sin mocks en la mayoría de los casos |
| `Aprendizaje.Aplicacion.Tests` | Casos de uso con repositorios mockeados |
| `Aprendizaje.Infraestructura.Tests` | Configuraciones de EF Core y comportamiento real de cascadas contra una base de datos real (Testcontainers) |
| `Aprendizaje.Arquitectura.Tests` | Reglas de dependencia entre proyectos y módulos, verificadas automáticamente (NetArchTest) |
| `Aprendizaje.Api.Tests` | Endpoints de extremo a extremo vía `WebApplicationFactory` |

### Referencias prohibidas (rotas por diseño, no por disciplina)

- `Dominio` → `Infraestructura`, `Aplicacion` o `Api` — prohibido a nivel de `.csproj`, ni siquiera compila si se intenta.
- `Dominio` → cualquier paquete NuGet de EF Core, ASP.NET Core o MediatR.
- `Aplicacion` → `Infraestructura` o `Api`.
- `Infraestructura` → `Api`.
- `Api` → `Dominio` directamente, saltándose `Aplicacion` (un endpoint nunca instancia ni manipula un Aggregate Root; siempre pasa por un caso de uso).

Las primeras cuatro se rompen automáticamente por el grafo de referencias de proyecto —
ni siquiera hace falta un test para eso, el compilador lo impide. La última (`Api` → `Dominio`)
sí compila si alguien lo hace por accidente, así que se verifica con `Aprendizaje.Arquitectura.Tests`.

---

## 2 y 3. Estructura de carpetas y dónde vive cada concepto

### `Aprendizaje.Dominio/`

```
Comun/
    Entidad.cs, AggregateRoot.cs, ValueObject.cs, IEventoDominio.cs
Roadmap/
    Tema.cs                     ← Aggregate Root, contiene CriterioTema como colección interna
    CriterioTema.cs             ← Entidad interna (Entidad, no AggregateRoot)
    Fase.cs, Competencia.cs, Certificacion.cs
    ValueObjects/
        NivelPercepcion.cs, IntervaloRepaso.cs
    Eventos/
        TemaDominadoEvento.cs, TemaEnRepasoEvento.cs
    Servicios/
        IValidadorDependencias.cs   ← interfaz; implementación pura de grafo, sin I/O
        ValidadorDependencias.cs
    Repositorios/
        ITemaRepository.cs, IFaseRepository.cs, ICompetenciaRepository.cs, ICertificacionRepository.cs
Study/
    SesionEstudio.cs, Herramienta.cs, EntradaBitacora.cs
    Repositorios/
Evidence/
    Proyecto.cs, Laboratorio.cs, Writeup.cs, ArtefactoTecnico.cs, CertificacionObtenida.cs, Nota.cs
    Repositorios/
Resource/
    Recurso.cs
    Repositorios/
Analytics/
    SnapshotProgreso.cs
    Repositorios/
Integration/
    Conector.cs, LogSincronizacion.cs
    Repositorios/
```

**Nota sobre `ValidadorDependencias`:** el algoritmo de detección de ciclos en el grafo de
`TemaDependencia` es lógica pura (recorrido de grafo en memoria, sin acceso a datos), así que
su implementación vive en `Dominio`, no en `Aplicacion`. Lo que sí vive en `Aplicacion` es el
caso de uso que **carga** las dependencias existentes desde el repositorio, llama a este servicio
de dominio, y decide si persiste la nueva arista — esa orquestación con I/O no es responsabilidad
del dominio.

### `Aprendizaje.Aplicacion/`

```
Comun/
    IUnitOfWork.cs, IDespachadorEventos.cs, ExcepcionAplicacion.cs
Roadmap/
    CasosDeUso/
        MarcarCriterioTema/     Comando.cs, Manejador.cs, Validador.cs
        CrearTema/               ...
        RegistrarDependencia/    ...
        ConsultarRoadmap/        Consulta.cs, Manejador.cs   ← lectura directa, sin pasar por Tema
Study/CasosDeUso/...
Evidence/CasosDeUso/...
Resource/CasosDeUso/...
Analytics/CasosDeUso/...
Integration/CasosDeUso/...
```

Cada caso de uso vive en su propia carpeta con su comando/consulta, su manejador y su validador
juntos — no una carpeta `Comandos/` y otra `Validadores/` separadas por tipo técnico. Esto es
alta cohesión aplicada literalmente: todo lo que cambia junto cuando modificas un caso de uso
vive en el mismo lugar.

### `Aprendizaje.Infraestructura/`

```
Persistencia/
    AprendizajeDbContext.cs
    Configuraciones/
        Roadmap/TemaConfiguration.cs, FaseConfiguration.cs, ...
        Study/..., Evidence/..., Resource/..., Analytics/..., Integration/...
    Repositorios/
        TemaRepository.cs, ...   ← implementan las interfaces de Dominio
    Interceptores/
        DespachoEventosInterceptor.cs
    Migraciones/
EventosDominio/
    MediatorDespachadorEventos.cs   ← implementa IDespachadorEventos
Integraciones/
    GitHub/, TryHackMe/, HackTheBox/, IA/
Autenticacion/
    ServicioTokenJwt.cs, ConfiguracionIdentity.cs
```

### `Aprendizaje.Api/`

```
Endpoints/
    Roadmap/TemaEndpoints.cs, FaseEndpoints.cs, ...
    Study/..., Evidence/..., Resource/..., Analytics/..., Integration/...
Middleware/
    ManejadorExcepcionesGlobal.cs
Configuracion/
    InyeccionDependencias.cs   ← AddAplicacion(), AddInfraestructura()
Program.cs
```

**Decisión: Minimal API endpoints, no Controllers.** Con seis módulos y una API que va a crecer
varios años, la sintaxis de Controllers con atributos de ruta añade ceremonia (`[ApiController]`,
`[HttpGet]`, inyección por constructor de clases que solo delegan a un caso de uso) sin aportar
nada que un método de extensión `MapTemaEndpoints(this IEndpointRouteBuilder app)` no resuelva
más directo. Es la opción más simple, no una preferencia estética.

### Middleware, autenticación y validaciones — tabla resumen

| Concepto | Ubicación |
|---|---|
| Aggregate Roots | `Dominio/<Modulo>/` |
| Entidades internas | `Dominio/<Modulo>/` (junto a su agregado, sin carpeta propia) |
| Value Objects | `Dominio/<Modulo>/ValueObjects/` |
| Domain Events | `Dominio/<Modulo>/Eventos/` |
| Interfaces de repositorio | `Dominio/<Modulo>/Repositorios/` |
| Implementación de repositorios | `Infraestructura/Persistencia/Repositorios/` |
| Casos de uso | `Aplicacion/<Modulo>/CasosDeUso/<CasoDeUso>/` |
| DTOs | `Aplicacion/<Modulo>/CasosDeUso/<CasoDeUso>/` (colocados con el caso de uso, no en una carpeta `Dtos/` global — ver crítica más abajo) |
| Validaciones | Junto a cada caso de uso (FluentValidation) |
| EF Core (DbContext + config) | `Infraestructura/Persistencia/` |
| Interceptors | `Infraestructura/Persistencia/Interceptores/` |
| Middleware | `Api/Middleware/` |
| Autenticación | `Infraestructura/Autenticacion/` (implementación) + `Api/Configuracion/` (registro) |
| Integraciones | `Infraestructura/Integraciones/` |
| Endpoints | `Api/Endpoints/<Modulo>/` |
| Tests | Un proyecto por tipo, ver sección 9 |

---

## 4. Validación de principios contra la estructura

- **El dominio no conoce EF Core / ASP.NET / Infraestructura** — no es una promesa, es una
  imposibilidad física: `Aprendizaje.Dominio.csproj` no tiene ni un `PackageReference` a
  `Microsoft.EntityFrameworkCore` ni un `ProjectReference` a `Infraestructura`. Verificado
  además por `Aprendizaje.Arquitectura.Tests` para que quede registrado como regla ejecutable,
  no solo como intención en un documento.
- **Bajo acoplamiento** — cada módulo dentro de `Dominio` solo conoce a otros módulos por `Guid`
  (nunca por navegación de objeto completo, tal como fijamos en la revisión de implementación).
- **Alta cohesión** — carpeta por módulo en las cuatro capas, y dentro de `Aplicacion`, carpeta
  por caso de uso — todo lo que cambia junto vive junto.
- **Mantenibilidad a varios años** — el conjunto de nueve proyectos es estable: añadir un módulo
  nuevo dentro de cinco años es una carpeta nueva en cuatro proyectos existentes, no una
  reestructuración de la solución.
- **Preparada para crecer sin romper módulos** — ver ruta de extracción en la crítica de la
  sección 5.

---

## 5. Política de dependencias entre módulos

Grafo de dependencias permitido dentro de `Dominio` (dirección = "puede referenciar el `Guid` de"):

```
Study      → Roadmap
Evidence   → Roadmap, Study
Resource   → Roadmap
Analytics  → (ninguna — ver nota)
Integration→ (ninguna — ver nota)
```

**Nota importante, y es una simplificación deliberada:** `Analytics` e `Integration` **no tienen
ninguna dependencia de dominio hacia otros módulos**. `SnapshotProgreso` solo referencia
`UsuarioId`; no necesita conocer `Tema` ni `Proyecto` como tipos de dominio, porque —según ya
decidimos— las consultas de analítica leen directamente de la base de datos con proyecciones,
sin pasar por agregados. De la misma forma, `Conector` y `LogSincronizacion` son autocontenidos;
la orquestación que mapea "un commit de GitHub" a un `Proyecto` ocurre en un caso de uso de
`Aplicacion` que sí conoce ambos módulos, pero el módulo `Integration` de `Dominio` en sí mismo
no depende de nadie. Esto reduce el acoplamiento real más de lo que el diagrama de módulos
original sugería — vale la pena tenerlo explícito.

**Regla de enforcement:** ninguna clase en el namespace `Dominio.Roadmap` puede referenciar
`Dominio.Study`, `Dominio.Evidence`, etc. (la dirección contraria a la tabla de arriba). Se
verifica con una regla de `Aprendizaje.Arquitectura.Tests` por cada módulo.

### Crítica: ¿deberían los módulos ser proyectos `.csproj` separados en vez de carpetas?

Esta es la decisión de sobreingeniería más tentadora de toda la arquitectura, y la que rechazo
explícitamente. Separar cada módulo en su propio proyecto (`Dominio.Roadmap.csproj`,
`Dominio.Study.csproj`...) daría aislamiento verificado por el compilador en vez de por tests —
pero para un desarrollador trabajando solo, en un sistema que hoy tiene seis módulos y podría
tener ocho en tres años, seis a nueve proyectos de dominio añaden fricción real (gestión de
referencias entre proyectos, tiempo de compilación, indirección para navegar código) a cambio
de una garantía que los tests de arquitectura ya dan con un costo mucho menor. La regla que
sigo aquí: **aislamiento físico solo cuando el equipo o el riesgo de violación accidental lo
justifiquen** — con un solo desarrollador y una suite de tests de arquitectura que falla el
build ante cualquier violación, la ganancia marginal de proyectos separados no compensa el
costo. Si algún día el sistema necesita extraer un módulo a un servicio independiente, la
disciplina de namespaces y de referencia-solo-por-ID que ya tenemos hace de esa extracción un
refactor mecánico, no una reescritura — el aislamiento físico sigue disponible como paso futuro,
no como costo pagado hoy sin necesidad.

---

## 6. Estrategia de CQRS

**CQRS como patrón de organización de código, no como arquitectura de infraestructura.**//
Esto significa: los casos de uso se separan explícitamente en Comandos (escriben, pasan por
Aggregate Roots y repositorios) y Consultas (leen, con proyecciones directas que ignoran por
completo los agregados) — ya lo decidimos en la revisión de implementación para el Dashboard y
Analytics. Lo que **no** se implementa, y sería sobreingeniería real para este proyecto:

- Base de datos de lectura separada de la de escritura.
- Replicación o sincronización entre modelos.
- Event Sourcing como mecanismo de persistencia del estado.

Ninguna de estas tres aporta valor a un sistema de un solo usuario con una única base de datos
SQL Server. Todas introducirían consistencia eventual entre lectura y escritura donde hoy no
existe ningún problema que la justifique — la definición exacta de complejidad accidental.

---

## 7. MediatR: sí, con alcance acotado y justificado

Se usa MediatR para dos cosas específicas, no como mecanismo general de indirección:

1. **Despacho de Domain Events** (`INotification` / `IPublisher`) — el ajuste natural, porque un
   evento puede tener cero, uno o varios handlers, y el agregado que lo emite no debe conocer
   cuántos ni cuáles. Escribir un pub-sub propio para esto sería reinventar un problema ya
   resuelto por una librería de ~200 KB sin dependencias pesadas.
2. **Comandos y consultas** (`IRequest` / `IRequestHandler`) — aquí la justificación es más
   débil por sí sola (inyectar el manejador directamente por DI funciona igual de bien y es más
   fácil de navegar en el IDE), pero se adopta **por consistencia**: usar MediatR para eventos y
   un mecanismo distinto para comandos significaría mantener dos formas de resolver dependencias
   en el mismo proyecto, lo cual es más complejidad acumulada que usar una sola herramienta para
   ambos casos. El valor real que sí aporta para comandos es `IPipelineBehavior<,>` — validación
   (FluentValidation) y logging aplicados una sola vez a todos los casos de uso, en vez de
   repetidos en cada manejador.

**Sobre el licenciamiento (verificado antes de escribir esto):** MediatR pasó a modelo comercial
en julio de 2025, gratuito para individuos y organizaciones con menos de 5 millones de USD de
facturación anual — cubre este proyecto sin ambigüedad hoy y en cualquier escenario razonable de
los próximos años. Como red de seguridad documentada: si ese contexto cambiara, `Mediator`
(basado en source generators, sin reflexión, licencia MIT) es una alternativa de migración de
bajo esfuerzo porque implementa una interfaz casi idéntica — no es una decisión que hoy comprometa
al proyecto a un costo futuro inevitable.

---

## 8. Domain Events — cierre completo del patrón

Ya fijamos en las convenciones (secciones 14.1–14.3) el contrato (`IEventoDominio`), el punto de
recolección (`AggregateRoot.RegistrarEvento`) y el interceptor de despacho post-`SaveChanges`.
Cierro los dos puntos que faltaban:

**Orden de ejecución:** MediatR ejecuta los `INotificationHandler` de un mismo evento de forma
secuencial en el orden de registro en el contenedor de DI, pero **el diseño no debe depender de
ese orden**. Si dos efectos de un mismo evento tienen una dependencia de orden entre sí (por
ejemplo, "actualizar el snapshot" debe ocurrir antes de "evaluar si se sugiere una recomendación
de IA"), eso es una señal de que en realidad son un solo handler con dos pasos internos, no dos
handlers independientes — mantener los handlers independientes y sin orden implícito es lo que
permite añadir un handler nuevo (por ejemplo, cuando llegue el módulo de IA) sin tocar los que ya
existen.

**Garantías de consistencia:** dos niveles distintos, y no deben confundirse.
- El cambio de estado del propio agregado (ej. `Tema` pasando a dominado) tiene consistencia
  fuerte — ocurre dentro de la misma transacción de `SaveChangesAsync`, con ACID completo.
- Los efectos secundarios disparados por el evento (actualizar un snapshot, notificar una
  integración) son **eventual y best-effort** — ocurren después de que la transacción ya se
  confirmó, y un fallo en un handler se registra en log pero nunca revierte ni bloquea el cambio
  principal. Esta distinción es la misma que ya aplicamos al diseñar que las integraciones y la
  IA son "aditivas, nunca bloqueantes" — aquí es donde esa regla de producto se convierte en
  mecanismo técnico concreto.

---

## 9. Organización de Tests

| Proyecto | Framework | Qué verifica | Dependencias externas reales |
|---|---|---|---|
| `Dominio.Tests` | xUnit + Shouldly | Invariantes de agregados, cálculo de Value Objects, `ValidadorDependencias` | Ninguna — son las pruebas más rápidas y numerosas |
| `Aplicacion.Tests` | xUnit + NSubstitute | Casos de uso con repositorios e `IDespachadorEventos` mockeados | Ninguna |
| `Infraestructura.Tests` | xUnit + Testcontainers | Que las configuraciones Fluent API produzcan el comportamiento de cascada correcto contra SQL Server real, no un proveedor en memoria | Contenedor Docker de SQL Server, efímero por ejecución |
| `Arquitectura.Tests` | NetArchTest.Rules | Las reglas de las secciones 1 y 5 de este documento, como aserciones ejecutables | Ninguna |
| `Api.Tests` | xUnit + `WebApplicationFactory` | Flujos HTTP completos de los casos de uso más críticos (registrar sesión, marcar criterio) | Testcontainers, igual que Infraestructura.Tests |

**Por qué Testcontainers y no el proveedor InMemory de EF Core:** el proveedor en memoria de EF
Core no aplica `CHECK` constraints, no respeta `ON DELETE NO ACTION` ni lanza el error de rutas
de cascada múltiples de SQL Server — exactamente los comportamientos que más nos costó ajustar en
la auditoría de consistencia del esquema relacional. Probar contra el proveedor en memoria daría
una falsa sensación de cobertura sobre justo la parte más delicada del sistema.

---

## Resumen de decisiones de simplicidad deliberada

Para que quede consolidado en un solo lugar, estas son las veces en este documento donde elegí
explícitamente **no** hacer la versión más sofisticada:

1. Módulos como carpetas/namespaces, no proyectos separados (sección 5).
2. CQRS como organización de código, no como infraestructura de bases separadas (sección 6).
3. MediatR con alcance acotado a dos usos concretos, no como capa de indirección universal (sección 7).
4. Sin Outbox ni cola de mensajes para eventos — despacho en memoria, con el punto de extensión
   ya identificado si algún día hace falta garantía de entrega (heredado de las convenciones, sección 8).
5. Autenticación con ASP.NET Core Identity + JWT propio, sin un servidor de autorización externo
   tipo IdentityServer/Duende — innecesario mientras la API la consuma un único cliente propio.
6. DTOs colocados junto a su caso de uso, no en una carpeta `Dtos/` global que crecería sin
   estructura clara a medida que se agreguen módulos.
7. Ninguna entidad del sistema utiliza `DEFAULT NEWSEQUENTIALID()` en su columna `Id`. La
   generación de identidad pertenece exclusivamente al dominio, mediante `Guid.CreateVersion7()`
   en el constructor de cada Aggregate Root — nunca a la base de datos. Un `DEFAULT` a nivel de
   SQL implicaría un segundo generador de identidad para el mismo concepto, aunque no se use en
   el flujo normal de la aplicación; además, una inserción externa que omita `Id` debe fallar de
   forma explícita e inmediata, no completarse silenciosamente con un valor generado por la base
   de datos — cualquier escritura que evada el dominio ya está evadiendo sus invariantes de
   negocio, y ocultar ese hecho con un `Id` válido es peor que un error ruidoso. Regla
   arquitectónica global, no una decisión puntual de `Tema`: se aplica al resto de los Aggregate
   Roots conforme se implementen, incluidas las diecisiete tablas del esquema relacional que
   todavía conservan `NEWSEQUENTIALID()` como recordatorio de una decisión anterior al momento en
   que esta regla se formalizó.

---

## Registro de congelamiento — `Tema`

**Estado: congelado como patrón oficial del dominio.**

`Tema.cs`, `CriterioTema.cs`, `NivelPercepcion.cs`, `IntervaloRepaso.cs`, `TemaDominadoEvento.cs`,
`TemaConfiguration.cs` y `CriterioTemaConfiguration.cs` quedan cerrados. A partir de este punto no
se proponen cambios sobre estos siete archivos salvo error demostrable por compilación, ejecución
o migración de prueba — no por revisión de estilo, preferencia o nueva idea arquitectónica.

**Diferido, no pendiente de decisión — se resuelve únicamente con la primera migración de prueba
contra SQL Server real:**
1. Constructor binding de `NivelPercepcion` e `IntervaloRepaso` como Owned Types de una sola propiedad.
2. Posible conflicto de mapeo entre el campo `_objetivos` y la propiedad pública `Objetivos`.
3. Comportamiento de nulabilidad de `VersionFila` (`byte[]?`) frente a la columna `ROWVERSION`.

Este agregado es, a partir de ahora, el patrón que se replica —no se rediseña— en el resto del dominio.

## Registro de congelamiento — `Fase`

**Estado: congelado como segundo Aggregate Root de referencia.**

`Fase.cs` y `FaseConfiguration.cs` quedan cerrados, aplicando exactamente la misma disciplina de
`Tema`: sin `CHECK` porque el script SQL nunca definió ninguno para esta tabla, y sin validación
de longitud máxima en el dominio (`Nombre`, `Color`, `Descripcion`) porque `Tema` ya estableció
ese precedente — la longitud se difiere deliberadamente a `HasMaxLength` en Fluent API y al ancho
de columna en SQL, nunca duplicada como invariante de dominio.

Sin riesgos diferidos pendientes de migración de prueba distintos a los ya registrados para `Tema`
(mismo mecanismo de Guid v7, mismo patrón de trazabilidad — ningún Value Object ni conversión
propia que introduzca un riesgo nuevo).

---

## 10. Auditoría crítica adversarial

Revisión intencionalmente hostil, buscando exclusivamente problemas reales — no de estilo,
nombres ni formato. Siete hallazgos, ordenados por gravedad.

### Hallazgo 1 — No existe estrategia de soft-delete ni trazabilidad histórica genérica

**Gravedad: Alta**

**Por qué es un problema:** todo el propósito declarado de este sistema es ser una "memoria a
largo plazo" del aprendizaje — el valor central es la evolución histórica, no el estado actual.
Sin embargo, ninguna entidad tiene un mecanismo de borrado lógico. Las FKs `NO ACTION` protegen
contra cascadas *accidentales*, pero no contra un `DELETE` explícito y deliberado (un usuario
"limpiando" su roadmap, un script de mantenimiento, un bug en un caso de uso) que destruye
permanentemente años de `SesionEstudio` o `Nota` una vez que las restricciones se sortean
manualmente. Tampoco existen columnas `FechaCreacion` / `FechaModificacion` genéricas — solo
fechas con significado de negocio (`FechaInicio`, `FechaCumplido`).

**Impacto a 5-10 años:** un borrado accidental de un `Tema` con tres años de historial vinculado
es, literalmente, la pérdida de datos más grave que este sistema puede sufrir — y hoy no hay nada
que lo distinga de un `DELETE` cualquiera.

**¿Vale la pena corregirlo?** Sí, y antes de escribir la primera entidad — retrofitear un filtro
global de EF Core (`HasQueryFilter`) después de tener veinte entidades ya escritas es mucho más
costoso que incluirlo en la clase base desde el inicio.

**Solución:** añadir `FechaCreacionUtc`, `FechaModificacionUtc` y `FechaEliminacionUtc` (nullable)
a `Entidad` (o a un marcador `IAuditable`/`IEliminableLogicamente` aplicado a los Aggregate Roots
con valor histórico: `Tema`, `Proyecto`, `Laboratorio`, `Writeup`, `ArtefactoTecnico`,
`SesionEstudio`, `Nota`, `EntradaBitacora`). Los catálogos (`Herramienta`, `Certificacion`) pueden
seguir con `DELETE` físico porque no son "historia" en el mismo sentido. EF Core aplica
`HasQueryFilter(e => e.FechaEliminacionUtc == null)` de forma uniforme; el repositorio expone
`EliminarAsync` como una actualización de esa columna, nunca como un `DbSet.Remove()`.

### Hallazgo 2 — La regla de negocio "Estado de un Tema" está duplicada en dos tecnologías sin garantía de sincronía

**Gravedad: Alta**

**Por qué es un problema:** el cálculo de `Dominado` / `EnRepaso` existe en **dos lugares
independientes**: en C#, dentro del agregado `Tema` (para decidir si emitir `TemaDominadoEvento`
al marcar un criterio), y en T-SQL, dentro de `roadmap.vw_TemaEstado` (para las consultas rápidas
del Dashboard). No hay ningún mecanismo — ni de compilador ni de esquema — que garantice que
ambas implementaciones calculen lo mismo. El día que cambie el umbral de días para degradar a
"En repaso" (algo que ya intuimos que podría variar por `TipoConocimiento` en el futuro), hay que
recordar cambiarlo en dos sitios, en dos lenguajes distintos, y nada avisa si uno se actualiza y
el otro no.

**Impacto a 5-10 años:** este es exactamente el tipo de bug silencioso que sobrevive años sin
detectarse — el Dashboard mostraría un tema como dominado mientras la lógica de dominio, en el
mismo instante, ya lo consideraría en repaso (o viceversa), y ambas respuestas parecerían
"correctas" porque cada una es internamente consistente con su propia implementación.

**¿Vale la pena corregirlo?** Sí, pero no eliminando la duplicación — eliminarla del todo
significaría volver a hidratar agregados completos para cada consulta de Dashboard, deshaciendo la
decisión de CQRS-lite que sí vale la pena mantener. La corrección correcta es hacer la duplicación
**consciente y verificada**, no prohibirla.

**Solución:** un conjunto de pruebas de "paridad" en `Aprendizaje.Infraestructura.Tests` que, para
una matriz de escenarios (0/5 criterios, criterios parciales, dominado reciente, dominado con
degradación), inserta datos, calcula el estado con `Tema.CalcularEstado()` en memoria, ejecuta la
misma consulta contra `vw_TemaEstado`, y falla el build si los dos resultados divergen. La regla
de negocio documenta su fórmula exacta una sola vez (como comentario en ambos lugares, apuntando
el uno al otro), y el test de paridad es lo que realmente impide que se desincronicen sin que
nadie lo note.

### Hallazgo 3 — Nunca se definió la política de dependencias entre casos de uso dentro de Aplicación

**Gravedad: Alta**

**Por qué es un problema:** fijamos con precisión el grafo de dependencias permitido entre
módulos **dentro de Dominio** (sección 5), pero nunca extendimos esa misma disciplina a
`Aplicacion`. Nada impide hoy que un caso de uso de `Integration` (por ejemplo,
`SincronizarGitHub`) llame directamente al `Manejador` de `CrearProyecto` en `Evidence`, o que
`Evidence` invoque un caso de uso de `Roadmap` en vez de usar su repositorio directamente. Es
precisamente en la capa de orquestación — no en el dominio — donde la tentación de "total, ya
tengo la clase inyectada, la llamo directo" es más fuerte, y es donde más se acumula acoplamiento
oculto entre módulos con el paso de los años, sin que ningún test de arquitectura lo detecte
porque nunca se escribió la regla.

**Impacto a 5-10 años:** en un sistema con seis módulos que ya sabemos que va a crecer
(especialización, integraciones, IA), la ausencia de esta regla es la vía más probable por la que
el bajo acoplamiento que tanto cuidamos en `Dominio` se erosiona silenciosamente en `Aplicacion`
sin que el diseño original tenga ninguna culpa aparente — el problema nunca se ve en el módulo que
lo causó.

**¿Vale la pena corregirlo?** Sí, es la corrección de menor costo de toda esta auditoría — es una
regla, no una reestructuración.

**Solución:** un caso de uso nunca invoca el `Manejador` de otro caso de uso. La orquestación
entre módulos ocurre únicamente de dos formas: (a) un caso de uso "orquestador" que use
directamente los repositorios de varios módulos (no los manejadores de otros casos de uso), o (b)
reacción desacoplada vía Domain Events. Se añade como regla explícita de
`Aprendizaje.Arquitectura.Tests`: ningún tipo `*Manejador` puede ser inyectado como dependencia de
otro tipo `*Manejador`.

### Hallazgo 4 — Condición de carrera real en la validación de ciclos de `TemaDependencia`

**Gravedad: Media**

**Por qué es un problema:** ya documentamos que la ausencia de ciclos es una invariante que cruza
múltiples filas de `Tema` y se valida en un servicio de aplicación, no en la base de datos. Lo que
no señalamos entonces: ese patrón "leer el grafo → validar → insertar" es un clásico
**time-of-check-to-time-of-use**. Dos solicitudes concurrentes agregando `A→B` y `B→A` por
separado pueden pasar la validación individualmente (cada una lee el grafo antes de que la otra
confirme) y, juntas, crear el ciclo que se supone que debíamos impedir.

**Impacto a 5-10 años:** hoy, con un solo usuario, la probabilidad real de que esto ocurra es casi
nula. Pero es exactamente el tipo de bug que "duerme" durante años y aparece el día que el sistema
tenga más de un punto de entrada concurrente (una app móvil sincronizando en paralelo con la web,
por ejemplo) — y para entonces nadie va a sospechar de esta validación porque "siempre funcionó".

**¿Vale la pena corregirlo?** Vale la pena **documentarlo y mitigarlo con poco esfuerzo**, no
sobre-diseñarlo con locks distribuidos que hoy no hacen falta.

**Solución:** envolver la operación completa (leer grafo + validar + insertar) en una transacción
con nivel de aislamiento `SERIALIZABLE` sobre las filas de `roadmap.TemaDependencia` del `Tema`
afectado — suficiente para un volumen de escritura bajo como el de este dominio, sin necesidad de
un mecanismo de bloqueo distribuido.

### Hallazgo 5 — Riesgo de "God Object de lectura" en las consultas del Dashboard

**Gravedad: Media-Alta**

**Por qué es un problema:** al decidir que Dashboard/Analytics leen con proyecciones directas que
ignoran los agregados (decisión correcta), concentramos toda la responsabilidad de lectura
transversal en un lugar. El Dashboard que diseñamos en la fase de producto necesita: progreso
global, qué estudiar hoy, alertas de repaso, racha semanal, última actividad, progreso por fase,
métricas rápidas — siete piezas de información cruzando cinco módulos. Si todo esto se implementa
como un único `ObtenerDashboardQuery`/`Manejador`, ese manejador va a crecer sin control durante
años cada vez que se agregue un widget nuevo, convirtiéndose en el God Object que evitamos en el
lado de escritura pero recreamos en el de lectura.

**Impacto a 5-10 años:** un manejador de cientos de líneas, con SQL embebido tocando seis módulos,
es exactamente el tipo de clase que nadie quiere tocar ni testear en detalle — se vuelve frágil
por acumulación, no por una mala decisión puntual.

**¿Vale la pena corregirlo?** Sí, y es barato corregirlo ahora que solo existe en el papel.

**Solución:** cada pieza del Dashboard es su propia consulta pequeña e independiente
(`ProgresoGlobalQuery`, `TemasParaHoyQuery`, `AlertasRepasoQuery`, `RachaSemanalQuery`,
`UltimaActividadQuery`...), cada una con su propio manejador testeable de forma aislada. El
Dashboard como pantalla es una composición de varias consultas pequeñas en el endpoint de la Api,
no una sola consulta gigante.

### Hallazgo 6 — Catálogos globales (`Herramienta`, `Certificacion`) sin política de gobernanza para el escenario multiusuario que ya anticipamos

**Gravedad: Media**

**Por qué es un problema:** modelamos `Herramienta` y `Certificacion` como catálogos compartidos
globales explícitamente para evitar duplicación en un futuro multiusuario. Pero nunca definimos
quién puede crear una fila nueva, ni qué pasa cuando dos usuarios distintos crean "Wireshark" y
"wireshark" como dos herramientas separadas. Es una contradicción latente con nuestro propio
objetivo declarado: dijimos que el sistema está "preparado para multiusuario desde el día uno", y
esta pieza específica no lo está — es un catálogo global mutable sin ningún mecanismo de curación.

**Impacto a 5-10 años:** invisible mientras el sistema tenga un solo usuario (que es la situación
actual y probablemente de los próximos años). El riesgo aparece exactamente cuando el objetivo que
nos propusimos se cumpla.

**¿Vale la pena corregirlo ahora?** No — y esta es la respuesta correcta, no una excusa. Diseñar
un mecanismo de moderación/fusión de catálogo hoy, para un problema que solo existe si el sistema
se vuelve multiusuario, sería la sobreingeniería que la sección 5 explícitamente evitó en otros
puntos. Se documenta como decisión diferida, no como corrección pendiente.

**Solución (cuando aplique, no ahora):** añadir `EstadoRevision` a `Herramienta`/`Certificacion`
(`Sugerida`/`Aprobada`) con un flujo de curación mínimo, o resolver por búsqueda difusa al crear.

### Hallazgo 7 — `RowVersion` aplicado a solo 3 de ~16 Aggregate Roots, sin criterio documentado

**Gravedad: Baja-Media**

**Por qué es un problema:** la convención 9 justifica `RowVersion` en `Tema`, `SesionEstudio` y
`Proyecto` como "protección mínima ante ediciones concurrentes", pero `Laboratorio`, `Writeup`,
`ArtefactoTecnico`, `Competencia`, `Recurso`, `Nota` y el resto no lo tienen — y esas entidades se
editan con la misma frecuencia (cambios de `EstadoMadurez`, hallazgos, notas). No hay un criterio
explícito de por qué unas sí y otras no; a una auditoría externa le parece arbitrario, porque lo es.

**Impacto a 5-10 años:** bajo — es una inconsistencia de robustez, no un riesgo de pérdida de
datos como el Hallazgo 1. Pero es el tipo de asimetría no justificada que la auditoría del esquema
relacional (la de las reglas de borrado) nos enseñó a no dejar pasar.

**¿Vale la pena corregirlo?** Sí, por ser trivial y barato: añadir `RowVersion` de forma uniforme
a **todos** los Aggregate Roots, no seleccionar un subconjunto.

**Solución:** mover `RowVersion` a la clase base `AggregateRoot` (o a un marcador
`IConcurrencyChecked`) en vez de declararla entidad por entidad — con eso la inconsistencia se
vuelve estructuralmente imposible en vez de depender de que alguien se acuerde.

---

### Nota adicional, no un hallazgo por sí solo

`Tema` es, por diseño, la entidad con más fan-in de referencias de todo el sistema — casi todos
los módulos la referencian por `Guid`. Esto no es una violación de límites de agregado (el
agregado en sí sigue siendo pequeño: `Tema` + `CriterioTema`), pero sí crea un punto de
convergencia de tráfico de lectura sobre esa tabla. No lo cuento como hallazgo corregible porque
es una consecuencia inherente y correcta de que el roadmap sea el centro semántico del dominio —
mencionarlo es una advertencia de monitoreo a futuro (índices, posible caché de lectura si el
volumen crece), no un defecto de diseño.
