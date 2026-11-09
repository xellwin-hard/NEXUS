using System;
using System.Collections.Generic;
using System.Threading;

namespace NexusV2
{
    abstract class Personaje
    {
        public string Nombre { get; set; }
        public int Salud { get; set; }
        public int SaludMaxima { get; set; }

        public Personaje(string nombre, int salud)
        {
            Nombre = nombre;
            Salud = salud;
            SaludMaxima = salud;
        }

        public abstract void Reaccionar();
    }

    class Cadete : Personaje
    {
        public string Rol { get; set; }
        public int Energia { get; set; }
        public int EnergiaMaxima { get; set; }
        public int Experiencia { get; set; }
        public List<Objeto> Inventario { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
        
        public Objeto? ArmaEquipada { get; set; }
        public Objeto? ArmaduraEquipada { get; set; }

        public Cadete(string nombre, string rol, int salud, int energia) : base(nombre, salud)
        {
            Rol = rol;
            Energia = energia;
            EnergiaMaxima = energia;
            Experiencia = 0;
            Inventario = new List<Objeto>();
            PosX = 0;
            PosY = 0;
        }

        public override void Reaccionar() { }
    }

    class Enemigo : Personaje
    {
        public int Daño { get; set; }
        public Enemigo(string nombre, int salud, int daño) : base(nombre, salud) { Daño = daño; }

        public override void Reaccionar()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[!] ¡El {Nombre} ruge furioso y adopta una postura letal de combate!");
            Console.ResetColor();
        }
    }

    class Robot : Personaje
    {
        public int NivelSeguridad { get; set; }
        public int Daño { get; set; }
        public Robot(string nombre, int salud, int nivelSeguridad, int daño) : base(nombre, salud) 
        { 
            NivelSeguridad = nivelSeguridad; 
            Daño = daño;
        }

        public override void Reaccionar()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[!] El {Nombre} despliega escudos de energía y activa protocolos de aniquilación (Seguridad Nivel {NivelSeguridad}).");
            Console.ResetColor();
        }
    }

    class Objeto
    {
        public string Nombre { get; set; }
        public string Tipo { get; set; }
        public int Valor { get; set; }

        public Objeto(string nombre, string tipo, int valor)
        {
            Nombre = nombre;
            Tipo = tipo;
            Valor = valor;
        }
    }

    class Program
    {
        static Cadete jugador = null!;
        static bool misionCompletada = false;
        static bool gameOver = false;
        static bool encontroNucleo = false;
        static Random random = new Random();

        static int limiteMapa = 5;

        static string[,] descripcionesMapa = new string[5, 5] {
            { "Zona de Aterrizaje Segura (Base Nexus)", "Pasillos oscuros con luces de emergencia parpadeantes.", "Laboratorio abandonado con terminales rotas.", "Cables colgando y olor a ozono quemado.", "Sala de servidores inundada de líquido refrigerante." },
            { "Almacén secundario de suministros", "Sección colapsada por un impacto de meteorito.", "Pasarela metálica sobre un abismo sin fin.", "Antiguo comedor militar cubierto de polvo.", "Celdas de contención vacías y abiertas." },
            { "Jardín hidropónico muerto y marchito", "Centro de control central con pantallas estáticas.", "Cámara de pruebas cinéticas.", "Túnel de ventilación industrial.", "Arsenal militar con estantes vacíos." },
            { "Hangar de naves nodrizas secundarias", "Pasillo secundario blindado.", "Área de cuarentena biológica.", "Estación de radio con transmisiones en bucle.", "Laboratorio de genética prohibida." },
            { "Cámara de descontaminación", "Subsuelo profundo de alta presión.", "Cripta de datos cuánticos.", "Antesala del núcleo principal.", "EL NÚCLEO ENERGÉTICO ANÓMALO" }
        };

        static void Main(string[] args)
        {
            Console.Title = "NEXUS V2 - Protocolo Omega";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("==================================================");
            Console.WriteLine("     N E X U S   V2.0 - PROTOCOLO OMEGA (2297)     ");
            Console.WriteLine("==================================================");
            Console.ResetColor();

            Console.Write("Ingrese su identificación de Operativo: ");
            string nombre = Console.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(nombre)) nombre = "Operativo Anónimo";

            Console.WriteLine("\nSeleccione su Especialización Táctica:");
            Console.WriteLine("1. Soldado (Salud: 120 | Energía: 80 | Mayor poder ofensivo)");
            Console.WriteLine("2. Ingeniero (Salud: 100 | Energía: 120 | Mayor eficiencia energética)");
            Console.WriteLine("3. Médico de Combate (Salud: 140 | Energía: 90 | Mayor resistencia general)");
            Console.Write("Selección (1-3): ");
            string seleccionClase = Console.ReadLine() ?? "";

            if (seleccionClase == "1")
            {
                jugador = new Cadete(nombre, "Soldado", 120, 80);
                jugador.Inventario.Add(new Objeto("Rifle de Asalto Básico", "Arma", 10));
                jugador.ArmaEquipada = jugador.Inventario[0];
            }
            else if (seleccionClase == "2")
            {
                jugador = new Cadete(nombre, "Ingeniero", 100, 120);
                jugador.Inventario.Add(new Objeto("Pistola Láser", "Arma", 6));
                jugador.ArmaEquipada = jugador.Inventario[0];
            }
            else
            {
                jugador = new Cadete(nombre, "Médico", 140, 90);
                jugador.Inventario.Add(new Objeto("Escalpelos Tácticos", "Arma", 4));
                jugador.ArmaEquipada = jugador.Inventario[0];
            }

            jugador.Inventario.Add(new Objeto("Célula de Energía", "Energia", 30));
            jugador.Inventario.Add(new Objeto("Botiquín de Emergencia", "Salud", 40));

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[SISTEMA] Operativo {jugador.Nombre} ({jugador.Rol}) registrado con éxito.");
            Console.WriteLine("[MISIÓN] 'La anomalía perdida'");
            Console.WriteLine("- Objetivo 1: Navegar el complejo, equiparte y hallar el Núcleo en (4,4).");
            Console.WriteLine("- Objetivo 2: Regresar vivo a la coordenada (0,0) para la extracción.\n");
            Console.ResetColor();

            Thread.Sleep(2500);

            while (!gameOver)
            {
                MostrarInterfaz();
                ProcesarMenuPrincipal();
            }
        }

        static void MostrarInterfaz()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("\n==================================================");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($" ESTADO | Salud: {jugador.Salud}/{jugador.SaludMaxima} | Energía: {jugador.Energia}/{jugador.EnergiaMaxima} | XP: {jugador.Experiencia}");
            Console.WriteLine($" POSICIÓN| X: {jugador.PosX} | Y: {jugador.PosY} [{descripcionesMapa[jugador.PosY, jugador.PosX]}]");
            string armaNombre = jugador.ArmaEquipada != null ? jugador.ArmaEquipada.Nombre : "Ninguna";
            string armaduraNombre = jugador.ArmaduraEquipada != null ? jugador.ArmaduraEquipada.Nombre : "Ninguna";
            Console.WriteLine($" EQUIPO | Arma: {armaNombre} | Armadura: {armaduraNombre}");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("==================================================");
            Console.ResetColor();
            Console.WriteLine("1. Explorar / Moverse por el complejo");
            Console.WriteLine("2. Abrir Inventario / Gestionar Equipamiento");
            Console.WriteLine("3. Ver Estado de Misión y Radar");
            Console.WriteLine("4. Abortar Misión (Salir)");
            Console.Write("Seleccione una opción táctica: ");
        }

        static void ProcesarMenuPrincipal()
        {
            string opcion = Console.ReadLine() ?? "";
            switch (opcion)
            {
                case "1":
                    Moverse();
                    break;
                case "2":
                    AdministrarInventario();
                    break;
                case "3":
                    VerMision();
                    break;
                case "4":
                    Console.WriteLine("\n[SISTEMA] Misión abortada por el usuario. Extracción de emergencia en curso...");
                    gameOver = true;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[ERROR] Comando no reconocido por la interfaz táctica.");
                    Console.ResetColor();
                    break;
            }
        }

        static void Moverse()
        {
            Console.Write("Dirección de avance (W=Norte, S=Sur, A=Oeste, D=Este): ");
            string dir = (Console.ReadLine() ?? "").ToUpper();

            int nuevaX = jugador.PosX;
            int nuevaY = jugador.PosY;

            if (dir == "W") nuevaY++;
            else if (dir == "S") nuevaY--;
            else if (dir == "D") nuevaX++;
            else if (dir == "A") nuevaX--;
            else
            {
                Console.WriteLine("[ERROR] Dirección táctica inválida.");
                return;
            }

            if (nuevaX < 0 || nuevaX >= limiteMapa || nuevaY < 0 || nuevaY >= limiteMapa)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("🚨 ADVERTENCIA: Perímetro de seguridad alcanzado. Muro impenetrable detectado.");
                Console.ResetColor();
                return;
            }

            jugador.PosX = nuevaX;
            jugador.PosY = nuevaY;
            jugador.Energia -= 8;

            Console.WriteLine($"\nDesplazándose al sector ({jugador.PosX}, {jugador.PosY})...");
            Thread.Sleep(800);

            if (jugador.Energia <= 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n🔋 ENERGÍA AGOTADA. Sistemas vitales apagados. Fin de la transmisión.");
                Console.ResetColor();
                gameOver = true;
                return;
            }

            GenerarEventoAleatorio();
        }

        static void GenerarEventoAleatorio()
        {
            if (jugador.PosX == 4 && jugador.PosY == 4 && !encontroNucleo)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("\n⚡ ¡ANOMALÍA CUÁNTICA DETECTADA EN LA ZONA! ⚡");
                Console.WriteLine("Has localizado el Núcleo Energético... pero una presencia masiva bloquea tu acceso.");
                Console.ResetColor();
                encontroNucleo = true;
                Enemigo jefe = new Enemigo("Guardián Cuántico del Núcleo", 110, 22);
                IniciarEncuentro(jefe);
                return;
            }

            int prob = random.Next(1, 101);

            if (prob <= 25)
            {
                Console.WriteLine("El sector está en completo silencio. No hay amenazas a la vista.");
            }
            else if (prob <= 50)
            {
                int tipoObj = random.Next(1, 5);
                Objeto nuevoObjeto;
                if (tipoObj == 1) nuevoObjeto = new Objeto("Célula de Energía Avanzada", "Energia", 45);
                else if (tipoObj == 2) nuevoObjeto = new Objeto("Nanobotiquín Médico", "Salud", 50);
                else if (tipoObj == 3) nuevoObjeto = new Objeto("Pechera de Titanio", "Armadura", 15);
                else nuevoObjeto = new Objeto("Granada EMP", "Granada", 35);

                jugador.Inventario.Add(nuevoObjeto);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"🎁 RECURSO DESCUBIERTO: [{nuevoObjeto.Nombre}] añadido al inventario.");
                Console.ResetColor();
            }
            else if (prob <= 70)
            {
                Console.WriteLine("👾 ¡ALERTA DE AMENAZA MECÁNICA!");
                Robot robot = new Robot("Dron Defensor MK-II", 60, 3, 14);
                IniciarEncuentro(robot);
            }
            else if (prob <= 90)
            {
                Console.WriteLine("👾 ¡ENTIDAD HOSTIL BIOLÓGICA DETECTADA!");
                Enemigo alien = new Enemigo("Mutante del Vacío", 70, 18);
                IniciarEncuentro(alien);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("🚨 ¡PELIGRO! Filtración masiva de radiación en el compartimento.");
                Console.ResetColor();
                int danioRad = 20;
                if (jugador.Rol == "Médico") danioRad = 10;
                jugador.Salud -= danioRad;
                Console.WriteLine($"Has sufrido {danioRad} puntos de daño por radiación ambiental.");
                VerificarSalud();
            }
        }

        static void IniciarEncuentro(Personaje entidad)
        {
            entidad.Reaccionar();
            bool enCombate = true;

            while (enCombate && !gameOver)
            {
                Console.WriteLine($"\n--- COMBATE TÁCTICO --- | {entidad.Nombre} (Salud: {entidad.Salud})");
                Console.WriteLine("1. Atacar con Arma");
                Console.WriteLine("2. Lanzar Granada EMP (Si posees una)");
                Console.WriteLine("3. Usar Objeto del Inventario");
                Console.WriteLine("4. Intentar Escapar");
                Console.WriteLine("5. Analizar Entidad");
                Console.Write("Seleccione acción de combate: ");

                string accion = Console.ReadLine() ?? "";

                switch (accion)
                {
                    case "1":
                        int baseDañoArma = jugador.ArmaEquipada != null ? jugador.ArmaEquipada.Valor : 5;
                        int dañoHecho = random.Next(10, 20) + baseDañoArma;
                        
                        bool critico = random.Next(1, 101) <= 20;
                        if (critico)
                        {
                            dañoHecho = (int)(dañoHecho * 1.5);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("💥 ¡GOLPE CRÍTICO TÁCTICO!");
                            Console.ResetColor();
                        }

                        entidad.Salud -= dañoHecho;
                        Console.WriteLine($"Atacaste a {entidad.Nombre} infligiendo {dañoHecho} de daño.");

                        if (entidad.Salud <= 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"¡Has aniquilado con éxito a {entidad.Nombre}!");
                            Console.ResetColor();
                            jugador.Experiencia += 40;
                            enCombate = false;

                            if (entidad.Nombre == "Guardián Cuántico del Núcleo")
                            {
                                CompletarMisionFase1();
                            }
                        }
                        break;

                    case "2":
                        Objeto? granada = jugador.Inventario.Find(o => o.Tipo == "Granada");
                        if (granada != null)
                        {
                            entidad.Salud -= granada.Valor;
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"¡Has lanzado una {granada.Nombre}! Causa {granada.Valor} de daño directo.");
                            Console.ResetColor();
                            jugador.Inventario.Remove(granada);

                            if (entidad.Salud <= 0)
                            {
                                Console.WriteLine($"¡La entidad ha caído ante la detonación!");
                                jugador.Experiencia += 40;
                                enCombate = false;
                                if (entidad.Nombre == "Guardián Cuántico del Núcleo") CompletarMisionFase1();
                            }
                        }
                        else
                        {
                            Console.WriteLine("[ERROR] No posees granadas EMP en el inventario.");
                        }
                        break;

                    case "3":
                        AdministrarInventario(true);
                        break;

                    case "4":
                        if (random.Next(1, 100) > 45)
                        {
                            Console.WriteLine("¡Maniobra de evasión exitosa! Has escapado del enfrentamiento.");
                            enCombate = false;
                        }
                        else
                        {
                            Console.WriteLine("¡La retirada ha sido bloqueada por la entidad!");
                        }
                        break;

                    case "5":
                        Console.WriteLine($"[ANALIZADOR TÁCTICO]: {entidad.Nombre}. Categoria: {(entidad is Robot ? "Sintético / Maquinaria" : "Orgánico / Mutado")}.");
                        break;

                    default:
                        Console.WriteLine("Comando de combate inválido. Pierdes tu turno.");
                        break;
                }

                if (enCombate && entidad.Salud > 0)
                {
                    int danioRecibido = 0;
                    if (entidad is Enemigo enemigo) danioRecibido = enemigo.Daño;
                    else if (entidad is Robot robot) danioRecibido = robot.Daño;

                    if (jugador.ArmaduraEquipada != null)
                    {
                        danioRecibido = Math.Max(2, danioRecibido - jugador.ArmaduraEquipada.Valor);
                    }

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"¡{entidad.Nombre} contraataca, causando {danioRecibido} de daño!");
                    Console.ResetColor();

                    jugador.Salud -= danioRecibido;
                    VerificarSalud();
                }
            }
        }

        static void AdministrarInventario(bool enCombate = false)
        {
            if (jugador.Inventario.Count == 0)
            {
                Console.WriteLine("\n[INVENTARIO] Tu inventario está completamente vacío.");
                return;
            }

            Console.WriteLine("\n--- INVENTARIO TÁCTICO ---");
            for (int i = 0; i < jugador.Inventario.Count; i++)
            {
                Objeto obj = jugador.Inventario[i];
                Console.WriteLine($"{i + 1}. {obj.Nombre} [{obj.Tipo}] -> Valor/Efecto: +{obj.Valor}");
            }
            Console.WriteLine("0. Volver");
            Console.Write("Selecciona un objeto para usar o equipar: ");

            if (int.TryParse(Console.ReadLine(), out int seleccion))
            {
                if (seleccion == 0) return;

                if (seleccion > 0 && seleccion <= jugador.Inventario.Count)
                {
                    Objeto obj = jugador.Inventario[seleccion - 1];

                    if (obj.Tipo == "Energia")
                    {
                        jugador.Energia = Math.Min(jugador.EnergiaMaxima, jugador.Energia + obj.Valor);
                        Console.WriteLine($"Has consumido {obj.Nombre}. Energía restaurada a: {jugador.Energia}");
                        jugador.Inventario.RemoveAt(seleccion - 1);
                    }
                    else if (obj.Tipo == "Salud")
                    {
                        jugador.Salud = Math.Min(jugador.SaludMaxima, jugador.Salud + obj.Valor);
                        Console.WriteLine($"Has aplicado {obj.Nombre}. Salud actual: {jugador.Salud}");
                        jugador.Inventario.RemoveAt(seleccion - 1);
                    }
                    else if (obj.Tipo == "Arma")
                    {
                        jugador.ArmaEquipada = obj;
                        Console.WriteLine($"¡Has equipado el arma: {obj.Nombre} (+{obj.Valor} de daño base)!");
                    }
                    else if (obj.Tipo == "Armadura")
                    {
                        jugador.ArmaduraEquipada = obj;
                        Console.WriteLine($"¡Has equipado la armadura: {obj.Nombre} (-{obj.Valor} de daño recibido)!");
                    }
                    else
                    {
                        Console.WriteLine("Este objeto no se puede consumir directamente aquí.");
                    }
                }
                else
                {
                    Console.WriteLine("[ERROR] Selección fuera de rango.");
                }
            }
            else
            {
                Console.WriteLine("[ERROR] Entrada no válida.");
            }
        }

        static void VerMision()
        {
            Console.WriteLine("\n--- REGISTRO DE MISIÓN ---");
            Console.WriteLine($"1. Encontrar el Núcleo Energético en (4,4): {(encontroNucleo ? "COMPLETADO" : "PENDIENTE")}");
            Console.WriteLine($"2. Regresar a la extracción (0,0) con vida: {(misionCompletada ? "COMPLETADO" : "PENDIENTE")}");

            if (encontroNucleo && jugador.PosX == 0 && jugador.PosY == 0)
            {
                misionCompletada = true;
                CompletarMisionFase2();
            }
            else if (encontroNucleo)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n[RADAR] ¡Núcleo asegurado! Dirígete de vuelta a la coordenada de origen (0,0) para completar la extracción.");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("\n[RADAR] Explora hacia el sureste para encontrar el origen de la anomalía en (4,4).");
            }
        }

        static void CompletarMisionFase1()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n¡Has asegurado el Núcleo Energético Anómalo!");
            Console.WriteLine("MISIÓN ACTUALIZADA: Vuelve a la base inicial (X:0, Y:0) para la extracción.");
            Console.ResetColor();
        }

        static void CompletarMisionFase2()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n==================================================");
            Console.WriteLine("          🟢 MISIÓN CUMPLIDA CON ÉXITO            ");
            Console.WriteLine("==================================================");
            Console.WriteLine($" Operativo: {jugador.Nombre} ({jugador.Rol})");
            Console.WriteLine(" Recompensa: +250 XP | Rango de Élite Desbloqueado");
            Console.WriteLine(" El Núcleo ha sido asegurado en los laboratorios centrales.");
            Console.WriteLine("==================================================");
            Console.ResetColor();
            gameOver = true;
        }

        static void VerificarSalud()
        {
            if (jugador.Salud <= 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n💀 SALUD CERO. Tus signos vitales han colapsado.");
                Console.WriteLine("==================================================");
                Console.WriteLine("              🔴 FIN DEL JUEGO (GAME OVER)        ");
                Console.WriteLine("==================================================");
                Console.ResetColor();
                gameOver = true;
            }
        }
    }
}
