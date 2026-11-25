# NEXUS — Videojuego de exploración y combate 2D

NEXUS es un proyecto académico en C# que evoluciona desde una aventura de exploración por consola hacia un videojuego de escritorio con interfaz gráfica. La propuesta conserva los personajes, recursos, inventario, eventos y misión del Núcleo Energético desarrollados en H3.

## Estado del proyecto

- **H3:** versión de consola con clases `Personaje`, `Cadete`, `Enemigo`, `Robot` y `Objeto`; exploración en mapa lógico 5 × 5, inventario, encuentros y misión principal.
- **Evolución gráfica:** el repositorio contiene `Form1.cs`, un prototipo de Windows Forms con pantallas de introducción, registro, hangar, selección de mapas y arena 2D; incluye animación, HUD, controles, clases, armas ficticias, enemigos y jefes.
- **SQL Server:** además de `Jugadores`, se añadió `database/02_ampliacion_partidas.sql` para partidas, sectores, catálogo de objetos, inventario y progreso de misiones. Se agregó `Microsoft.Data.SqlClient`, una fábrica de conexión por variable de entorno y un repositorio ADO.NET para guardar/recuperar instantáneas.
- **Integración WinForms:** el proyecto ya está configurado como aplicación Windows Forms, el punto de entrada inicia `Form1` y la solución apunta al proyecto correcto. Se añadió una compilación automática para Windows mediante GitHub Actions.
- **Pendiente de prueba local:** ejecutar los dos scripts SQL, configurar `NEXUS_SQL_CONNECTION`, probar el repositorio contra una instancia real y conectar el DTO `PartidaSnapshot` con el estado privado del formulario. La capa de persistencia está creada, pero todavía no está enlazada a los botones del juego.

> El estado de los archivos del repositorio debe distinguirse de las funciones propuestas. El código gráfico todavía requiere verificar la configuración de proyecto y los archivos parciales antes de considerarlo una aplicación WinForms autónoma. El guardado completo de partidas no está confirmado.

## Tecnologías

- C# y .NET (el proyecto actual declara `net10.0`).
- Windows Forms y `System.Drawing` en el prototipo gráfico.
- SQL Server para el registro de jugadores y futura persistencia.
- `Microsoft.Data.SqlClient` documentado como proveedor usado durante el avance H4.

## Estructura principal

```text
NEXUS/
├── Program.cs
├── Form1.cs
├── NEXUS.csproj
├── NEXUS.slnx
├── database/
│   └── NEXUS_DB.sql
└── docs/
    ├── diseno-del-juego.md
    ├── arquitectura-actual.md
    ├── arquitectura-propuesta.md
    ├── modelo-de-datos-propuesto.md
    ├── analisis-tecnico.md
    ├── AVANCES_HITO_4.md
    ├── SQL_SERVER.md
    └── evidencias/
```

## Cómo revisar y ejecutar

1. Clona el repositorio: `git clone https://github.com/xellwin-hard/NEXUS.git`.
2. Abre `NEXUS.slnx` o `NEXUS.csproj` con una versión de Visual Studio compatible con el SDK .NET instalado.
3. El proyecto está configurado para Windows Forms sobre `net10.0-windows`. Abre la solución y compila en Windows con .NET 10 SDK. La interfaz construye sus controles desde `Form1.cs`, por lo que no depende de un archivo Designer.
4. Configura SQL Server local y ejecuta `database/NEXUS_DB.sql` desde SQL Server Management Studio para crear la base y la tabla inicial de jugadores.
5. Ajusta la cadena de conexión en la configuración local, nunca con credenciales o nombres de servidor personales dentro del repositorio.
6. Configura la variable de entorno `NEXUS_SQL_CONNECTION` con una cadena de conexión local a `NEXUS_DB`. No publiques esa cadena ni credenciales.\n7. Para ampliar el esquema, ejecuta primero `database/NEXUS_DB.sql` y después `database/02_ampliacion_partidas.sql` en SQL Server Management Studio.\n8. Compila con `dotnet build NEXUS.slnx --configuration Release` y ejecuta desde Visual Studio o con `dotnet run --project NEXUS.csproj`. La compilación automática se puede consultar en Actions.

## Documentación

- [Diseño del videojuego](docs/diseno-del-juego.md)
- [Arquitectura actual](docs/arquitectura-actual.md)
- [Arquitectura propuesta](docs/arquitectura-propuesta.md)
- [Modelo de datos propuesto](docs/modelo-de-datos-propuesto.md)
- [Análisis técnico](docs/analisis-tecnico.md)
- [Bocetos y diagramas](docs/evidencias/)\n- [Guía de capturas reales](docs/evidencias/README.md)

## Base de datos

El script inicial `database/NEXUS_DB.sql` crea `NEXUS_DB` y `dbo.Jugadores`. El script `database/02_ampliacion_partidas.sql` agrega las tablas de persistencia. `Data/JugadorRepository.cs` registra/consulta jugadores y `Data/PartidaRepository.cs` implementa guardado transaccional y recuperación de instantáneas. Todavía debe probarse en una instancia local y conectarse al estado real de la interfaz.

## Autor

Axel Erwin Pozo Teran

## Repositorio

https://github.com/xellwin-hard/NEXUS
