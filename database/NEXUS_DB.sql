/*
  NEXUS - Script inicial de SQL Server
  Base de datos: NEXUS_DB

  Este script crea la base de datos y una tabla básica de jugadores
  si todavía no existen. Está basado en el uso confirmado de la tabla
  Jugadores y del campo Nombre desde ConexionBD.cs.

  Ejecutar desde SQL Server Management Studio.
*/

IF DB_ID(N'NEXUS_DB') IS NULL
BEGIN
    CREATE DATABASE NEXUS_DB;
END;
GO

USE NEXUS_DB;
GO

IF OBJECT_ID(N'dbo.Jugadores', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Jugadores
    (
        IdJugador INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL
    );
END;
GO

-- Verificar la tabla y sus columnas.
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo'
  AND TABLE_NAME = 'Jugadores'
ORDER BY ORDINAL_POSITION;
GO

-- Consultar los jugadores registrados.
SELECT * FROM dbo.Jugadores;
GO
