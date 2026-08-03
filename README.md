# Xbox360 Deployment Toolkit

Aplicación WPF para Windows que guía y documenta la preparación responsable de una Xbox 360 con RGH. Es un proyecto de portafolio y una herramienta operativa: **no contiene ni descarga juegos, DLC, BIOS, ROMs, dashboards ni otros archivos protegidos o de procedencia dudosa**.

## Alcance del MVP

1. Primera ejecución guiada: diagnóstico de consola, modalidad de instalación, almacenamiento, componentes y juegos.
2. Subchecklist de preparación y checklist persistente por fases, notas, progreso y auditoría.
3. Detección de unidades, espacio disponible y creación segura de carpetas (dry-run por defecto; nunca formatea ni borra).
4. FTP para Aurora/XeXMenu: listar, cargar un archivo y verificar su tamaño remoto.
5. Catálogo JSON para títulos conocidos y personalizados; estructura multidisco, contenido y reportes JSON/CSV.

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
2. En la primera ejecución, elige **Comenzar con el deployment** y completa el diagnóstico. También puedes explorar y reabrirlo con **Asistente inicial**.
3. Revisa la pestaña **Preparación**, que separa hardware, herramientas, contenido propio y elementos opcionales.
4. Continúa con el checklist de procedimiento, auditoría y respaldo.
5. Elige la unidad/carpeta y simula la estructura. Revisa el destino antes de desactivar dry-run.
6. En FTP, usa la IP de la consola y credenciales del servidor de Aurora/XeXMenu. La opción “Recordar” cifra la credencial con DPAPI para el usuario actual de Windows.
7. Edita `Configuration/games.json` para que las rutas coincidan con tus archivos.
8. Valida y exporta el reporte.

El perfil de onboarding se guarda en `%LOCALAPPDATA%\Xbox360DeploymentToolkit\deployment-profile.json`. La biblioteca opcional de PC contiene únicamente estructura vacía y una advertencia; el toolkit no descarga juegos, DLC, BIOS ni ROMs.

Los datos del usuario, credenciales protegidas, logs y reportes se guardan en `%LOCALAPPDATA%\Xbox360DeploymentToolkit`.

## Arquitectura

- `Models`: estado observable y contratos de configuración.
- `Services`: unidades, FTP, validación, credenciales, JSON, logs y reportes.
- `ViewModels`: coordinación de la interfaz y comandos.
- `Configuration`: procedimiento, catálogo y ajustes externos.
- `MainWindow.xaml`: vista WPF sin lógica operativa.
- `EmbeddedWizardView`: onboarding, perfil y confirmación RGH dentro del único AppShell.
- `Presentation/Themes`: tokens y estilos del Design System oscuro.

La aplicación utiliza una sola ventana. Confirmaciones y feedback se muestran mediante overlays y toasts internos; solo los selectores nativos de archivos/carpetas pueden abrirse aparte.

Consulta [docs/SECURITY.md](docs/SECURITY.md) y [docs/PORTFOLIO.md](docs/PORTFOLIO.md).
