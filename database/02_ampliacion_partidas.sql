/*
  NEXUS - Ampliación de persistencia
  Ejecutar DESPUÉS de database/NEXUS_DB.sql en SQL Server Management Studio.
  Conserva dbo.Jugadores y agrega las tablas necesarias para guardar campañas.
*/
USE NEXUS_DB;
GO

IF OBJECT_ID(N'dbo.Sectores', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Sectores (
        IdSector INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Sectores PRIMARY KEY,
        Codigo NVARCHAR(30) NOT NULL CONSTRAINT UQ_Sectores_Codigo UNIQUE,
        Nombre NVARCHAR(100) NOT NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.Partidas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Partidas (
        IdPartida INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Partidas PRIMARY KEY,
        IdJugador INT NOT NULL,
        IdSector INT NULL,
        NombreOperativo NVARCHAR(100) NOT NULL,
        Clase NVARCHAR(30) NOT NULL,
        Dificultad NVARCHAR(20) NOT NULL,
        SaludActual INT NOT NULL,
        EnergiaActual INT NOT NULL,
        Experiencia INT NOT NULL CONSTRAINT DF_Partidas_Experiencia DEFAULT (0),
        Creditos INT NOT NULL CONSTRAINT DF_Partidas_Creditos DEFAULT (0),
        PosicionX INT NOT NULL CONSTRAINT DF_Partidas_PosX DEFAULT (0),
        PosicionY INT NOT NULL CONSTRAINT DF_Partidas_PosY DEFAULT (0),
        Estado NVARCHAR(20) NOT NULL CONSTRAINT DF_Partidas_Estado DEFAULT (N'EnCurso'),
        FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_Partidas_FechaCreacion DEFAULT (SYSDATETIME()),
        FechaActualizacion DATETIME2 NOT NULL CONSTRAINT DF_Partidas_FechaActualizacion DEFAULT (SYSDATETIME()),
        CONSTRAINT FK_Partidas_Jugadores FOREIGN KEY (IdJugador) REFERENCES dbo.Jugadores(IdJugador),
        CONSTRAINT FK_Partidas_Sectores FOREIGN KEY (IdSector) REFERENCES dbo.Sectores(IdSector),
        CONSTRAINT CK_Partidas_Salud CHECK (SaludActual >= 0),
        CONSTRAINT CK_Partidas_Energia CHECK (EnergiaActual >= 0),
        CONSTRAINT CK_Partidas_Experiencia CHECK (Experiencia >= 0),
        CONSTRAINT CK_Partidas_Creditos CHECK (Creditos >= 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.CatalogoObjetos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CatalogoObjetos (
        IdObjeto INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CatalogoObjetos PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        Tipo NVARCHAR(30) NOT NULL,
        Valor INT NOT NULL CONSTRAINT DF_CatalogoObjetos_Valor DEFAULT (0),
        CONSTRAINT UQ_CatalogoObjetos_Nombre_Tipo UNIQUE (Nombre, Tipo)
    );
END;
GO

IF OBJECT_ID(N'dbo.InventarioPartida', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.InventarioPartida (
        IdPartida INT NOT NULL,
        IdObjeto INT NOT NULL,
        Cantidad INT NOT NULL CONSTRAINT DF_Inventario_Cantidad DEFAULT (1),
        Equipado BIT NOT NULL CONSTRAINT DF_Inventario_Equipado DEFAULT (0),
        CONSTRAINT PK_InventarioPartida PRIMARY KEY (IdPartida, IdObjeto),
        CONSTRAINT FK_Inventario_Partidas FOREIGN KEY (IdPartida)
            REFERENCES dbo.Partidas(IdPartida) ON DELETE CASCADE,
        CONSTRAINT FK_Inventario_Objetos FOREIGN KEY (IdObjeto)
            REFERENCES dbo.CatalogoObjetos(IdObjeto),
        CONSTRAINT CK_Inventario_Cantidad CHECK (Cantidad > 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.Misiones', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Misiones (
        IdMision INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Misiones PRIMARY KEY,
        Codigo NVARCHAR(30) NOT NULL CONSTRAINT UQ_Misiones_Codigo UNIQUE,
        Nombre NVARCHAR(120) NOT NULL,
        Descripcion NVARCHAR(500) NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.ProgresoMision', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProgresoMision (
        IdPartida INT NOT NULL,
        IdMision INT NOT NULL,
        Estado NVARCHAR(20) NOT NULL CONSTRAINT DF_ProgresoMision_Estado DEFAULT (N'Pendiente'),
        ObjetivoActual INT NOT NULL CONSTRAINT DF_ProgresoMision_Objetivo DEFAULT (0),
        FechaCompletada DATETIME2 NULL,
        CONSTRAINT PK_ProgresoMision PRIMARY KEY (IdPartida, IdMision),
        CONSTRAINT FK_ProgresoMision_Partidas FOREIGN KEY (IdPartida)
            REFERENCES dbo.Partidas(IdPartida) ON DELETE CASCADE,
        CONSTRAINT FK_ProgresoMision_Misiones FOREIGN KEY (IdMision)
            REFERENCES dbo.Misiones(IdMision),
        CONSTRAINT CK_ProgresoMision_Objetivo CHECK (ObjetivoActual >= 0)
    );
END;
GO

-- Verificación rápida del esquema creado.
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = N'dbo'
  AND TABLE_NAME IN
      (N'Jugadores', N'Sectores', N'Partidas', N'CatalogoObjetos',
       N'InventarioPartida', N'Misiones', N'ProgresoMision')
ORDER BY TABLE_NAME;
GO
