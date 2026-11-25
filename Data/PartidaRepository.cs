using System.Data;
using Microsoft.Data.SqlClient;

namespace NexusV2.Data;

/// <summary>
/// Guarda y recupera una instantánea de campaña. Requiere ejecutar
/// database/02_ampliacion_partidas.sql previamente.
/// </summary>
public sealed class PartidaRepository
{
    public int Guardar(PartidaSnapshot partida)
    {
        ArgumentNullException.ThrowIfNull(partida);
        if (partida.IdJugador <= 0)
            throw new ArgumentException("IdJugador debe ser mayor que cero.");
        if (string.IsNullOrWhiteSpace(partida.NombreOperativo))
            throw new ArgumentException("El nombre del operativo es obligatorio.");

        using SqlConnection connection = NexusDbConnectionFactory.Create();
        connection.Open();
        using SqlTransaction transaction = connection.BeginTransaction();

        try
        {
            int idSector = ObtenerOCrearSector(connection, transaction,
                partida.CodigoSector, partida.NombreSector);

            using (SqlCommand command = new("""
                IF @IdPartida > 0 AND EXISTS
                    (SELECT 1 FROM dbo.Partidas WHERE IdPartida = @IdPartida)
                BEGIN
                    UPDATE dbo.Partidas
                    SET IdJugador=@IdJugador, IdSector=@IdSector,
                        NombreOperativo=@NombreOperativo, Clase=@Clase,
                        Dificultad=@Dificultad, SaludActual=@Salud,
                        EnergiaActual=@Energia, Experiencia=@Experiencia,
                        Creditos=@Creditos, PosicionX=@PosX, PosicionY=@PosY,
                        Estado=@Estado, FechaActualizacion=SYSDATETIME()
                    WHERE IdPartida=@IdPartida;
                    SELECT @IdPartida;
                END
                ELSE
                BEGIN
                    INSERT INTO dbo.Partidas
                        (IdJugador, IdSector, NombreOperativo, Clase, Dificultad,
                         SaludActual, EnergiaActual, Experiencia, Creditos,
                         PosicionX, PosicionY, Estado)
                    VALUES
                        (@IdJugador, @IdSector, @NombreOperativo, @Clase, @Dificultad,
                         @Salud, @Energia, @Experiencia, @Creditos,
                         @PosX, @PosY, @Estado);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);
                END
                """, connection, transaction))
            {
                command.Parameters.Add("@IdPartida", SqlDbType.Int).Value = partida.IdPartida;
                command.Parameters.Add("@IdJugador", SqlDbType.Int).Value = partida.IdJugador;
                command.Parameters.Add("@IdSector", SqlDbType.Int).Value = idSector;
                command.Parameters.Add("@NombreOperativo", SqlDbType.NVarChar, 100).Value = partida.NombreOperativo;
                command.Parameters.Add("@Clase", SqlDbType.NVarChar, 30).Value = partida.Clase;
                command.Parameters.Add("@Dificultad", SqlDbType.NVarChar, 20).Value = partida.Dificultad;
                command.Parameters.Add("@Salud", SqlDbType.Int).Value = partida.SaludActual;
                command.Parameters.Add("@Energia", SqlDbType.Int).Value = partida.EnergiaActual;
                command.Parameters.Add("@Experiencia", SqlDbType.Int).Value = partida.Experiencia;
                command.Parameters.Add("@Creditos", SqlDbType.Int).Value = partida.Creditos;
                command.Parameters.Add("@PosX", SqlDbType.Int).Value = partida.PosicionX;
                command.Parameters.Add("@PosY", SqlDbType.Int).Value = partida.PosicionY;
                command.Parameters.Add("@Estado", SqlDbType.NVarChar, 20).Value = partida.Estado;
                partida.IdPartida = Convert.ToInt32(command.ExecuteScalar());
            }

            using (SqlCommand delete = new(
                "DELETE FROM dbo.InventarioPartida WHERE IdPartida=@IdPartida;",
                connection, transaction))
            {
                delete.Parameters.Add("@IdPartida", SqlDbType.Int).Value = partida.IdPartida;
                delete.ExecuteNonQuery();
            }

            foreach (ObjetoSnapshot item in partida.Inventario)
            {
                int idObjeto = ObtenerOCrearObjeto(connection, transaction, item);
                using SqlCommand insert = new("""
                    INSERT INTO dbo.InventarioPartida
                        (IdPartida, IdObjeto, Cantidad, Equipado)
                    VALUES (@IdPartida, @IdObjeto, @Cantidad, @Equipado);
                    """, connection, transaction);
                insert.Parameters.Add("@IdPartida", SqlDbType.Int).Value = partida.IdPartida;
                insert.Parameters.Add("@IdObjeto", SqlDbType.Int).Value = idObjeto;
                insert.Parameters.Add("@Cantidad", SqlDbType.Int).Value = Math.Max(1, item.Cantidad);
                insert.Parameters.Add("@Equipado", SqlDbType.Bit).Value = item.Equipado;
                insert.ExecuteNonQuery();
            }

            using (SqlCommand delete = new(
                "DELETE FROM dbo.ProgresoMision WHERE IdPartida=@IdPartida;",
                connection, transaction))
            {
                delete.Parameters.Add("@IdPartida", SqlDbType.Int).Value = partida.IdPartida;
                delete.ExecuteNonQuery();
            }

            foreach (MisionSnapshot mision in partida.Misiones)
            {
                int idMision = ObtenerOCrearMision(connection, transaction, mision);
                using SqlCommand insert = new("""
                    INSERT INTO dbo.ProgresoMision
                        (IdPartida, IdMision, Estado, ObjetivoActual, FechaCompletada)
                    VALUES (@IdPartida, @IdMision, @Estado, @Objetivo, @Fecha);
                    """, connection, transaction);
                insert.Parameters.Add("@IdPartida", SqlDbType.Int).Value = partida.IdPartida;
                insert.Parameters.Add("@IdMision", SqlDbType.Int).Value = idMision;
                insert.Parameters.Add("@Estado", SqlDbType.NVarChar, 20).Value = mision.Estado;
                insert.Parameters.Add("@Objetivo", SqlDbType.Int).Value = mision.ObjetivoActual;
                insert.Parameters.Add("@Fecha", SqlDbType.DateTime2).Value =
                    mision.FechaCompletada.HasValue ? mision.FechaCompletada.Value : DBNull.Value;
                insert.ExecuteNonQuery();
            }

            transaction.Commit();
            return partida.IdPartida;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public PartidaSnapshot? Obtener(int idPartida)
    {
        using SqlConnection connection = NexusDbConnectionFactory.Create();
        connection.Open();

        PartidaSnapshot? partida;
        using (SqlCommand command = new("""
            SELECT p.IdPartida, p.IdJugador, p.NombreOperativo, p.Clase,
                   p.Dificultad, s.Codigo, s.Nombre, p.SaludActual,
                   p.EnergiaActual, p.Experiencia, p.Creditos,
                   p.PosicionX, p.PosicionY, p.Estado
            FROM dbo.Partidas p
            LEFT JOIN dbo.Sectores s ON s.IdSector=p.IdSector
            WHERE p.IdPartida=@IdPartida;
            """, connection))
        {
            command.Parameters.Add("@IdPartida", SqlDbType.Int).Value = idPartida;
            using SqlDataReader reader = command.ExecuteReader();
            if (!reader.Read()) return null;
            partida = new PartidaSnapshot
            {
                IdPartida = reader.GetInt32(0),
                IdJugador = reader.GetInt32(1),
                NombreOperativo = reader.GetString(2),
                Clase = reader.GetString(3),
                Dificultad = reader.GetString(4),
                CodigoSector = reader.IsDBNull(5) ? "SECTOR-01" : reader.GetString(5),
                NombreSector = reader.IsDBNull(6) ? "Sector inicial" : reader.GetString(6),
                SaludActual = reader.GetInt32(7),
                EnergiaActual = reader.GetInt32(8),
                Experiencia = reader.GetInt32(9),
                Creditos = reader.GetInt32(10),
                PosicionX = reader.GetInt32(11),
                PosicionY = reader.GetInt32(12),
                Estado = reader.GetString(13)
            };
        }

        using (SqlCommand command = new("""
            SELECT o.Nombre, o.Tipo, o.Valor, i.Cantidad, i.Equipado
            FROM dbo.InventarioPartida i
            INNER JOIN dbo.CatalogoObjetos o ON o.IdObjeto=i.IdObjeto
            WHERE i.IdPartida=@IdPartida;
            """, connection))
        {
            command.Parameters.Add("@IdPartida", SqlDbType.Int).Value = idPartida;
            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
                partida.Inventario.Add(new ObjetoSnapshot
                {
                    Nombre = reader.GetString(0),
                    Tipo = reader.GetString(1),
                    Valor = reader.GetInt32(2),
                    Cantidad = reader.GetInt32(3),
                    Equipado = reader.GetBoolean(4)
                });
        }

        using (SqlCommand command = new("""
            SELECT m.Codigo, m.Nombre, m.Descripcion, pm.Estado,
                   pm.ObjetivoActual, pm.FechaCompletada
            FROM dbo.ProgresoMision pm
            INNER JOIN dbo.Misiones m ON m.IdMision=pm.IdMision
            WHERE pm.IdPartida=@IdPartida;
            """, connection))
        {
            command.Parameters.Add("@IdPartida", SqlDbType.Int).Value = idPartida;
            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
                partida.Misiones.Add(new MisionSnapshot
                {
                    Codigo = reader.GetString(0),
                    Nombre = reader.GetString(1),
                    Descripcion = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Estado = reader.GetString(3),
                    ObjetivoActual = reader.GetInt32(4),
                    FechaCompletada = reader.IsDBNull(5) ? null : reader.GetDateTime(5)
                });
        }

        return partida;
    }

    private static int ObtenerOCrearSector(SqlConnection c, SqlTransaction t, string codigo, string nombre)
    {
        using SqlCommand cmd = new("""
            IF NOT EXISTS (SELECT 1 FROM dbo.Sectores WHERE Codigo=@Codigo)
                INSERT INTO dbo.Sectores (Codigo, Nombre) VALUES (@Codigo, @Nombre);
            ELSE
                UPDATE dbo.Sectores SET Nombre=@Nombre WHERE Codigo=@Codigo;
            SELECT IdSector FROM dbo.Sectores WHERE Codigo=@Codigo;
            """, c, t);
        cmd.Parameters.Add("@Codigo", SqlDbType.NVarChar, 30).Value = codigo;
        cmd.Parameters.Add("@Nombre", SqlDbType.NVarChar, 100).Value = nombre;
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    private static int ObtenerOCrearObjeto(SqlConnection c, SqlTransaction t, ObjetoSnapshot item)
    {
        using SqlCommand cmd = new("""
            IF NOT EXISTS (SELECT 1 FROM dbo.CatalogoObjetos WHERE Nombre=@Nombre AND Tipo=@Tipo)
                INSERT INTO dbo.CatalogoObjetos (Nombre, Tipo, Valor) VALUES (@Nombre, @Tipo, @Valor);
            ELSE
                UPDATE dbo.CatalogoObjetos SET Valor=@Valor WHERE Nombre=@Nombre AND Tipo=@Tipo;
            SELECT IdObjeto FROM dbo.CatalogoObjetos WHERE Nombre=@Nombre AND Tipo=@Tipo;
            """, c, t);
        cmd.Parameters.Add("@Nombre", SqlDbType.NVarChar, 100).Value = item.Nombre;
        cmd.Parameters.Add("@Tipo", SqlDbType.NVarChar, 30).Value = item.Tipo;
        cmd.Parameters.Add("@Valor", SqlDbType.Int).Value = item.Valor;
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    private static int ObtenerOCrearMision(SqlConnection c, SqlTransaction t, MisionSnapshot mision)
    {
        using SqlCommand cmd = new("""
            IF NOT EXISTS (SELECT 1 FROM dbo.Misiones WHERE Codigo=@Codigo)
                INSERT INTO dbo.Misiones (Codigo, Nombre, Descripcion)
                VALUES (@Codigo, @Nombre, @Descripcion);
            ELSE
                UPDATE dbo.Misiones SET Nombre=@Nombre, Descripcion=@Descripcion
                WHERE Codigo=@Codigo;
            SELECT IdMision FROM dbo.Misiones WHERE Codigo=@Codigo;
            """, c, t);
        cmd.Parameters.Add("@Codigo", SqlDbType.NVarChar, 30).Value = mision.Codigo;
        cmd.Parameters.Add("@Nombre", SqlDbType.NVarChar, 120).Value = mision.Nombre;
        cmd.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 500).Value =
            string.IsNullOrWhiteSpace(mision.Descripcion) ? DBNull.Value : mision.Descripcion;
        return Convert.ToInt32(cmd.ExecuteScalar());
    }
}
