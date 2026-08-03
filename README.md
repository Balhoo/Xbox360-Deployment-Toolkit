# Xbox360 Deployment Toolkit

Aplicación WPF para Windows que guía y documenta la preparación responsable de una Xbox 360 con RGH. Es un proyecto de portafolio y una herramienta operativa: **no contiene ni descarga juegos, DLC, BIOS, ROMs, dashboards ni otros archivos protegidos o de procedencia dudosa**.

## Alcance del MVP

1. Subchecklist de preparación y checklist persistente por fases, notas, progreso y auditoría.
2. Detección de unidades, espacio disponible y creación segura de carpetas (dry-run por defecto; nunca formatea ni borra).
3. FTP para Aurora/XeXMenu: listar, cargar un archivo y verificar su tamaño remoto.
4. Manifiestos JSON para GTA V y Halo 4 multidisco, GTA IV + DLC, Xbox clásico y emuladores; validación local y reportes JSON/CSV.

Fuera del MVP: formateo de discos, instalación automática de dashboards, descarga de contenido, FXP, reanudación de cargas, hash remoto y administración completa del árbol FTP.

## Requisitos y build

- Windows 10/11 x64.
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (el runtime por sí solo no compila).

Desde PowerShell:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\build.ps1
```

El resultado queda en `dist\Xbox360DeploymentToolkit.exe`. Es self-contained y single-file; los JSON editables se publican junto al ejecutable en `dist\Configuration`.

Para desarrollo:

```powershell
dotnet run --project .\src\Xbox360DeploymentToolkit\Xbox360DeploymentToolkit.csproj
```

## Uso seguro

1. Abre la app y conserva **Modo simulación** activo.
2. Completa la pestaña **Preparación**, que separa hardware, herramientas, contenido propio y elementos opcionales.
3. Continúa con el checklist de procedimiento, auditoría y respaldo.
4. Elige la unidad/carpeta y simula la estructura. Revisa el destino antes de desactivar dry-run.
5. En FTP, usa la IP de la consola y credenciales del servidor de Aurora/XeXMenu. La opción “Recordar” cifra la credencial con DPAPI para el usuario actual de Windows.
6. Edita `Configuration/games.json` para que las rutas coincidan con tus archivos legítimos.
7. Valida y exporta el reporte.

Los datos del usuario, credenciales protegidas, logs y reportes se guardan en `%LOCALAPPDATA%\Xbox360DeploymentToolkit`.

## Arquitectura

- `Models`: estado observable y contratos de configuración.
- `Services`: unidades, FTP, validación, credenciales, JSON, logs y reportes.
- `ViewModels`: coordinación de la interfaz y comandos.
- `Configuration`: procedimiento, catálogo y ajustes externos.
- `MainWindow.xaml`: vista WPF sin lógica operativa.

Consulta [docs/SECURITY.md](docs/SECURITY.md) y [docs/PORTFOLIO.md](docs/PORTFOLIO.md).
