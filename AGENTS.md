# Proyecto

Nombre: Aprendizaje-Solucion

Stack:

- .NET 10 LTS
- C#
- ASP.NET Core Minimal APIs
- EF Core 10
- SQL Server
- Clean Architecture
- DDD
- Modular monolith

Antes de cada tarea, leer:

- docs/estado-proyecto.md
- docs/decisiones-arquitectura.md
- docs/lista-maestra.md

# Proyectos

- Aprendizaje.Dominio
- Aprendizaje.Aplicacion
- Aprendizaje.Infraestructura
- Aprendizaje.Api

Dependencias permitidas:

- Dominio -> ninguna capa interna
- Aplicacion -> Dominio
- Infraestructura -> Dominio + Aplicacion
- Api -> Aplicacion + Infraestructura

# Idioma

Dominio y casos de negocio: español.

Infraestructura tecnica: puede utilizar ingles estandar cuando corresponda.

# Reglas de implementacion

- No introducir paquetes sin necesidad demostrada.
- No introducir MediatR, AutoMapper, FluentValidation u otros frameworks preventivamente.
- No crear repositorios genericos.
- Crear repositorios solo cuando un caso de uso real los requiera.
- Application coordina casos de uso.
- API no contiene logica de dominio.
- Dominio no depende de EF Core.
- Infraestructura no realiza SaveChanges desde repositories.
- IUnitOfWork controla persistencia del caso de uso.
- GUID de entidades se genera en dominio con Guid.CreateVersion7().
- No usar NEWSEQUENTIALID().
- Enums persistidos como string donde ya esta definido.
- Preservar invariantes existentes.
- No cambiar arquitectura congelada salvo autorizacion explicita.

# EF / Migraciones

- No modificar una migracion ya aplicada.
- Antes de crear migracion: build + design-time check.
- Auditar Up y Down antes de aplicar.
- No ejecutar database update salvo autorizacion explicita.
- SQL maestro sigue siendo referencia estructural.
- Politica de paridad DDL: hibrida.
- RowVersion selectivo unicamente donde esta documentado.
- vw_TemaEstado esta deliberadamente diferida.

# Git

- Branch principal: main.
- No commit automaticamente salvo que la tarea lo autorice.
- No push automaticamente.
- No configurar remote automaticamente.
- Antes de cambios importantes, comprobar git status.
- Despues de cambios, reportar git status y diff stat.
- No incluir .vs/bin/obj/logs.

# Build

Comando principal:

```powershell
dotnet build Aprendizaje.slnx
```

Criterio de cierre normal:

0 warnings, 0 errores.

# Flujo de trabajo

Para cada tarea:

1. Leer documentacion operativa.
2. Inspeccionar codigo relevante.
3. No ampliar alcance.
4. Implementar minimo necesario.
5. Build.
6. Ejecutar pruebas autorizadas.
7. Revisar git diff --check.
8. Revisar git diff --stat.
9. No commit salvo autorizacion.
10. Entregar reporte compacto.

# Regla de detencion

Si aparece:

- contradiccion arquitectonica;
- migracion inesperada;
- paquete nuevo requerido;
- cambio fuera de alcance;
- riesgo sobre datos;
- fallo cuya solucion requiere cambiar decisiones congeladas;

DETENERSE Y REPORTAR.

No improvisar solucion estructural.

# Reporte por defecto

Al terminar una tarea normal responder con:

- ESTADO
- ARCHIVOS CREADOS
- ARCHIVOS MODIFICADOS
- BUILD
- PRUEBAS
- GIT DIFF --STAT
- BLOQUEADORES
- DECISIONES
- SIGUIENTE ESTADO

No repetir toda la arquitectura salvo solicitud explicita.
