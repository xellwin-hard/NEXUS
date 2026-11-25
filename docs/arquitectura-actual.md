# Arquitectura actual de NEXUS (H3 y evolución gráfica)

## 1. Alcance de la revisión

La revisión se realizó sobre los archivos visibles en la rama principal del repositorio:

- `Program.cs`
- `Form1.cs`
- `NEXUS.csproj`
- `NEXUS.slnx`
- `database/NEXUS_DB.sql`
- `docs/AVANCES_HITO_4.md`
- `docs/SQL_SERVER.md`

Se diferencia el código de H3, el prototipo gráfico añadido después y los avances de SQL Server. Esta distinción evita presentar como terminadas funciones que todavía no están conectadas.

## 2. Código base de H3: Program.cs

El archivo `Program.cs` contiene una versión de consola organizada con clases y métodos estáticos.

### Clases identificadas

| Clase | Responsabilidad observada |
|---|---|
| `Personaje` | Clase abstracta con nombre, salud, salud máxima y método `Reaccionar()`. |
| `Cadete` | Operativo controlado: rol, energía, experiencia, inventario, posición y equipo. |
| `Enemigo` | Amenaza con daño y reacción propia. |
| `Robot` | Amenaza mecánica con nivel de seguridad y daño. |
| `Objeto` | Elemento de inventario con nombre, tipo y valor. |
| `Program` | Inicio, menú, movimiento, eventos, combate, inventario y misión. |

### Funcionalidades comprobables

- Registro de nombre y selección de rol Soldado, Ingeniero o Médico.
- Valores de salud y energía según el rol.
- Inventario inicial con equipo y recursos.
- Mapa lógico de 5 × 5 con descripciones por coordenada.
- Movimiento mediante W, A, S y D, con validación de límites.
- Consumo de energía al desplazarse.
- Eventos aleatorios: zonas sin amenazas, recursos, robots, enemigos y radiación.
- Combate por turnos con ataque, objeto, retirada y análisis.
- Misión del Núcleo Energético en (4,4) y regreso a extracción en (0,0).
- Experiencia y recompensa al completar la misión.

### Limitaciones de esta versión

- La interfaz depende de consola y texto.
- La lógica está concentrada en métodos estáticos de `Program`.
- Los datos viven en memoria y se pierden al cerrar el programa.
- No hay separación formal entre presentación, reglas y persistencia.
- El mapa se representa por coordenadas y descripciones, no como escenario gráfico interactivo.

## 3. Evolución gráfica: Form1.cs

El repositorio contiene un archivo `Form1.cs` de aproximadamente 2.700 líneas que representa una evolución visual de NEXUS. Incluye controles, dibujo 2D, animaciones y lógica de combate en una misma clase.

### Tipos internos observados

- `ClaseOperativo`: atributos y habilidad de cada clase.
- `ArmaFuturista`: daño, coste de energía, precio, desbloqueo y color de proyectil.
- `MapaSector`: nombre, subtítulo, efecto y colores del sector.
- `JugadorActivo`: cadete, clase, arma, mejoras, créditos, posición y jetpack.
- `Enemigo`: salud, daño, recompensas, posición, animación y tiempos de acción.
- `ItemJetpackMapa`, `Proyectil2D` y `ParticulaEfecto`: elementos visuales y de juego.

### Pantallas y métodos identificados

- `ConstruirPantallaIntro()`
- `ConstruirPantallaRegistro()`
- `ConstruirPantallaHangar()`
- `ConstruirPantallaMapas()`
- `ConstruirPantallaArena()`
- `IniciarCombateEnMapa()`
- `GenerarOleadaAmenazas()`
- `BtnAtacar_Click()`, `BtnHabilidad_Click()`, `BtnCurar_Click()`, `BtnRecargar_Click()`
- Métodos de mejora de arma, habilidad y traje.
- Métodos de dibujo de fondos, operativo, armas, enemigos, partículas y HUD.

También se identifican un temporizador de motor gráfico, controles de teclado, pausa, desplazamiento de cámara y renderizado mediante `System.Drawing`.

### Estado de la evolución

El código gráfico contiene variables para uno o dos jugadores y una constante de oleada final igual a 3. El diseño de esta tarea prioriza el modo individual; la ruta de dos jugadores no se considera una función estable ni un requisito de esta entrega.

## 4. Estado real de integración y compilación

En la revisión inicial, `NEXUS.csproj` estaba configurado como consola (`Exe`, `net10.0`) y la solución apuntaba a una carpeta `NexusForms` que no existe en el repositorio. Se actualizó el proyecto para usar `WinExe`, `net10.0-windows`, `UseWindowsForms=true` y `EnableWindowsTargeting=true`. También se corrigió `NEXUS.slnx` para que apunte al proyecto real de la raíz.

El constructor de `Form1` creaba los controles mediante código, pero llamaba a `InitializeComponent()` sin que existiera un archivo `Form1.Designer.cs` en el repositorio. Se retiró esa llamada para que la inicialización dependa de los métodos de construcción que ya contiene `Form1.cs`. El punto de entrada de `Program.cs` ahora inicia `NexusForms.Form1` con `Application.Run`.

La configuración fue actualizada en GitHub. La compilación local no se pudo ejecutar en este entorno porque no tiene instalado el SDK de .NET. Se añadió un flujo de GitHub Actions para compilar en un agente Windows y comprobar la integración automáticamente.

## 5. SQL Server: estado comprobado

El repositorio contiene `database/NEXUS_DB.sql`, que crea la base `NEXUS_DB` y la tabla `dbo.Jugadores` con `IdJugador` y `Nombre`.

Los documentos de H4 registran que se creó localmente `ConexionBD.cs`, se utilizó `Microsoft.Data.SqlClient` y se probó el registro de nombres. Sin embargo, el archivo real `ConexionBD.cs` no aparece en la raíz ni en las carpetas listadas del repositorio revisado. Tampoco existe una tabla de partidas en el script actual. El registro de jugadores no equivale a guardar y recuperar una partida completa.

## 6. Resumen de brechas

| Área | Estado observado | Trabajo necesario |
|---|---|---|
| Reglas base | Implementadas en consola | Extraer a clases de dominio reutilizables |
| Interfaz gráfica | Prototipo amplio en `Form1.cs` | Integrar y compilar como WinForms |
| Presentación y lógica | Mezcladas en `Form1.cs` | Separar servicios y modelos |
| Modo individual | Base existente | Convertirlo en flujo principal |
| Multijugador | Variables y controles experimentales | Posponer |
| Jugadores SQL | Script y avance documentado | Incorporar clase real y validar |
| Guardado de partidas | No comprobado | Diseñar e implementar |
| Documentación | Parcial antes de esta tarea | Completar los documentos de `docs/` |

