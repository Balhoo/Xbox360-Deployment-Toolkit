# Arquitectura UI y Design System v2.0

## Principio rector

El rediseño será una evolución de la capa de presentación. Los servicios, modelos, JSON, comandos operativos, validaciones y formatos de reporte actuales son contratos protegidos. La interfaz podrá introducir ViewModels de presentación, adaptadores y controles reutilizables, pero no reimplementar operaciones.

## Arquitectura objetivo

```text
Xbox360DeploymentToolkit/
├── Core/                         # Contratos y tipos compartidos (evolución futura)
├── Models/                       # Modelos actuales, sin cambios funcionales
├── Services/                     # Servicios actuales, sin cambios funcionales
├── ViewModels/
│   ├── Shell/
│   │   ├── AppShellViewModel.cs
│   │   └── NavigationItemViewModel.cs
│   ├── Pages/
│   │   ├── DashboardViewModel.cs
│   │   ├── PreparationViewModel.cs
│   │   ├── DeploymentViewModel.cs
│   │   ├── StorageViewModel.cs
│   │   ├── FtpViewModel.cs
│   │   ├── GamesViewModel.cs
│   │   ├── ValidationViewModel.cs
│   │   ├── ReportsViewModel.cs
│   │   └── SettingsViewModel.cs
│   └── Dialogs/
│       ├── DeploymentWizardViewModel.cs
│       └── ConfirmationDialogViewModel.cs
├── Presentation/
│   ├── Shell/AppShell.xaml
│   ├── Pages/*.xaml
│   ├── Controls/*.xaml
│   ├── Dialogs/*.xaml
│   ├── Themes/
│   │   ├── Tokens.xaml
│   │   ├── Typography.xaml
│   │   ├── Controls.xaml
│   │   └── Dark.xaml
│   └── Assets/Icons/
└── Configuration/               # JSON actuales, sin cambios funcionales
```

Durante la migración, `MainViewModel` puede actuar como fuente de verdad. Los nuevos Page ViewModels serán adaptadores sobre sus colecciones y comandos hasta que cada responsabilidad pueda separarse sin cambiar comportamiento.

## AppShell

### Regiones

```text
┌──────────────────────────────────────────────────────────────┐
│ Title bar: app · console · FTP · notifications · settings   │
├──────────────┬─────────────────────────────────┬─────────────┤
│ Sidebar      │ Page workspace                  │ Inspector   │
│ 240 / 64 px  │ responsive · one main scroll    │ 0 / 320 px  │
├──────────────┴─────────────────────────────────┴─────────────┤
│ Status bar: dry-run · FTP · free space · queue · version    │
└──────────────────────────────────────────────────────────────┘
```

### Reglas

- Sidebar expandida: 240 px; colapsada: 64 px.
- Inspector: 320 px en `Wide`, overlay o drawer en `Standard`, oculto hasta selección en `Compact`.
- Status bar siempre visible.
- Workspace posee el único scroll vertical principal.
- Title bar muestra estado, no acciones de módulo.
- Dry-run se presenta como badge persistente y control accesible.

## Mapa de navegación

La navegación agrupa responsabilidades sin crear una lista extensa.

| Sección | Páginas/submódulos | Funcionalidad existente cubierta |
|---|---|---|
| Dashboard | Resumen, actividad, acciones rápidas | Progreso, auditoría, unidades, FTP |
| Preparation | Requisitos, perfil de consola, almacenamiento | `PreparationItems`, onboarding |
| Deployment | Procedimiento, fase actual, wizard | `Steps`, progreso, notas |
| Content | Juegos, multidisco, DLC, Xbox clásico, emuladores | `Games`, manifiestos y carpetas |
| Network | FTP Explorer, transfer queue | Browse, upload, verify |
| Validation | Archivos, juegos, unidades, resultados | ValidationService y estados |
| Reports | Sesiones, exportación, actividad | ReportService y Audit |
| Settings | Rutas, credenciales, dry-run, apariencia | ToolkitSettings y CredentialStore |

La sidebar mostrará siete entradas máximas; submódulos se resuelven dentro del PageHeader o mediante navegación secundaria contextual.

## Dashboard

El Dashboard no reemplaza módulos; consume sus estados actuales.

### Fila de métricas

- Console Status: confirmación RGH, modelo, kernel.
- FTP Status: host, conexión y última operación.
- HDD Space: libre/total.
- USB Space: libre/total.
- Deployment Progress: porcentaje, fase y paso.
- Alerts: errores y advertencias.

### Zona operativa

- Workflow Card: fases agrupadas, máximo 8 visibles.
- Current Phase Card: 3–5 pasos relevantes, acción principal.
- Quick Actions Card: abrir FTP, verificar, exportar, reabrir wizard.
- Recent Activity Card: últimos eventos de `Audit`.

No se incluirán tutoriales largos ni documentación permanente.

## Wizard profesional

El wizard existente conservará sus siete etapas y decisiones, pero se presentará sin tabs visibles.

```text
Step 2 of 6
Console verification
Confirm how RGH was verified and register kernel/NAND evidence.

[ objective card ]
[ form card ]
[ warning/info banner ]

Back                                      Continue
```

### Reglas

- Indicador `Step n of total` con texto y progreso.
- Un objetivo primario por pantalla.
- Footer fijo con Back/Next.
- Verificación RGH usa el diálogo existente adaptado a `ConfirmationDialog`.
- Resumen final muestra evidencia, riesgo aceptado y cambios que se crearán.
- El usuario puede explorar la app y reabrir el wizard desde Dashboard o Settings.

## Checklist

### ChecklistItem

Campos visibles:

- Icono de estado.
- Título.
- Descripción breve, máximo dos líneas.
- Nivel/severidad.
- Estado textual.
- Acción contextual.

Campos del inspector:

- Instrucciones completas.
- Dependencias.
- Advertencias.
- Notas.
- Evidencia y actividad relacionada.

### Agrupación

- Una card por fase.
- 3–7 elementos visibles por grupo.
- Grupos completados colapsables.
- No más de 10 elementos simultáneos sin agrupación.

## FTP Workspace

```text
┌───────────────────────┬───────────────────────┐
│ Local                 │ Xbox                  │
│ breadcrumb · list     │ breadcrumb · list     │
├───────────────────────┴───────────────────────┤
│ Transfer Queue · progress · pause/retry       │
├───────────────────────────────────────────────┤
│ Session Log                                  │
└───────────────────────────────────────────────┘
```

- Se reutilizan `FtpEntries`, comandos Browse/Upload/Verify y logs actuales.
- La primera versión del panel local puede limitarse al archivo seleccionado actual; el diseño permite evolucionar sin fingir capacidades inexistentes.
- Credenciales se ubican en Connection Card/Settings, no junto al listado permanente.
- Password usa `PasswordBox` o control seguro equivalente; nunca texto visible normal.
- Cola representa únicamente operaciones reales soportadas; no se inventa pausa/reanudación hasta implementarlas en servicios.

## Juegos y contenido

### GameCard

- Cover placeholder o aportado localmente; no descarga arte automáticamente.
- Nombre, formato, estado, destino y progreso.
- Acciones: Verify, Transfer, Details.
- Badges: Multi-disc, DLC, Pending, Verified.

### InspectorPanel

- Title ID y Media ID cuando existan en datos.
- Disco 1/2 y contenido instalado.
- Rutas esperadas.
- Resultado de validación.
- Notas.

Los campos aún no disponibles se muestran como `Not recorded`, no se inventan.

## Design System interno

### Tokens de color

| Token | Valor | Uso |
|---|---:|---|
| `Color.Background` | `#0F1115` | Fondo raíz |
| `Color.Surface` | `#171A21` | Sidebar y cards |
| `Color.SurfaceElevated` | `#20242C` | Inspector, menús y hover |
| `Color.Border` | `#2F3642` | Divisores y outlines |
| `Color.TextPrimary` | `#F5F7FA` | Títulos y cuerpo principal |
| `Color.TextSecondary` | `#AEB7C2` | Metadatos y ayuda |
| `Color.Success` | `#3FB950` | Completado |
| `Color.Warning` | `#F2C94C` | Advertencia |
| `Color.Error` | `#EB5757` | Error/bloqueo |
| `Color.Accent` | `#107C10` | Acción primaria y selección |

Se crearán brushes semánticos derivados. Ninguna vista utilizará hex literal.

### Tipografía

| Estilo | Tamaño | Peso | Uso |
|---|---:|---|---|
| Display | 28 | Semibold | Bienvenida excepcional |
| Title | 24 | Semibold | Título de página |
| Subtitle | 18 | Semibold | Título de card |
| Body | 14 | Normal | Contenido |
| Label | 13 | Semibold | Inputs y acciones |
| Caption | 12 | Normal | Metadatos |
| Code | 13 | Normal | Rutas, hashes, IDs |

Familias: `Segoe UI Variable`, fallback `Segoe UI`; código `Cascadia Mono`.

### Espaciado

Tokens permitidos: 4, 8, 12, 16, 24, 32 y 48 px.

- Card padding: 16 o 24.
- Gap interno: 8 o 12.
- Gap entre cards: 12 o 16.
- Page inset: 24 estándar, 16 compacto.

### Geometría

- Card radius: 10 px.
- Button radius: 8 px.
- Input radius: 6 px.
- Border: 1 px.
- Focus ring: 2 px, separado 2 px.
- Sombras: solo elevated overlays, opacidad baja.

### Movimiento

- Duración: 150–200 ms.
- Curvas: ease-out para entrada, ease-in para salida.
- Permitido: fade, expand/collapse y progress.
- `SystemParameters.ClientAreaAnimation` y preferencias de movimiento deben respetarse.

## Catálogo de componentes reutilizables

| Componente | Responsabilidad | Variantes |
|---|---|---|
| `AppShell` | Estructura global y navegación | Expanded, Compact |
| `Sidebar` | Secciones e identidad XDT | Expanded, Collapsed |
| `PageHeader` | Título, contexto y acciones | Standard, WithBreadcrumb |
| `Card` | Superficie de responsabilidad | Default, Elevated, Interactive |
| `MetricCard` | Métrica y tendencia/estado | Neutral, Success, Warning, Error |
| `StatusBadge` | Icono+texto+color | Connected, Pending, Warning, Blocked |
| `ProgressCard` | Progreso con contexto | Linear, Circular |
| `ChecklistItem` | Paso operativo | Pending, Active, Complete, Blocked |
| `WizardStep` | Contenedor de fase | Default, Review |
| `WarningBanner` | Riesgo/advertencia | Inline, Persistent |
| `InfoBanner` | Contexto no crítico | Inline |
| `FilePath` | Ruta, tooltip y acciones | File, Folder, Remote |
| `TransferQueue` | Operaciones FTP soportadas | Empty, Active, Error |
| `GameCard` | Resumen de juego | Compact, Standard |
| `InspectorPanel` | Detalle de entidad | Docked, Drawer |
| `ConfirmationDialog` | Impacto y decisión | Safe, Warning, Destructive |
| `Toast` | Feedback transitorio | Success, Info, Warning, Error |
| `ReportCard` | Reporte y exportaciones | Session, Validation |

## Iconografía y branding

- Un único set Fluent, preferentemente geometría vectorial local para evitar dependencias runtime.
- Todos los iconos incluyen texto o `AutomationProperties.Name`.
- Logo XDT original: símbolo geométrico que combine nodos de red, validación y deployment.
- No usar esfera, X oficial, logotipo Xbox ni marcas registradas como elemento central.
- Acento verde limitado a selección, progreso, éxito y CTA primario.

## Accesibilidad

- Contraste mínimo AA.
- Orden de tabulación coincide con lectura visual.
- Shortcut hints y access keys para acciones principales.
- Focus siempre visible.
- Estado comunicado por icono, texto y color.
- Tooltips para icon-only buttons.
- Targets mínimos: 32 px; primarios 40 px.
- Pruebas a 100, 125, 150 y 200 %.
- Narrador: nombres y ayuda para host FTP, rutas, progreso y decisiones críticas.

## Responsive

| Modo | Ancho sugerido | Comportamiento |
|---|---:|---|
| Compact | 900–1099 | Sidebar 64, inspector drawer, métricas 1–2 columnas |
| Standard | 1100–1439 | Sidebar 240, inspector drawer, métricas 2–3 columnas |
| Wide | 1440+ | Sidebar 240, inspector 320, métricas 4–6 columnas |

No se fijarán breakpoints al monitor físico; se calcularán con `ActualWidth` y triggers/behaviors de presentación.

## Estrategia de migración sin reescritura

### Fase 1 — Foundations

- Crear tokens, typography, styles y controles base.
- Introducir `AppShell` y mantener páginas actuales embebidas temporalmente.
- Añadir logo original y set único de iconos.

### Fase 2 — Navigation and Dashboard

- Sustituir tabs por navegación lateral.
- Crear Dashboard a partir de propiedades y colecciones actuales.
- Mantener comandos originales.

### Fase 3 — Core workflows

- Convertir Preparation y Deployment a cards/checklist.
- Rediseñar wizard y VerificationGate con componentes compartidos.
- Migrar cambios visuales de code-behind a ViewModels/triggers.

### Fase 4 — FTP and Content

- Crear layout dual-pane usando capacidades FTP existentes.
- Crear GameCard e InspectorPanel.
- No exponer controles para funciones que los servicios aún no soportan.

### Fase 5 — Validation, Reports and QA

- Consolidar auditoría, validación y exportaciones.
- Revisar accesibilidad, escalado, teclado y estados vacíos.
- Comparar resultados funcionales con la versión anterior.

## Pruebas de no regresión

Antes y después de cada fase:

1. Guardar y restaurar progreso.
2. Completar onboarding en sus rutas verificada/no verificada.
3. Preparar estructura en dry-run y modo confirmado.
4. Listar FTP, transferir y verificar tamaño.
5. Validar juegos y rutas.
6. Exportar JSON/CSV y comparar estructura.
7. Confirmar que credenciales no aparecen en logs/reportes.
8. Verificar que Git no incluya `dist`, datos de usuario o secretos.

## Decisiones explícitas frente a la imagen de referencia

- Se adopta su jerarquía de shell, cards, métricas y status bar.
- No se copia su logo ni iconos; se crea identidad XDT original.
- No se mostrarán métricas falsas: IP, espacio, velocidad o progreso solo aparecen cuando existan datos reales.
- La documentación lateral de la referencia no vivirá permanentemente dentro del workspace; se resolverá mediante ayuda contextual y documentación separada.
- Funciones aún no implementadas —hash remoto, pause/resume FTP, covers automáticos— no aparecerán como controles activos.
- El flujo se reduce a agrupaciones comprensibles sin ocultar ninguna capacidad existente.

