# Análisis técnico de NEXUS: alternativas y decisiones

## 1. Punto de partida

La solución debe evolucionar el proyecto que ya existe. H3 aporta las reglas de exploración, personaje, inventario, eventos y misión. La evolución gráfica de H4 aporta un prototipo en Windows Forms con pantallas, movimiento 2D, HUD, animaciones, clases, mapas y encuentros con jefes. La base SQL comprobada registra jugadores, pero todavía no guarda partidas completas.

## 2. Alternativas de interfaz

| Alternativa | Ventajas | Desventajas | Decisión |
|---|---|---|---|
| Consola | Es sencilla y conserva H3; útil para probar reglas | No cumple la experiencia gráfica prevista | Mantener como referencia de reglas, no como interfaz final |
| Windows Forms | Ya existe código gráfico en `Form1.cs`; permite controles, eventos, teclado y dibujo 2D | Está orientado a Windows; el formulario actual concentra demasiadas responsabilidades | Elegida para la siguiente etapa |
| WPF | Permite interfaces XAML y separación visual más clara | Exige migrar el prototipo y rehacer parte de la presentación | No elegir ahora por el coste de migración |
| Motor de videojuegos | Herramientas específicas para escenas y animación | Cambiaría la arquitectura y requeriría migrar el código existente | Fuera del alcance de esta etapa |

Windows Forms ofrece controles, eventos, dibujo y temporizadores adecuados para una aplicación de escritorio con pantallas y gráficos 2D. La documentación oficial de Microsoft describe WinForms como una interfaz gráfica para aplicaciones Windows y documenta controles, eventos y dibujo personalizado.

## 3. Alternativas de persistencia

| Alternativa | Ventajas | Desventajas | Decisión |
|---|---|---|---|
| Archivos JSON | Fáciles para prototipos y pruebas sin servidor | Menos adecuados para consultas relacionadas y varios perfiles | Útiles solo para pruebas locales |
| SQL Server con ADO.NET | Ya se inició la integración; consultas SQL explícitas y control de transacciones | Requiere configurar servidor y conexión | Elegida |
| Entity Framework Core | Reduce código repetitivo y permite mapear entidades | Añade curva de aprendizaje y configuración | Evaluar más adelante |

Se recomienda continuar con `Microsoft.Data.SqlClient` y repositorios pequeños, ya que existe un avance previo con SQL Server. Los valores deben enviarse mediante parámetros SQL y la cadena de conexión debe configurarse por entorno.

## 4. Decisiones recomendadas

1. Corregir primero el proyecto para que sea una aplicación Windows Forms real: configurar el SDK de escritorio, habilitar Windows Forms, establecer el formulario de inicio y comprobar los archivos parciales.
2. Extraer las entidades que hoy están en `Program.cs` y las clases privadas de `Form1.cs`.
3. Mantener la lógica de combate, energía, inventario y misiones fuera de los formularios.
4. Crear servicios de aplicación para iniciar, guardar y cargar partidas.
5. Centralizar el acceso a SQL Server en repositorios con parámetros.
6. Priorizar una campaña individual y dejar el multijugador para después.
7. Añadir pruebas de reglas antes de migrar todas las mecánicas.
8. No ejecutar SQL en el temporizador de animación ni durante el dibujo de pantalla.

## 5. Riesgos y mitigaciones

- **Formulario demasiado grande:** extraer modelos, reglas y controles por partes; no reescribir todo de golpe.
- **Pérdida de reglas de H3:** crear pruebas para movimiento, límites, consumo de energía, objetos y objetivos.
- **Guardado incompleto:** usar transacciones al persistir partida, inventario y misiones.
- **Problemas de conexión:** mostrar un mensaje comprensible y permitir reintentar sin cerrar inesperadamente.
- **Diferencias entre equipos:** evitar cadenas de conexión con nombres de servidor personales.
- **Alcance excesivo:** entregar primero el modo individual, una misión y guardado/carga antes de ampliar contenido.

## 6. Plan de trabajo sugerido

| Orden | Trabajo | Resultado esperado |
|---|---|---|
| 1 | Configurar y compilar WinForms | Aplicación abre el formulario principal |
| 2 | Extraer modelos de H3/H4 | Clases reutilizables |
| 3 | Separar servicios de juego | Reglas probables de probar sin UI |
| 4 | Conectar registro de jugador | Alta SQL verificada |
| 5 | Crear tablas de partidas y relaciones | Esquema de persistencia |
| 6 | Implementar guardar y cargar | Continuar una campaña individual |
| 7 | Probar flujos y errores | Evidencia de funcionamiento |
| 8 | Evaluar mejoras de contenido | Nueva misión o sector |

## 7. Fuentes técnicas consultadas

Fuentes oficiales de Microsoft:

- [Windows Forms para .NET — documentación](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/). Describe la tecnología de escritorio para Windows, controles y eventos.
- [Windows Forms Overview](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/overview/?view=netdesktop-9.0). Explica formularios, controles, eventos y dibujo.
- [Overview of Using Controls](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/controls/overview). Documenta controles, organización visual y eventos.
- [Configure parameters with Microsoft.Data.SqlClient](https://learn.microsoft.com/en-us/sql/connect/ado-net/configure-parameters?view=sql-server-ver17). Explica parámetros con nombre y tipos de datos.
- [Connection Strings for Microsoft.Data.SqlClient](https://learn.microsoft.com/en-us/sql/connect/ado-net/connection-string-syntax?view=sql-server-ver17). Explica la estructura de las cadenas de conexión.
- [Primary and foreign key constraints — SQL Server](https://learn.microsoft.com/en-us/sql/relational-databases/tables/primary-and-foreign-key-constraints?view=sql-server-ver16). Explica claves y relaciones para mantener integridad referencial.

## 8. Verificación de información obtenida con IA

La IA se utilizó como apoyo para ordenar alternativas y preparar un primer borrador de arquitectura y modelo de datos. Las decisiones no se aceptaron sin contraste:

- Se revisó el contenido real de `Program.cs`, `Form1.cs`, `NEXUS.csproj` y el script SQL del repositorio.
- Se comparó la recomendación de Windows Forms con la documentación oficial de Microsoft.
- Se verificó el uso de parámetros SQL y la función de claves foráneas con documentación oficial.
- Se identificó una diferencia importante: los documentos de H4 mencionan `ConexionBD.cs`, pero ese archivo no aparece en el repositorio público revisado.
- Se verificó que el script actual solo crea `Jugadores`; las tablas de partidas, inventario y misiones son propuestas, no funciones ya implementadas.
- No se afirma que el proyecto compile ni que guarde partidas hasta realizar esas pruebas en el entorno local.

## 9. Conclusión técnica

La evolución más viable es conservar C# y Windows Forms, aprovechar el prototipo gráfico existente y extraer gradualmente la lógica heredada de H3. SQL Server puede cubrir el registro de jugadores y, mediante un modelo relacional ampliado, el guardado de campañas individuales. La principal tarea previa es corregir la integración del proyecto y separar responsabilidades antes de añadir más contenido.
