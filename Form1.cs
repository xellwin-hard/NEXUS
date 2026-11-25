using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using NexusV2;

namespace NexusForms
{
    public partial class Form1 : Form
    {
        // =========================================================================
        // 1. MODELOS DE DATOS DEL JUEGO
        // =========================================================================
        private class ClaseOperativo
        {
            public string Nombre { get; set; } = "";
            public string Rol { get; set; } = "";
            public int SaludMax { get; set; }
            public int EnergiaMax { get; set; }
            public double ReduccionDanio { get; set; }
            public int ProbCritico { get; set; }
            public string HabilidadNombre { get; set; } = "";
            public string Descripcion { get; set; } = "";
            public Color ColorTema { get; set; }
        }

        private class ArmaFuturista
        {
            public int IdVisual { get; set; }
            public string Nombre { get; set; } = "";
            public int DanioMin { get; set; }
            public int DanioMax { get; set; }
            public int CostoEnergia { get; set; }
            public int PrecioCR { get; set; }
            public bool Desbloqueada { get; set; }
            public Color ColorProyectil { get; set; }
        }

        private class MapaSector
        {
            public string Nombre { get; set; } = "";
            public string Subtitulo { get; set; } = "";
            public string EfectoUnico { get; set; } = "";
            public string CodigoEfecto { get; set; } = "";
            public Color ColorPrimario { get; set; }
            public Color ColorFondo { get; set; }
        }

        private class JugadorActivo
        {
            public Cadete Entidad { get; set; } = null!;
            public ClaseOperativo Clase { get; set; } = null!;
            public ArmaFuturista Arma { get; set; } = null!;
            public int NivelArma { get; set; } = 1;
            public int NivelHabilidad { get; set; } = 1;
            public int Escudo { get; set; } = 0;
            public int Botiquines { get; set; } = 3;
            public int XP { get; set; } = 0;
            public int Creditos { get; set; } = 180;
            public int TurnosDesangrado { get; set; } = 0;

            public float PosX { get; set; } = 90f;
            public float PosY { get; set; } = 250f;
            public float VelY { get; set; } = 0f;
            public int CombustibleJetpack { get; set; } = 0;
            public int CombustibleJetpackMax { get; set; } = 320;
            public bool TieneJetpack => CombustibleJetpack > 0;
            public bool Vivo => Entidad.Salud > 0;
        }

        private class Enemigo
        {
            public string Nombre { get; set; } = "";
            public bool EsAlien { get; set; }
            public bool EsJefe { get; set; }
            public int Salud { get; set; }
            public int SaludMax { get; set; }
            public int DanioMin { get; set; }
            public int DanioMax { get; set; }
            public int RecompensaCR { get; set; }
            public int RecompensaXP { get; set; }
            public Color ColorAura { get; set; }
            public float PosX { get; set; }
            public float PosY { get; set; }
            public float VX { get; set; }
            public float VY { get; set; }
            public int CooldownDisparo { get; set; }
            public int CooldownInvocacion { get; set; }
            public int CooldownMegaAtaque { get; set; }
            public int CargandoMega { get; set; }
            public float FaseAnim { get; set; }
            public bool ExplosionGenerada { get; set; } = false;
            public bool Vivo => Salud > 0;
        }

        private class ItemJetpackMapa
        {
            public bool Activo { get; set; }
            public float X { get; set; }
            public float Y { get; set; }
            public float Fase { get; set; }
        }

        private class Proyectil2D
        {
            public float X, Y, VX, VY;
            public int Tamano = 6;
            public Color Color;
            public bool EsDeJugador;
            public int Danio;
            public bool CausaSangrado;
        }

        private class ParticulaEfecto
        {
            public float X, Y, VX, VY, Tamano;
            public int Vida;
            public Color Color;
            public string Tipo = "AMBIENTE";
        }

        // =========================================================================
        // 2. ESTADO GLOBAL Y VARIABLES DE ANIMACIÓN
        // =========================================================================
        private readonly Random rng = new Random();
        private System.Windows.Forms.Timer tmrMotorGrafico = null!;
        private System.Windows.Forms.Timer tmrIntroTexto = null!;

        private bool esMultijugador = false;
        private string nombreJ1 = "LIXER";
        private string nombreJ2 = "NOVA";
        private int pasoSeleccionJugador = 1;
        private string dificultad = "INTERMEDIO";
        private double multDificultad = 1.15;

        private List<ClaseOperativo> catalogoClases = new List<ClaseOperativo>();
        private List<ArmaFuturista> catalogoArmas = new List<ArmaFuturista>();
        private List<MapaSector> catalogoMapas = new List<MapaSector>();

        private List<JugadorActivo> escuadron = new List<JugadorActivo>();
        private int indiceTurnoJugador = 0;
        private MapaSector mapaActual = null!;
        private List<Enemigo> enemigosActivos = new List<Enemigo>();
        private int oleada = 1;
        private const int OLEADA_JEFE_FINAL = 3;

        // Estado de Pausa, Cámara Parallax, Jetpack y Controles 4-Direcciones
        private bool juegoPausado = false;
        private float desplazamientoMapa = 0f;
        private bool teclaIzquierda = false;
        private bool teclaDerecha = false;
        private bool teclaArriba = false;
        private bool teclaAbajo = false;

        private ItemJetpackMapa itemJetpack = new ItemJetpackMapa();
        private int timerSpawnJetpack = 140;

        private List<Proyectil2D> proyectiles = new List<Proyectil2D>();
        private List<ParticulaEfecto> particulas = new List<ParticulaEfecto>();
        private readonly Dictionary<string, Bitmap> cacheSprites = new Dictionary<string, Bitmap>();
        private readonly List<PictureBox> previewsHangar = new List<PictureBox>();
        private float pulsoNeon = 0f;
        private int framesCuracion = 0;
        private int framesSacudida = 0;

        // Contenedores de Pantallas
        private Panel pnlIntro = null!;
        private Panel pnlRegistro = null!;
        private Panel pnlHangar = null!;
        private Panel pnlMapas = null!;
        private Panel pnlArena = null!;

        private Label lblTextoIntro = null!;
        private string textoIntroCompleto = "";
        private int indiceLetraIntro = 0;

        private TextBox txtNombre1 = null!;
        private TextBox txtNombre2 = null!;
        private RadioButton rb1Jugador = null!;
        private RadioButton rb2Jugadores = null!;

        private Label lblTituloHangar = null!;
        private ComboBox cmbArmaInicial = null!;
        private PictureBox picPreviewArmaInicial = null!;
        private Label lblDificultadActual = null!;

        private PictureBox picArena2D = null!;
        private Panel pnlStatsEscuadron = null!;
        private ListBox lstConsola = null!;
        private ComboBox cmbTiendaArmas = null!;

        private Button btnPausa = null!;
        private Button btnAtacar = null!;
        private Button btnHabilidad = null!;
        private Button btnCurar = null!;
        private Button btnRecargar = null!;
        private Button btnMejorarArmaXP = null!;
        private Button btnMejorarHabXP = null!;
        private Button btnMejorarTrajeXP = null!;
        private Button btnComprarArmaCR = null!;
        private Button btnComprarBotiquinCR = null!;

        // =========================================================================
        // 3. INICIALIZACIÓN PRINCIPAL (PANTALLA COMPLETA)
        // =========================================================================
        public Form1()
        {
            // Los controles se crean desde el propio código; no se requiere archivo Designer.
            OcultarControlesDiseñador();
            InicializarCatalogos();
            InicializarParticulasAmbiente();
            ConfigurarVentana();

            ConstruirPantallaIntro();
            ConstruirPantallaRegistro();
            ConstruirPantallaHangar();
            ConstruirPantallaMapas();
            ConstruirPantallaArena();

            IniciarMotorAnimacion();
            MostrarPantalla(pnlIntro);
            IniciarAnimacionIntro();
        }

        private void OcultarControlesDiseñador()
        {
            foreach (Control c in this.Controls) c.Visible = false;
        }

        private void ConfigurarVentana()
        {
            this.Text = "NEXUS // PROTOCOLO OMEGA v8.0 — PC EDITION";
            this.BackColor = Color.FromArgb(8, 11, 16);
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;
            this.KeyPreview = true;

            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
        }

        private float ObtenerSueloY()
        {
            if (picArena2D == null || picArena2D.Height <= 120) return 300f;
            return picArena2D.Height - 110f;
        }

        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (this.FormBorderStyle == FormBorderStyle.None)
                {
                    this.FormBorderStyle = FormBorderStyle.Sizable;
                    this.WindowState = FormWindowState.Normal;
                    this.Size = new Size(1280, 760);
                }
                else
                {
                    this.FormBorderStyle = FormBorderStyle.None;
                    this.WindowState = FormWindowState.Maximized;
                }
                return;
            }

            if (!pnlArena.Visible) return;

            if (e.KeyCode == Keys.P)
            {
                AlternarPausa();
                return;
            }

            if (juegoPausado || !escuadron.Exists(j => j.Vivo)) return;

            if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left) teclaIzquierda = true;
            if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right) teclaDerecha = true;
            if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up)
            {
                teclaArriba = true;
                if (!JugadorEnTurno().TieneJetpack) SaltarJugadorActivo();
            }
            if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down) teclaAbajo = true;
            if (e.KeyCode == Keys.Space) SaltarJugadorActivo();
        }

        private void Form1_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left) teclaIzquierda = false;
            if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right) teclaDerecha = false;
            if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up) teclaArriba = false;
            if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down) teclaAbajo = false;
        }

        private void AlternarPausa()
        {
            juegoPausado = !juegoPausado;
            teclaIzquierda = false;
            teclaDerecha = false;
            teclaArriba = false;
            teclaAbajo = false;
            btnPausa.Text = juegoPausado ? "▶ REANUDAR (P)" : "❚❚ PAUSAR (P)";
            btnPausa.ForeColor = juegoPausado ? Color.FromArgb(80, 250, 123) : Color.Gold;
            btnPausa.FlatAppearance.BorderColor = btnPausa.ForeColor;
            picArena2D.Invalidate();
        }

        private void SaltarJugadorActivo()
        {
            if (juegoPausado) return;
            var j = JugadorEnTurno();
            float sueloY = ObtenerSueloY();
            bool enSuelo = j.PosY >= sueloY - 2f;

            if (j.Vivo && enSuelo && !j.TieneJetpack)
            {
                j.VelY = -16.5f;
                CrearExplosionPixelArt(j.PosX + 20, j.PosY + 54, Color.Cyan, 8);
            }
        }

        private void InicializarCatalogos()
        {
            catalogoClases = new List<ClaseOperativo>
            {
                new ClaseOperativo
                {
                    Nombre = "VANGUARD",
                    Rol = "Asalto Blindado",
                    SaludMax = 240,
                    EnergiaMax = 115,
                    ReduccionDanio = 0.25,
                    ProbCritico = 15,
                    HabilidadNombre = "BARRERA DE PLASMA (30 EN)",
                    Descripcion = "Exoesqueleto pesado con casco de titanio.\n• Salud: 240 | Energía: 115\n• Blindaje: -25% daño recibido\n• Habilidad: Escudo molecular que\n  absorbe impactos y frena sangrado.",
                    ColorTema = Color.FromArgb(255, 85, 95)
                },
                new ClaseOperativo
                {
                    Nombre = "SPECTRE",
                    Rol = "Infiltrado Cibernético",
                    SaludMax = 175,
                    EnergiaMax = 155,
                    ReduccionDanio = 0.10,
                    ProbCritico = 40,
                    HabilidadNombre = "CORTE DIMENSIONAL (35 EN)",
                    Descripcion = "Unidad sigilosa con capucha táctica.\n• Salud: 175 | Energía: 155\n• Crítico Base: 40% (Daño x2.2)\n• Habilidad: Impacto perforante\n  múltiple contra objetivos.",
                    ColorTema = Color.FromArgb(0, 240, 255)
                },
                new ClaseOperativo
                {
                    Nombre = "NETRUNNER",
                    Rol = "Ingeniero Táctico",
                    SaludMax = 195,
                    EnergiaMax = 185,
                    ReduccionDanio = 0.15,
                    ProbCritico = 25,
                    HabilidadNombre = "SOBRECARGA EMP (40 EN)",
                    Descripcion = "Especialista con visor holográfico.\n• Salud: 195 | Energía: 185\n• Detiene desangrado al curar\n• Habilidad: Daña enemigos y\n  regenera HP a todo el escuadrón.",
                    ColorTema = Color.FromArgb(189, 147, 249)
                }
            };

            // 6 ARMAS CON DISEÑOS PIXEL ART ÚNICOS (IdVisual 0..5)
            catalogoArmas = new List<ArmaFuturista>
            {
                new ArmaFuturista { IdVisual = 0, Nombre = "Pistola de Pulso MK-I", DanioMin = 24, DanioMax = 34, CostoEnergia = 10, PrecioCR = 0, Desbloqueada = true, ColorProyectil = Color.Cyan },
                new ArmaFuturista { IdVisual = 1, Nombre = "Rifle Criogénico Sub-Zero", DanioMin = 32, DanioMax = 46, CostoEnergia = 15, PrecioCR = 0, Desbloqueada = true, ColorProyectil = Color.DeepSkyBlue },
                new ArmaFuturista { IdVisual = 2, Nombre = "Carabina de Plasma Solar", DanioMin = 45, DanioMax = 64, CostoEnergia = 20, PrecioCR = 140, Desbloqueada = false, ColorProyectil = Color.OrangeRed },
                new ArmaFuturista { IdVisual = 3, Nombre = "Lanzador de Agujas Tesla", DanioMin = 58, DanioMax = 78, CostoEnergia = 24, PrecioCR = 220, Desbloqueada = false, ColorProyectil = Color.Yellow },
                new ArmaFuturista { IdVisual = 4, Nombre = "Escopeta Antimateria V8", DanioMin = 72, DanioMax = 96, CostoEnergia = 28, PrecioCR = 310, Desbloqueada = false, ColorProyectil = Color.Lime },
                new ArmaFuturista { IdVisual = 5, Nombre = "Cañón Railgun Orbital", DanioMin = 92, DanioMax = 128, CostoEnergia = 35, PrecioCR = 420, Desbloqueada = false, ColorProyectil = Color.Magenta }
            };

            catalogoMapas = new List<MapaSector>
            {
                new MapaSector
                {
                    Nombre = "01 // CIUDAD NEÓN SUBTERRÁNEA",
                    Subtitulo = "Metrópoli Ciberpunk | Jefe Final: LEVIATÁN MECHA",
                    EfectoUnico = "ESTÁTICA URBANA: Regenera +8 EN extra cada turno.",
                    CodigoEfecto = "ESTATICA",
                    ColorPrimario = Color.FromArgb(0, 240, 255),
                    ColorFondo = Color.FromArgb(10, 18, 32)
                },
                new MapaSector
                {
                    Nombre = "02 // BOSQUE BIOLUMINISCENTE",
                    Subtitulo = "Reserva Botánica | Jefe Final: TITÁN SIMBIONTE",
                    EfectoUnico = "ESPORAS VITALES: Regenera +6 HP al final de cada turno.",
                    CodigoEfecto = "BOSQUE",
                    ColorPrimario = Color.FromArgb(80, 250, 123),
                    ColorFondo = Color.FromArgb(10, 28, 22)
                },
                new MapaSector
                {
                    Nombre = "03 // REACTOR DE FUSIÓN NÚCLEO-7",
                    Subtitulo = "Cámara Térmica | Jefe Final: COLOSO DE PLASMA",
                    EfectoUnico = "SOBRECALENTAMIENTO: Todos los disparos infligen +20% de daño.",
                    CodigoEfecto = "RADIACION",
                    ColorPrimario = Color.FromArgb(255, 130, 45),
                    ColorFondo = Color.FromArgb(32, 14, 12)
                },
                new MapaSector
                {
                    Nombre = "04 // COLMENA ORBITAL AETHERIS",
                    Subtitulo = "Nido Alienígena | Jefe Final: EMPERATRIZ XENOMORFA",
                    EfectoUnico = "GRAVEDAD CERO: +20% Crítico | Alta presencia Xenomorfa.",
                    CodigoEfecto = "GRAVEDAD",
                    ColorPrimario = Color.FromArgb(195, 130, 255),
                    ColorFondo = Color.FromArgb(22, 12, 34)
                }
            };
        }

        private void InicializarParticulasAmbiente()
        {
            particulas.Clear();
            for (int i = 0; i < 65; i++)
            {
                particulas.Add(new ParticulaEfecto
                {
                    X = rng.Next(0, 1920),
                    Y = rng.Next(0, 1080),
                    VX = (float)(rng.NextDouble() * 1.4 - 0.7),
                    VY = (float)(rng.NextDouble() * -1.2 - 0.3),
                    Tamano = rng.Next(2, 6),
                    Vida = 9999,
                    Color = i % 3 == 0 ? Color.FromArgb(120, 0, 240, 255)
                          : (i % 3 == 1 ? Color.FromArgb(100, 189, 147, 249)
                          : Color.FromArgb(110, 80, 250, 123)),
                    Tipo = "AMBIENTE"
                });
            }
        }

        // =========================================================================
        // MOTOR DE FÍSICAS, SALTO CORREGIDO, JETPACK Y JEFES FINALES
        // =========================================================================
        private void IniciarMotorAnimacion()
        {
            tmrMotorGrafico = new System.Windows.Forms.Timer { Interval = 28 };
            tmrMotorGrafico.Tick += (s, e) =>
            {
                if (juegoPausado)
                {
                    if (pnlArena.Visible) picArena2D.Invalidate();
                    return;
                }

                pulsoNeon += 0.16f;

                if (framesCuracion > 0) framesCuracion--;
                if (framesSacudida > 0) framesSacudida--;

                if (pnlIntro.Visible) pnlIntro.Invalidate();
                if (pnlHangar.Visible)
                {
                    foreach (var pb in previewsHangar) pb.Invalidate();
                }

                // Actualizar partículas
                for (int i = particulas.Count - 1; i >= 0; i--)
                {
                    var p = particulas[i];
                    p.X += p.VX;
                    p.Y += p.VY;

                    if (p.Tipo == "EXPLOSION") p.VY += 0.35f;

                    if (p.Tipo == "AMBIENTE")
                    {
                        if (p.Y < 0) { p.Y = 1080; p.X = rng.Next(0, 1920); }
                        if (p.X < 0) p.X = 1920;
                        if (p.X > 1920) p.X = 0;
                    }
                    else
                    {
                        p.Vida--;
                        if (p.Vida <= 0) particulas.RemoveAt(i);
                    }
                }

                // Lógica en tiempo real dentro de la Arena
                if (pnlArena.Visible && escuadron.Count > 0)
                {
                    int anchoArena = picArena2D.Width;
                    float sueloY = ObtenerSueloY();

                    // Aparición periódica de Mochila Cohete (Jetpack) en el mapa
                    if (!itemJetpack.Activo)
                    {
                        timerSpawnJetpack--;
                        if (timerSpawnJetpack <= 0)
                        {
                            itemJetpack.Activo = true;
                            itemJetpack.X = rng.Next(140, Math.Max(300, anchoArena - 240));
                            itemJetpack.Y = sueloY + 12f; // Directamente sobre el suelo
                            itemJetpack.Fase = 0f;
                            LogArena("[SUMINISTRO] ¡Mochila Cohete (JETPACK) aterrizó en el suelo del sector!");
                        }
                    }
                    {
                        itemJetpack.Y = sueloY + 12f;
                        itemJetpack.Fase += 0.15f;
                    }

                    var j = JugadorEnTurno();
                    if (j.Vivo)
                    {
                        // Movimiento horizontal + Parallax del mapa
                        if (teclaIzquierda)
                        {
                            j.PosX = Math.Max(30f, j.PosX - 7.5f);
                            desplazamientoMapa -= 4.8f;
                        }
                        if (teclaDerecha)
                        {
                            j.PosX = Math.Min(anchoArena - 130f, j.PosX + 7.5f);
                            desplazamientoMapa += 4.8f;
                        }

                        // Comprobar si recoge la Mochila Cohete
                        if (itemJetpack.Activo &&
                            Math.Abs((j.PosX + 24) - (itemJetpack.X + 16)) < 38 &&
                            Math.Abs((j.PosY + 28) - (itemJetpack.Y + 16)) < 42)
                        {
                            itemJetpack.Activo = false;
                            timerSpawnJetpack = 360;
                            j.CombustibleJetpack = j.CombustibleJetpackMax;
                            j.VelY = 0f;
                            CrearExplosionPixelArt(itemJetpack.X + 16, itemJetpack.Y + 16, Color.Gold, 18);
                            LogArena($"[JETPACK EQUIPADO] {j.Entidad.Nombre} puede volar en 4 direcciones (W/A/S/D) por tiempo limitado.");
                        }

                        // FÍSICA DE VUELO CON JETPACK VS FÍSICA DE SALTO Y GRAVEDAD CORREGIDA
                        if (j.TieneJetpack)
                        {
                            j.CombustibleJetpack--;
                            j.VelY = 0f;

                            if (teclaArriba) j.PosY = Math.Max(45f, j.PosY - 7.5f);
                            if (teclaAbajo) j.PosY = Math.Min(sueloY, j.PosY + 7.5f);

                            // Chispas constantes del propulsor
                            particulas.Add(new ParticulaEfecto
                            {
                                X = j.PosX + (teclaIzquierda ? 38 : 10),
                                Y = j.PosY + 34,
                                VX = (float)(rng.NextDouble() * 3 - 1.5),
                                VY = (float)(rng.NextDouble() * 3 + 2.5),
                                Tamano = rng.Next(4, 7),
                                Vida = 12,
                                Color = rng.Next(2) == 0 ? Color.OrangeRed : Color.Gold,
                                Tipo = "FUEGO"
                            });

                            if (j.CombustibleJetpack == 0)
                            {
                                LogArena($"[JETPACK AGOTADO] Combustible de {j.Entidad.Nombre} terminado. Descendiendo...");
                            }
                        }
                        else
                        {
                            // Gravedad real hasta tocar sueloY (corrige el bug de quedarse flotando)
                            if (j.PosY < sueloY || j.VelY < 0)
                            {
                                j.PosY += j.VelY;
                                j.VelY += 0.95f;
                                if (j.PosY >= sueloY)
                                {
                                    j.PosY = sueloY;
                                    j.VelY = 0f;
                                }
                            }
                            else
                            {
                                j.PosY = sueloY;
                                j.VelY = 0f;
                            }
                        }

                        // Gotas de sangre si está desangrándose
                        if (j.TurnosDesangrado > 0 && rng.Next(0, 4) == 0)
                        {
                            particulas.Add(new ParticulaEfecto
                            {
                                X = j.PosX + rng.Next(8, 36),
                                Y = j.PosY + rng.Next(18, 45),
                                VX = (float)(rng.NextDouble() * 1.4 - 0.7),
                                VY = (float)(rng.NextDouble() * 2.5 + 1.5),
                                Tamano = 4,
                                Vida = 18,
                                Color = Color.FromArgb(240, 220, 20, 35),
                                Tipo = "SANGRE"
                            });
                        }
                    }

                    // Movimiento e IA de las amenazas y Jefes Finales
                    for (int idxEn = enemigosActivos.Count - 1; idxEn >= 0; idxEn--)
                    {
                        var en = enemigosActivos[idxEn];
                        if (!en.Vivo) continue;

                        int margenDerecho = en.EsJefe ? 140 : 85;
                        en.PosX += en.VX;
                        if (en.PosX < 250f) { en.PosX = 250f; en.VX = Math.Abs(en.VX); }
                        if (en.PosX > anchoArena - margenDerecho) { en.PosX = anchoArena - margenDerecho; en.VX = -Math.Abs(en.VX); }

                        if (!en.EsAlien || en.EsJefe)
                        {
                            float limiteInf = en.EsJefe ? sueloY - 45f : sueloY;
                            en.PosY += en.VY;
                            if (en.PosY < 45f) { en.PosY = 45f; en.VY = Math.Abs(en.VY); }
                            if (en.PosY > limiteInf) { en.PosY = limiteInf; en.VY = -Math.Abs(en.VY); }
                        }
                        else
                        {
                            en.PosY = sueloY;
                        }

                        // HABILIDADES ESPECIALES DEL JEFE FINAL (Invocar aliados + Mega Ataques)
                        if (en.EsJefe && j.Vivo)
                        {
                            // 1. Invocar aliados continuamente mientras el Jefe siga vivo
                            en.CooldownInvocacion--;
                            if (en.CooldownInvocacion <= 0)
                            {
                                en.CooldownInvocacion = dificultad == "DIFÍCIL" ? 240 : 320;
                                if (enemigosActivos.FindAll(x => x.Vivo).Count < 5)
                                {
                                    InvocarAliadoDesdeJefe(en, sueloY);
                                }
                            }

                            // 2. Mega Ataque periódico con carga previa
                            en.CooldownMegaAtaque--;
                            if (en.CooldownMegaAtaque <= 45 && en.CooldownMegaAtaque > 0)
                            {
                                en.CargandoMega = en.CooldownMegaAtaque;
                            }
                            else if (en.CooldownMegaAtaque <= 0)
                            {
                                en.CargandoMega = 0;
                                en.CooldownMegaAtaque = dificultad == "DIFÍCIL" ? 210 : 280;
                                EjecutarMegaAtaqueJefe(en, j);
                            }
                        }

                        // Disparo normal continuo dirigido hacia el jugador
                        if (j.Vivo)
                        {
                            en.CooldownDisparo--;
                            if (en.CooldownDisparo <= 0)
                            {
                                en.CooldownDisparo = dificultad == "FÁCIL" ? rng.Next(85, 120)
                                                   : dificultad == "INTERMEDIO" ? rng.Next(65, 95)
                                                   : rng.Next(48, 72);

                                int dmg = rng.Next(en.DanioMin, en.DanioMax + 1);
                                bool sangrado = en.EsAlien || (rng.Next(100) < 35);

                                float dx = (j.PosX + 20) - en.PosX;
                                float dy = (j.PosY + 25) - (en.PosY + 30);
                                float dist = Math.Max(1f, (float)Math.Sqrt(dx * dx + dy * dy));
                                float velProy = 11.5f;

                                proyectiles.Add(new Proyectil2D
                                {
                                    X = en.PosX,
                                    Y = en.PosY + 30f,
                                    VX = (dx / dist) * velProy,
                                    VY = (dy / dist) * velProy,
                                    Tamano = en.EsJefe ? 9 : 6,
                                    Color = en.EsJefe ? Color.Magenta : (en.EsAlien ? Color.Lime : Color.OrangeRed),
                                    EsDeJugador = false,
                                    Danio = dmg,
                                    CausaSangrado = sangrado
                                });
                            }
                        }
                    }

                    // Actualizar proyectiles 2D
                    for (int i = proyectiles.Count - 1; i >= 0; i--)
                    {
                        var pr = proyectiles[i];
                        pr.X += pr.VX;
                        pr.Y += pr.VY;

                        if (pr.EsDeJugador)
                        {
                            var objetivo = enemigosActivos.Find(en => en.Vivo &&
                                Math.Abs(pr.X - (en.PosX + (en.EsJefe ? 52 : 24))) < (en.EsJefe ? 55 : 32) &&
                                Math.Abs(pr.Y - (en.PosY + (en.EsJefe ? 55 : 28))) < (en.EsJefe ? 60 : 36));

                            if (objetivo != null)
                            {
                                CrearExplosionPixelArt(pr.X, pr.Y, pr.Color, 14);
                                proyectiles.RemoveAt(i);
                                continue;
                            }
                        }
                        else
                        {
                            int radioHit = pr.Tamano > 8 ? 28 : 20;
                            if (j.Vivo && Math.Abs(pr.X - (j.PosX + 24)) < radioHit && Math.Abs(pr.Y - (j.PosY + 26)) < radioHit + 6)
                            {
                                AplicarImpactoEnemigoAJugador(j, pr.Danio, pr.CausaSangrado);
                                CrearExplosionPixelArt(pr.X, pr.Y, pr.Color, 16);
                                proyectiles.RemoveAt(i);
                                ActualizarHUDCompleto();
                                continue;
                            }
                        }

                        if (pr.X > anchoArena + 30 || pr.X < -30 || pr.Y > picArena2D.Height || pr.Y < 0)
                            proyectiles.RemoveAt(i);
                    }
                }

                if (pnlArena.Visible) picArena2D.Invalidate();
            };
            tmrMotorGrafico.Start();
        }

        private void InvocarAliadoDesdeJefe(Enemigo jefe, float sueloY)
        {
            bool esAlien = jefe.EsAlien;
            int hpRefuerzo = (int)((110 + oleada * 25) * multDificultad);
            enemigosActivos.Add(new Enemigo
            {
                Nombre = esAlien ? "CRÍA XENOMORFA INVOCADA" : "DRON ESCOLTA INVOCADO",
                EsAlien = esAlien,
                EsJefe = false,
                Salud = hpRefuerzo,
                SaludMax = hpRefuerzo,
                DanioMin = (int)(10 * multDificultad),
                DanioMax = (int)(17 * multDificultad),
                RecompensaCR = 25,
                RecompensaXP = 20,
                ColorAura = Color.Cyan,
                PosX = jefe.PosX - 40f,
                PosY = esAlien ? sueloY : jefe.PosY + 20f,
                VX = -2.6f,
                VY = esAlien ? 0f : 1.8f,
                CooldownDisparo = 45,
                FaseAnim = 0.5f
            });
            CrearExplosionPixelArt(jefe.PosX, jefe.PosY + 40, Color.Magenta, 20);
            LogArena($"[INVOCACIÓN DE JEFE] ¡{jefe.Nombre} despliega un refuerzo en combate!");
        }

        private void EjecutarMegaAtaqueJefe(Enemigo jefe, JugadorActivo j)
        {
            framesSacudida = 15;
            int megaDmg = (int)((26 + oleada * 5) * multDificultad);
            float[] angulos = { -0.22f, -0.11f, 0f, 0.11f, 0.22f };

            float dx = (j.PosX + 20) - jefe.PosX;
            float dy = (j.PosY + 25) - (jefe.PosY + 45);
            float anguloBase = (float)Math.Atan2(dy, dx);

            foreach (float offset in angulos)
            {
                float ang = anguloBase + offset;
                proyectiles.Add(new Proyectil2D
                {
                    X = jefe.PosX,
                    Y = jefe.PosY + 45f,
                    VX = (float)Math.Cos(ang) * 13f,
                    VY = (float)Math.Sin(ang) * 13f,
                    Tamano = 12,
                    Color = Color.Gold,
                    EsDeJugador = false,
                    Danio = megaDmg,
                    CausaSangrado = true
                });
            }

            LogArena($"[MEGA ATAQUE] ¡{jefe.Nombre} libera una RÁFAGA OMEGA DE 5 PROYECTILES ({megaDmg} DMG c/u)!");
        }

        private void CrearExplosionPixelArt(float x, float y, Color cBase, int cantidad)
        {
            Color[] paletaFuego = { cBase, Color.OrangeRed, Color.Gold, Color.White, Color.Gray };
            for (int i = 0; i < cantidad; i++)
            {
                particulas.Add(new ParticulaEfecto
                {
                    X = x,
                    Y = y,
                    VX = (float)(rng.NextDouble() * 11 - 5.5),
                    VY = (float)(rng.NextDouble() * -9 - 1),
                    Tamano = rng.Next(4, 9),
                    Vida = rng.Next(18, 30),
                    Color = paletaFuego[rng.Next(paletaFuego.Length)],
                    Tipo = "EXPLOSION"
                });
            }
        }

        private void MostrarPantalla(Panel destino)
        {
            juegoPausado = false;
            pnlIntro.Visible = false;
            pnlRegistro.Visible = false;
            pnlHangar.Visible = false;
            pnlMapas.Visible = false;
            pnlArena.Visible = false;

            destino.Visible = true;
            destino.BringToFront();
            this.Focus();
        }

        // =========================================================================
        // PANTALLA 1: INTRODUCCIÓN CINEMÁTICA ANIMADA + LORE PROFUNDO
        // =========================================================================

        private struct MeteoroIntro { public float X, Y, VX, VY, Largo; }
        private readonly List<MeteoroIntro> meteorosIntro = new List<MeteoroIntro>();

        private void ConstruirPantallaIntro()
        {
            // Panel principal adaptado al tamaño completo de la ventana
            pnlIntro = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(6, 9, 18) };

            // Activar Doble Búfer nativo para eliminar parpadeos de forma limpia
            typeof(Control).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, pnlIntro, new object[] { true });

            pnlIntro.Paint += DibujarFondoCinematicoIntro;

            // Botón Salir anclado correctamente a la esquina superior derecha
            Button btnSalirApp = CrearBotonNeon("✕ SALIR", new Point(0, 0), new Size(110, 36), Color.FromArgb(255, 60, 110));
            btnSalirApp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSalirApp.Location = new Point(pnlIntro.Width - 135, 30);
            pnlIntro.Resize += (s, e) => { btnSalirApp.Location = new Point(pnlIntro.Width - 135, 30); };

            Label lblHeader = new Label
            {
                Text = "N E X U S   //   H O R I Z O N T E   O M E G A",
                Font = new Font("Consolas", 24F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 240, 255),
                Location = new Point(50, 30),
                Size = new Size(900, 45),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };

            // Cuadro de texto del lore cómodo y bien posicionado a la izquierda
            lblTextoIntro = new Label
            {
                Text = "",
                Font = new Font("Consolas", 11.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(235, 245, 250),
                Location = new Point(50, 95),
                Size = new Size(720, 360),
                BackColor = Color.FromArgb(180, 8, 13, 24),
                Padding = new Padding(14)
            };

            Button btnContinuarIntro = CrearBotonNeon("INICIAR ENLACE NEURONAL  >>", new Point(50, 475), new Size(360, 55), Color.FromArgb(0, 240, 255));
            btnContinuarIntro.Click += (s, e) =>
            {
                tmrIntroTexto.Stop();
                MostrarPantalla(pnlRegistro);
            };

            pnlIntro.Controls.AddRange(new Control[] { btnSalirApp, lblHeader, lblTextoIntro, btnContinuarIntro });
            this.Controls.Add(pnlIntro);
        }

        private void DibujarFondoCinematicoIntro(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Usar el tamaño real actual del panel para que cubra toda la pantalla sin espacios negros
            int w = pnlIntro.Width;
            int h = pnlIntro.Height;

            // 1. Cielo espacial con nebulosa adaptada a todo el ancho y alto
            using LinearGradientBrush bg = new LinearGradientBrush(
                new Rectangle(0, 0, w, h),
                Color.FromArgb(6, 14, 34),
                Color.FromArgb(32, 8, 42),
                LinearGradientMode.Horizontal);
            g.FillRectangle(bg, 0, 0, w, h);

            // Resplandor nebular magenta a la derecha
            using GraphicsPath gpNebula = new GraphicsPath();
            gpNebula.AddEllipse(w - 600, 20, 650, h - 100);
            using PathGradientBrush pgbNebula = new PathGradientBrush(gpNebula)
            {
                CenterColor = Color.FromArgb(110, 220, 35, 140),
                SurroundColors = new Color[] { Color.Transparent }
            };
            g.FillPath(pgbNebula, gpNebula);

            // 2. Planeta Azul Gigante posicionado proporcionalmente
            int pRadio = Math.Min(w, h) / 4;
            int px = (int)(w * 0.48) - pRadio;
            int py = 40;

            using GraphicsPath gpHalo = new GraphicsPath();
            gpHalo.AddEllipse(px - 20, py - 20, (pRadio + 20) * 2, (pRadio + 20) * 2);
            using PathGradientBrush pgbHalo = new PathGradientBrush(gpHalo)
            {
                CenterColor = Color.FromArgb(130, 0, 195, 255),
                SurroundColors = new Color[] { Color.Transparent }
            };
            g.FillPath(pgbHalo, gpHalo);

            using LinearGradientBrush brPlaneta = new LinearGradientBrush(
                new Rectangle(px, py, pRadio * 2, pRadio * 2),
                Color.FromArgb(45, 165, 255),
                Color.FromArgb(8, 26, 68),
                LinearGradientMode.ForwardDiagonal);
            g.FillEllipse(brPlaneta, px, py, pRadio * 2, pRadio * 2);

            // 3. Lluvia de meteoritos en diagonal
            for (int i = 0; i < meteorosIntro.Count; i++)
            {
                var m = meteorosIntro[i];
                m.X += m.VX;
                m.Y += m.VY;
                if (m.X < -150 || m.Y > h + 150)
                {
                    m.X = rng.Next(400, w + 300);
                    m.Y = rng.Next(-200, 150);
                }
                meteorosIntro[i] = m;

                using Pen pCola = new Pen(Color.FromArgb(190, 255, 55, 115), 3);
                g.DrawLine(pCola, m.X, m.Y, m.X + m.Largo, m.Y - m.Largo * 0.55f);
                g.FillEllipse(Brushes.White, m.X - 3, m.Y - 3, 6, 6);
            }

            // 4. Ciudadela Futurista con el símbolo OMEGA (Ω) a la derecha
            int cx = w - 360;
            int cy = h - 420;
            using SolidBrush brTorre = new SolidBrush(Color.FromArgb(16, 14, 30));
            g.FillPolygon(brTorre, new Point[] { new Point(cx + 80, cy + 280), new Point(cx + 125, cy - 20), new Point(cx + 170, cy + 280) });
            g.FillRectangle(brTorre, cx + 30, cy + 85, 190, 210);
            g.FillRectangle(brTorre, cx - 80, cy + 150, 320, 160);

            using Pen pRayoRojo = new Pen(Color.FromArgb(210, 255, 35, 95), 3);
            g.DrawLine(pRayoRojo, cx + 125, cy + 10, cx + 125, cy + 75);

            int pulsoAlpha = 185 + (int)(Math.Sin(pulsoNeon * 1.5) * 65);
            using Font fOmega = new Font("Consolas", 44F, FontStyle.Bold);
            g.DrawString("Ω", fOmega, new SolidBrush(Color.FromArgb(pulsoAlpha, 255, 35, 85)), cx + 96, cy + 95);

            // 5. Suelo rocoso y silueta del Guerrero NEXUS abajo a la izquierda
            using LinearGradientBrush brNiebla = new LinearGradientBrush(
                new Rectangle(0, h - 180, w, 180),
                Color.Transparent,
                Color.FromArgb(170, 45, 75, 145),
                LinearGradientMode.Vertical);
            g.FillRectangle(brNiebla, 0, h - 180, w, 180);

            g.FillPolygon(new SolidBrush(Color.FromArgb(10, 14, 24)), new Point[]
            {
                new Point(0, h), new Point(0, h - 160),
                new Point(200, h - 140), new Point(320, h - 60), new Point(420, h)
            });

            // Silueta del Guerrero NEXUS
            int gx = 150;
            int gy = h - 275;
            g.FillEllipse(new SolidBrush(Color.FromArgb(15, 22, 36)), gx + 14, gy, 24, 24);
            g.FillRectangle(Brushes.Cyan, gx + 28, gy + 8, 9, 4);
            g.FillRectangle(new SolidBrush(Color.FromArgb(18, 26, 42)), gx + 10, gy + 24, 30, 46);
            g.FillRectangle(new SolidBrush(Color.FromArgb(15, 22, 36)), gx + 12, gy + 70, 11, 45);
            g.FillRectangle(new SolidBrush(Color.FromArgb(15, 22, 36)), gx + 27, gy + 70, 11, 45);
            g.FillRectangle(Brushes.Black, gx + 34, gy + 46, 34, 9);
            g.FillRectangle(Brushes.Cyan, gx + 38, gy + 48, 24, 4);

            // 6. Marco HUD Sci-Fi completo alrededor de toda la ventana
            using Pen pHudCian = new Pen(Color.FromArgb(0, 240, 255), 3);
            using Pen pHudMagenta = new Pen(Color.FromArgb(255, 45, 115), 3);

            g.DrawLines(pHudCian, new Point[] { new Point(25, 140), new Point(25, 25), new Point(w / 2 - 50, 25) });
            g.DrawLines(pHudCian, new Point[] { new Point(25, h - 140), new Point(25, h - 25), new Point(w / 2 - 50, h - 25) });

            g.DrawLines(pHudMagenta, new Point[] { new Point(w - 25, 140), new Point(w - 25, 25), new Point(w / 2 + 50, 25) });
            g.DrawLines(pHudMagenta, new Point[] { new Point(w - 25, h - 140), new Point(w - 25, h - 25), new Point(w / 2 + 50, h - 25) });

            DibujarFondoParticulas(g, w, h);
        }
        private void IniciarAnimacionIntro()
        {
            textoIntroCompleto =
                "BITÁCORA CLASIFICADA // AÑO 2148 — HORIZONTE OMEGA\n" +
                "------------------------------------------------------------------------\n" +
                "Sobre las ruinas de Aetheris-9 se alza la Ciudadela Omega: una brecha\n" +
                "dimensional que desató al Enjambre Xenomorfo y corrompió la red robótica.\n\n" +
                "Como operativo NEXUS, cruzarás los 4 sectores críticos hasta destruir a\n" +
                "los Jefes Finales Gigantes antes de que la fisura consuma el planeta.\n\n" +
                "• [A/D] Avanzar/Retroceder  |  [ESPACIO] Saltar  |  [P] Pausar combate.\n" +
                "• Recoge el JETPACK en el suelo para volar en 4 direcciones (W/A/S/D).\n" +
                "• Desbloquea las 6 armas experimentales y evoluciona tu traje con XP/CR.\n\n" +
                ">>> ENLACE NEURONAL LISTO. PULSA INICIAR PARA EL DESPLIEGUE...";

            indiceLetraIntro = 0;
            lblTextoIntro.Text = "";
            lblTextoIntro.Click += (s, e) => { lblTextoIntro.Text = textoIntroCompleto; tmrIntroTexto.Stop(); };

            tmrIntroTexto = new System.Windows.Forms.Timer { Interval = 10 };
            tmrIntroTexto.Tick += (s, e) =>
            {
                if (indiceLetraIntro < textoIntroCompleto.Length)
                {
                    // Avanza 4 caracteres por tick para lectura rápida y fluida
                    int paso = Math.Min(4, textoIntroCompleto.Length - indiceLetraIntro);
                    lblTextoIntro.Text += textoIntroCompleto.Substring(indiceLetraIntro, paso);
                    indiceLetraIntro += paso;
                }
                else tmrIntroTexto.Stop();
            };
            tmrIntroTexto.Start();
        }

        // =========================================================================
        // PANTALLA 2: REGISTRO DE NOMBRE Y MODALIDAD (1P O 2P)
        // =========================================================================
        private void ConstruirPantallaRegistro()
        {

            pnlRegistro = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(10, 14, 20), Visible = false };

            Button btnVolverIntro = CrearBotonNeon("<< VOLVER AL INICIO", new Point(35, 25), new Size(210, 40), Color.FromArgb(150, 165, 185));
            btnVolverIntro.Click += (s, e) => { MostrarPantalla(pnlIntro); IniciarAnimacionIntro(); };

            Label lblTitulo = new Label
            {
                Text = "// CONFIGURACIÓN DE ESCUADRÓN Y MODALIDAD",
                Font = new Font("Consolas", 20F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 240, 255),
                Location = new Point(260, 25),
                Size = new Size(850, 45),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Panel cardRegistro = new Panel { Location = new Point(360, 120), Size = new Size(640, 480), BackColor = Color.FromArgb(16, 22, 32) };
            cardRegistro.Paint += (s, e) =>
            {
                using Pen p = new Pen(Color.FromArgb(0, 240, 255), 2);
                e.Graphics.DrawRectangle(p, 1, 1, cardRegistro.Width - 3, cardRegistro.Height - 3);
            };

            Label lblModo = new Label { Text = "1. SELECCIONA MODALIDAD DE JUEGO:", Font = new Font("Consolas", 12F, FontStyle.Bold), ForeColor = Color.White, Location = new Point(40, 30), Size = new Size(540, 28) };

            rb1Jugador = new RadioButton { Text = "UN JUGADOR (Incursión Solitaria)", Font = new Font("Consolas", 11.5F, FontStyle.Bold), ForeColor = Color.FromArgb(80, 250, 123), Location = new Point(55, 68), Size = new Size(500, 32), Checked = true };
            rb2Jugadores = new RadioButton { Text = "DOS JUGADORES (Cooperativo Táctico)", Font = new Font("Consolas", 11.5F, FontStyle.Bold), ForeColor = Color.FromArgb(189, 147, 249), Location = new Point(55, 105), Size = new Size(520, 32) };

            Label lblJ1 = new Label { Text = "2. NOMBRE / CALLSIGN OPERATIVO 01:", Font = new Font("Consolas", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(0, 240, 255), Location = new Point(40, 160), Size = new Size(540, 28) };
            txtNombre1 = new TextBox { Text = "LIXER", Font = new Font("Consolas", 14F, FontStyle.Bold), BackColor = Color.FromArgb(26, 34, 48), ForeColor = Color.White, Location = new Point(40, 192), Size = new Size(550, 34), MaxLength = 16 };

            Label lblJ2 = new Label { Text = "3. NOMBRE / CALLSIGN OPERATIVO 02 (Solo 2 Jugadores):", Font = new Font("Consolas", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(189, 147, 249), Location = new Point(40, 248), Size = new Size(560, 28) };
            txtNombre2 = new TextBox { Text = "NOVA", Font = new Font("Consolas", 14F, FontStyle.Bold), BackColor = Color.FromArgb(26, 34, 48), ForeColor = Color.White, Location = new Point(40, 280), Size = new Size(550, 34), MaxLength = 16, Enabled = false };

            rb2Jugadores.CheckedChanged += (s, e) => txtNombre2.Enabled = rb2Jugadores.Checked;

            Button btnIrHangar = CrearBotonNeon("CONTINUAR AL HANGAR DE ARMAS  >>", new Point(120, 370), new Size(400, 60), Color.FromArgb(0, 240, 255));
            btnIrHangar.Click += (s, e) =>
            {
                esMultijugador = rb2Jugadores.Checked;
                nombreJ1 = string.IsNullOrWhiteSpace(txtNombre1.Text) ? "LIXER" : txtNombre1.Text.Trim().ToUpper();
                nombreJ2 = string.IsNullOrWhiteSpace(txtNombre2.Text) ? "NOVA" : txtNombre2.Text.Trim().ToUpper();

                escuadron.Clear();
                pasoSeleccionJugador = 1;
                ActualizarTituloHangar();
                MostrarPantalla(pnlHangar);
            };

            cardRegistro.Controls.AddRange(new Control[] { lblModo, rb1Jugador, rb2Jugadores, lblJ1, txtNombre1, lblJ2, txtNombre2, btnIrHangar });
            pnlRegistro.Controls.AddRange(new Control[] { btnVolverIntro, lblTitulo, cardRegistro });
            this.Controls.Add(pnlRegistro);
        }

        // =========================================================================
        // PANTALLA 3: HANGAR CON IMAGEN DE PERSONAJES Y VISTA PREVIA DE ARMAS
        // =========================================================================
        private void ConstruirPantallaHangar()
        {
            pnlHangar = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(10, 14, 20), Visible = false };
            previewsHangar.Clear();

            Button btnVolverInicio = CrearBotonNeon("<< INICIO", new Point(35, 22), new Size(150, 38), Color.FromArgb(150, 165, 185));
            btnVolverInicio.Click += (s, e) => MostrarPantalla(pnlRegistro);

            lblTituloHangar = new Label
            {
                Text = "// HANGAR DE DESPLIEGUE — CONFIGURANDO JUGADOR 1",
                Font = new Font("Consolas", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 240, 255),
                Location = new Point(210, 22),
                Size = new Size(940, 38),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Panel pnlArmaInit = new Panel { Location = new Point(140, 72), Size = new Size(1060, 66), BackColor = Color.FromArgb(18, 24, 35) };
            pnlArmaInit.Paint += (s, e) =>
            {
                using Pen p = new Pen(Color.FromArgb(60, 75, 95), 1);
                e.Graphics.DrawRectangle(p, 0, 0, pnlArmaInit.Width - 1, pnlArmaInit.Height - 1);
            };

            Label lblArm = new Label { Text = "ARMA INICIAL EQUIPADA:", Font = new Font("Consolas", 11F, FontStyle.Bold), ForeColor = Color.White, Location = new Point(20, 21), Size = new Size(235, 25) };

            cmbArmaInicial = new ComboBox
            {
                Location = new Point(260, 18),
                Size = new Size(380, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(28, 36, 50),
                ForeColor = Color.Cyan,
                Font = new Font("Consolas", 10.5F, FontStyle.Bold)
            };
            cmbArmaInicial.Items.Add("Pistola de Pulso MK-I (24-34 DMG | 10 EN)");
            cmbArmaInicial.Items.Add("Rifle Criogénico Sub-Zero (32-46 DMG | 15 EN)");
            cmbArmaInicial.SelectedIndex = 0;

            picPreviewArmaInicial = new PictureBox
            {
                Location = new Point(660, 10),
                Size = new Size(110, 46),
                BackColor = Color.FromArgb(12, 16, 24)
            };
            picPreviewArmaInicial.Paint += (s, e) =>
            {
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                var armaSel = catalogoArmas[cmbArmaInicial.SelectedIndex];
                DibujarArmaDetalladaPixelArt(e.Graphics, 16, 12, armaSel, 1, 2);
            };
            cmbArmaInicial.SelectedIndexChanged += (s, e) => picPreviewArmaInicial.Invalidate();

            Label lblInfoExtra = new Label { Text = "+ 4 armas únicas en la armería.", Font = new Font("Consolas", 10F), ForeColor = Color.FromArgb(150, 165, 180), Location = new Point(785, 22), Size = new Size(265, 25) };

            pnlArmaInit.Controls.AddRange(new Control[] { lblArm, cmbArmaInicial, picPreviewArmaInicial, lblInfoExtra });
            pnlHangar.Controls.AddRange(new Control[] { btnVolverInicio, lblTituloHangar, pnlArmaInit });

            int x = 140;
            foreach (var cls in catalogoClases)
            {
                Panel card = new Panel { Location = new Point(x, 152), Size = new Size(330, 535), BackColor = Color.FromArgb(16, 22, 32) };
                card.Paint += (s, e) =>
                {
                    using Pen p = new Pen(cls.ColorTema, 2);
                    e.Graphics.DrawRectangle(p, 1, 1, card.Width - 3, card.Height - 3);
                    using SolidBrush b = new SolidBrush(cls.ColorTema);
                    e.Graphics.FillRectangle(b, 0, 0, card.Width, 8);
                };

                Label lNom = new Label { Text = cls.Nombre, Font = new Font("Consolas", 15F, FontStyle.Bold), ForeColor = cls.ColorTema, Location = new Point(15, 18), Size = new Size(300, 28), TextAlign = ContentAlignment.MiddleCenter };
                Label lRol = new Label { Text = $"[{cls.Rol.ToUpper()}]", Font = new Font("Consolas", 10F, FontStyle.Bold), ForeColor = Color.White, Location = new Point(15, 46), Size = new Size(300, 20), TextAlign = ContentAlignment.MiddleCenter };

                PictureBox picPreview = new PictureBox
                {
                    Location = new Point(95, 74),
                    Size = new Size(140, 145),
                    BackColor = Color.FromArgb(11, 15, 24)
                };
                picPreview.Paint += (s, e) =>
                {
                    e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                    e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
                    using Pen b = new Pen(Color.FromArgb(50, cls.ColorTema), 1);
                    e.Graphics.DrawRectangle(b, 0, 0, picPreview.Width - 1, picPreview.Height - 1);

                    e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(120, 80, 90, 105)), 38, 124, 64, 10);
                    int frameAnim = ((int)(pulsoNeon * 1.5f)) % 2;
                    Bitmap sp = ObtenerSpriteOperativo(cls.Nombre, cls.ColorTema, frameAnim, false);
                    e.Graphics.DrawImage(sp, 32, 16, 76, 112);
                };
                previewsHangar.Add(picPreview);

                Label lDes = new Label { Text = cls.Descripcion, Font = new Font("Consolas", 9.8F), ForeColor = Color.FromArgb(205, 215, 225), Location = new Point(18, 232), Size = new Size(295, 220) };

                Button btnSel = CrearBotonNeon("SELECCIONAR OPERATIVO", new Point(25, 462), new Size(280, 50), cls.ColorTema);
                btnSel.Click += (s, e) => ConfirmarSeleccionPersonaje(cls);

                card.Controls.AddRange(new Control[] { lNom, lRol, picPreview, lDes, btnSel });
                pnlHangar.Controls.Add(card);
                x += 365;
            }

            this.Controls.Add(pnlHangar);
        }

        private void ActualizarTituloHangar()
        {
            string nombreActual = pasoSeleccionJugador == 1 ? nombreJ1 : nombreJ2;
            lblTituloHangar.Text = $"// HANGAR DE DESPLIEGUE — OPERATIVO 0{pasoSeleccionJugador}: {nombreActual}";
        }

        private void ConfirmarSeleccionPersonaje(ClaseOperativo cls)
        {
            string nombreOp = pasoSeleccionJugador == 1 ? nombreJ1 : nombreJ2;
            var armaBase = catalogoArmas[cmbArmaInicial.SelectedIndex];

            var armaJugador = new ArmaFuturista
            {
                IdVisual = armaBase.IdVisual,
                Nombre = armaBase.Nombre,
                DanioMin = armaBase.DanioMin,
                DanioMax = armaBase.DanioMax,
                CostoEnergia = armaBase.CostoEnergia,
                Desbloqueada = true,
                ColorProyectil = armaBase.ColorProyectil
            };

            escuadron.Add(new JugadorActivo
            {
                Entidad = new Cadete(nombreOp, cls.Rol, cls.SaludMax, cls.EnergiaMax),
                Clase = cls,
                Arma = armaJugador
            });

            if (esMultijugador && pasoSeleccionJugador == 1)
            {
                pasoSeleccionJugador = 2;
                ActualizarTituloHangar();
                MessageBox.Show($"Operativo {nombreJ1} listo. Ahora configura al Operativo 02 ({nombreJ2}).", "NEXUS Escuadrón", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MostrarPantalla(pnlMapas);
            }
        }

        // =========================================================================
        // PANTALLA 4: SELECCIÓN DE 4 MAPAS Y DIFICULTAD
        // =========================================================================
        private void ConstruirPantallaMapas()
        {
            pnlMapas = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(10, 14, 20), Visible = false };

            Button btnVolverInicio = CrearBotonNeon("<< VOLVER AL INICIO", new Point(35, 22), new Size(200, 38), Color.FromArgb(150, 165, 185));
            btnVolverInicio.Click += (s, e) => MostrarPantalla(pnlRegistro);

            Label lblTit = new Label
            {
                Text = "// SELECCIÓN DE SECTOR Y NIVEL DE DIFICULTAD",
                Font = new Font("Consolas", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 240, 255),
                Location = new Point(250, 22),
                Size = new Size(920, 38),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Panel pnlDif = new Panel { Location = new Point(200, 75), Size = new Size(960, 65), BackColor = Color.FromArgb(16, 22, 32) };
            lblDificultadActual = new Label
            {
                Text = "DIFICULTAD: INTERMEDIO (Jefe Final en Oleada 3)",
                Font = new Font("Consolas", 10.5F, FontStyle.Bold),
                ForeColor = Color.Gold,
                Location = new Point(20, 20),
                Size = new Size(560, 25)
            };

            Button btnFacil = CrearBotonNeon("FÁCIL", new Point(595, 14), new Size(105, 38), Color.FromArgb(80, 250, 123));
            Button btnMedio = CrearBotonNeon("INTERMEDIO", new Point(710, 14), new Size(115, 38), Color.Gold);
            Button btnDificil = CrearBotonNeon("DIFÍCIL", new Point(835, 14), new Size(105, 38), Color.FromArgb(255, 85, 95));

            btnFacil.Click += (s, e) => { dificultad = "FÁCIL"; multDificultad = 0.85; lblDificultadActual.Text = "DIFICULTAD: FÁCIL (Enemigos estándar)"; lblDificultadActual.ForeColor = Color.FromArgb(80, 250, 123); };
            btnMedio.Click += (s, e) => { dificultad = "INTERMEDIO"; multDificultad = 1.15; lblDificultadActual.Text = "DIFICULTAD: INTERMEDIO (Jefe Final en Oleada 3)"; lblDificultadActual.ForeColor = Color.Gold; };
            btnDificil.Click += (s, e) => { dificultad = "DIFÍCIL"; multDificultad = 1.75; lblDificultadActual.Text = "DIFICULTAD: DIFÍCIL (Jefe Gigante con Mega Ataques)"; lblDificultadActual.ForeColor = Color.FromArgb(255, 85, 95); };

            pnlDif.Controls.AddRange(new Control[] { lblDificultadActual, btnFacil, btnMedio, btnDificil });
            pnlMapas.Controls.AddRange(new Control[] { btnVolverInicio, lblTit, pnlDif });

            int[,] coords = { { 200, 160 }, { 695, 160 }, { 200, 425 }, { 695, 425 } };
            for (int i = 0; i < catalogoMapas.Count; i++)
            {
                var mapa = catalogoMapas[i];
                Panel cardMapa = new Panel { Location = new Point(coords[i, 0], coords[i, 1]), Size = new Size(465, 245), BackColor = mapa.ColorFondo };
                cardMapa.Paint += (s, e) =>
                {
                    using Pen p = new Pen(mapa.ColorPrimario, 2);
                    e.Graphics.DrawRectangle(p, 1, 1, cardMapa.Width - 3, cardMapa.Height - 3);
                };

                Label lNom = new Label { Text = mapa.Nombre, Font = new Font("Consolas", 13F, FontStyle.Bold), ForeColor = mapa.ColorPrimario, Location = new Point(15, 15), Size = new Size(435, 28) };
                Label lSub = new Label { Text = mapa.Subtitulo, Font = new Font("Consolas", 10F, FontStyle.Italic), ForeColor = Color.White, Location = new Point(15, 45), Size = new Size(435, 22) };
                Label lEfe = new Label { Text = mapa.EfectoUnico, Font = new Font("Consolas", 10F), ForeColor = Color.FromArgb(220, 230, 240), Location = new Point(15, 78), Size = new Size(435, 85) };

                Button btnDesplegar = CrearBotonNeon("DESPLEGAR EN ESTE SECTOR", new Point(15, 175), new Size(435, 48), mapa.ColorPrimario);
                btnDesplegar.Click += (s, e) => IniciarCombateEnMapa(mapa);

                cardMapa.Controls.AddRange(new Control[] { lNom, lSub, lEfe, btnDesplegar });
                pnlMapas.Controls.Add(cardMapa);
            }

            this.Controls.Add(pnlMapas);
        }

        // =========================================================================
        // PANTALLA 5: ARENA DE COMBATE PIXEL ART FULLSCREEN CON JETPACK Y JEFE FINAL
        // =========================================================================
        private void ConstruirPantallaArena()
        {
            pnlArena = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(8, 11, 16), Visible = false };

            Panel pnlDerecha = new Panel
            {
                Dock = DockStyle.Right,
                Width = 400,
                BackColor = Color.FromArgb(11, 15, 22),
                Padding = new Padding(10)
            };

            pnlStatsEscuadron = new Panel
            {
                Location = new Point(10, 12),
                Size = new Size(380, 330),
                BackColor = Color.FromArgb(15, 20, 29)
            };
            pnlStatsEscuadron.Paint += DibujarStatsEscuadron;

            Panel pnlMejoras = new Panel
            {
                Location = new Point(10, 352),
                Size = new Size(380, 395),
                BackColor = Color.FromArgb(15, 20, 29)
            };
            pnlMejoras.Paint += (s, e) =>
            {
                using Pen p = new Pen(Color.FromArgb(50, 65, 85), 1);
                e.Graphics.DrawRectangle(p, 0, 0, pnlMejoras.Width - 1, pnlMejoras.Height - 1);
            };

            Label lblMejTit = new Label { Text = "// EVOLUCIÓN (XP) Y ARMERÍA (6 ARMAS)", Font = new Font("Consolas", 9.8F, FontStyle.Bold), ForeColor = Color.FromArgb(0, 240, 255), Location = new Point(12, 10), Size = new Size(358, 22) };

            btnMejorarArmaXP = CrearBotonNeon("MEJORAR ARMA (+8 DMG) [50 XP]", new Point(12, 38), new Size(356, 38), Color.FromArgb(0, 240, 255));
            btnMejorarArmaXP.Click += BtnMejorarArmaXP_Click;

            btnMejorarHabXP = CrearBotonNeon("POTENCIAR HABILIDAD [60 XP]", new Point(12, 82), new Size(356, 38), Color.FromArgb(189, 147, 249));
            btnMejorarHabXP.Click += BtnMejorarHabXP_Click;

            btnMejorarTrajeXP = CrearBotonNeon("EVOLUCIONAR TRAJE (+30 HP) [45 XP]", new Point(12, 126), new Size(356, 38), Color.FromArgb(80, 250, 123));
            btnMejorarTrajeXP.Click += BtnMejorarTrajeXP_Click;

            cmbTiendaArmas = new ComboBox
            {
                Location = new Point(12, 176),
                Size = new Size(356, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(25, 32, 44),
                ForeColor = Color.Gold,
                Font = new Font("Consolas", 8.8F, FontStyle.Bold)
            };
            for (int i = 2; i < catalogoArmas.Count; i++)
            {
                var a = catalogoArmas[i];
                cmbTiendaArmas.Items.Add($"{a.Nombre} ({a.DanioMin}-{a.DanioMax} DMG) - {a.PrecioCR} CR");
            }
            cmbTiendaArmas.SelectedIndex = 0;

            btnComprarArmaCR = CrearBotonNeon("DESBLOQUEAR Y EQUIPAR ARMA (CR)", new Point(12, 208), new Size(356, 38), Color.Gold);
            btnComprarArmaCR.Click += BtnComprarArmaCR_Click;

            btnComprarBotiquinCR = CrearBotonNeon("COMPRAR BOTIQUÍN NANOBOT [40 CR]", new Point(12, 252), new Size(356, 38), Color.FromArgb(255, 120, 80));
            btnComprarBotiquinCR.Click += BtnComprarBotiquinCR_Click;

            Button btnCambiarSector = CrearBotonNeon("CAMBIAR MAPA", new Point(12, 298), new Size(172, 38), Color.FromArgb(140, 150, 165));
            btnCambiarSector.Click += (s, e) => MostrarPantalla(pnlMapas);

            Button btnVolverMenuPrincipal = CrearBotonNeon("<< MENÚ INICIO", new Point(196, 298), new Size(172, 38), Color.FromArgb(255, 107, 107));
            btnVolverMenuPrincipal.Click += (s, e) => MostrarPantalla(pnlRegistro);

            pnlMejoras.Controls.AddRange(new Control[]
            {
                lblMejTit, btnMejorarArmaXP, btnMejorarHabXP, btnMejorarTrajeXP,
                cmbTiendaArmas, btnComprarArmaCR, btnComprarBotiquinCR, btnCambiarSector, btnVolverMenuPrincipal
            });

            pnlDerecha.Controls.AddRange(new Control[] { pnlStatsEscuadron, pnlMejoras });

            Panel pnlIzquierda = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(8, 11, 16),
                Padding = new Padding(14)
            };

            Panel pnlAccionesInferior = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 135,
                BackColor = Color.Transparent
            };

            btnAtacar = CrearBotonNeon("DISPARAR ARMA", new Point(0, 8), new Size(440, 56), Color.FromArgb(255, 75, 85));
            btnAtacar.Click += BtnAtacar_Click;

            btnHabilidad = CrearBotonNeon("HABILIDAD ESPECIAL", new Point(455, 8), new Size(440, 56), Color.FromArgb(189, 147, 249));
            btnHabilidad.Click += BtnHabilidad_Click;

            btnCurar = CrearBotonNeon("USAR BOTIQUÍN (CURA + DETIENE SANGRADO)", new Point(0, 72), new Size(440, 56), Color.FromArgb(80, 250, 123));
            btnCurar.Click += BtnCurar_Click;

            btnRecargar = CrearBotonNeon("RECARGAR NÚCLEO (+50 EN)", new Point(455, 72), new Size(440, 56), Color.FromArgb(0, 240, 255));
            btnRecargar.Click += BtnRecargar_Click;

            pnlAccionesInferior.Controls.AddRange(new Control[] { btnAtacar, btnHabilidad, btnCurar, btnRecargar });

            lstConsola = new ListBox
            {
                Dock = DockStyle.Bottom,
                Height = 165,
                BackColor = Color.FromArgb(12, 16, 23),
                ForeColor = Color.FromArgb(215, 225, 235),
                Font = new Font("Consolas", 9.8F),
                BorderStyle = BorderStyle.FixedSingle
            };

            picArena2D = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(10, 15, 24)
            };
            picArena2D.Paint += DibujarArena2D_PixelArt;

            Button btnMoverAtras = CrearBotonNeon("◄ ATRÁS (A)", new Point(16, 42), new Size(125, 32), Color.FromArgb(0, 240, 255));
            btnMoverAtras.MouseDown += (s, e) => { teclaIzquierda = true; this.Focus(); };
            btnMoverAtras.MouseUp += (s, e) => { teclaIzquierda = false; this.Focus(); };

            Button btnSaltar = CrearBotonNeon("▲ SALTAR / SUBIR (W/ESP)", new Point(148, 42), new Size(195, 32), Color.Gold);
            btnSaltar.MouseDown += (s, e) => { teclaArriba = true; SaltarJugadorActivo(); this.Focus(); };
            btnSaltar.MouseUp += (s, e) => { teclaArriba = false; this.Focus(); };

            Button btnMoverAdelante = CrearBotonNeon("ADELANTE (D) ►", new Point(350, 42), new Size(140, 32), Color.FromArgb(0, 240, 255));
            btnMoverAdelante.MouseDown += (s, e) => { teclaDerecha = true; this.Focus(); };
            btnMoverAdelante.MouseUp += (s, e) => { teclaDerecha = false; this.Focus(); };

            btnPausa = CrearBotonNeon("❚❚ PAUSAR (P)", new Point(498, 42), new Size(155, 32), Color.Gold);
            btnPausa.Click += (s, e) => { AlternarPausa(); this.Focus(); };

            picArena2D.Controls.AddRange(new Control[] { btnMoverAtras, btnSaltar, btnMoverAdelante, btnPausa });

            pnlIzquierda.Controls.Add(picArena2D);
            pnlIzquierda.Controls.Add(lstConsola);
            pnlIzquierda.Controls.Add(pnlAccionesInferior);

            pnlArena.Controls.Add(pnlIzquierda);
            pnlArena.Controls.Add(pnlDerecha);
            this.Controls.Add(pnlArena);
        }

        private void IniciarCombateEnMapa(MapaSector mapa)
        {
            mapaActual = mapa;
            oleada = 1;
            indiceTurnoJugador = 0;
            desplazamientoMapa = 0f;
            juegoPausado = false;
            proyectiles.Clear();

            itemJetpack.Activo = true;
            itemJetpack.X = 280f;
            itemJetpack.Y = ObtenerSueloY() - 55f;

            for (int i = 0; i < escuadron.Count; i++)
            {
                var j = escuadron[i];
                j.Entidad.Salud = j.Entidad.SaludMaxima;
                j.Entidad.Energia = j.Entidad.EnergiaMaxima;
                j.Escudo = 0;
                j.TurnosDesangrado = 0;
                j.CombustibleJetpack = 0;
                j.PosX = 85f + (i * 55f);
                j.PosY = ObtenerSueloY();
                j.VelY = 0f;
            }

            lstConsola.Items.Clear();
            LogArena("===========================================================================");
            LogArena($"  DESPLIEGUE EN {mapa.Nombre}  |  DIFICULTAD: {dificultad}");
            LogArena("===========================================================================");
            LogArena("INFO: Recoge la Mochila Cohete (JETPACK) para volar en 4 direcciones (W/A/S/D).");
            LogArena($"ALERTA DE SECTOR: En la Oleada {OLEADA_JEFE_FINAL} aparecerá el JEFE FINAL GIGANTE.");
            LogArena("");

            GenerarOleadaAmenazas();
            MostrarPantalla(pnlArena);
            ActualizarHUDCompleto();
        }

        // =========================================================================
        // 6. GENERACIÓN DE OLEADAS Y JEFE FINAL GIGANTE
        // =========================================================================
        private void GenerarOleadaAmenazas()
        {
            enemigosActivos.Clear();
            float sueloY = ObtenerSueloY();

            // OLEADA 3: JEFE FINAL GIGANTE DEL SECTOR
            if (oleada >= OLEADA_JEFE_FINAL)
            {
                bool esAlien = mapaActual.CodigoEfecto == "GRAVEDAD" || mapaActual.CodigoEfecto == "BOSQUE";
                int hpJefe = (int)((680 + (oleada * 180)) * multDificultad * (dificultad == "DIFÍCIL" ? 1.55 : 1.0));
                int dmgJefe = (int)((24 + (oleada * 5)) * multDificultad);

                string nombreJefe = mapaActual.CodigoEfecto switch
                {
                    "ESTATICA" => "LEVIATÁN MECHA CIBERPUNK",
                    "BOSQUE" => "TITÁN SIMBIONTE ANCESTRAL",
                    "RADIACION" => "COLOSO TERMONUCLEAR OMEGA",
                    _ => "EMPERATRIZ XENOMORFA ABISAL"
                };

                enemigosActivos.Add(new Enemigo
                {
                    Nombre = nombreJefe,
                    EsAlien = esAlien,
                    EsJefe = true,
                    Salud = hpJefe,
                    SaludMax = hpJefe,
                    DanioMin = dmgJefe,
                    DanioMax = dmgJefe + 14,
                    RecompensaCR = (int)(320 * multDificultad),
                    RecompensaXP = (int)(220 * multDificultad),
                    ColorAura = Color.Gold,
                    PosX = 560f,
                    PosY = sueloY - 60f,
                    VX = -1.9f,
                    VY = -1.6f,
                    CooldownDisparo = 45,
                    CooldownInvocacion = 150,
                    CooldownMegaAtaque = 180,
                    FaseAnim = 0f
                });

                framesSacudida = 18;
                LogArena($"[!!! JEFE FINAL DETECTADO !!!] {nombreJefe} ({hpJefe} HP) — Invocará aliados hasta caer.");
                return;
            }

            // Oleadas normales (1 y 2)
            int cantidadEnemigos = dificultad == "FÁCIL" ? 2
                                 : dificultad == "INTERMEDIO" ? 3
                                 : 4;

            string[] robots = { "DRON DE ASALTO AÉREO", "CENTINELA VOLADOR MK-V", "INTERCEPTOR OMEGA" };
            string[] aliens = { "XENOMORFO ACECHADOR", "DEVORADOR ÁCIDO", "PARÁSITO ABISAL" };

            for (int i = 0; i < cantidadEnemigos; i++)
            {
                bool esAlien = (mapaActual.CodigoEfecto == "GRAVEDAD" || mapaActual.CodigoEfecto == "BOSQUE") ? (rng.Next(100) < 50) : (i % 2 == 1);
                int hpBase = 145 + (oleada * 40);
                int dmgBase = 13 + (oleada * 3);
                int saludFinal = (int)(hpBase * multDificultad * (esMultijugador ? 1.45 : 1.0));

                float vx = (float)(rng.NextDouble() * 2.4 + 1.5) * (i % 2 == 0 ? -1 : 1);
                float vy = esAlien ? 0f : (float)(rng.NextDouble() * 2.2 + 1.2) * (i % 2 == 0 ? 1 : -1);

                enemigosActivos.Add(new Enemigo
                {
                    Nombre = esAlien ? aliens[rng.Next(aliens.Length)] : robots[rng.Next(robots.Length)],
                    EsAlien = esAlien,
                    EsJefe = false,
                    Salud = saludFinal,
                    SaludMax = saludFinal,
                    DanioMin = (int)(dmgBase * multDificultad),
                    DanioMax = (int)((dmgBase + 9) * multDificultad),
                    RecompensaCR = (int)(65 * multDificultad),
                    RecompensaXP = (int)(50 * multDificultad),
                    ColorAura = esAlien ? Color.FromArgb(80, 250, 123) : Color.FromArgb(255, 75, 85),
                    PosX = 460f + (i * 85f),
                    PosY = esAlien ? sueloY : rng.Next(70, (int)sueloY - 40),
                    VX = vx,
                    VY = vy,
                    CooldownDisparo = 35 + (i * 25),
                    FaseAnim = i * 1.5f
                });
            }

            LogArena($"[RADAR] ¡Oleada {oleada}/{OLEADA_JEFE_FINAL}! {enemigosActivos.Count} amenazas detectadas.");
        }

        // =========================================================================
        // 7. COMBATE, CURACIÓN, DESANGRADO Y EXPLOSIONES
        // =========================================================================
        private JugadorActivo JugadorEnTurno() => escuadron[indiceTurnoJugador];

        private void BtnAtacar_Click(object? sender, EventArgs e)
        {
            this.Focus();
            if (juegoPausado) return;

            if (escuadron.TrueForAll(j => !j.Vivo))
            {
                IniciarCombateEnMapa(mapaActual);
                return;
            }

            // Priorizar al Jefe Final o al enemigo más cercano
            var objetivo = enemigosActivos.Find(en => en.Vivo && en.EsJefe) ?? enemigosActivos.Find(en => en.Vivo);
            if (objetivo == null)
            {
                GenerarOleadaAmenazas();
                ActualizarHUDCompleto();
                return;
            }

            var j = JugadorEnTurno();
            if (j.Entidad.Energia < j.Arma.CostoEnergia)
            {
                LogArena($"[!] {j.Entidad.Nombre} requiere {j.Arma.CostoEnergia} EN. Recarga el núcleo.");
                return;
            }

            j.Entidad.Energia -= j.Arma.CostoEnergia;
            int danio = rng.Next(j.Arma.DanioMin, j.Arma.DanioMax + 1);

            if (mapaActual.CodigoEfecto == "RADIACION") danio = (int)(danio * 1.20);

            int critTotal = j.Clase.ProbCritico + (mapaActual.CodigoEfecto == "GRAVEDAD" ? 20 : 0);
            bool critico = rng.Next(1, 101) <= critTotal;
            if (critico) danio = (int)(danio * 2.1);

            objetivo.Salud = Math.Max(0, objetivo.Salud - danio);

            float dx = (objetivo.PosX + (objetivo.EsJefe ? 45 : 20)) - (j.PosX + 42);
            float dy = (objetivo.PosY + (objetivo.EsJefe ? 50 : 25)) - (j.PosY + 26);
            float dist = Math.Max(1f, (float)Math.Sqrt(dx * dx + dy * dy));

            proyectiles.Add(new Proyectil2D
            {
                X = j.PosX + 42,
                Y = j.PosY + 26,
                VX = (dx / dist) * 24f,
                VY = (dy / dist) * 24f,
                Tamano = 7 + j.Arma.IdVisual,
                Color = j.Arma.ColorProyectil,
                EsDeJugador = true,
                Danio = danio
            });

            LogArena($"[{j.Entidad.Nombre}] Dispara a {objetivo.Nombre} -> {danio} DMG{(critico ? " ¡CRÍTICO!" : "")}");
            AvanzarTurnoCombate();
        }

        private void BtnHabilidad_Click(object? sender, EventArgs e)
        {
            this.Focus();
            if (juegoPausado) return;

            var j = JugadorEnTurno();
            int bonusNivel = (j.NivelHabilidad - 1) * 18;

            if (j.Clase.Nombre == "VANGUARD")
            {
                if (j.Entidad.Energia < 30) { LogArena("[!] Requiere 30 EN."); return; }
                j.Entidad.Energia -= 30;
                int escudoGanado = 65 + bonusNivel;
                j.Escudo += escudoGanado;
                j.TurnosDesangrado = 0;
                LogArena($"[{j.Entidad.Nombre}] BARRERA DE PLASMA (+{escudoGanado} Escudo y frena sangrado).");
            }
            else if (j.Clase.Nombre == "SPECTRE")
            {
                if (j.Entidad.Energia < 35) { LogArena("[!] Requiere 35 EN."); return; }
                j.Entidad.Energia -= 35;
                int danio = (int)(j.Arma.DanioMax * 2.1) + bonusNivel;
                foreach (var en in enemigosActivos)
                {
                    if (en.Vivo)
                    {
                        en.Salud = Math.Max(0, en.Salud - danio);
                        CrearExplosionPixelArt(en.PosX + 25, en.PosY + 25, Color.Cyan, 16);
                    }
                }
                LogArena($"[{j.Entidad.Nombre}] ¡CORTE DIMENSIONAL! Daña a TODAS las amenazas ({danio} DMG).");
            }
            else if (j.Clase.Nombre == "NETRUNNER")
            {
                if (j.Entidad.Energia < 40) { LogArena("[!] Requiere 40 EN."); return; }
                j.Entidad.Energia -= 40;
                int danio = 45 + bonusNivel;
                int cura = 35 + (j.NivelHabilidad * 8);
                foreach (var en in enemigosActivos)
                {
                    if (en.Vivo) en.Salud = Math.Max(0, en.Salud - danio);
                }
                foreach (var aliado in escuadron)
                {
                    if (aliado.Vivo)
                    {
                        aliado.Entidad.Salud = Math.Min(aliado.Entidad.SaludMaxima, aliado.Entidad.Salud + cura);
                        aliado.TurnosDesangrado = 0;
                    }
                }
                ActivarEfectoCuracionVisual(j);
                LogArena($"[{j.Entidad.Nombre}] SOBRECARGA EMP: -{danio} HP enemigos | +{cura} HP escuadrón.");
            }

            AvanzarTurnoCombate();
        }

        private void BtnCurar_Click(object? sender, EventArgs e)
        {
            this.Focus();
            if (juegoPausado) return;

            var j = JugadorEnTurno();
            if (j.Botiquines <= 0) { LogArena($"[!] {j.Entidad.Nombre} no tiene botiquines."); return; }
            if (j.Entidad.Salud >= j.Entidad.SaludMaxima && j.TurnosDesangrado == 0) { LogArena("[INFO] Salud al 100% y sin heridas."); return; }

            j.Botiquines--;
            int cura = 65;
            j.Entidad.Salud = Math.Min(j.Entidad.SaludMaxima, j.Entidad.Salud + cura);
            j.TurnosDesangrado = 0;

            ActivarEfectoCuracionVisual(j);
            LogArena($"[CURACIÓN] {j.Entidad.Nombre} aplica nanobots (+{cura} HP) y detiene el DESANGRADO.");
            AvanzarTurnoCombate();
        }

        private void ActivarEfectoCuracionVisual(JugadorActivo j)
        {
            framesCuracion = 28;
            for (int i = 0; i < 24; i++)
            {
                particulas.Add(new ParticulaEfecto
                {
                    X = j.PosX + rng.Next(-8, 42),
                    Y = j.PosY + rng.Next(8, 55),
                    VX = 0f,
                    VY = (float)(rng.NextDouble() * -2.5 - 0.8),
                    Tamano = 5,
                    Vida = 28,
                    Color = Color.FromArgb(80, 250, 123),
                    Tipo = "CURA"
                });
            }
        }

        private void BtnRecargar_Click(object? sender, EventArgs e)
        {
            this.Focus();
            if (juegoPausado) return;

            var j = JugadorEnTurno();
            j.Entidad.Energia = Math.Min(j.Entidad.EnergiaMaxima, j.Entidad.Energia + 50);
            LogArena($"[{j.Entidad.Nombre}] Núcleo estabilizado (+50 EN).");
            AvanzarTurnoCombate();
        }

        private void AvanzarTurnoCombate()
        {
            foreach (var en in enemigosActivos)
            {
                if (!en.Vivo && !en.ExplosionGenerada)
                {
                    en.ExplosionGenerada = true;
                    framesSacudida = en.EsJefe ? 25 : 12;
                    CrearExplosionPixelArt(en.PosX + (en.EsJefe ? 55 : 24), en.PosY + (en.EsJefe ? 55 : 24), en.ColorAura, en.EsJefe ? 85 : 38);
                    LogArena($"[EXPLOSIÓN] ¡{en.Nombre} ha sido destruido!");
                }
            }

            if (enemigosActivos.TrueForAll(en => !en.Vivo))
            {
                int totalCR = 0, totalXP = 0;
                foreach (var en in enemigosActivos)
                {
                    totalCR += en.RecompensaCR;
                    totalXP += en.RecompensaXP;
                }

                bool fueJefe = oleada >= OLEADA_JEFE_FINAL;
                LogArena(fueJefe
                    ? $"[★★★ SECTOR CONQUISTADO ★★★] ¡JEFE FINAL DERROTADO! +{totalCR} CR | +{totalXP} XP"
                    : $"[OLEADA {oleada} COMPLETADA] +{totalCR} CR | +{totalXP} XP para cada operativo.");

                foreach (var j in escuadron)
                {
                    j.Creditos += totalCR;
                    j.XP += totalXP;
                }
                oleada++;
                ActualizarHUDCompleto();
                return;
            }

            var jActual = JugadorEnTurno();
            if (jActual.Vivo && jActual.TurnosDesangrado > 0)
            {
                int danioSangre = 10 + (oleada * 2);
                jActual.Entidad.Salud = Math.Max(0, jActual.Entidad.Salud - danioSangre);
                jActual.TurnosDesangrado--;
                LogArena($"[DESANGRADO] {jActual.Entidad.Nombre} pierde -{danioSangre} HP ({jActual.TurnosDesangrado} turnos rest.).");
            }

            if (esMultijugador)
            {
                int sig = (indiceTurnoJugador + 1) % escuadron.Count;
                if (sig != 0 && escuadron[sig].Vivo)
                {
                    indiceTurnoJugador = sig;
                    AplicarEfectoAmbiental(JugadorEnTurno());
                    ActualizarHUDCompleto();
                    return;
                }
                indiceTurnoJugador = escuadron[0].Vivo ? 0 : 1;
            }

            if (JugadorEnTurno().Vivo) AplicarEfectoAmbiental(JugadorEnTurno());

            if (escuadron.TrueForAll(j => !j.Vivo))
            {
                LogArena("!!! ESCUADRÓN ELIMINADO — MISIÓN FALLIDA !!!");
                btnAtacar.Text = "REINTENTAR SECTOR";
                btnHabilidad.Enabled = false;
                btnCurar.Enabled = false;
                btnRecargar.Enabled = false;
            }

            ActualizarHUDCompleto();
        }

        private void AplicarImpactoEnemigoAJugador(JugadorActivo objetivo, int danioBruto, bool causaSangrado)
        {
            int danioFinal = (int)(danioBruto * (1.0 - objetivo.Clase.ReduccionDanio));

            if (objetivo.Escudo > 0)
            {
                int abs = Math.Min(objetivo.Escudo, danioFinal);
                objetivo.Escudo -= abs;
                danioFinal -= abs;
            }

            if (danioFinal > 0)
            {
                objetivo.Entidad.Salud = Math.Max(0, objetivo.Entidad.Salud - danioFinal);
                framesSacudida = 9;

                if (causaSangrado && objetivo.Escudo == 0)
                {
                    objetivo.TurnosDesangrado = 3;
                    LogArena($"[IMPACTO + SANGRADO] {objetivo.Entidad.Nombre} recibe -{danioFinal} HP y sufre DESANGRADO (3T).");
                }
                else
                {
                    LogArena($"[IMPACTO] {objetivo.Entidad.Nombre} recibe -{danioFinal} HP.");
                }
            }

            if (escuadron.TrueForAll(j => !j.Vivo))
            {
                LogArena("!!! ESCUADRÓN ELIMINADO — MISIÓN FALLIDA !!!");
                btnAtacar.Text = "REINTENTAR SECTOR";
                btnHabilidad.Enabled = false;
                btnCurar.Enabled = false;
                btnRecargar.Enabled = false;
            }
        }

        private void AplicarEfectoAmbiental(JugadorActivo j)
        {
            if (mapaActual.CodigoEfecto == "ESTATICA") j.Entidad.Energia = Math.Min(j.Entidad.EnergiaMaxima, j.Entidad.Energia + 8);
            else if (mapaActual.CodigoEfecto == "BOSQUE") j.Entidad.Salud = Math.Min(j.Entidad.SaludMaxima, j.Entidad.Salud + 6);
        }

        // --- MEJORAS CON XP Y COMPRAS ---
        private void BtnMejorarArmaXP_Click(object? sender, EventArgs e)
        {
            this.Focus();
            var j = JugadorEnTurno();
            if (j.XP < 50) { LogArena($"[XP] {j.Entidad.Nombre} necesita 50 XP."); return; }
            j.XP -= 50;
            j.NivelArma++;
            j.Arma.DanioMin += 8;
            j.Arma.DanioMax += 10;
            LogArena($"[EVOLUCIÓN XP] {j.Arma.Nombre} mejorada a Nv.{j.NivelArma}.");
            ActualizarHUDCompleto();
        }

        private void BtnMejorarHabXP_Click(object? sender, EventArgs e)
        {
            this.Focus();
            var j = JugadorEnTurno();
            if (j.XP < 60) { LogArena($"[XP] {j.Entidad.Nombre} necesita 60 XP."); return; }
            j.XP -= 60;
            j.NivelHabilidad++;
            LogArena($"[EVOLUCIÓN XP] Habilidad potenciada a Nv.{j.NivelHabilidad}.");
            ActualizarHUDCompleto();
        }

        private void BtnMejorarTrajeXP_Click(object? sender, EventArgs e)
        {
            this.Focus();
            var j = JugadorEnTurno();
            if (j.XP < 45) { LogArena($"[XP] {j.Entidad.Nombre} necesita 45 XP."); return; }
            j.XP -= 45;
            j.Entidad.SaludMaxima += 30;
            j.Entidad.Salud += 30;
            LogArena($"[EVOLUCIÓN XP] Traje reforzado (+30 HP Máx).");
            ActualizarHUDCompleto();
        }

        private void BtnComprarArmaCR_Click(object? sender, EventArgs e)
        {
            this.Focus();
            var j = JugadorEnTurno();
            var plantilla = catalogoArmas[cmbTiendaArmas.SelectedIndex + 2];
            if (j.Creditos < plantilla.PrecioCR) { LogArena($"[CR] Créditos insuficientes ({j.Creditos}/{plantilla.PrecioCR})."); return; }

            j.Creditos -= plantilla.PrecioCR;
            j.Arma = new ArmaFuturista
            {
                IdVisual = plantilla.IdVisual,
                Nombre = plantilla.Nombre,
                DanioMin = plantilla.DanioMin + ((j.NivelArma - 1) * 8),
                DanioMax = plantilla.DanioMax + ((j.NivelArma - 1) * 10),
                CostoEnergia = plantilla.CostoEnergia,
                ColorProyectil = plantilla.ColorProyectil,
                Desbloqueada = true
            };
            LogArena($"[ARMERÍA] {j.Entidad.Nombre} equipa {j.Arma.Nombre}.");
            ActualizarHUDCompleto();
        }

        private void BtnComprarBotiquinCR_Click(object? sender, EventArgs e)
        {
            this.Focus();
            var j = JugadorEnTurno();
            if (j.Creditos < 40) { LogArena("[CR] Necesitas 40 CR."); return; }
            j.Creditos -= 40;
            j.Botiquines++;
            LogArena($"[ARMERÍA] Botiquín adquirido (Total: {j.Botiquines}).");
            ActualizarHUDCompleto();
        }

        private void ActualizarHUDCompleto()
        {
            if (escuadron.Exists(j => j.Vivo))
            {
                var j = JugadorEnTurno();
                btnHabilidad.Enabled = true;
                btnCurar.Enabled = true;
                btnRecargar.Enabled = true;
                btnHabilidad.Text = $"{j.Clase.HabilidadNombre} [Nv.{j.NivelHabilidad}]";
                btnCurar.Text = $"USAR BOTIQUÍN ({j.Botiquines}) [CURA + DETIENE SANGRADO]";

                btnAtacar.Text = enemigosActivos.TrueForAll(en => !en.Vivo)
                    ? (oleada == OLEADA_JEFE_FINAL ? "¡ENFRENTAR AL JEFE FINAL!" : $"SIGUIENTE OLEADA (OLEADA {oleada})")
                    : $"[{j.Entidad.Nombre}] DISPARAR {j.Arma.Nombre.ToUpper()} (-{j.Arma.CostoEnergia} EN)";
            }

            pnlStatsEscuadron.Invalidate();
            picArena2D.Invalidate();
        }

        private void LogArena(string msg)
        {
            lstConsola.Items.Add(msg);
            lstConsola.TopIndex = lstConsola.Items.Count - 1;
        }

        // =========================================================================
        // 8. MOTOR GRÁFICO PIXEL ART (JETPACK, 6 ARMAS ÚNICAS, JEFES GIGANTES)
        // =========================================================================
        private void DibujarFondoParticulas(Graphics g, int w, int h)
        {
            g.SmoothingMode = SmoothingMode.None;
            foreach (var p in particulas)
            {
                using SolidBrush b = new SolidBrush(p.Color);
                if (p.Tipo == "CURA")
                {
                    g.FillRectangle(b, (int)p.X, (int)p.Y - 4, 3, 11);
                    g.FillRectangle(b, (int)p.X - 4, (int)p.Y, 11, 3);
                }
                else
                {
                    int tam = Math.Max(2, (int)p.Tamano);
                    g.FillRectangle(b, (int)(p.X % w), (int)(p.Y % h), tam, tam);
                }
            }
        }

        private void DibujarFondoMapaPixelArt(Graphics g, int sueloY)
        {
            int w = picArena2D.Width;
            int h = picArena2D.Height;

            using LinearGradientBrush bg = new LinearGradientBrush(
                new Rectangle(0, 0, w, h),
                mapaActual.ColorFondo,
                Color.FromArgb(6, 9, 14),
                LinearGradientMode.Vertical);
            g.FillRectangle(bg, 0, 0, w, h);

            int pasoObj = 130;
            int offsetFondo = ((int)(desplazamientoMapa * 0.5f) % pasoObj + pasoObj) % pasoObj;

            if (mapaActual.CodigoEfecto == "BOSQUE")
            {
                using SolidBrush brMontaña = new SolidBrush(Color.FromArgb(14, 36, 28));
                int offsetMont = ((int)(desplazamientoMapa * 0.2f) % 300 + 300) % 300;
                for (int mx = -offsetMont - 150; mx < w + 300; mx += 300)
                {
                    g.FillPolygon(brMontaña, new Point[] { new Point(mx, sueloY), new Point(mx + 150, sueloY - 145), new Point(mx + 300, sueloY) });
                }

                for (int tx = -offsetFondo; tx < w + pasoObj; tx += pasoObj)
                {
                    int alturaTronco = 100 + (Math.Abs((tx + (int)(desplazamientoMapa * 0.5f)) / pasoObj * 17) % 45);
                    g.FillRectangle(new SolidBrush(Color.FromArgb(24, 48, 38)), tx + 22, sueloY - alturaTronco, 14, alturaTronco);
                    g.FillRectangle(new SolidBrush(Color.FromArgb(80, 250, 123)), tx + 27, sueloY - alturaTronco + 10, 4, alturaTronco - 15);
                    g.FillRectangle(new SolidBrush(Color.FromArgb(20, 95, 65)), tx, sueloY - alturaTronco - 36, 58, 40);
                    g.FillRectangle(new SolidBrush(Color.FromArgb(40, 180, 115)), tx + 8, sueloY - alturaTronco - 46, 42, 16);
                    g.DrawRectangle(new Pen(Color.FromArgb(80, 250, 123), 1), tx, sueloY - alturaTronco - 36, 58, 40);
                }
            }
            else if (mapaActual.CodigoEfecto == "RADIACION")
            {
                using SolidBrush brCol = new SolidBrush(Color.FromArgb(42, 22, 18));
                for (int cx = -offsetFondo; cx < w + pasoObj; cx += pasoObj)
                {
                    g.FillRectangle(brCol, cx + 20, 45, 36, sueloY - 45);
                    g.FillRectangle(Brushes.OrangeRed, cx + 34, 55, 8, sueloY - 65);
                }
                int radioNucleo = 52 + (int)(Math.Sin(pulsoNeon) * 6);
                g.FillEllipse(new SolidBrush(Color.FromArgb(90, 255, 110, 20)), w / 2 - radioNucleo, sueloY / 2 - radioNucleo, radioNucleo * 2, radioNucleo * 2);
                g.DrawEllipse(new Pen(Color.Gold, 2), w / 2 - radioNucleo, sueloY / 2 - radioNucleo, radioNucleo * 2, radioNucleo * 2);
            }
            else if (mapaActual.CodigoEfecto == "GRAVEDAD")
            {
                g.FillEllipse(new SolidBrush(Color.FromArgb(45, 25, 75)), w - 240, 45, 105, 105);
                g.DrawEllipse(new Pen(Color.FromArgb(189, 147, 249), 2), w - 265, 85, 155, 26);
                for (int ax = -offsetFondo; ax < w + pasoObj; ax += pasoObj)
                {
                    g.FillPolygon(new SolidBrush(Color.FromArgb(30, 18, 48)), new Point[] { new Point(ax, sueloY), new Point(ax + 28, sueloY - 125), new Point(ax + 56, sueloY) });
                    g.FillEllipse(new SolidBrush(Color.FromArgb(140, 80, 250, 123)), ax + 21, sueloY - 75, 14, 22);
                }
            }
            else
            {
                using SolidBrush brEdif = new SolidBrush(Color.FromArgb(16, 22, 34));
                using SolidBrush brVentana = new SolidBrush(Color.FromArgb(75, 0, 240, 255));
                for (int bx = -offsetFondo; bx < w + pasoObj; bx += pasoObj)
                {
                    int bh = 95 + (Math.Abs((bx + (int)(desplazamientoMapa * 0.5f)) / pasoObj * 31) % 95);
                    g.FillRectangle(brEdif, bx + 10, sueloY - bh, 68, bh);
                    g.DrawLine(new Pen(Color.Cyan, 1), bx + 44, sueloY - bh, bx + 44, sueloY - bh - 24);
                    for (int vy = sueloY - bh + 12; vy < sueloY - 16; vy += 18)
                    {
                        g.FillRectangle(brVentana, bx + 20, vy, 12, 8);
                        g.FillRectangle(brVentana, bx + 48, vy, 12, 8);
                    }
                }
            }
        }

        private void DibujarArena2D_PixelArt(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            if (framesSacudida > 0 && !juegoPausado) g.TranslateTransform(rng.Next(-4, 5), rng.Next(-3, 4));

            int sueloY = picArena2D.Height - 64;

            // 1. Fondo temático con desplazamiento Parallax
            DibujarFondoMapaPixelArt(g, sueloY);

            // 2. Suelo metálico con baldosas que se desplazan con el movimiento del jugador
            g.FillRectangle(new SolidBrush(Color.FromArgb(24, 30, 42)), 0, sueloY, picArena2D.Width, 64);
            g.FillRectangle(new SolidBrush(mapaActual.ColorPrimario), 0, sueloY, picArena2D.Width, 4);

            using Pen pBaldosa = new Pen(Color.FromArgb(45, 58, 78), 2);
            int offsetSuelo = ((int)desplazamientoMapa % 32 + 32) % 32;
            for (int gx = -offsetSuelo; gx < picArena2D.Width + 32; gx += 32)
            {
                g.DrawLine(pBaldosa, gx, sueloY + 4, gx, picArena2D.Height);
            }
            g.DrawLine(pBaldosa, 0, sueloY + 32, picArena2D.Width, sueloY + 32);

            // Encabezado HUD
            using Font fMapa = new Font("Consolas", 10F, FontStyle.Bold);
            string infoJet = JugadorEnTurno().TieneJetpack ? $"  |  JETPACK: {(int)((double)JugadorEnTurno().CombustibleJetpack / JugadorEnTurno().CombustibleJetpackMax * 100)}%" : "";
            g.DrawString($"{mapaActual.Nombre}  |  OLEADA 0{oleada}/0{OLEADA_JEFE_FINAL}  |  AMENAZAS: {enemigosActivos.FindAll(x => x.Vivo).Count}{infoJet}", fMapa, new SolidBrush(mapaActual.ColorPrimario), 16, 12);

            // 3. Ítem Mochila Cohete (Jetpack) recolectable en el mapa
            if (itemJetpack.Activo)
            {
                int jy = (int)(itemJetpack.Y + Math.Sin(itemJetpack.Fase) * 6);
                int jx = (int)itemJetpack.X;
                g.DrawEllipse(new Pen(Color.Gold, 2), jx - 6, jy - 6, 44, 44);
                // Mochila cohete pixel art
                g.FillRectangle(Brushes.Black, jx + 5, jy + 3, 22, 24);
                g.FillRectangle(Brushes.OrangeRed, jx + 7, jy + 5, 7, 18);
                g.FillRectangle(Brushes.OrangeRed, jx + 18, jy + 5, 7, 18);
                g.FillRectangle(Brushes.Silver, jx + 12, jy + 7, 8, 12);
                g.FillRectangle(Brushes.Cyan, jx + 8, jy + 23, 5, 5);
                g.FillRectangle(Brushes.Cyan, jx + 19, jy + 23, 5, 5);
                using Font fJet = new Font("Consolas", 7.5F, FontStyle.Bold);
                g.DrawString("JETPACK", fJet, Brushes.Gold, jx - 4, jy - 16);
            }

            // 4. Operativos Pixel Art
            for (int i = 0; i < escuadron.Count; i++)
            {
                var j = escuadron[i];
                DibujarOperativoPixelArt(g, (int)j.PosX, (int)j.PosY - 14, j, i == indiceTurnoJugador, sueloY);
            }

            // 5. Enemigos Pixel Art y Jefe Final Gigante
            foreach (var en in enemigosActivos)
            {
                if (!en.Vivo) continue;
                DibujarEnemigoPixelArt(g, (int)en.PosX, (int)en.PosY - 14, en, sueloY);
            }

            // 6. Proyectiles Pixel Art
            foreach (var pr in proyectiles)
            {
                int tam = pr.Tamano;
                g.FillRectangle(Brushes.Black, pr.X - tam - 2, pr.Y - tam / 2 - 2, tam * 3 + 4, tam + 4);
                g.FillRectangle(new SolidBrush(pr.Color), pr.X - tam, pr.Y - tam / 2, tam * 3, tam);
                g.FillRectangle(Brushes.White, pr.X - tam / 2, pr.Y - 1, tam * 2, 2);
            }

            // 7. Partículas
            DibujarFondoParticulas(g, picArena2D.Width, picArena2D.Height);

            // 8. Cartel de PAUSA si está activo
            if (juegoPausado)
            {
                using SolidBrush overlay = new SolidBrush(Color.FromArgb(165, 8, 11, 16));
                g.FillRectangle(overlay, 0, 0, picArena2D.Width, picArena2D.Height);

                using Font fPausa = new Font("Consolas", 24F, FontStyle.Bold);
                using Font fSubPausa = new Font("Consolas", 11F, FontStyle.Bold);
                string txtPausa = "// JUEGO EN PAUSA //";
                SizeF sz = g.MeasureString(txtPausa, fPausa);
                g.DrawString(txtPausa, fPausa, new SolidBrush(Color.Gold), (picArena2D.Width - sz.Width) / 2, picArena2D.Height / 2 - 25);
                g.DrawString("Presiona [P] o el botón PAUSAR para continuar", fSubPausa, Brushes.White, (picArena2D.Width - 380) / 2, picArena2D.Height / 2 + 20);
            }

            using Pen marco = new Pen(mapaActual.ColorPrimario, 2);
            g.DrawRectangle(marco, 1, 1, picArena2D.Width - 3, picArena2D.Height - 3);
        }

        private void DibujarOperativoPixelArt(Graphics g, int x, int y, JugadorActivo j, bool enTurno, int sueloY)
        {
            using SolidBrush brSombra = new SolidBrush(Color.FromArgb(140, 80, 90, 105));
            g.FillRectangle(brSombra, x + 4, sueloY - 2, 40, 6);
            g.FillRectangle(brSombra, x + 8, sueloY - 4, 32, 10);

            bool mirarIzq = teclaIzquierda && enTurno;
            bool moviendo = enTurno && (teclaIzquierda || teclaDerecha);
            int frame = j.TieneJetpack ? 1 : (j.PosY < ObtenerSueloY() - 2f ? 1 : (moviendo ? ((int)(pulsoNeon * 2.2f) % 2) : 0));

            // Animación de Mochila Cohete en la espalda cuando está activa
            if (j.TieneJetpack)
            {
                int jetX = mirarIzq ? x + 36 : x - 6;
                g.FillRectangle(Brushes.Black, jetX - 1, y + 20, 16, 24);
                g.FillRectangle(Brushes.OrangeRed, jetX, y + 21, 14, 22);
                g.FillRectangle(Brushes.Silver, jetX + 3, y + 24, 8, 12);

                int llama = ((int)(pulsoNeon * 4) % 2 == 0) ? 16 : 10;
                g.FillRectangle(Brushes.Cyan, jetX + 2, y + 44, 10, llama);
                g.FillRectangle(Brushes.White, jetX + 4, y + 44, 6, llama - 4);

                // Barra de combustible flotante
                g.FillRectangle(Brushes.Black, x, y - 26, 48, 5);
                int wFuel = (int)((double)j.CombustibleJetpack / j.CombustibleJetpackMax * 46);
                g.FillRectangle(Brushes.Gold, x + 1, y - 25, Math.Max(0, wFuel), 3);
            }

            Bitmap sprite = ObtenerSpriteOperativo(j.Clase.Nombre, j.Clase.ColorTema, frame, mirarIzq);
            g.DrawImage(sprite, x, y, 48, 60);

            // Dibujar el arma equipada con diseño Pixel Art único según el modelo
            int dir = mirarIzq ? -1 : 1;
            int armaX = dir == 1 ? x + 26 : x - 14;
            int armaY = y + 30 + (frame == 1 ? -1 : 0);
            DibujarArmaDetalladaPixelArt(g, armaX, armaY, j.Arma, dir, 1);

            if (framesCuracion > 0 && enTurno)
            {
                int offset = (28 - framesCuracion) * 2;
                using Pen pCura = new Pen(Color.FromArgb(200, 80, 250, 123), 3);
                g.DrawRectangle(pCura, x - 4, y + 50 - offset, 56, 10);
            }

            if (j.TurnosDesangrado > 0 && j.Vivo)
            {
                using Font fBleed = new Font("Consolas", 7.5F, FontStyle.Bold);
                g.DrawString("¡SANGRANDO!", fBleed, Brushes.Red, x - 8, y - 36);
            }

            if (j.Escudo > 0 && j.Vivo)
            {
                using Pen pEsc = new Pen(Color.FromArgb(170, 0, 240, 255), 2);
                g.DrawRectangle(pEsc, x - 6, y - 4, 60, 66);
            }

            using Font fNom = new Font("Consolas", 8.5F, FontStyle.Bold);
            g.DrawString(j.Entidad.Nombre, fNom, new SolidBrush(j.Clase.ColorTema), x + 4, y - 16);
        }

        // Dibuja cada una de las 6 armas con silueta, mira y detalles únicos en Pixel Art
        private void DibujarArmaDetalladaPixelArt(Graphics g, int x, int y, ArmaFuturista arma, int dir, int escala)
        {
            using SolidBrush brCol = new SolidBrush(arma.ColorProyectil);
            using SolidBrush brMetal = new SolidBrush(Color.FromArgb(155, 165, 180));
            using SolidBrush brOscuro = new SolidBrush(Color.FromArgb(55, 65, 80));

            switch (arma.IdVisual)
            {
                case 0: // Pistola de Pulso MK-I (compacta con núcleo brillante)
                    g.FillRectangle(Brushes.Black, x, y, 22 * escala, 10 * escala);
                    g.FillRectangle(brMetal, x + 2 * escala, y + 2 * escala, 18 * escala, 5 * escala);
                    g.FillRectangle(brOscuro, (dir == 1 ? x + 3 : x + 13) * escala, y + 6 * escala, 6 * escala, 4 * escala);
                    g.FillRectangle(brCol, (dir == 1 ? x + 10 : x + 6) * escala, y + 3 * escala, 6 * escala, 2 * escala);
                    break;

                case 1: // Rifle Criogénico Sub-Zero (cañón largo con tanque azul)
                    g.FillRectangle(Brushes.Black, x - 2, y - 1, 30 * escala, 11 * escala);
                    g.FillRectangle(brMetal, x, y + 1 * escala, 26 * escala, 6 * escala);
                    g.FillRectangle(brCol, (dir == 1 ? x + 8 : x + 10) * escala, y + 6 * escala, 10 * escala, 4 * escala);
                    g.FillRectangle(Brushes.White, (dir == 1 ? x + 20 : x + 2) * escala, y + 2 * escala, 5 * escala, 3 * escala);
                    break;

                case 2: // Carabina de Plasma Solar (doble bobina térmica naranja)
                    g.FillRectangle(Brushes.Black, x - 3, y - 2, 32 * escala, 12 * escala);
                    g.FillRectangle(brOscuro, x - 1, y, 28 * escala, 8 * escala);
                    g.FillRectangle(brCol, (dir == 1 ? x + 8 : x + 10) * escala, y - 1, 4 * escala, 10 * escala);
                    g.FillRectangle(brCol, (dir == 1 ? x + 16 : x + 4) * escala, y - 1, 4 * escala, 10 * escala);
                    break;

                case 3: // Lanzador de Agujas Tesla (anillos electromagnéticos dorados)
                    g.FillRectangle(Brushes.Black, x - 4, y - 2, 34 * escala, 12 * escala);
                    g.FillRectangle(brMetal, x - 2, y + 1 * escala, 30 * escala, 6 * escala);
                    g.FillRectangle(brCol, (dir == 1 ? x + 6 : x + 16) * escala, y - 2, 3 * escala, 12 * escala);
                    g.FillRectangle(brCol, (dir == 1 ? x + 13 : x + 9) * escala, y - 2, 3 * escala, 12 * escala);
                    g.FillRectangle(brCol, (dir == 1 ? x + 20 : x + 2) * escala, y - 2, 3 * escala, 12 * escala);
                    break;

                case 4: // Escopeta Antimateria V8 (cañón grueso de doble calibre)
                    g.FillRectangle(Brushes.Black, x - 4, y - 3, 34 * escala, 14 * escala);
                    g.FillRectangle(brOscuro, x - 2, y - 1, 30 * escala, 10 * escala);
                    g.FillRectangle(brCol, (dir == 1 ? x + 6 : x + 8) * escala, y + 2 * escala, 16 * escala, 4 * escala);
                    g.FillRectangle(brMetal, (dir == 1 ? x + 24 : x) * escala, y - 1, 4 * escala, 10 * escala);
                    break;

                default: // Cañón Railgun Orbital (acelerador largo con mira holográfica)
                    g.FillRectangle(Brushes.Black, x - 6, y - 4, 40 * escala, 15 * escala);
                    g.FillRectangle(brMetal, x - 4, y - 1, 36 * escala, 4 * escala);
                    g.FillRectangle(brMetal, x - 4, y + 5 * escala, 36 * escala, 4 * escala);
                    g.FillRectangle(brCol, x - 2, y + 3 * escala, 34 * escala, 2 * escala);
                    g.FillRectangle(Brushes.Cyan, (dir == 1 ? x + 8 : x + 18) * escala, y - 4, 8 * escala, 3 * escala);
                    break;
            }
        }

        private Bitmap ObtenerSpriteOperativo(string clase, Color colorTraje, int frame, bool mirarIzquierda)
        {
            string clave = $"OP_{clase}_{frame}_{mirarIzquierda}";
            if (cacheSprites.TryGetValue(clave, out var bmpGuardado)) return bmpGuardado;

            string[][] patronesVanguard = new string[][]
            {
                new string[]
                {
                    "...KKKKKKKKKK...",
                    "..KHHHHHHHHHHK..",
                    ".KHHHHHHHHHHHHK.",
                    ".KHHVVVVVVVVHHK.",
                    ".KHHVVVVVVVVHHK.",
                    "..KHHHHHHHHHHK..",
                    "...KKKKKKKKKK...",
                    ".KKTTTTTTTTTTKK.",
                    "KTTTTTTTTTTTTTTK",
                    "KHHKTTTTTTTTKHHK",
                    "KSSKTTTTTTTTKSSK",
                    ".KKKDDDDDDDDKKK.",
                    "...KPPPPPPPPK...",
                    "...KPPPKKPPPK...",
                    "...KPPPKKPPPK...",
                    "..KBBBBKKBBBBK..",
                    "..KKKKKKKKKKKK..",
                    "................",
                    "................",
                    "................"
                },
                new string[]
                {
                    "...KKKKKKKKKK...",
                    "..KHHHHHHHHHHK..",
                    ".KHHHHHHHHHHHHK.",
                    ".KHHVVVVVVVVHHK.",
                    ".KHHVVVVVVVVHHK.",
                    "..KHHHHHHHHHHK..",
                    "...KKKKKKKKKK...",
                    ".KKTTTTTTTTTTKK.",
                    "KTTTTTTTTTTTTTTK",
                    "KHHKTTTTTTTTKHHK",
                    ".KKKDDDDDDDDKKK.",
                    "...KPPPPPPPPK...",
                    "..KPPPK..KPPPK..",
                    ".KBBBK....KBBBK.",
                    ".KKKK......KKKK.",
                    "................",
                    "................",
                    "................",
                    "................",
                    "................"
                }
            };

            string[][] patronesSpectre = new string[][]
            {
                new string[]
                {
                    "....KKKKKKKK....",
                    "...KHHHHHHHHK...",
                    "..KHHHHHHHHHHK..",
                    "..KHKKKKKKKKHK..",
                    "..KHKVVKKVVKHk..",
                    "..KHKKKKKKKKHK..",
                    "...KVVVVVVVVK...",
                    "....KKKKKKKK....",
                    "...KTTTTTTTTK...",
                    "..KTTTTTTTTTTK..",
                    "..KSSKTTTTKSSK..",
                    "...KKKDDDDKKK...",
                    "....KPPPPPPK....",
                    "....KPPKKPPK....",
                    "...KPPK..KPPK...",
                    "...KBBK..KBBK...",
                    "...KKKK..KKKK...",
                    "................",
                    "................",
                    "................"
                },
                new string[]
                {
                    "....KKKKKKKK....",
                    "...KHHHHHHHHK...",
                    "..KHHHHHHHHHHK..",
                    "..KHKKKKKKKKHK..",
                    "..KHKVVKKVVKHk..",
                    "..KHKKKKKKKKHK..",
                    "...KVVVVVVVVK...",
                    "....KKKKKKKK....",
                    "...KTTTTTTTTK...",
                    "..KTTTTTTTTTTK..",
                    ".KSSKTTTTTTKSSK.",
                    "..KKKDDDDDDKKK..",
                    "....KPPPPPPK....",
                    "...KPPK..KPPK...",
                    "..KBBK....KBBK..",
                    "..KKKK....KKKK..",
                    "................",
                    "................",
                    "................",
                    "................"
                }
            };

            string[][] patronesNetrunner = new string[][]
            {
                new string[]
                {
                    "...KKKKKKKKKK...",
                    "..KHHHHHHHHHHK..",
                    ".KHHHHHHHHHHHHK.",
                    "..KHHSSSSSSHHK..",
                    "..KHVVVVVVVVHK..",
                    "..KHSSKSSKSSHK..",
                    "...KSSSSSSSSK...",
                    "....KKKKKKKK....",
                    "...KTTTTTTTTK...",
                    "..KTTTTTTTTTTK..",
                    "..KSSKTTTTKSSK..",
                    "..KTTKDDDDKTTK..",
                    "..KKKKPPPPKKKK..",
                    "....KPPKKPPK....",
                    "....KPPKKPPK....",
                    "...KBBBKKBBBK...",
                    "...KKKKKKKKKK...",
                    "................",
                    "................",
                    "................"
                },
                new string[]
                {
                    "...KKKKKKKKKK...",
                    "..KHHHHHHHHHHK..",
                    ".KHHHHHHHHHHHHK.",
                    "..KHHSSSSSSHHK..",
                    "..KHVVVVVVVVHK..",
                    "..KHSSKSSKSSHK..",
                    "...KSSSSSSSSK...",
                    "....KKKKKKKK....",
                    "...KTTTTTTTTK...",
                    "..KTTTTTTTTTTK..",
                    ".KSSKTTTTTTKSSK.",
                    ".KKKKDDDDDDKKKK.",
                    "....KPPPPPPK....",
                    "...KPPK..KPPK...",
                    "..KBBK....KBBK..",
                    "..KKKK....KKKK..",
                    "................",
                    "................",
                    "................",
                    "................"
                }
            };

            string[] patron = clase == "VANGUARD"
                ? patronesVanguard[frame % 2]
                : (clase == "SPECTRE" ? patronesSpectre[frame % 2] : patronesNetrunner[frame % 2]);

            Bitmap bmp = new Bitmap(16, 20);

            Color cCascoPelo = clase == "VANGUARD" ? Color.FromArgb(200, 55, 65)
                             : (clase == "SPECTRE" ? Color.FromArgb(30, 42, 58)
                             : Color.FromArgb(175, 115, 250));
            Color cVisor = clase == "VANGUARD" ? Color.White
                         : (clase == "SPECTRE" ? Color.Cyan
                         : Color.FromArgb(0, 240, 255));
            Color cPiel = Color.FromArgb(235, 175, 130);
            Color cTrajeOscuro = ControlPaint.Dark(colorTraje, 0.25f);
            Color cPantalon = Color.FromArgb(55, 65, 80);
            Color cBotas = Color.FromArgb(35, 40, 48);

            for (int py = 0; py < 20; py++)
            {
                for (int px = 0; px < 16; px++)
                {
                    char ch = char.ToUpper(patron[py][px]);
                    Color col = ch switch
                    {
                        'K' => Color.Black,
                        'H' => cCascoPelo,
                        'V' => cVisor,
                        'S' => cPiel,
                        'T' => colorTraje,
                        'D' => cTrajeOscuro,
                        'P' => cPantalon,
                        'B' => cBotas,
                        _ => Color.Transparent
                    };
                    bmp.SetPixel(px, py, col);
                }
            }

            if (mirarIzquierda) bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
            cacheSprites[clave] = bmp;
            return bmp;
        }

        private void DibujarEnemigoPixelArt(Graphics g, int x, int y, Enemigo en, int sueloY)
        {
            int ancho = en.EsJefe ? 112 : 48;
            int alto = en.EsJefe ? 132 : 60;
            int paso = ((int)(pulsoNeon * 2.2f + en.FaseAnim)) % 2;
            bool mirarDerecha = en.VX > 0;

            g.FillRectangle(new SolidBrush(Color.FromArgb(140, 80, 90, 105)), x + 6, sueloY - 2, ancho - 12, 6);

            // Aura de carga cuando el Jefe prepara un Mega Ataque
            if (en.EsJefe && en.CargandoMega > 0)
            {
                int radio = 75 + (en.CargandoMega % 12);
                g.DrawEllipse(new Pen(Color.Gold, 3), x + ancho / 2 - radio, y + alto / 2 - radio, radio * 2, radio * 2);
                using Font fWarn = new Font("Consolas", 9F, FontStyle.Bold);
                g.DrawString("¡MEGA ATAQUE INMINENTE!", fWarn, Brushes.Gold, x - 20, y - 42);
            }

            // Propulsores para robots voladores y Jefes
            if (!en.EsAlien || en.EsJefe)
            {
                int llama = paso == 0 ? (en.EsJefe ? 22 : 12) : (en.EsJefe ? 14 : 7);
                g.FillRectangle(Brushes.Cyan, x + ancho / 2 - 10, y + alto - 14, 20, llama);
                g.FillRectangle(Brushes.White, x + ancho / 2 - 5, y + alto - 14, 10, llama - 4);
            }

            Bitmap spriteEnemigo = ObtenerSpriteEnemigoPixelArt(en.EsAlien, en.ColorAura, paso, mirarDerecha);
            g.DrawImage(spriteEnemigo, x, y + (60 - alto), ancho, alto);

            int barraW = en.EsJefe ? 120 : 56;
            using Font f = new Font("Consolas", en.EsJefe ? 9F : 7.8F, FontStyle.Bold);
            g.DrawString($"{en.Nombre} [{en.Salud}/{en.SaludMax}]", f, new SolidBrush(en.ColorAura), x - 10, y + (60 - alto) - 24);
            g.FillRectangle(Brushes.Black, x - 1, y + (60 - alto) - 9, barraW + 2, 8);
            g.FillRectangle(Brushes.DarkRed, x, y + (60 - alto) - 8, barraW, 6);
            int w = (int)((double)en.Salud / en.SaludMax * barraW);
            g.FillRectangle(new SolidBrush(en.ColorAura), x, y + (60 - alto) - 8, Math.Max(0, w), 6);
        }

        private Bitmap ObtenerSpriteEnemigoPixelArt(bool esAlien, Color aura, int paso, bool mirarDerecha)
        {
            string clave = $"EN_{esAlien}_{aura.ToArgb()}_{paso}_{mirarDerecha}";
            if (cacheSprites.TryGetValue(clave, out var bmpGuardado)) return bmpGuardado;

            string[] patronRobot0 = new string[]
            {
                "....KKKKKKKK....",
                "...KMMMMMMMMK...",
                "..KMMMEEEEMMMK..",
                "..KMMMEEEEMMMK..",
                "...KMMMMMMMMK...",
                "....KKKKKKKK....",
                "..KKMMMMMMMMKK..",
                ".KMMMMMMMMMMMMK.",
                ".KMMEEMMMMMMMMK.",
                ".KMMMMMMMMMMMMK.",
                "..KKDDDDDDDDKK..",
                "....KMMMMMMK....",
                "....KMMKKMMK....",
                "....KMMKKMMK....",
                "...KDDDKKDDDK...",
                "...KKKKKKKKKK...",
                "................",
                "................",
                "................",
                "................"
            };

            string[] patronRobot1 = new string[]
            {
                "....KKKKKKKK....",
                "...KMMMMMMMMK...",
                "..KMMMEEEEMMMK..",
                "..KMMMEEEEMMMK..",
                "...KMMMMMMMMK...",
                "....KKKKKKKK....",
                "..KKMMMMMMMMKK..",
                ".KMMMMMMMMMMMMK.",
                ".KMMEEMMMMMMMMK.",
                ".KMMMMMMMMMMMMK.",
                "..KKDDDDDDDDKK..",
                "....KMMMMMMK....",
                "...KMMK..KMMK...",
                "..KMMK....KMMK..",
                "..KDDK....KDDK..",
                "..KKKK....KKKK..",
                "................",
                "................",
                "................",
                "................"
            };

            string[] patronAlien0 = new string[]
            {
                "..KKKKKKKKKK....",
                ".KAAAAAAAAAAK...",
                "KAAEEAAEEAAAAK..",
                "KAAAAAAAAAAAAK..",
                ".KAAKKKKKAAAK...",
                "..KKKKKKKKKK....",
                "...KAAAAAAAAK...",
                "..KAAAAAAAAAAK..",
                ".KAAKAAAAAAKAAK.",
                ".KAAKAAAAAAKAAK.",
                "..KKDDDDDDDDKK..",
                "....KAAAAAAK....",
                "....KAAKKAAK....",
                "...KAAK..KAAK...",
                "...KDDK..KDDK...",
                "...KKKK..KKKK...",
                "................",
                "................",
                "................",
                "................"
            };

            string[] patronAlien1 = new string[]
            {
                "..KKKKKKKKKK....",
                ".KAAAAAAAAAAK...",
                "KAAEEAAEEAAAAK..",
                "KAAAAAAAAAAAAK..",
                ".KAAKKKKKAAAK...",
                "..KKKKKKKKKK....",
                "...KAAAAAAAAK...",
                "..KAAAAAAAAAAK..",
                ".KAAKAAAAAAKAAK.",
                "..KKDDDDDDDDKK..",
                "....KAAAAAAK....",
                "...KAAK..KAAK...",
                "..KAAK....KAAK..",
                "..KDDK....KDDK..",
                "..KKKK....KKKK..",
                "................",
                "................",
                "................",
                "................",
                "................"
            };

            string[] patron = esAlien
                ? (paso == 0 ? patronAlien0 : patronAlien1)
                : (paso == 0 ? patronRobot0 : patronRobot1);

            Bitmap bmp = new Bitmap(16, 20);
            Color cCuerpo = esAlien ? Color.FromArgb(45, 165, 95) : Color.FromArgb(120, 130, 148);
            Color cOscuro = ControlPaint.Dark(cCuerpo, 0.3f);

            for (int py = 0; py < 20; py++)
            {
                for (int px = 0; px < 16; px++)
                {
                    char ch = patron[py][px];
                    Color col = ch switch
                    {
                        'K' => Color.Black,
                        'M' => cCuerpo,
                        'A' => cCuerpo,
                        'D' => cOscuro,
                        'E' => aura,
                        _ => Color.Transparent
                    };
                    bmp.SetPixel(px, py, col);
                }
            }

            if (mirarDerecha) bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
            cacheSprites[clave] = bmp;
            return bmp;
        }

        private void DibujarStatsEscuadron(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using Pen borde = new Pen(Color.FromArgb(0, 240, 255), 1);
            g.DrawRectangle(borde, 0, 0, pnlStatsEscuadron.Width - 1, pnlStatsEscuadron.Height - 1);

            using Font fTit = new Font("Consolas", 10F, FontStyle.Bold);
            using Font fTxt = new Font("Consolas", 8.8F, FontStyle.Bold);

            int y = 12;
            for (int i = 0; i < escuadron.Count; i++)
            {
                var j = escuadron[i];
                bool activo = (i == indiceTurnoJugador);
                string marcaTurno = activo ? " [ACTIVO]" : "";
                string estadoSangre = j.TurnosDesangrado > 0 ? $" [SANGRADO: {j.TurnosDesangrado}T]" : "";

                g.DrawString($"{j.Entidad.Nombre} ({j.Clase.Nombre}){marcaTurno}", fTit, new SolidBrush(j.Clase.ColorTema), 12, y);
                y += 20;
                g.DrawString($"HP: {j.Entidad.Salud}/{j.Entidad.SaludMaxima} | EN: {j.Entidad.Energia}/{j.Entidad.EnergiaMaxima} | ESC: {j.Escudo}", fTxt, Brushes.White, 12, y);
                y += 18;
                g.DrawString($"XP: {j.XP} | CR: {j.Creditos} | ARMA Nv.{j.NivelArma}{estadoSangre}", fTxt, j.TurnosDesangrado > 0 ? Brushes.OrangeRed : Brushes.LightGray, 12, y);
                y += 18;

                g.FillRectangle(Brushes.DarkSlateGray, 12, y, 350, 10);
                int wHp = (int)((double)j.Entidad.Salud / j.Entidad.SaludMaxima * 350);
                g.FillRectangle(new SolidBrush(Color.FromArgb(255, 75, 85)), 12, y, Math.Max(0, wHp), 10);
                y += 14;

                g.FillRectangle(Brushes.DarkSlateGray, 12, y, 350, 10);
                int wEn = (int)((double)j.Entidad.Energia / j.Entidad.EnergiaMaxima * 350);
                g.FillRectangle(new SolidBrush(Color.FromArgb(80, 250, 123)), 12, y, Math.Max(0, wEn), 10);
                y += 28;
            }
        }

        private Button CrearBotonNeon(string texto, Point loc, Size size, Color colorNeon)
        {
            Button btn = new Button
            {
                Text = texto,
                Location = loc,
                Size = size,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(22, 29, 42),
                ForeColor = colorNeon,
                Font = new Font("Consolas", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = colorNeon;
            btn.FlatAppearance.BorderSize = 1;
            btn.MouseEnter += (s, e) => { btn.BackColor = colorNeon; btn.ForeColor = Color.Black; };
            btn.MouseLeave += (s, e) => { btn.BackColor = Color.FromArgb(22, 29, 42); btn.ForeColor = colorNeon; };
            return btn;
        }

        private void button1_Click(object sender, EventArgs e) { }
        private void Form1_Load(object sender, EventArgs e) { }
    }
}