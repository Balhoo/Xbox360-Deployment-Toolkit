# Seguridad y límites operativos

- El modo dry-run está activo por defecto.
- La app no contiene funciones de formateo, borrado recursivo ni sobrescritura masiva.
- Antes de una creación real de carpetas o una carga FTP se muestra el destino y se pide confirmación.
- Las rutas locales se normalizan y deben permanecer dentro de la raíz elegida.
- Las credenciales opcionales se protegen mediante Windows DPAPI (`CurrentUser`); nunca se escriben en logs o reportes.
- FTP clásico no cifra el tráfico. Úsalo únicamente en una red local confiable y desconecta el servidor al terminar.
- La verificación FTP del MVP compara tamaños. Un hash remoto exigiría soporte adicional en el servidor o descargar nuevamente el archivo.
- Conserva respaldos verificados antes de manipular almacenamiento de una consola.
