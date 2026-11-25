using System.Data;
using Microsoft.Data.SqlClient;

namespace NexusV2.Data;

/// <summary>
/// Registra y consulta el perfil de jugador utilizado por las partidas.
/// </summary>
public sealed class JugadorRepository
{
    public int ObtenerOCrear(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del jugador es obligatorio.", nameof(nombre));

        string nombreLimpio = nombre.Trim();

        using SqlConnection connection = NexusDbConnectionFactory.Create();
        connection.Open();

        using SqlCommand command = new("""
            DECLARE @IdJugador INT;
            SELECT @IdJugador = IdJugador
            FROM dbo.Jugadores
            WHERE Nombre = @Nombre;

            IF @IdJugador IS NULL
            BEGIN
                INSERT INTO dbo.Jugadores (Nombre) VALUES (@Nombre);
                SET @IdJugador = CAST(SCOPE_IDENTITY() AS INT);
            END;

            SELECT @IdJugador;
            """, connection);

        command.Parameters.Add("@Nombre", SqlDbType.NVarChar, 100).Value = nombreLimpio;
        return Convert.ToInt32(command.ExecuteScalar());
    }
}
