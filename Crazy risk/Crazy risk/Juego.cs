using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            Jugador jugador1 = new Jugador(NombreJugador1, DiccionarioColor_Nombre.obtener(NombreJugador1), 1, territorios1, 40);

            ListaEnlazada<string> territorios2 = new ListaEnlazada<string>();
            Jugador jugador2 = new Jugador(NombreJugador2, DiccionarioColor_Nombre.obtener(NombreJugador2), 1, territorios2, 40);

            ListaEnlazada<string> territorios3 = new ListaEnlazada<string>();
            Jugador jugador3 = new Jugador("Neutral", DiccionarioColor_Nombre.obtener("Neutral"), 1, territorios3, 40);

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
            rondaInicial = false;

            RepartirTerritorios();
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

        //Activa la ronda inicial, en la cual los jugadores se turnan para colocar sus tropas iniciales
        public void EjecutarRondaInicial()
        {
            rondaInicial = true;
        }

        public void selecionarTerritorio()
        {
            rondaInicial = false;
        }

        //Devuelve el jugador que tiene el turno actual
        public Jugador ObtenerJugadorActual() 
        {
         return listaJugadores.ObtenerEnIndice(jugadorActivo);
        }

    }
}
