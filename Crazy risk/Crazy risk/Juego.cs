using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media;


namespace Crazy_risk
{
    public class Juego
    {
        public ListaTerritorios listaTerritorios { get; private set; }
        public ListaEnlazada<Jugador> listaJugadores { get; private set; }
        public int contadorFibonacciCartas { get; private set; }
        public Random generadorAleatorio { get; private set; }
        public bool rondaInicial { get; private set; }
        public int jugadorActivo { get; private set; } //Puede ser 0 o 1, es el indice de la lista "jugadores"



        //Constructor del juego, recibe los nombres de los dos jugadores
        public Juego(string NombreJugador1,string NombreJugador2)
        {
            ListaEnlazada<Brush> colores = new ListaEnlazada<Brush>();
            colores.Añadir(Brushes.LightSalmon);
            colores.Añadir(Brushes.LightGreen);
            colores.Añadir(Brushes.MediumPurple);

            DiccionarioColor_Nombre.registrar(NombreJugador1, colores.Seleccionar_Y_Eliminar_Random());
            DiccionarioColor_Nombre.registrar(NombreJugador2, colores.Seleccionar_Y_Eliminar_Random());
            DiccionarioColor_Nombre.registrar("Neutral", colores.Seleccionar_Y_Eliminar_Random());

            ListaEnlazada<string> territorios1 = new ListaEnlazada<string>();
            Jugador jugador1 = new Jugador(NombreJugador1, DiccionarioColor_Nombre.obtener(NombreJugador1), 1, territorios1, 26);

            ListaEnlazada<string> territorios2 = new ListaEnlazada<string>();
            Jugador jugador2 = new Jugador(NombreJugador2, DiccionarioColor_Nombre.obtener(NombreJugador2), 1, territorios2, 26);

            ListaEnlazada<string> territorios3 = new ListaEnlazada<string>();
            Jugador jugador3 = new Jugador("Neutral", DiccionarioColor_Nombre.obtener("Neutral"), 1, territorios3, 26);

            ListaEnlazada<Jugador> jugadores = new ListaEnlazada<Jugador>();
            //El orden de inserción es importante, ya que el jugador en el indice 0 será el primero en jugar
            jugadores.Añadir(jugador3);
            jugadores.Añadir(jugador2);
            jugadores.Añadir(jugador1);

            this.listaTerritorios = cargarDatosTerritorios();
            this.listaJugadores = jugadores;
            this.contadorFibonacciCartas = 1;
            this.jugadorActivo = 0;
            Random generadorAleatorio = new Random();
            this.generadorAleatorio = generadorAleatorio;
            rondaInicial = true;

            RepartirTerritorios();
        }

        public void iniciarJuego()
        {
            
        }

        //Esta función se encarga de tomar todos los territorios y asignarles un dueño de manera aleatoria
        internal void RepartirTerritorios()
        {
            int indiceJugador = 0;
            ListaTerritorios territoriosDisponibles = listaTerritorios.CopiarLista();
            while (territoriosDisponibles.size > 0)
            {

                Territorio territorio = territoriosDisponibles.Seleccionar_Y_Eliminar_Random();

                Jugador jugador = listaJugadores.ObtenerEnIndice(indiceJugador);

                jugador.AsignarTerritorio(territorio);

                indiceJugador = (indiceJugador + 1) % listaJugadores.size;
            }
        }

        internal ListaTerritorios cargarDatosTerritorios()
        {
            string json = File.ReadAllText("DatosTerritorios.json");
            var territoriosJson = JsonSerializer.Deserialize<List<TerritorioJson>>(json);

            ListaTerritorios territorios = new();

            foreach (var t in territoriosJson)
            {
                var listaAdyacentes = new ListaEnlazada<string>();
                foreach (var nombreAdj in t.Adyacentes)
                {
                    listaAdyacentes.Añadir(nombreAdj);
                }
                Console.WriteLine(t.Nombre);
                Territorio territorio = new Territorio(t.Nombre, t.Estado, listaAdyacentes, t.Tropas, t.Continente);
                territorios.Añadir(territorio);
            }

            
            return territorios;
        }


        //Verifica si hay un ganador, en caso de que lo haya devuelve el objeto jugador correspondiente, si no retorna null
        public Jugador VerificarVictoria()
        {
            string ganador = listaTerritorios.VerificarVictoria();
            if (ganador != null)
            {
                return this.listaJugadores.BuscarPorCondición(j => j.Nombre == ganador);
            }
            return null;
        }
        //Simula el lanzamiento de un dado, devolviendo un numero entre 1 y 6/
        public int LanzarDado()
        {
            return generadorAleatorio.Next(1, 7);
        }

        //Cambia el turno al siguiente jugador
        public void cambiarTurno()
        {
            jugadorActivo = (jugadorActivo + 1) % 2;
        }

        //Devuelve el jugador que tiene el turno actual
        public Jugador ObtenerJugadorActual() 
        {
         return listaJugadores.ObtenerEnIndice(jugadorActivo);
        }
        public bool AñadirTropas(Territorio territorio) 
        {
            Jugador jugador= ObtenerJugadorActual();
            if (territorio.Conquistador == jugador.Nombre && jugador.TropasDisponibles > 0)
            {
                territorio.AgregarTropas(1);
                jugador.TropasDisponibles--;

                // Si ya no tiene tropas, pasa turno
                if (jugador.TropasDisponibles == 0 && rondaInicial)
                {

                    AvanzarFase();
                    return true;
                }
            }
            return false;
        }

        //Avanza a la siguiente fase del juego
        public void AvanzarFase() 
        {

            if (rondaInicial)
            {
                if (jugadorActivo == 1)
                {
                    repartirtropasBot();
                    rondaInicial = false;
                }
                cambiarTurno();
                calcularTropasRefuerzo(ObtenerJugadorActual());
            }
            else 
            {
                Jugador jugador = ObtenerJugadorActual();
                jugador.Fase++;
                if (jugador.Fase > 3)
                {   
                    jugador.Fase = 1;
                    if (jugadorActivo == 1)
                    {
                        calcularTropasRefuerzo(listaJugadores.ObtenerEnIndice(2)); //el bot recibe tropas de refuerzo y las reparte entre sus territorios
                        repartirtropasBot();
                    }
                    cambiarTurno();
                    calcularTropasRefuerzo(ObtenerJugadorActual());
                }
            }
        }


        private void calcularTropasRefuerzo(Jugador jugador) 
        {
            jugador.AgregarTropas((jugador.territorios_Conquistados.size / 3) + CalcularBonusContinentes(jugador));

        }

        private int CalcularBonusContinentes(Jugador jugador) 
        {
            int tropasBonus = 0;
            for (int i = 0; i <= 5; i++) 
            {

               if (listaTerritorios.Enumerar().All(t => t.Continente == i && t.Conquistador == jugador.Nombre))
               {
                    if (i == 1 || i == 5)
                    {
                        tropasBonus += 2;
                    }
                    else if (i == 0 || i == 3)
                    {
                        tropasBonus += 3;
                    }
                    else 
                    {
                        tropasBonus += 3 + i;
                    }
               }
                    break;
            }
            return tropasBonus;
        }



        public void selecionarTerritorio(Territorio territorio)
        {
            territorio!.EstaSeleccionado = !territorio.EstaSeleccionado;
        }

        //Cambia el estado de todos los territorios a no seleccionados
        public void deseleccionarTerritorios() 
        {
        foreach (var territorio in listaTerritorios.Enumerar())
            {
                territorio.EstaSeleccionado = false;
            }
        }

        public void seleccionarCartas(Carta carta)
        {
            int contadorCartasSeleccionadas = 0;
            foreach (var c in ObtenerJugadorActual().Cartas)
            {
                if (c.Seleccionada) contadorCartasSeleccionadas++;
                if (contadorCartasSeleccionadas >= 3 && !carta.Seleccionada) return;
            }
            carta.Seleccionada = !carta.Seleccionada;
        }
        public void deseleccionarCartas()
        {
            foreach (var carta in ObtenerJugadorActual().Cartas)
            {
                carta.Seleccionada = false;
            }
        }


        //Verifica que la cantidad de cartas seleccionadas sea 3, luego verifica que sean del mismo tipo o de tipos diferntes entre ellos,
        //si cualquiera de las condiciónes se cumple, cambia las 3 cartas por tropas para el jugador
        public void IntercambiarCartas()
        {
            Jugador jugador = ObtenerJugadorActual();
            ListaEnlazada<Carta> cartasACanjear = new ListaEnlazada<Carta>();
            foreach (var carta in jugador.Cartas)
            {
                if (carta.Seleccionada) cartasACanjear.Añadir(carta);
            }
            if (cartasACanjear.size == 3)
            {
                TipoCarta tipo1 = cartasACanjear.Head.Value.Tipo;
                TipoCarta tipo2 = cartasACanjear.Head.Next.Value.Tipo;
                TipoCarta tipo3 = cartasACanjear.Head.Next.Next.Value.Tipo;

                bool tresIguales = (tipo1 == tipo2 && tipo2 == tipo3);
                bool tresDistintos = (tipo1 != tipo2 && tipo2 != tipo3 && tipo1 != tipo3);
                if (tresIguales || tresDistintos)
                {
                    jugador.TropasDisponibles += calcularPosiciónFibonacci();
                    deseleccionarCartas();
                    foreach (Carta carta in cartasACanjear.Enumerar())
                    {
                        jugador.Cartas.Remove(carta);
                    }
                    IncrementarContadorFibonacciCartas();

                }
            }
        }


        //Calcula el numero en x posición en la suceccion de fibonacci
        private int calcularPosiciónFibonacci()
        {
            int val1 = 1;
            int val2 = 1;
            int res;

            for (int i = 0; i < contadorFibonacciCartas; i++) 
            {
                res = val1 + val2;
                val1 = val2;
                val2 = res;
            }
            return val2;
        }

        public void IncrementarContadorFibonacciCartas()
        {
            contadorFibonacciCartas++;
        }


        //Reparte las tropas del bot de manera aleatoria entre sus territorios conquistados
        public void repartirtropasBot()
        {
            Jugador bot = listaJugadores.ObtenerEnIndice(2);
            while (bot.TropasDisponibles > 0)
            {
                string territorioNombre = bot.territorios_Conquistados.ObtenerEnIndice(generadorAleatorio.Next(0, bot.territorios_Conquistados.size));
                Territorio territorio = listaTerritorios.BuscarPorCondición(t => t.Nombre == territorioNombre)!;
                if (territorio != null)
                {
                    territorio.AgregarTropas(1);
                    bot.TropasDisponibles--;
                }
                else
                {
                    bot.TropasDisponibles--;
                }
            }
        }
    }
}
