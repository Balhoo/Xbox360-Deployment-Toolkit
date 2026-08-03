# Presentación para portafolio

## Problema

Los despliegues manuales mezclan checklist, organización de archivos, FTP y comprobaciones sin una fuente de verdad.

## Solución

Una aplicación Windows offline-first con arquitectura MVVM, configuración externa, persistencia local, operaciones simulables, confirmaciones explícitas, credenciales DPAPI, auditoría y reportes portables.

## Decisiones destacables

- WPF + .NET 8 para integración nativa con Windows y distribución single-file.
- Sin paquetes NuGet de terceros: superficie de suministro reducida y build simple.
- Servicios aislados de la vista para evolucionar FTP, pruebas o una futura UI.
- Manifiestos de juegos declarativos: el código no conoce contenido protegido.
- El MVP evita automatizar operaciones irreversibles.

## Próximas iteraciones

- Pruebas unitarias con abstracciones de filesystem/FTP.
- Navegación FTP jerárquica, colas, cancelación y reanudación.
- Verificación SHA-256 cuando el servidor remoto lo permita.
- Dependencias visuales entre pasos y evidencias adjuntas.
- Exportación HTML/PDF y firma del ejecutable.
