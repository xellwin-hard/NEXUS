# NEXUS — Avances del Hito 4

**Estudiante:** Axel Erwin Pozo Teran  
**Asignatura:** Programación II  
**Repositorio:** https://github.com/xellwin-hard/NEXUS  
**Fecha:** 23 de noviembre de 2026

## 1. Estado general

Este documento registra los avances confirmados del proyecto para el Hito 4. Se separan los trabajos comprobados de las tareas que todavía requieren ejecución o evidencia.

## 2. Avances confirmados

- Se integró el videojuego con una interfaz desarrollada en C# mediante Windows Forms.
- Se trabajó en mejoras visuales para que el juego tenga una presentación más cercana a un videojuego completo.
- Se ajustaron los enfrentamientos contra jefes y se planteó que la dificultad máxima incluya tres jefes.
- Se creó la clase `ConexionBD.cs` en el proyecto `NexusForms`.
- Se incorporó el paquete NuGet `Microsoft.Data.SqlClient`.
- Se configuró la conexión local con SQL Server hacia la base de datos `NEXUS_DB`.
- Se probó el registro del nombre del jugador en la tabla `Jugadores` y se comprobó el registro mediante una consulta SQL.
- Se preparó la presentación de arquitectura correspondiente a la actividad 1.

## 3. Configuración de base de datos usada en desarrollo

La cadena de conexión local utilizada fue:

```text
Server=LAPTOP-C8E51TFM;Database=NEXUS_DB;Integrated Security=True;TrustServerCertificate=True;
```

Esta configuración corresponde al equipo de desarrollo y debe adaptarse si el proyecto se ejecuta en otra computadora.

Consulta utilizada para verificar los registros:

```sql
USE NEXUS_DB;
SELECT * FROM Jugadores;
```

## 4. Actividades del Hito 4

| Actividad | Estado registrado |
|---|---|
| 1. Presentación de arquitectura | Realizada según lo confirmado durante el desarrollo. |
| 2. Reto C# y control de versiones | Parcial: se confirmó el registro de jugadores. Falta adjuntar evidencia del cambio final y su commit. |
| 3. Clonación limpia | Pendiente de evidencia de clonación, compilación y ejecución desde una carpeta nueva. |
| 4. Integración y persistencia | Parcial: el registro en `Jugadores` fue probado. No se ha confirmado aquí la recuperación completa de partidas. |
| 5. Publicación del ejecutable | Pendiente de verificar una GitHub Release y su ejecución fuera de Visual Studio. |
| 6. Testing y debugging | Pendiente de adjuntar rama, casos de prueba, correcciones y resultados. |

## 5. Mejora recomendada para validar el nombre

Se recomienda validar que el nombre no esté vacío antes de ejecutar el INSERT. Este fragmento es una propuesta y debe incorporarse y probarse en el código fuente antes de considerarse implementado:

```csharp
if (string.IsNullOrWhiteSpace(nombre))
{
    throw new ArgumentException("El nombre del jugador no puede estar vacío.");
}

nombre = nombre.Trim();
```

## 6. Evidencias que deben añadirse

- Presentación PDF de la arquitectura.
- Capturas de la interfaz y de las mejoras del videojuego.
- Captura del código de `ConexionBD.cs`.
- Captura de la tabla `Jugadores` con el registro realizado.
- Código antes y después de la mejora de validación y enlace al commit correspondiente.
- Evidencia de clonación limpia y ejecución.
- Evidencia de guardado y recuperación de partidas, si esa función está implementada.
- Enlace a la Release y captura de ejecución del paquete publicado.
- Pruebas de entradas inválidas y de fallo de conexión SQL Server.
- Reporte exportado de ActivityWatch solicitado por la evaluación.

## 7. Nota de mantenimiento

Este archivo documenta el estado conocido del proyecto a la fecha indicada. No afirma que las tareas pendientes se hayan completado. Actualizar esta tabla cuando se ejecuten las pruebas y se incorporen las evidencias reales.
