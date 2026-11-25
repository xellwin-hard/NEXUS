using Microsoft.Data.SqlClient;

namespace NexusV2.Data;

/// <summary>
/// Crea conexiones a NEXUS_DB sin guardar credenciales en el repositorio.
/// Configurar la variable de entorno NEXUS_SQL_CONNECTION en cada equipo.
/// </summary>
public static class NexusDbConnectionFactory
{
    public static SqlConnection Create()
    {
        string? connectionString =
            Environment.GetEnvironmentVariable("NEXUS_SQL_CONNECTION");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "No se configuró la variable de entorno NEXUS_SQL_CONNECTION.");
        }

        return new SqlConnection(connectionString);
    }

    public static bool ProbarConexion(out string mensaje)
    {
        try
        {
            using SqlConnection connection = Create();
            connection.Open();
            mensaje = "Conexión correcta con NEXUS_DB.";
            return true;
        }
        catch (Exception ex)
        {
            mensaje = "No se pudo conectar con SQL Server: " + ex.Message;
            return false;
        }
    }
}
