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

## Ejecutable Local Windows

El proyecto puede publicarse como una carpeta ejecutable local para Windows:

```powershell
cd <repo>
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\publish-windows.ps1
```

El resultado queda en:

```text
artifacts\CiberseguridadOS-win-x64
```

Para ejecutar la aplicacion publicada:

```powershell
.\artifacts\CiberseguridadOS-win-x64\CiberseguridadOS.exe
```

El ejecutable publicado inicia ASP.NET Core en modo `Personal`, sirve Angular production desde `wwwroot`, expone la API en same-origin bajo `/api` y abre `http://localhost:64021/dashboard`. No requiere `npm start` ni `dotnet run` para uso publicado.

Prerequisito de datos: SQL Server local debe estar iniciado y la instancia `.\MSSQLSERVER01` debe tener disponible `AprendizajePersonalDb`. `appsettings.Personal.json` queda incluido en la carpeta publicada y puede editarse si cambia la conexion local. Esto no es un installer; es una carpeta portable de publish `win-x64` self-contained.

## Backup de AprendizajePersonalDb

Backup manual recomendado:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\backup-personal-db.ps1
```

El script usa Windows Authentication contra `.\MSSQLSERVER01`, descubre la carpeta nativa de backups de SQL Server, crea un backup full `COPY_ONLY` con `CHECKSUM` y ejecuta `RESTORE VERIFYONLY` con checksum. No hace restore y no toca datos de negocio.

Tambien permite parametros opcionales:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\backup-personal-db.ps1 -ServerInstance ".\MSSQLSERVER01" -Database "AprendizajePersonalDb" -BackupDirectory "D:\Backups"
```

Politica operativa:

- Obligatorio antes de migrations, cambios de schema o population estructural importante.
- Recomendado semanal o quincenal durante uso frecuente, mensual con uso ligero y al cerrar hitos importantes.
- Conservar varios backups recientes, backups pre-migration y snapshots de hitos.
- Mantener al menos una copia privada fuera del disco principal, por ejemplo en disco externo, almacenamiento privado cifrado o nube privada personal.
- No subir archivos `.bak` a GitHub; contienen datos Personal.

## Testing

Frontend: 255 tests.

Backend: 543 tests baseline.

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
