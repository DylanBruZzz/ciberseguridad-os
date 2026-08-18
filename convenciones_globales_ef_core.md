# Convenciones globales del proyecto

Este documento fija las reglas que se aplican de forma uniforme a partir de este punto.
Ninguna entidad, configuración o clase posterior se desvía de aquí sin que se documente
y justifique explícitamente la excepción.

---

## 1. Principio rector: el dominio no sabe que EF Core existe

Las clases de dominio son POCOs puros. Cero atributos de `System.ComponentModel.DataAnnotations`,
cero referencias a `Microsoft.EntityFrameworkCore` en el proyecto de Dominio. Toda la configuración
de persistencia vive exclusivamente en clases `IEntityTypeConfiguration<T>` con Fluent API, en el
proyecto de Infraestructura.

## 2. Idioma: ubicuo en español, técnico en inglés

- **Dominio y base de datos → español.** `Tema`, `CriterioTema`, `MarcarCriterio()`, `EstaDominado`.
  Es el lenguaje ubicuo usado en todo el diseño; traducirlo al inglés introduciría una traducción
  mental constante entre lo discutido y lo implementado.
- **Infraestructura genérica y patrones → inglés técnico estándar.** `IRepository<T>`, `IUnitOfWork`,
  `DomainEvent`, `ValueObject`.
- Nunca se mezclan los dos dentro del mismo identificador — un nombre completo está en un idioma
  o en el otro, nunca partido.

## 3. Target framework

**.NET 10 (LTS).** Para un sistema pensado para evolucionar varios años, la versión de soporte
extendido (3 años) es la decisión correcta frente a una versión STS de 18 meses. `Nullable`
habilitado en todos los proyectos, `ImplicitUsings` activado.

## 4. Building blocks de dominio

Tres clases base, ubicadas en `Dominio.Comun`, de las que heredan todas las entidades:

```csharp
public abstract class Entidad
{
    public Guid Id { get; protected set; }
    public DateTime FechaCreacionUtc { get; protected set; }
    public DateTime? FechaModificacionUtc { get; protected set; }

    protected Entidad() { } // requerido por EF Core, nunca público

    protected Entidad(Guid id)
    {
        Id = id;
        FechaCreacionUtc = DateTime.UtcNow;
    }

    public override bool Equals(object? obj) =>
        obj is Entidad otra && otra.GetType() == GetType() && otra.Id == Id;

    public override int GetHashCode() => Id.GetHashCode();
}

public abstract class AggregateRoot : Entidad
{
    private readonly List<IEventoDominio> _eventos = new();
    public IReadOnlyCollection<IEventoDominio> EventosDominio => _eventos.AsReadOnly();

    protected AggregateRoot() { }
    protected AggregateRoot(Guid id) : base(id) { }

    protected void RegistrarEvento(IEventoDominio evento) => _eventos.Add(evento);
    public void LimpiarEventos() => _eventos.Clear();
}

public abstract class ValueObject
{
    protected abstract IEnumerable<object?> ComponentesIguales();

    public override bool Equals(object? obj) =>
        obj is ValueObject otro && GetType() == otro.GetType()
        && ComponentesIguales().SequenceEqual(otro.ComponentesIguales());

    public override int GetHashCode() =>
        ComponentesIguales().Aggregate(0, (h, c) => HashCode.Combine(h, c));
}
```

`FechaCreacionUtc` se fija una sola vez en el constructor. `FechaModificacionUtc` no se asigna a
mano en cada método de comportamiento — sería ruido repetido en todos los agregados. Un
interceptor de EF Core (`AuditoriaInterceptor`, ver sección 12) la actualiza automáticamente en
cualquier entidad marcada como modificada, justo antes de `SaveChangesAsync`.

Solo `Tema`, `Proyecto`, `Laboratorio`, `Writeup`, `ArtefactoTecnico`, `CertificacionObtenida`,
`Competencia`, `Certificacion`, `Fase`, `Recurso`, `SesionEstudio`, `EntradaBitacora`, `Conector`,
`SnapshotProgreso`, `Nota` y `Usuario` heredan de `AggregateRoot`. Únicamente `CriterioTema` hereda
de `Entidad` — es la única entidad interna que no es raíz de agregado.

## 5. Borrado lógico y trazabilidad histórica selectiva

El sistema existe para preservar la evolución del aprendizaje en el tiempo — un `DELETE` físico
sobre una entidad con valor histórico destruye ese activo de forma irreversible. La trazabilidad
(`FechaCreacionUtc` / `FechaModificacionUtc`) es universal, definida en `Entidad` para todas las
entidades sin excepción. El **borrado lógico**, en cambio, es selectivo — se aplica solo donde
realmente hay historia que proteger, no como regla ciega sobre toda la base:

```csharp
public interface IEliminableLogicamente
{
    DateTime? FechaEliminacionUtc { get; }
    void MarcarComoEliminado();
}
```

Implementan `IEliminableLogicamente`: `Usuario`, `Tema`, `Proyecto`, `Laboratorio`, `Writeup`,
`ArtefactoTecnico`, `CertificacionObtenida`, `SesionEstudio`, `Nota`, `EntradaBitacora`, `Recurso`
— las entidades cuyo borrado representaría pérdida real de historial de aprendizaje. `Usuario` es
un caso particular dentro de esta lista: es el propietario lógico de todo el historial, y sus
relaciones ya usan `NO ACTION` sin excepción, así que el borrado de una cuenta debe desactivarla
lógicamente sin destruir el historial de aprendizaje asociado — nunca un `DELETE` físico dentro
del flujo normal de la aplicación. Impedir el acceso/autenticación de un `Usuario` eliminado es
responsabilidad de la capa de aplicación/seguridad, no del dominio de persistencia. **No**
implementan `IEliminableLogicamente`: `Fase`, `Competencia`, `Herramienta`, `Certificacion`
(catálogo), `Conector`, `LogSincronizacion` ni `SnapshotProgreso` — son metadatos organizativos,
catálogos compartidos o datos operativos/derivados, no historia personal; siguen con `DELETE`
físico protegido por las reglas de FK ya definidas en el esquema relacional.

Configuración EF Core: `HasQueryFilter(e => e.FechaEliminacionUtc == null)` aplicado a cada
entidad que implementa la interfaz. El repositorio expone `EliminarAsync` como una actualización
de `FechaEliminacionUtc`, nunca como `DbSet.Remove()`, para las entidades de esta lista.

## 6. Identificadores: generados por el dominio, no por la base de datos

El `Guid` de un Aggregate Root se genera en el constructor del dominio con `Guid.CreateVersion7()`
(GUID v7, ordenable temporalmente — mismo criterio de índice secuencial que `NEWSEQUENTIALID()`
en SQL Server), no se delega a la base de datos. En EF Core se configura con
`.ValueGeneratedNever()` en cada Fluent API. El `DEFAULT NEWSEQUENTIALID()` del esquema SQL queda
como red de seguridad para inserciones fuera de la aplicación, no como mecanismo primario.

## 7. Nulabilidad

Nullable reference types habilitado en todos los proyectos. La nulabilidad de cada propiedad de
dominio refleja exactamente la nulabilidad ya definida en el esquema relacional — ninguna
propiedad es `nullable` en C# si su columna es `NOT NULL`, y viceversa.

## 8. Fechas y horas

- Columnas `DATE` del esquema (`FechaInicio`, `Fecha` en `SesionEstudio`) → tipo `DateOnly` en C#.
- Columnas `DATETIME2` (`FechaCumplido`, `Fecha` en `Nota`/`EntradaBitacora`) → `DateTime` **siempre
  en UTC** (`DateTime.UtcNow`, nunca `DateTime.Now`).

## 9. Enums

Enums de C# fuertemente tipados (`EstadoMadurez`, `TipoCriterio`, `TipoConocimiento`...),
persistidos como `string` vía `.HasConversion<string>()` — nunca como `int`. Un `int` que se
reordena o inserta un valor intermedio en una migración futura es una fuente de bugs silenciosos;
un `string` no.

## 10. Encapsulación y mapeo con EF Core

- Todas las propiedades con **setter privado**. La única forma de cambiar el estado de una
  entidad es a través de un método de comportamiento (`tema.MarcarCriterio(...)`,
  `proyecto.Publicar()`), nunca asignación directa desde fuera del agregado.
- Las colecciones internas (`CriterioTema` dentro de `Tema`) se exponen como
  `IReadOnlyCollection<T>`; el campo de respaldo (`List<T>` privado) se mapea con
  `.UsePropertyAccessMode(PropertyAccessMode.Field)`.
- Ningún constructor público sin parámetros en las entidades de dominio salvo el `protected` que
  exige EF Core.

## 11. Concurrencia optimista: RowVersion selectivo

`RowVersion` no es obligatorio para todos los Aggregate Roots. Se utiliza únicamente cuando existe
una necesidad real y verificable de concurrencia optimista derivada de los casos de uso o del
riesgo de actualizaciones concurrentes.

El estado definitivo actual conserva `RowVersion` solo en `Usuario`, `Tema`, `SesionEstudio` y
`Proyecto`, con paridad entre SQL, dominio y EF Core (`VersionFila` + `IsRowVersion()`). No se
agrega preventivamente a `Fase`, `Certificacion`, `Competencia`, `Herramienta`,
`EntradaBitacora`, `Laboratorio`, `Writeup`, `ArtefactoTecnico`, `CertificacionObtenida`, `Nota`,
`Recurso`, `SnapshotProgreso` ni `Conector`.

Si en el futuro otro Aggregate Root requiere concurrencia optimista, la incorporación debe
justificarse mediante un caso de uso real y coordinar cambios en dominio, configuración EF Core,
SQL/migración y pruebas de concurrencia. `VersionFila` no se mueve a `AggregateRoot` como
propiedad base.

## 12. DbContext y organización de configuraciones

Un único `AprendizajeDbContext`. Las clases `IEntityTypeConfiguration<T>` se agrupan en carpetas
que reflejan los schemas SQL (`Persistencia/Roadmap`, `Persistencia/Study`, `Persistencia/Evidence`...)
y se registran con:

```csharp
modelBuilder.ApplyConfigurationsFromAssembly(typeof(AprendizajeDbContext).Assembly);
```

Cada configuración fija explícitamente su schema (`builder.ToTable("Tema", "roadmap")`). El
`AuditoriaInterceptor` mencionado en la sección 4 y el `DespachoEventosInterceptor` de la sección
16 se registran como `SaveChangesInterceptor` sobre este mismo `DbContext`.

## 13. Migraciones

Nombre descriptivo en PascalCase por cambio lógico (`AgregarIntervaloRepasoATema`, no `Update1`).
Una migración nunca se edita después de aplicarse en cualquier entorno compartido — un cambio
posterior es una migración nueva.

## 14. Namespaces por módulo

```
Aprendizaje.Dominio.Roadmap        (Tema, Fase, Competencia, Certificacion, CriterioTema...)
Aprendizaje.Dominio.Study          (SesionEstudio, Herramienta, EntradaBitacora)
Aprendizaje.Dominio.Evidence       (Proyecto, Laboratorio, Writeup, ArtefactoTecnico, Nota)
Aprendizaje.Dominio.Resource       (Recurso)
Aprendizaje.Dominio.Analytics      (SnapshotProgreso)
Aprendizaje.Dominio.Integration    (Conector, LogSincronizacion)
Aprendizaje.Dominio.Comun          (Entidad, AggregateRoot, ValueObject, IEventoDominio, IEliminableLogicamente)
```

Los namespaces son el espejo exacto de los schemas SQL y de los módulos definidos en la
arquitectura general.

## 15. Async

Todo método de acceso a datos termina en `Async` y recibe `CancellationToken` como último
parámetro (`Task<Tema?> ObtenerPorIdAsync(Guid id, CancellationToken ct)`). Sin excepciones.

---

## 16. Estrategia de Domain Events: cuándo y cómo se despachan

El dominio **produce** eventos (`AggregateRoot.RegistrarEvento`), pero no sabe cómo ni cuándo se
despachan — eso es responsabilidad exclusiva de infraestructura.

### 16.1 — El evento es un concepto de dominio puro

```csharp
public interface IEventoDominio
{
    DateTime OcurrioEnUtc { get; }
}

public sealed record TemaDominadoEvento(Guid TemaId, DateTime OcurrioEnUtc) : IEventoDominio;
public sealed record SesionRegistradaEvento(Guid SesionId, Guid TemaId, DateTime OcurrioEnUtc) : IEventoDominio;
```

`IEventoDominio` vive en `Dominio.Comun`, sin dependencia de MediatR ni de EF Core. Un evento se
registra dentro de un método de comportamiento del agregado:

```csharp
public void MarcarCriterio(TipoCriterio tipo)
{
    // ... lógica de validación y actualización del criterio ...
    if (EstaDominado())
        RegistrarEvento(new TemaDominadoEvento(Id, DateTime.UtcNow));
}
```

### 16.2 — El despacho ocurre después de `SaveChangesAsync`, nunca antes ni dentro

Se implementa como un `SaveChangesInterceptor` de EF Core (Infraestructura):

```csharp
public sealed class DespachoEventosInterceptor : SaveChangesInterceptor
{
    private readonly IDespachadorEventos _despachador;

    public DespachoEventosInterceptor(IDespachadorEventos despachador) => _despachador = despachador;

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken ct = default)
    {
        var contexto = eventData.Context;
        if (contexto is null) return result;

        var raices = contexto.ChangeTracker.Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(a => a.EventosDominio.Any())
            .ToList();

        var eventos = raices.SelectMany(a => a.EventosDominio).ToList();
        raices.ForEach(a => a.LimpiarEventos());

        foreach (var evento in eventos)
            await _despachador.PublicarAsync(evento, ct);

        return result;
    }
}
```

**Por qué después, no antes:** publicar antes de que la transacción se confirme arriesga procesar
un evento cuyo cambio de origen termina revirtiéndose. Publicar dentro de la misma transacción
acopla la duración de la escritura principal a la de todos sus efectos secundarios.

### 16.3 — Manejo de errores y alcance

`IDespachadorEventos.PublicarAsync` captura y registra cualquier excepción de un handler
individual, sin propagarla — un handler roto de un efecto secundario nunca revierte ni bloquea la
operación principal que ya tuvo éxito. El despacho es en memoria, dentro del mismo proceso (sin
outbox ni cola de mensajes); si en el futuro un handler necesita garantía de entrega, se añade una
tabla de outbox sin tocar el dominio.

Se usa MediatR para dos cosas acotadas: `INotification`/`IPublisher` para el despacho de eventos
de dominio, y `IRequest`/`IRequestHandler` para comandos y consultas, aprovechando
`IPipelineBehavior<,>` para validación (FluentValidation) y logging aplicados una sola vez a todos
los casos de uso. MediatR es gratuito para individuos y organizaciones con menos de 5 millones de
USD de facturación anual desde su cambio a licenciamiento comercial en julio de 2025 — cubre este
proyecto sin ambigüedad.

**Orden de ejecución:** los `INotificationHandler` de un mismo evento se ejecutan de forma
secuencial en el orden de registro, pero el diseño no debe depender de ese orden. Si dos efectos
de un mismo evento tienen una dependencia real de secuencia entre sí, es señal de que en realidad
son un solo handler con dos pasos internos, no dos handlers independientes.

**Garantías de consistencia:** el cambio de estado del propio agregado tiene consistencia fuerte
(misma transacción de `SaveChangesAsync`, ACID completo). Los efectos secundarios disparados por
el evento son eventual y best-effort — ocurren después de que la transacción ya se confirmó.

---

## 17. Cuándo un Value Object y cuándo una propiedad simple

Una propiedad se promueve a Value Object **solo si cumple al menos uno** de estos cuatro
criterios. Si no cumple ninguno, se queda como tipo primitivo o enum:

1. **Tiene una invariante propia que debe validarse en construcción**, más allá de "no vacío" o
   "no nulo". Ejemplo: `NivelPercepcion` para `DificultadPercibida` y `Confianza` — un `int`
   suelto permite un `7`; el VO lo hace estructuralmente imposible.
2. **Tiene comportamiento de dominio propio.** Ejemplo: `IntervaloRepaso`, con su método
   `ProximaFecha(DateTime ultimaPractica)`.
3. **Se compone de varios campos primitivos que solo tienen sentido juntos** y se validan como
   unidad. Ningún candidato actual lo cumple; queda como criterio para el futuro.
4. **La misma regla de validación se repetiría en más de un lugar** si no se centraliza. Ningún
   campo actual lo amerita todavía.

**No se crea un Value Object solo por sonar "más DDD".** Envolver `Nombre`, `Descripcion`, `Url`
o `Texto` sin una invariante real añade ceremonia sin protección nueva.

| Candidato | Criterio que cumple | Decisión |
|---|---|---|
| `NivelPercepcion` (dificultad, confianza) | #1 | ✅ Value Object |
| `IntervaloRepaso` | #2 | ✅ Value Object |
| `Nombre`, `Descripcion`, `Url`, `Texto` | Ninguno | ❌ `string` simple |
| `EstadoMadurez`, `TipoCriterio`, `TipoConocimiento` | Ninguno (vocabulario cerrado, no valor validable) | ❌ Enum, no VO |

Un **enum** es un conjunto cerrado y nombrado de valores discretos; un **Value Object** es un
valor con estructura y/o comportamiento propio — son conceptos distintos que no se confunden
aunque un enum "tenga reglas" de transición. Los dos Value Objects confirmados se mapean como
Owned Types (`.OwnsOne()`).

---

## 18. Política de dependencias entre Casos de Uso en Aplicación

El grafo de dependencias permitido entre módulos de `Dominio` (`Study → Roadmap`,
`Evidence → Roadmap, Study`, `Resource → Roadmap`) se extiende con la misma disciplina a la capa
de Aplicación, donde el acoplamiento oculto entre módulos es más probable porque es donde ocurre
la orquestación:

**Regla:** un caso de uso nunca invoca el `Manejador` de otro caso de uso. Ningún tipo `*Manejador`
puede ser inyectado como dependencia de otro tipo `*Manejador`. La orquestación entre módulos
ocurre únicamente de dos formas:

- **Caso de uso orquestador:** un único manejador que usa directamente los repositorios de varios
  módulos (no los manejadores de otros casos de uso) cuando una operación de negocio
  genuinamente cruza módulos — por ejemplo, sincronizar una plataforma externa y crear evidencia
  a partir de ella.
- **Reacción desacoplada vía Domain Events:** cuando el segundo efecto no necesita ser parte de
  la misma operación transaccional.

Esta regla se verifica automáticamente en `Aprendizaje.Arquitectura.Tests`.

## 19. Consultas de lectura: una responsabilidad por consulta

Ninguna consulta de lectura agrega información de más de un widget o vista pequeña en un único
manejador. El caso más exigente es el Dashboard, que necesita progreso global, temas para hoy,
alertas de repaso, racha semanal, última actividad, progreso por fase y métricas rápidas —
información cruzando cinco módulos. Esto **no** se implementa como un único
`ObtenerDashboardQuery`: cada pieza es su propia consulta pequeña e independiente
(`ProgresoGlobalQuery`, `TemasParaHoyQuery`, `AlertasRepasoQuery`, `RachaSemanalQuery`,
`UltimaActividadQuery`...), cada una con su propio manejador testeable de forma aislada. La
pantalla de Dashboard es una composición de varias consultas pequeñas en el endpoint de la Api,
nunca una sola consulta que crece sin control cada vez que se agrega un widget.

Esta regla aplica a cualquier consulta futura con la misma forma, no solo al Dashboard actual.

## 20. Verificación de paridad entre reglas de dominio y proyecciones SQL

El cálculo de `Estado` de un `Tema` (`Dominado` / `EnRepaso` / etc.) existe en dos lugares por
diseño: en C#, dentro del agregado `Tema`, para decidir cuándo emitir `TemaDominadoEvento`; y en
una vista SQL (`roadmap.vw_TemaEstado`), para las consultas de lectura que no pasan por agregados.
Esta duplicación no se elimina — hacerlo obligaría a hidratar agregados completos en cada consulta
de Dashboard, deshaciendo la separación de lectura/escritura que sí vale la pena mantener. Se
hace, en cambio, **consciente y verificada**:

`Aprendizaje.Infraestructura.Tests` incluye una suite de tests de paridad que, para una matriz de
escenarios (cero criterios, criterios parciales, dominado reciente, dominado con degradación por
tiempo), calcula el estado con `Tema.CalcularEstado()` en memoria y ejecuta la misma consulta
contra `vw_TemaEstado`, fallando el build si los dos resultados divergen. Cualquier cambio a la
fórmula de negocio (por ejemplo, el umbral de días para degradar a "En repaso") debe actualizarse
en ambos lugares y pasar esta suite antes de integrarse.

## 21. Deuda técnica documentada y decisiones diferidas

Dos decisiones se aceptan explícitamente como pendientes, para que queden registradas en vez de
olvidadas:

- **Condición de carrera en la validación de ciclos de `TemaDependencia`.** La invariante de
  "grafo sin ciclos" se valida en un servicio de aplicación mediante lectura-y-escritura, sin
  aislamiento transaccional explícito todavía. Con un solo usuario el riesgo práctico es bajo.
  Queda documentado como deuda consciente a resolver (envolviendo la operación en una transacción
  `SERIALIZABLE` sobre las filas afectadas) en el momento en que exista escritura concurrente real
  sobre el mismo grafo.
- **Gobernanza de catálogos globales (`Herramienta`, `Certificacion`).** Al ser catálogos
  compartidos sin `UsuarioId`, no existe hoy ningún mecanismo de moderación o fusión de entradas
  duplicadas entre usuarios distintos. Se difiere explícitamente por YAGNI: diseñar ese mecanismo
  ahora, para un problema que solo existe en un escenario multiusuario todavía hipotético, sería
  la sobreingeniería que el resto de estas convenciones evita deliberadamente. Se revisita cuando
  el sistema tenga más de un usuario real.
