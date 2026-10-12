using System;
using System.Collections.Generic;
using System.Threading;

namespace NexusV1
{
    // --- 1. SISTEMA DE PERSONAJES (HERENCIA Y POLIMORFISMO) ---
    abstract class Personaje
    {
        public string Nombre { get; set; }
        public int Salud { get; set; }

        public Personaje(string nombre, int salud)
        {
            Nombre = nombre;
            Salud = salud;
        }

        public abstract void Reaccionar();
    }

    class Cadete : Personaje
    {
        public int Energia { get; set; }
        public int Experiencia { get; set; }
        public List<Objeto> Inventario { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }

        public Cadete(string nombre) : base(nombre, 100)
        {
            Energia = 100;
            Experiencia = 0;
            Inventario = new List<Objeto>();
            PosX = 0;
            PosY = 0;
        }

        public override void Reaccionar() { } // El cadete reacciona mediante el menú del jugador
    }

    class Enemigo : Personaje
    {
        public int Daño { get; set; }
        public Enemigo(string nombre, int salud, int daño) : base(nombre, salud) { Daño = daño; }

        public override void Reaccionar()
        {
            Console.WriteLine($"[!] El {Nombre} ruge e inicia una postura de combate.");
        }
    }

    class Robot : Personaje
    {
        public int NivelSeguridad { get; set; }
        public Robot(string nombre, int salud, int nivelSeguridad) : base(nombre, salud) { NivelSeguridad = nivelSeguridad; }

        public override void Reaccionar()
        {
            Console.WriteLine($"[!] El {Nombre} activa sus protocolos de defensa nivel {NivelSeguridad}.");
        }
    }

    // --- SISTEMA DE INVENTARIO ---
    class Objeto
    {
        public string Nombre { get; set; }
        public string Tipo { get; set; } // "Energia", "Salud", "Ataque", "Analizador"
        public int Valor { get; set; }

        public Objeto(string nombre, string tipo, int valor)
        {
            Nombre = nombre;
            Tipo = tipo;
            Valor = valor;
        }
    }

    // --- CONTROLADOR PRINCIPAL DEL JUEGO ---
    class Program
    {
        static Cadete jugador = null!;
        static bool misionCompletada = false;
        static bool gameOver = false;
        static bool encontroNucleo = false;
        static Random random = new Random();

        // Mapa de 5x5 (Representa la matriz 2D de la realidad desconocida)
        static int limiteMapa = 5;

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("==================================================");
            Console.WriteLine("       N E X U S  v1.0 - AÑO 2297                 ");
            Console.WriteLine("==================================================");
            Console.ResetColor();

            // Identificación
            Console.Write("Ingrese su identificación de Cadete: ");
            string nombre = Console.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(nombre)) nombre = "Explorer Desconocido";

            jugador = new Cadete(nombre);
            jugador.Inventario.Add(new Objeto("Célula de energía básica", "Energia", 25)); // Objeto inicial

            Console.WriteLine($"\n[SISTEMA] Bienvenido, Cadete {jugador.Nombre}.");
            Console.WriteLine("[MISIÓN ASIGNADA] 'La anomalía perdida'");
            Console.WriteLine("- Objetivo 1: Encontrar el núcleo energético (Ubicación desconocida).");
            Console.WriteLine("- Objetivo 2: Sobrevivir y extraer.\n");

            Thread.Sleep(2000);

            // Bucle principal (Control de situaciones)
            while (!gameOver)
            {
                MostrarInterfaz();
                ProcesarMenuPrincipal();
            }
        }

        static void MostrarInterfaz()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n--- ESTADO --- | Salud: {jugador.Salud}/100 | Energía: {jugador.Energia}/100 | XP: {jugador.Experiencia}");
            Console.WriteLine($"--- POSICIÓN - | X: {jugador.PosX} Y: {jugador.PosY}");
            Console.ResetColor();
            Console.WriteLine("1. Explorar (Moverse)");
            Console.WriteLine("2. Abrir Inventario");
            Console.WriteLine("3. Ver Estado de Misión");
            Console.WriteLine("4. Abortar Misión (Salir)");
            Console.Write("Seleccione una acción: ");
        }

        // --- 7. CONTROL DE SITUACIONES (Validaciones) ---
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
                    Console.WriteLine("[SISTEMA] Misión abortada. El cadete ha escapado.");
                    gameOver = true;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[ERROR] Opción inválida. El sistema táctico no reconoce la orden.");
                    Console.ResetColor();
                    break;
            }
        }

        // --- 5. EXPLORACIÓN Y 6. EVENTOS ---
        static void Moverse()
        {
            Console.Write("Dirección (W=Norte, S=Sur, A=Oeste, D=Este): ");
            string dir = (Console.ReadLine() ?? "").ToUpper();

            int nuevaX = jugador.PosX;
            int nuevaY = jugador.PosY;

            if (dir == "W") nuevaY++;
            else if (dir == "S") nuevaY--;
            else if (dir == "D") nuevaX++;
            else if (dir == "A") nuevaX--;
            else
            {
                Console.WriteLine("[ERROR] Movimiento no reconocido.");
                return;
            }

            // Control de límites del mapa
            if (nuevaX < 0 || nuevaX >= limiteMapa || nuevaY < 0 || nuevaY >= limiteMapa)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("🚨 ADVERTENCIA: Intentando moverse fuera de los límites seguros. Movimiento cancelado.");
                Console.ResetColor();
                return;
            }

            jugador.PosX = nuevaX;
            jugador.PosY = nuevaY;
            jugador.Energia -= 5; // Costo de exploración

            Console.WriteLine($"\nDesplazándose a la zona ({jugador.PosX}, {jugador.PosY})...");
            Thread.Sleep(1000);

            if (jugador.Energia <= 0)
            {
                Console.WriteLine("🔋 PÉRDIDA DE ENERGÍA CRÍTICA. No puedes continuar. Fin de la transmisión.");
                gameOver = true;
                return;
            }

            GenerarEventoAleatorio();
        }

        static void GenerarEventoAleatorio()
        {
            int prob = random.Next(1, 101);

            // Evento Misión Principal: La Anomalía (Fijada en 4,4 por ejemplo)
            if (jugador.PosX == 4 && jugador.PosY == 4 && !encontroNucleo)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("\n⚡ ANOMALÍA DETECTADA ⚡");
                Console.WriteLine("Has encontrado el Núcleo Energético... pero está protegido.");
                Console.ResetColor();
                encontroNucleo = true;
                Enemigo jefe = new Enemigo("Guardián del Núcleo", 80, 25);
                IniciarEncuentro(jefe);
                return;
            }

            // Eventos aleatorios
            if (prob <= 30) // 30% nada
            {
                Console.WriteLine("La zona está despejada. No se detecta nada inusual.");
            }
            else if (prob <= 55) // 25% objeto
            {
                Objeto nuevoObjeto = new Objeto("Célula de Energía", "Energia", 30);
                jugador.Inventario.Add(nuevoObjeto);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"🎁 OBJETO ENCONTRADO: {nuevoObjeto.Nombre} ha sido añadido al inventario.");
                Console.ResetColor();
            }
            else if (prob <= 70) // 15% Robot
            {
                Console.WriteLine("👾 ENTIDAD DETECTADA");
                Robot robot = new Robot("Dron Centinela", 40, 2);
                IniciarEncuentro(robot);
            }
            else if (prob <= 90) // 20% Enemigo
            {
                Console.WriteLine("👾 ENTIDAD DETECTADA");
                Enemigo alien = new Enemigo("Rastreador del Vacío", 50, 15);
                IniciarEncuentro(alien);
            }
            else // 10% Trampa o Sistema dañado
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("🚨 SISTEMA DAÑADO: Has entrado en un campo de radiación.");
                Console.ResetColor();
                jugador.Salud -= 15;
                VerificarSalud();
            }
        }

        // --- 2. SISTEMA DE ENCUENTROS ---
        static void IniciarEncuentro(Personaje entidad)
        {
            entidad.Reaccionar(); // Polimorfismo en acción
            bool enCombate = true;

            while (enCombate && !gameOver)
            {
                Console.WriteLine($"\n--- ENFRENTAMIENTO --- | {entidad.Nombre} (Salud: {entidad.Salud})");
                Console.WriteLine("1. Atacar");
                Console.WriteLine("2. Escapar");
                Console.WriteLine("3. Utilizar Objeto");
                Console.WriteLine("4. Analizar entidad");
                Console.Write("Acción táctica: ");

                string accion = Console.ReadLine() ?? "";

                switch (accion)
                {
                    case "1":
                        // Atacar
                        int dañoHecho = random.Next(15, 30);
                        entidad.Salud -= dañoHecho;
                        Console.WriteLine($"Atacaste a {entidad.Nombre} causando {dañoHecho} de daño.");
                        if (entidad.Salud <= 0)
                        {
                            Console.WriteLine($"Has derrotado a {entidad.Nombre}!");
                            jugador.Experiencia += 30;
                            enCombate = false;

                            // Si era el jefe
                            if (entidad.Nombre == "Guardián del Núcleo")
                            {
                                CompletarMision();
                            }
                        }
                        break;
                    case "2":
                        // Escapar
                        if (random.Next(1, 100) > 40)
                        {
                            Console.WriteLine("Has logrado escapar de la entidad.");
                            enCombate = false;
                        }
                        else
                        {
                            Console.WriteLine("Intento de escape fallido!");
                        }
                        break;
                    case "3":
                        AdministrarInventario(true); // Uso en combate
                        break;
                    case "4":
                        Console.WriteLine($"ANALIZADOR: {entidad.Nombre}. Es un {(entidad is Robot ? "Robot de seguridad" : "Enemigo biológico")}.");
                        break;
                    default:
                        Console.WriteLine("Comando inválido. Pierdes tu turno.");
                        break;
                }

                // Turno del enemigo si sigue vivo y no escapamos
                if (enCombate && entidad.Salud > 0)
                {
                    if (entidad is Enemigo enemigo)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"El {enemigo.Nombre} te ataca y causa {enemigo.Daño} de daño!");
                        Console.ResetColor();
                        jugador.Salud -= enemigo.Daño;
                        VerificarSalud();
                    }
                    else if (entidad is Robot robot)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"El {robot.Nombre} dispara un láser de supresión causando 10 de daño.");
                        Console.ResetColor();
                        jugador.Salud -= 10;
                        VerificarSalud();
                    }
                }
            }
        }

        // --- 3. SISTEMA DE INVENTARIO ---
        static void AdministrarInventario(bool enCombate = false)
        {
            if (jugador.Inventario.Count == 0)
            {
                Console.WriteLine("\n[INVENTARIO] Vacío.");
                return;
            }

            Console.WriteLine("\n--- INVENTARIO ---");
            for (int i = 0; i < jugador.Inventario.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {jugador.Inventario[i].Nombre} (+{jugador.Inventario[i].Valor} {jugador.Inventario[i].Tipo})");
            }
            Console.WriteLine("0. Volver");
            Console.Write("Seleccione un objeto para usar: ");

            if (int.TryParse(Console.ReadLine(), out int seleccion))
            {
                if (seleccion == 0) return;

                if (seleccion > 0 && seleccion <= jugador.Inventario.Count)
                {
                    Objeto obj = jugador.Inventario[seleccion - 1];
                    if (obj.Tipo == "Energia")
                    {
                        jugador.Energia = Math.Min(100, jugador.Energia + obj.Valor);
                        Console.WriteLine($"Has usado {obj.Nombre}. Energía actual: {jugador.Energia}");
                    }
                    else if (obj.Tipo == "Salud")
                    {
                        jugador.Salud = Math.Min(100, jugador.Salud + obj.Valor);
                        Console.WriteLine($"Has usado {obj.Nombre}. Salud actual: {jugador.Salud}");
                    }
                    jugador.Inventario.RemoveAt(seleccion - 1); // Eliminar al usar
                }
                else
                {
                    Console.WriteLine("[ERROR] El objeto seleccionado no existe en tu inventario."); // Control de situación
                }
            }
            else
            {
                Console.WriteLine("[ERROR] Entrada inválida.");
            }
        }

        // --- 4. SISTEMA DE MISIONES ---
        static void VerMision()
        {
            Console.WriteLine("\n--- MISIÓN: LA ANOMALÍA PERDIDA ---");
            Console.WriteLine($"1. Encontrar el núcleo: {(encontroNucleo ? "COMPLETADO" : "PENDIENTE")}");
            Console.WriteLine($"2. Extraer con vida: {(misionCompletada ? "COMPLETADO" : "PENDIENTE")}");

            if (encontroNucleo && jugador.PosX == 0 && jugador.PosY == 0)
            {
                Console.WriteLine("\n¡Has vuelto al punto de extracción con el núcleo!");
                misionCompletada = true;
                CompletarMisionFase2();
            }
            else if (encontroNucleo)
            {
                Console.WriteLine("\n[ALERTA] Vuelve a la coordenada X:0, Y:0 para extraer!");
            }
        }

        static void CompletarMision()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n¡Has recuperado el Núcleo Energético!");
            Console.WriteLine("MISIÓN ACTUALIZADA: Regresa a la zona de aterrizaje (X:0, Y:0) para la extracción.");
            Console.ResetColor();
        }

        static void CompletarMisionFase2()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n==================================================");
            Console.WriteLine("               🟢 MISIÓN COMPLETADA              ");
            Console.WriteLine("==================================================");
            Console.WriteLine("+ 100 XP");
            Console.WriteLine("+ 1 Módulo desconocido añadido a la base");
            Console.WriteLine("==================================================");
            Console.ResetColor();
            gameOver = true;
        }

        static void VerificarSalud()
        {
            if (jugador.Salud <= 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nSALUD CRÍTICA. El cadete ha sido eliminado en acción.");
                Console.WriteLine("--- FIN DEL JUEGO ---");
                Console.ResetColor();
                gameOver = true;
            }
        }
    }
}


