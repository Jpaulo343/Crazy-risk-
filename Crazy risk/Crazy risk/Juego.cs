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

        public Territorio origenSeleccionado { get; private set; }
        public Territorio destinoSeleccionado { get; private set; }

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
        /*Carga los datos de los territorios desde un archivo JSON y crea la lista de territorios del juego*/   
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


        /*Verifica si hay un ganador, en caso de que lo haya devuelve el objeto jugador correspondiente, si no retorna null*/
        public Jugador VerificarVictoria()
        {
            string ganador = listaTerritorios.VerificarVictoria();
            if (ganador != null)
            {
                return this.listaJugadores.BuscarPorCondición(j => j.Nombre == ganador);
            }
            return null;
        }
        /*Simula el lanzamiento de un dado, devolviendo un numero entre 1 y 6*/
        public int LanzarDado()
        {
            return generadorAleatorio.Next(1, 7);
        }

        /*Cambia el turno al siguiente jugador*/
        public void cambiarTurno()
        {
            jugadorActivo = (jugadorActivo + 1) % 2;
        }

        /*Devuelve el jugador que tiene el turno actual*/
        public Jugador ObtenerJugadorActual() 
        {
         return listaJugadores.ObtenerEnIndice(jugadorActivo);
        }

        /* Añade una tropa al territorio especificado, siempre y cuando el territorio pertenezca al jugador actual
         * y siempre y cuando el jugador tenga tropas disponibles
         * Si el jugador se queda sin tropas, avanza la fase del juego
         */
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

        /*Avanza la fase del jugador actual, y en caso de que haya terminado su turno, cambia al siguiente jugador
         * Si es el primer turno del juego, reparte las tropas del bot y calcula las tropas de refuerzo del primer jugador
         */
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

                if (jugador.Fase == 3) deseleccionarTerritorios();
                if (jugador.Fase > 3)
                {
                    if (origenSeleccionado != null || destinoSeleccionado != null) 
                    {
                        origenSeleccionado.EstaSeleccionado = false;
                        destinoSeleccionado.EstaSeleccionado = false;
                        origenSeleccionado = null!;
                        destinoSeleccionado = null!;
                    }

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

        /*Calcula las tropas de refuerzo que recibe un jugador al inicio de su turno*/
        private void calcularTropasRefuerzo(Jugador jugador) 
        {
            jugador.AgregarTropas((jugador.territorios_Conquistados.size / 3) + CalcularBonusContinentes(jugador));

        }

        /*Calcula el bonus de tropas que recibe un jugador por tener el control total de uno o más continentes*/
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

        /*Cambia el estado de todos los territorios a no seleccionados*/
        public void deseleccionarTerritorios() 
        {
        foreach (var territorio in listaTerritorios.Enumerar())
            {
                if (territorio != destinoSeleccionado) 
                    territorio.EstaSeleccionado = false;
            }
        }

        /*Permite seleccionar o deseleccionar una carta, siempre y cuando no se hayan seleccionado ya 3 cartas
         */
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

        /*Cambia el estado de todas las cartas del jugador actual a no seleccionadas
         */
        public void deseleccionarCartas()
        {
            foreach (var carta in ObtenerJugadorActual().Cartas)
            {
                carta.Seleccionada = false;
            }
        }


        /*Verifica que la cantidad de cartas seleccionadas sea 3, luego verifica que sean del mismo tipo o de tipos diferntes entre ellos,
        si cualquiera de las condiciónes se cumple, cambia las 3 cartas por tropas para el jugador*/
        public bool IntercambiarCartas()
        {
            Jugador jugador = ObtenerJugadorActual();
            ListaEnlazada<Carta> cartasACanjear = new ListaEnlazada<Carta>();
            foreach (var carta in jugador.Cartas)
            {
                if (carta.Seleccionada) cartasACanjear.Añadir(carta);
            }
            if (cartasACanjear.size == 3)
            {
                TipoCarta tipo1 = cartasACanjear.Head!.Value.Tipo;
                TipoCarta tipo2 = cartasACanjear.Head.Next!.Value.Tipo;
                TipoCarta tipo3 = cartasACanjear.Head.Next.Next!.Value.Tipo;

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
                    return true;
                }
            }
            return false;
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

        /* Asigna el territorio origen seleccionado, siempre y cuando no sea el mismo que el destino
         * y siempre y cuando el territorio origen pertenezca al jugador actual
         */
        public void AsignarOrigenSeleccionado(Territorio t)
        {
            if (destinoSeleccionado == t) return;
            if (origenSeleccionado==t)
            {
                t.EstaSeleccionado = false;
                origenSeleccionado= null!;
                return;
            }
            Jugador jugadoractual = ObtenerJugadorActual();
            if (jugadoractual.verificarTerritorio(t))
            {
                if (origenSeleccionado != null)
                    origenSeleccionado.EstaSeleccionado = false;
                t.EstaSeleccionado = true;
                origenSeleccionado = t;
                Debug.WriteLine("Origen seleccionado: " + origenSeleccionado.Nombre);
            }
         

        }
        /* Asigna el territorio destino seleccionado, siempre y cuando no sea el mismo que el origen
         * y siempre y cuando el territorio destino pertenezca al jugador actual
         */
        public void AsignarDestinoSeleccionado(Territorio t)
        {
            if (t==origenSeleccionado) return;
            if (destinoSeleccionado == t)
            {
                t.EstaSeleccionado = false;
                destinoSeleccionado = null!;
                return;
            }
            Jugador jugadoractual = ObtenerJugadorActual();
            if (jugadoractual.verificarTerritorio(t))
            {
                if (destinoSeleccionado != null)
                    destinoSeleccionado.EstaSeleccionado = false;
                t.EstaSeleccionado = true;
                destinoSeleccionado = t;
                Debug.WriteLine("Destino seleccionado: " + destinoSeleccionado.Nombre);
            }
            
        }
        
        internal bool verificarTransferenciaPosible(int cantidad) 
        {
            return (origenSeleccionado.Tropas > cantidad);
        }
        internal void cancelarTrasnferencia() 
        {
            destinoSeleccionado.EstaSeleccionado = false;
            destinoSeleccionado = null;
            origenSeleccionado.EstaSeleccionado = false;
            origenSeleccionado = null;
        }

        public bool verificarOrigenValido() 
        {
            return (origenSeleccionado.Tropas>1);
        }

        /* Transfiere la cantidad de tropas especificada desde el territorio origen al territorio destino
         * siempre y cuando ambos territorios hayan sido seleccionados y la transferencia sea posible
         */
        public void TransferenciaTropas(int cantidad)
        {
            if (origenSeleccionado != null && destinoSeleccionado != null)
            {
                if (verificarTransferenciaPosible(cantidad))
                {
                    int tropasParaTransferir = origenSeleccionado.Tropas - cantidad;
                    origenSeleccionado.Tropas -= tropasParaTransferir;
                    destinoSeleccionado.Tropas += tropasParaTransferir;
                }
                cancelarTrasnferencia();
            }

        }

    }
}
