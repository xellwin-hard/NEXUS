# NEXUS — Videojuego de exploración y combate 2D

NEXUS es un proyecto académico en C# que evoluciona desde una aventura de exploración por consola hacia un videojuego de escritorio con interfaz gráfica. La propuesta conserva los personajes, recursos, inventario, eventos y misión del Núcleo Energético desarrollados en H3.

## Estado del proyecto

- **H3:** versión de consola con clases `Personaje`, `Cadete`, `Enemigo`, `Robot` y `Objeto`; exploración en mapa lógico 5 × 5, inventario, encuentros y misión principal.
- **Evolución gráfica:** el repositorio contiene `Form1.cs`, un prototipo de Windows Forms con pantallas de introducción, registro, hangar, selección de mapas y arena 2D; incluye animación, HUD, controles, clases, armas ficticias, enemigos y jefes.
- **SQL Server:** existe un script inicial para `NEXUS_DB` y la tabla `Jugadores`. El registro se documentó como probado durante H4.
- **Pendiente:** completar la integración compilable de Windows Forms, separar la lógica, incorporar el archivo real de conexión SQL y diseñar/implementar el guardado y recuperación de partidas.

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
3. Antes de ejecutar la versión gráfica, revisa la configuración del proyecto: el archivo actual declara `OutputType=Exe` y `TargetFramework=net10.0`, pero aún no declara `UseWindowsForms`. También debe comprobarse la clase de inicio y la presencia de los archivos parciales de Form1.
4. Configura SQL Server local y ejecuta `database/NEXUS_DB.sql` desde SQL Server Management Studio para crear la base y la tabla inicial de jugadores.
5. Ajusta la cadena de conexión en la configuración local, nunca con credenciales o nombres de servidor personales dentro del repositorio.
6. Compila y ejecuta después de completar la integración WinForms. No se declara aquí una compilación verificada de la instantánea actual.

## Documentación

- [Diseño del videojuego](docs/diseno-del-juego.md)
- [Arquitectura actual](docs/arquitectura-actual.md)
- [Arquitectura propuesta](docs/arquitectura-propuesta.md)
- [Modelo de datos propuesto](docs/modelo-de-datos-propuesto.md)
- [Análisis técnico](docs/analisis-tecnico.md)
- [Bocetos y diagramas](docs/evidencias/)

## Base de datos

El script inicial `database/NEXUS_DB.sql` crea `NEXUS_DB` y `dbo.Jugadores`. El diseño de tablas para partidas, inventario y misiones está documentado como evolución futura en `docs/modelo-de-datos-propuesto.md`; no forma parte del esquema inicial desplegado.

## Autor

Axel Erwin Pozo Teran

## Repositorio

https://github.com/xellwin-hard/NEXUS
