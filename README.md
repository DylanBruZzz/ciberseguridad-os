# Ciberseguridad OS

Ciberseguridad OS es una plataforma personal local para organizar, estudiar y dar seguimiento a un roadmap de aprendizaje en ciberseguridad.

El proyecto combina una experiencia de workstation personal con un backend local, datos factuales y modulos orientados a estudio, recursos, evidencia, portafolio y analitica.

## Estado

Version estable historica: `v1.0.0`.

HEAD actual: contiene un polish visual posterior al tag `v1.0.0`.

El tag `v1.0.0` representa la primera release estable previa al checkpoint visual final. El commit actual conserva esa base funcional y agrega alineacion visual posterior sin mover el tag.

## Funcionalidades

- Dashboard
- Roadmap
- Topic Workspace
- Study
- Resources
- Evidence
- Portfolio
- Global Search
- Analytics

## Tecnologias

Backend:

- .NET 10
- ASP.NET Core
- EF Core 10
- SQL Server

Frontend:

- Angular 21
- TypeScript
- CSS nativo

Arquitectura:

- Clean Architecture
- DDD
- Modular monolith

## Ejecucion Local

Prerequisitos:

- .NET 10 SDK
- Node.js/npm compatible con Angular 21
- SQL Server local

Backend:

```powershell
cd <repo>
$env:ASPNETCORE_ENVIRONMENT = 'Personal'
dotnet run --project src\Aprendizaje.Api\Aprendizaje.Api.csproj --no-launch-profile
```

Frontend:

```powershell
cd frontend
npm install
npm start
```

URL local:

[http://localhost:4200/dashboard](http://localhost:4200/dashboard)

## Testing

Frontend: 255 tests.

Backend: 535 tests baseline.

## Uso de IA

Este proyecto fue desarrollado con asistencia de herramientas de IA para diseno, implementacion, revision y testing. Las decisiones, integracion, validacion y aprendizaje del sistema forman parte del proceso de desarrollo del autor.

## Futuro

Mejoras posibles para versiones V1.x/V2:

- ejecutable local;
- Light;
- Dock;
- Timer/Pomodoro;
- Semantic/AI Search;
- uploads/export;
- mejoras de operacion y distribucion local.
