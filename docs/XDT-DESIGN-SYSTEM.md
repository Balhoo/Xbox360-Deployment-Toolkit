# XDT Design System v1.0

## Propósito

XDT Design System es la capa visual compartida de Xbox360 Deployment Toolkit. Su objetivo es ofrecer una interfaz coherente, accesible y mantenible inspirada en Fluent 2 y Windows 11, sin imitar el dashboard de Xbox ni convertirse en un launcher gamer.

La lógica de negocio no depende de esta capa. Servicios, modelos, validaciones, FTP y reportes pueden evolucionar sin duplicar decisiones visuales.

## Arquitectura

```text
Presentation/DesignSystem/
├── Tokens/
│   ├── Colors.xaml
│   ├── Typography.xaml
│   ├── Spacing.xaml
│   ├── Sizing.xaml
│   ├── Radius.xaml
│   ├── Elevation.xaml
│   └── Motion.xaml
├── Styles/
│   ├── ButtonStyles.xaml
│   ├── InputStyles.xaml
│   ├── CardStyles.xaml
│   ├── TextStyles.xaml
│   ├── DialogStyles.xaml
│   ├── ListStyles.xaml
│   └── ComponentStyles.xaml
├── Controls/
│   └── XdtControls.cs
└── Themes/
    ├── Dark.xaml
    └── Light.xaml
```

`Presentation/Themes/DarkTheme.xaml` actúa como punto único de composición. Las vistas consumen recursos semánticos y no conocen códigos de color.

## Reglas globales

- Cuadrícula base de 8 px, con incrementos permitidos de 4, 8, 12, 16, 24, 32, 40, 48 y 64.
- Tema oscuro como configuración predeterminada.
- Segoe UI Variable para interfaz; Cascadia Mono para rutas y datos técnicos.
- Un `ScrollViewer` principal como máximo por página.
- Títulos de una línea con `TextTrimming` y `ToolTip` cuando puedan truncarse.
- Estados representados mediante texto, icono y color.
- Confirmaciones operativas dentro del workspace; overlays reservados para bloqueos de seguridad como RGH no verificado.
- Selectores nativos de archivo o carpeta son las únicas ventanas externas permitidas.

## Component specification

### XdtAppShell

- **Propósito:** alojar top bar, sidebar, contenido y status bar.
- **Usar:** como raíz del workspace principal.
- **No usar:** dentro de páginas o diálogos.
- **Comportamiento:** 1440×900 de diseño, mínimo 1200×720 y adaptación al área de trabajo.
- **Accesibilidad:** orden de foco top bar → navegación → contenido → estado.

### XdtSidebar

- **Propósito:** navegación primaria entre siete áreas del producto.
- **Variantes:** expandida 240 px y colapsada 72 px.
- **Estados:** normal, hover, active, focused y disabled.
- **Comportamiento:** el elemento activo muestra barra izquierda, superficie elevada y texto primario.
- **Accesibilidad:** cada icono debe conservar etiqueta o tooltip en modo colapsado.

### XdtPageHeader

- **Propósito:** presentar título, descripción y una acción primaria opcional.
- **Usar:** una vez al inicio de cada página.
- **No usar:** dentro de cards.
- **Propiedades:** `Header`, `Content` y acción contextual de la vista.
- **Accesibilidad:** encabezado visible y descripción envolvente.

### XdtButton

- **Propósito:** ejecutar acciones con geometría y comportamiento consistentes.
- **Variantes:** `Primary`, `Secondary`, `Success`, `Warning`, `Danger`, `Ghost`, `Outline`, `Text`.
- **Tamaños:** `Small` 32 px, `Medium` 40 px y `Large` 48 px.
- **Propiedades públicas:** `Variant`, `ControlSize` y las propiedades estándar de `Button`.
- **Estados:** normal, hover, pressed, focused y disabled.
- **Usar:** Primary para una sola acción dominante; Danger para una acción destructiva o salida explícita.
- **No usar:** colores directos o cambios locales de padding, radio o peso tipográfico.
- **Accesibilidad:** foco visible de 2 px, texto descriptivo y área mínima de 32 px.

### XdtTextBox

- **Propósito:** entrada de texto coherente con el tema oscuro.
- **Variantes previstas:** normal, search, password y read-only.
- **Altura:** 40 px.
- **Estados:** normal, focused, disabled, invalid y read-only.
- **Accesibilidad:** etiqueta visible; el placeholder nunca sustituye la etiqueta.

### XdtCard / XdtMetricCard

- **Propósito:** agrupar una responsabilidad o una métrica relacionada.
- **Tipos:** default, metric, progress, status, warning, report y game.
- **Propiedades públicas:** `CardType`, `Content`, `Padding`, `Margin`.
- **Comportamiento:** padding de 24 px, radio de 10 px y elevación nivel 1.
- **No usar:** como decoración vacía o para envolver cada control individual.

### XdtStatusBadge

- **Propósito:** comunicar estado breve mediante texto y color semántico.
- **Variantes:** success, warning, error, pending, offline, completed e in-progress.
- **Altura:** 24 px.
- **Accesibilidad:** nunca depender exclusivamente del color; incluir palabra o símbolo de estado.

### XdtProgressBar

- **Propósito:** mostrar progreso determinado.
- **Altura:** 8 px.
- **Comportamiento:** fondo elevado y progreso accent; animaciones limitadas a 180 ms.
- **Accesibilidad:** acompañar con porcentaje o descripción textual.

### XdtBanner

- **Propósito:** mostrar información contextual persistente dentro de una página.
- **Variantes:** info, warning, success y error.
- **Contenido:** icono, título, descripción y cierre cuando sea descartable.
- **No usar:** para confirmaciones que exijan una decisión inmediata.

### XdtDialog

- **Propósito:** confirmar operaciones críticas o bloquear por seguridad.
- **Contenido obligatorio:** título, descripción específica, acción primaria y cancelar.
- **Usar:** RGH no verificado y operaciones materialmente destructivas.
- **No usar:** navegación, onboarding, ayuda o formularios normales.
- **Accesibilidad:** foco contenido, Escape para cancelar y retorno al control que lo abrió.

### XdtGameCard

- **Propósito:** representar un juego sin recurrir a una tabla extensa.
- **Contenido:** nombre, estado, formato, tamaño, progreso y acciones disponibles.
- **Comportamiento:** la selección actualiza el inspector lateral; no expande la tarjeta.
- **Accesibilidad:** título completo mediante tooltip cuando se trunque.

### XdtInspectorPanel

- **Propósito:** mostrar detalles y acciones de la entidad seleccionada.
- **Ancho:** 360 px.
- **Usar:** juegos, pasos, archivos y transferencias.
- **No usar:** como modal o panel flotante.

### XdtTransferQueue

- **Propósito:** mostrar el archivo activo, estado y progreso entre Local y Xbox.
- **Comportamiento:** ocupa la columna central de FTP y prioriza una transferencia clara sobre un explorador complejo.
- **Accesibilidad:** estado y porcentaje siempre visibles en texto.

### XdtToast

- **Propósito:** feedback no bloqueante.
- **Posición:** esquina inferior derecha.
- **Duración:** cinco segundos, con cierre manual.
- **No usar:** errores que impidan continuar o confirmaciones críticas.

### XdtChecklistItem

- **Propósito:** representar un requisito con título, descripción, nivel, estado, dependencias y notas.
- **Comportamiento:** el estado debe expresarse con icono, texto y color.
- **Accesibilidad:** el control completo puede enfocarse; el check conserva etiqueta explícita.

### XdtEmptyState

- **Propósito:** explicar por qué una región está vacía y cuál es el siguiente paso útil.
- **Contenido:** título corto, descripción y una acción opcional.
- **No usar:** cuando los datos todavía se están cargando; en ese caso se usa estado de progreso.

## Estado de adopción

La versión 1.0 se aplica a la bienvenida, App Shell, Dashboard, FTP, Games, inputs, botones, cards, tablas visuales, diálogos de seguridad y toasts. Las páginas operativas restantes heredan los mismos tokens y estilos globales, por lo que nuevos componentes deben agregarse al sistema antes de emplearse en una vista.
