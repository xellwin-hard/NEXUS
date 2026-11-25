namespace NexusV2.Data;

/// <summary>
/// Estado serializable de una campaña. La interfaz debe mapear su estado actual
/// a este DTO antes de invocar PartidaRepository.
/// </summary>
public sealed class PartidaSnapshot
{
    public int IdPartida { get; set; }
    public int IdJugador { get; set; }
    public string NombreOperativo { get; set; } = "";
    public string Clase { get; set; } = "";
    public string Dificultad { get; set; } = "INTERMEDIO";
    public string CodigoSector { get; set; } = "SECTOR-01";
    public string NombreSector { get; set; } = "Sector inicial";
    public int SaludActual { get; set; }
    public int EnergiaActual { get; set; }
    public int Experiencia { get; set; }
    public int Creditos { get; set; }
    public int PosicionX { get; set; }
    public int PosicionY { get; set; }
    public string Estado { get; set; } = "EnCurso";
    public List<ObjetoSnapshot> Inventario { get; set; } = new();
    public List<MisionSnapshot> Misiones { get; set; } = new();
}

public sealed class ObjetoSnapshot
{
    public string Nombre { get; set; } = "";
    public string Tipo { get; set; } = "";
    public int Valor { get; set; }
    public int Cantidad { get; set; } = 1;
    public bool Equipado { get; set; }
}

public sealed class MisionSnapshot
{
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string Estado { get; set; } = "Pendiente";
    public int ObjetivoActual { get; set; }
    public DateTime? FechaCompletada { get; set; }
}
