# Auditoría de interfaz — Xbox360 Deployment Toolkit

Versión: 2.0  
Alcance: presentación actual WPF frente a la especificación Fluent/Windows 11 proporcionada  
Restricción: esta auditoría no autoriza cambios en servicios, modelos, configuración, FTP, validaciones ni reportes

## Resumen ejecutivo

La aplicación actual es funcional, pero su interfaz refleja el crecimiento incremental del MVP: cada módulo se añadió como una pestaña o ventana con controles WPF estándar. Esto permite operar, aunque no comunica todavía la madurez de una herramienta profesional de deployment.

Los riesgos principales son:

1. La navegación no expresa el flujo ni la relación entre módulos.
2. La densidad de tablas y texto aumenta la carga cognitiva.
3. No existe un Design System interno: color, espaciado, estados y jerarquía se resuelven localmente.
4. Las ventanas no comparten un shell, lenguaje visual o patrón de diálogo coherente.
5. Existen interacciones y cambios visuales en code-behind, lo que dificulta probar y escalar la presentación.
6. La interfaz clara actual contradice el tema oscuro solicitado y contiene colores literales que no responden a temas.

La evolución recomendada conserva los bindings, comandos, servicios y estructuras de datos, y reemplaza únicamente la capa de presentación por un `AppShell` con navegación lateral, páginas, tarjetas e inspector contextual.

## Inventario actual

### Ventanas

| Vista | Responsabilidad actual | Observación |
|---|---|---|
| `MainWindow` | Preparación, procedimiento, unidades, FTP, juegos y auditoría | Seis módulos heterogéneos dentro de un único `TabControl` |
| `DeploymentWizardWindow` | Onboarding y creación del plan | Siete pestañas visibles que funcionan como pasos de un instalador |
| `VerificationGateWindow` | Decisión de verificación o riesgo | Diálogo específico, visualmente separado del resto |

### Recursos globales

`App.xaml` contiene estilos básicos para `Button`, `TextBox`, `ComboBox` y `GroupBox`. No hay tokens semánticos, tipografía global, iconografía, estados, elevación, focus visual ni variantes reutilizables.

### Patrones de contenido

- Cuatro `DataGrid` principales en `MainWindow`.
- Dos `DataGrid` adicionales dentro del wizard.
- Pestañas utilizadas tanto para navegación principal como para progreso secuencial.
- Mensajes informativos implementados como `TextBlock` con color literal.
- Confirmaciones repartidas entre `MessageBox` y una ventana personalizada.
- Estado global limitado a texto y una barra de progreso inferior.

## Hallazgos por categoría

### 1. Arquitectura de navegación

Severidad: alta.

- `TabControl` presenta Preparación, Procedimiento, HDD/USB, FTP, Juegos y Auditoría al mismo nivel, aunque pertenecen a grupos conceptuales distintos.
- No existe Dashboard ni resumen de sesión.
- El usuario pierde el contexto al cambiar de pestaña; estado de consola, FTP, dry-run y advertencias no permanecen visibles de forma consistente.
- El onboarding muestra las pestañas Bienvenida, Consola, Alcance, Almacenamiento, Componentes, Juegos y Resumen, permitiendo navegación visual que no se siente como instalador bloqueado por pasos.
- No hay breadcrumbs, encabezado de página ni acciones primarias estables.

Recomendación: `AppShell` persistente con sidebar agrupada y `Frame/ContentControl` para páginas; wizard separado con indicador de pasos y sin pestañas visibles.

### 2. Jerarquía visual

Severidad: alta.

- La cabecera oscura contrasta con superficies claras, pero no existe una jerarquía intermedia entre ventana, módulo y entidad.
- Títulos, acciones, estado y contenido compiten dentro de filas horizontales.
- Las advertencias aparecen como texto rojo aislado, sin icono, título ni contenedor semántico.
- Las tablas convierten descripciones, instrucciones, advertencias y notas en columnas equivalentes, aunque su importancia es diferente.
- La barra inferior mezcla estado textual y progreso de transferencia sin explicar su alcance.

Recomendación: PageHeader, Cards, StatusBadge, WarningBanner, ProgressCard e InspectorPanel.

### 3. Densidad y legibilidad

Severidad: alta.

- Preparación contiene ocho columnas; varias incluyen frases largas.
- Procedimiento contiene instrucciones, advertencias y notas en una misma fila.
- FTP mezcla credenciales, ruta, tabla remota, archivo local y acciones en una sola página lineal.
- Juegos se presenta como tabla editable, sin una separación clara entre identidad, validación, estado y acciones.
- Las rutas se muestran en `TextBox` de ancho fijo; no hay componente de ruta con ellipsis, tooltip y acción copiar/abrir.

Recomendación: mostrar resúmenes en cards/list items y llevar detalles largos al inspector derecho.

### 4. Consistencia visual

Severidad: alta.

- Se usan fondos `#F5F5F5`, `#F7F7F7`, blanco, `#151515`, `#FFF4CE`, `#E8F3FF`, `MistyRose`, `Honeydew` y `LemonChiffon` sin tokens semánticos.
- Márgenes de 4, 5, 6, 10, 14, 15, 18 y 20 px rompen el sistema solicitado de múltiplos definidos.
- Botones primarios se distinguen mediante colores aplicados directamente en cada vista.
- No existe radio de borde consistente; la mayoría de controles conserva apariencia WPF clásica.
- El wizard y el diálogo de verificación no comparten plantillas de encabezado, footer o banner.

Recomendación: diccionarios de recursos por tokens, estilos base y variantes semánticas.

### 5. Tipografía

Severidad: media.

- No se define Segoe UI Variable globalmente.
- Hay tamaños locales de 14, 15, 21, 24, 25 y 28 sin escala tipográfica documentada.
- El resumen utiliza `Consolas` en lugar del fallback solicitado `Cascadia Mono`.
- No hay estilos para Display, Title, Subtitle, Body, Caption y Code.

Recomendación: escala tipográfica de seis roles, dos familias máximas.

### 6. Estados y feedback

Severidad: alta.

- Los estados de checklist dependen principalmente de checkbox y texto.
- Estado FTP no permanece visible fuera de su pestaña.
- Dry-run se muestra como checkbox en la cabecera, pero no como estado semántico persistente con explicación.
- Los errores async terminan en `MessageBox`; no existe Toast ni centro de actividad.
- La auditoría guarda eventos, pero no se reutiliza como actividad reciente en Dashboard.
- No hay estados vacíos, loading/skeleton, retry inline o cancelación visible.

Recomendación: `StatusBadge` con icono+texto+color, ToastHost y banners persistentes.

### 7. Prevención de errores

Severidad: media-alta.

Fortalezas existentes:

- Dry-run activo por defecto.
- Confirmaciones antes de crear carpetas o transferir.
- Diálogo específico para RGH sin verificar.
- Credenciales protegidas con DPAPI.

Problemas de presentación:

- Las confirmaciones no siguen una plantilla única.
- El diálogo de creación de biblioteca no muestra una ficha estructurada de destino, cambios y resultado.
- `MessageBox` no admite explicación progresiva ni confirmación explícita del objetivo.
- No se diferencia visualmente entre acción segura, reversible, externa y crítica.

Recomendación: `ConfirmationDialog` reutilizable con resumen de impacto; mantener intactas las salvaguardas de servicios.

### 8. Accesibilidad

Severidad: alta.

- No hay focus visual personalizado ni documentación del orden de tabulación.
- Los estados no incluyen iconografía consistente.
- Algunos textos de color sobre fondos claros dependen del color para expresar significado.
- Las columnas densas afectan ampliación de texto y lectura con zoom.
- No hay `AutomationProperties.Name/HelpText` en acciones relevantes.
- Los iconos aún no existen, por lo que tampoco hay etiquetas accesibles.
- Ventanas con tamaños fijos o `ResizeMode="NoResize"` limitan escalado y accesibilidad.

Recomendación: contraste AA, icono+texto, focus de 2 px, targets mínimos de 32–40 px y validación al 100/125/150/200 %.

### 9. Responsive y scroll

Severidad: alta.

- `MainWindow` tiene mínimo de 900 px, pero múltiples anchos fijos pueden desbordar.
- `WrapPanel` cambia la distribución de forma impredecible.
- El wizard combina páginas con y sin `ScrollViewer`.
- No hay estrategia de colapso para sidebar, inspector o tarjetas.
- Las tablas dependen de scroll propio; un rediseño debe evitar scroll vertical anidado.

Recomendación: breakpoints lógicos `Compact`, `Standard`, `Wide`; sidebar 64/240, inspector 0/320 y un solo scroll principal por página.

### 10. Mantenibilidad de presentación

Severidad: alta.

- Las vistas principales están comprimidas en muy pocas líneas con árboles XAML extensos.
- Existen eventos `Click` y manipulación visual directa en `DeploymentWizardWindow.xaml.cs`.
- Colores de banners se cambian desde code-behind.
- Apertura de archivos, fuentes, navegación y composición del resumen viven en la vista.
- No hay `UserControl` reutilizable para cards, banners, filas de checklist o rutas.

Recomendación: mover estado de presentación a ViewModels sin alterar servicios o modelos de dominio; la vista debe limitarse a bindings, triggers y behaviors.

## Componentes redundantes o candidatos a consolidación

| Patrón actual | Problema | Componente futuro |
|---|---|---|
| Headers oscuros separados | Implementaciones distintas | `PageHeader` / `WizardHeader` |
| Texto amarillo/rojo/azul en `Border` | Semántica inconsistente | `WarningBanner` / `InfoBanner` |
| Botones con color inline | Variantes duplicadas | `PrimaryButton`, `SecondaryButton`, `DangerButton` |
| TextBox de rutas | Sin truncado ni acciones | `FilePath` |
| ProgressBar + TextBlock | Repetición y contexto débil | `ProgressCard` |
| Checkbox + estado | Sin icono/estado compuesto | `ChecklistItem` |
| MessageBox | Información insuficiente | `ConfirmationDialog` / `Toast` |
| Tablas de juegos | Demasiada densidad | `GameCard` + `InspectorPanel` |
| DataGrid FTP | Solo un lado remoto | `DualPaneFileBrowser` + `TransferQueue` |

## Aspectos valiosos que deben conservarse

- Dry-run visible y activo por defecto.
- Persistencia de progreso y perfil.
- Auditoría y logs.
- Confirmación RGH por software o decisión explícita de riesgo.
- Separación de configuración JSON.
- Comandos y servicios actuales.
- Proceso self-contained y offline-first.
- Lenguaje claro sobre contenido aportado por el usuario.

## Criterios de aceptación del futuro rediseño

1. Ningún comando, servicio o resultado funcional desaparece.
2. Las seis áreas actuales siguen siendo accesibles mediante la nueva navegación.
3. El wizard conserva sus decisiones y bloqueos.
4. Toda acción existente puede completarse con teclado.
5. Ninguna página tiene más de un scroll vertical principal.
6. No hay colores, márgenes o tipografías literales fuera del Design System salvo casos documentados.
7. Estado no se comunica únicamente por color.
8. La interfaz funciona a 900×620 y escala correctamente en 125/150/200 %.
9. FTP, checklist y juegos adoptan layouts específicos sin cambiar sus contratos de datos.
10. Build, persistencia y reportes producen los mismos resultados que antes.

