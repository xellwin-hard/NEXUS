# Integración con SQL Server — NEXUS

**Proyecto:** NEXUS  
**Estudiante:** Axel Erwin Pozo Teran  
**Base de datos:** NEXUS_DB

## Avances confirmados

- Se creó la base de datos local `NEXUS_DB`.
- Se instaló el paquete NuGet `Microsoft.Data.SqlClient` en el proyecto Windows Forms.
- Se creó la clase `ConexionBD.cs` dentro del proyecto `NexusForms`.
- La clase centraliza la apertura de conexiones y las operaciones relacionadas con el registro de jugadores.
- Se probó el registro del nombre del jugador desde la aplicación.
- Se comprobó el registro consultando la tabla `Jugadores` en SQL Server.

## Cadena de conexión usada en el equipo de desarrollo

```text
Server=LAPTOP-C8E51TFM;Database=NEXUS_DB;Integrated Security=True;TrustServerCertificate=True;
```

Esta cadena es específica del equipo local. No debe copiarse sin cambios a otra computadora: el nombre del servidor puede ser diferente.

## Consulta de comprobación

```sql
USE NEXUS_DB;
GO

SELECT * FROM Jugadores;
GO
```

## Funciones descritas de ConexionBD.cs

- `ObtenerConexion()`: prepara una conexión a la base de datos.
- `ProbarConexion()`: permite comprobar si se puede conectar.
- `RegistrarJugador(string nombre)`: registra el nombre del jugador en la tabla `Jugadores`.

## Pendiente de completar o adjuntar

- Incorporar al repositorio el archivo real `ConexionBD.cs`, revisado desde el proyecto local.
- Incorporar el script SQL real de creación de la base de datos y sus tablas, después de verificar el esquema existente.
- Documentar pruebas de nombre vacío y de error de conexión.
- Verificar e implementar, si corresponde, el guardado y la recuperación de partidas.
- Añadir capturas de SQL Server Management Studio que demuestren la conexión y los registros.

> Este documento describe únicamente los avances confirmados. No reemplaza los archivos fuente ni los scripts SQL ejecutables.
