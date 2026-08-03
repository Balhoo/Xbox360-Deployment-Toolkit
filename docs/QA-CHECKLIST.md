# Checklist de verificación

## Build

- [ ] Instalar .NET 8 SDK x64.
- [ ] Ejecutar `build.ps1` sin errores.
- [ ] Confirmar `dist/Xbox360DeploymentToolkit.exe` y `dist/Configuration/*.json`.
- [ ] Abrir el EXE en una cuenta estándar de Windows.

## Funcional

- [ ] Marcar pasos, escribir notas, cerrar y confirmar persistencia al reabrir.
- [ ] Confirmar que dry-run está activo en el primer inicio.
- [ ] Simular preparación y verificar que no se crean carpetas.
- [ ] Desactivar dry-run, cancelar el diálogo y verificar que no hay cambios.
- [ ] Confirmar una carpeta temporal y comprobar las seis carpetas esperadas.
- [ ] Conectar a un servidor FTP de prueba y listar `/Hdd1`.
- [ ] Simular carga FTP y después probar una carga pequeña autorizada.
- [ ] Comparar tamaño local/remoto.
- [ ] Modificar una ruta de `games.json` y comprobar la validación.
- [ ] Exportar JSON/CSV y revisar que no haya credenciales.

## Seguridad

- [ ] Revisar que los logs no contienen contraseña.
- [ ] Confirmar que la credencial recordada solo funciona para el mismo usuario de Windows.
- [ ] Probar una raíz inexistente y una ruta fuera de la raíz; deben bloquearse.
- [ ] Confirmar por revisión de código que no existen operaciones de formato o borrado recursivo.
