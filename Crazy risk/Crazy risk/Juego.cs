using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public Juego(ListaTerritorios territorios, ListaEnlazada<Jugador> jugadores, Random generadorDeNumeros)
        {
            this.listaTerritorios = territorios;
            this.listaJugadores = jugadores;
            this.contadorFibonacciCartas = contadorFibonacciCartas;
            this.jugadorActivo = 0;
            this.generadorAleatorio = generadorDeNumeros;
            rondaInicial = false;
        }


        public Jugador VerificarVictoria()
        {
            string ganador = listaTerritorios.VerificarVictoria();
            if (ganador != null)
            {
                return this.listaJugadores.BuscarPorCondición(j => j.Nombre == ganador);
            }
            return null;
        }
        public int LanzarDado()
        {
            return generadorAleatorio.Next(1, 7);
        }

        public void cambiarTurno()
        {
            jugadorActivo = (jugadorActivo + 1) % 2;
        }

        public void EjecutarRondaInicial()
        {
            rondaInicial = true;
        }

        public Jugador ObtenerJugadorActual() 
        {
         return listaJugadores.ObtenerEnIndice(jugadorActivo);
        }

    }
}
