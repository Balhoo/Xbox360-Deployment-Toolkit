# Flujos de deployment v0.4

## Principio general

El deployment comienza únicamente después de completar Preparación. Esta fase reúne la auditoría, el respaldo, la selección y organización de almacenamiento, los paquetes aportados por el usuario y el contenido seleccionado. XDT no formatea unidades ni descarga juegos, DLC, firmware o paquetes.

## Instalación limpia

1. Conectar y validar acceso por USB o FTP sin escribir todavía.
2. Instalar el gestor de archivos inicial aportado por el usuario.
3. Copiar Aurora y comprobar su inicio manual.
4. Configurar rutas y escaneo de contenido.
5. Configurar DashLaunch con una copia recuperable de `launch.ini`.
6. Transferir y validar los juegos Xbox 360 seleccionados, incluidos discos y DLC cuando apliquen.
7. Ejecutar los flujos opcionales de Xbox clásico y emuladores solo si fueron seleccionados.
8. Realizar pruebas finales y exportar el reporte.

## Instalación existente, reparación o migración

1. Revalidar dashboard, kernel, rutas, plugins, espacio y FTP contra la auditoría inicial.
2. Respaldar configuración, bases de datos, carátulas y datos que deban conservarse.
3. Reparar o actualizar Aurora sustituyendo únicamente lo necesario.
4. Conciliar DashLaunch, plugins, ruta de inicio y rutas de escaneo.
5. Integrar los títulos nuevos y validar multidisco, DLC y espacio disponible.
6. Ejecutar pruebas de regresión y documentar cada cambio.

## Estados lineales

- **Pendiente:** todavía está bloqueado por el paso anterior.
- **En proceso:** es el único paso que puede completarse.
- **Completado:** fue confirmado por el usuario y queda guardado automáticamente.

El progreso y las notas se almacenan en cada cambio; no existe un botón de guardado obligatorio.
