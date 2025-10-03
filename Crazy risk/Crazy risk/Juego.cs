using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
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
        public int jugadorActivo { get; private set; }

        public Territorio? origenSeleccionado { get; private set; }
        public Territorio? destinoSeleccionado { get; private set; }

        public Juego(string NombreJugador1, string NombreJugador2)
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
            jugadores.Añadir(jugador3);
            jugadores.Añadir(jugador2);
            jugadores.Añadir(jugador1);

            this.jugadorActivo = 0;

            this.listaTerritorios = cargarDatosTerritorios();
            this.listaJugadores = jugadores;
            this.contadorFibonacciCartas = 1;
            this.generadorAleatorio = new Random();
            rondaInicial = true;

            RepartirTerritorios();
        }

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
                    listaAdyacentes.Añadir(nombreAdj);

                Territorio territorio = new Territorio(t.Nombre, t.Estado, listaAdyacentes, t.Tropas, t.Continente);
                territorios.Añadir(territorio);
            }
            return territorios;
        }

        public void ForzarRondaInicial(bool valor) => rondaInicial = valor;

        public void ForzarTurnoPorNombre(string nombre)
        {
            if (listaJugadores.ObtenerEnIndice(0).Nombre == nombre) jugadorActivo = 0;
            else if (listaJugadores.ObtenerEnIndice(1).Nombre == nombre) jugadorActivo = 1;
            else jugadorActivo = 0;
        }

        public Jugador VerificarVictoria()
        {
            string ganador = listaTerritorios.VerificarVictoria();
            if (ganador != null)
            {
                return this.listaJugadores.BuscarPorCondición(j => j.Nombre == ganador);
            }
            else
            {
                if (ObtenerJugadorNoActivo().territorios_Conquistados.size == 0)
                {
                    return ObtenerJugadorActual();
                }
            }
            return null;
        }

        public int LanzarDado() => generadorAleatorio.Next(1, 7);

        private bool SonAdyacentes(Territorio a, Territorio b) => a.Adyacentes.Buscar(b.Nombre);

        public bool PuedeAtacarDesde(Territorio origen)
        {
            var j = ObtenerJugadorActual();
            return origen != null && origen.Conquistador == j.Nombre && origen.Tropas >= 2;
        }

        public bool PuedeAtacarA(Territorio origen, Territorio destino)
        {
            var j = ObtenerJugadorActual();
            return destino != null && destino.Conquistador != j.Nombre && SonAdyacentes(origen, destino);
        }

        // Ordena de forma descendente usando solo ListaEnlazada<int>
        private static ListaEnlazada<int> OrdenarDesc(ListaEnlazada<int> lista)
        {
            ListaEnlazada<int> trabajo = new ListaEnlazada<int>();
            foreach (var v in lista.Enumerar()) trabajo.Añadir(v);

            ListaEnlazada<int> ascPorMax = new ListaEnlazada<int>();
            while (trabajo.size > 0)
            {
                int max = int.MinValue;
                foreach (var v in trabajo.Enumerar())
                    if (v > max) max = v;

                trabajo.Eliminar(max);
                ascPorMax.Añadir(max);
            }

            ListaEnlazada<int> desc = new ListaEnlazada<int>();
            foreach (var v in ascPorMax.Enumerar())
                desc.Añadir(v);

            return desc;
        }

        private (int perdidasAtq, int perdidasDef, ListaEnlazada<int> tiradaAtq, ListaEnlazada<int> tiradaDef)
        ResolverTirada(int dadosAtq, int dadosDef)
        {
            var tiradaAtq = new ListaEnlazada<int>();
            var tiradaDef = new ListaEnlazada<int>();

            for (int i = 0; i < dadosAtq; i++) tiradaAtq.Añadir(LanzarDado());
            for (int i = 0; i < dadosDef; i++) tiradaDef.Añadir(LanzarDado());

            tiradaAtq = OrdenarDesc(tiradaAtq);
            tiradaDef = OrdenarDesc(tiradaDef);

            int comps = Math.Min(tiradaAtq.size, tiradaDef.size);
            int perdAtq = 0, perdDef = 0;

            for (int i = 0; i < comps; i++)
            {
                int a = tiradaAtq.ObtenerEnIndice(i);
                int d = tiradaDef.ObtenerEnIndice(i);
                if (a > d) perdDef++; else perdAtq++;
            }
            return (perdAtq, perdDef, tiradaAtq, tiradaDef);
        }

        public (string resumen, bool conquistado) AtacarUnaVez()
        {
            if (origenSeleccionado == null || destinoSeleccionado == null)
                return ("Selecciona origen (tuyo) y destino (enemigo adyacente).", false);

            if (!PuedeAtacarDesde(origenSeleccionado))
                return ($"No puedes atacar desde {origenSeleccionado.Nombre}: requiere ≥2 tropas y ser tuyo.", false);
            if (!PuedeAtacarA(origenSeleccionado, destinoSeleccionado))
                return ($"{destinoSeleccionado.Nombre} no es enemigo adyacente a {origenSeleccionado.Nombre}.", false);

            int dadosAtq = Math.Min(3, origenSeleccionado.Tropas - 1);
            int dadosDef = Math.Min(2, destinoSeleccionado.Tropas);
            if (dadosAtq <= 0 || dadosDef <= 0)
                return ("No hay dados válidos para atacar o defender.", false);

            var (perdAtq, perdDef, tirAtq, tirDef) = ResolverTirada(dadosAtq, dadosDef);

            origenSeleccionado.Tropas -= perdAtq;
            destinoSeleccionado.Tropas -= perdDef;

            bool conquista = destinoSeleccionado.Tropas == 0;
            if (conquista)
            {
                int aMover = Math.Min(dadosAtq, origenSeleccionado.Tropas - 1);
                if (aMover < 1) aMover = 1;

                string defensorPrevio = destinoSeleccionado.Conquistador;
                var atacante = ObtenerJugadorActual();
                var defensor = listaJugadores.BuscarPorCondición(j => j.Nombre == defensorPrevio);

                GenerarCarta(destinoSeleccionado, atacante);

                defensor?.PerderTerritorio(destinoSeleccionado.Nombre);
                atacante.territorios_Conquistados.Añadir(destinoSeleccionado.Nombre);
                destinoSeleccionado.Conquistador = atacante.Nombre;

                destinoSeleccionado.Tropas = 0;
                origenSeleccionado.Tropas -= aMover;
                destinoSeleccionado.Tropas += aMover;
            }

            string resumen = $"Atq [{string.Join(",", tirAtq.Enumerar())}] vs Def [{string.Join(",", tirDef.Enumerar())}]  " +
                             $"=> pérdidas Atacante:{perdAtq} Defensor:{perdDef}" +
                             (conquista ? $"  | ¡Conquistado {destinoSeleccionado.Nombre}!" : "");

            deseleccionarTerritorios();
            cancelarTrasnferencia();
            return (resumen, conquista);
        }

        public void cambiarTurno() => jugadorActivo = (jugadorActivo + 1) % 2;

        public Jugador ObtenerJugadorActual() => listaJugadores.ObtenerEnIndice(jugadorActivo);

        public Jugador ObtenerJugadorNoActivo() => listaJugadores.ObtenerEnIndice((jugadorActivo + 1) % 2);

        public bool AñadirTropas(Territorio territorio)
        {
            Jugador jugador = ObtenerJugadorActual();
            if (territorio.Conquistador == jugador.Nombre && jugador.TropasDisponibles > 0)
            {
                territorio.AgregarTropas(1);
                jugador.TropasDisponibles--;

                if (jugador.TropasDisponibles == 0 && rondaInicial)
                {
                    AvanzarFase();
                    return true;
                }
            }
            return false;
        }

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

                if (jugador.Fase == 3 || jugador.Fase == 2)
                {
                    deseleccionarTerritorios();
                    cancelarTrasnferencia();
                }

                if (jugador.Fase > 3)
                {
                    if (origenSeleccionado != null) origenSeleccionado.EstaSeleccionado = false;
                    if (destinoSeleccionado != null) destinoSeleccionado.EstaSeleccionado = false;
                    origenSeleccionado = null;
                    destinoSeleccionado = null;

                    jugador.Fase = 1;
                    if (jugadorActivo == 1)
                    {
                        calcularTropasRefuerzo(listaJugadores.ObtenerEnIndice(2));
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
                    if (i == 1 || i == 5) tropasBonus += 2;
                    else if (i == 0 || i == 3) tropasBonus += 3;
                    else tropasBonus += 3 + i;
                }
                break;
            }
            return tropasBonus;
        }

        public void selecionarTerritorio(Territorio territorio)
        {
            territorio!.EstaSeleccionado = !territorio.EstaSeleccionado;
        }

        public void deseleccionarTerritorios()
        {
            foreach (var territorio in listaTerritorios.Enumerar())
            {
                if (territorio != destinoSeleccionado)
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
                carta.Seleccionada = false;
        }

        public bool IntercambiarCartas()
        {
            Jugador jugador = ObtenerJugadorActual();
            ListaEnlazada<Carta> cartasACanjear = new ListaEnlazada<Carta>();
            foreach (var carta in jugador.Cartas)
                if (carta.Seleccionada) cartasACanjear.Añadir(carta);

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
                        jugador.Cartas.Remove(carta);
                    IncrementarContadorFibonacciCartas();
                    return true;
                }
            }
            return false;
        }

        private int calcularPosiciónFibonacci()
        {
            int val1 = 1, val2 = 1, res;
            for (int i = 0; i < contadorFibonacciCartas; i++)
            {
                res = val1 + val2;
                val1 = val2;
                val2 = res;
            }
            return val2;
        }

        public void IncrementarContadorFibonacciCartas() => contadorFibonacciCartas++;

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

        public void AsignarOrigenSeleccionado(Territorio t)
        {
            if (destinoSeleccionado == t) return;
            if (origenSeleccionado == t)
            {
                t.EstaSeleccionado = false;
                origenSeleccionado = null;
                return;
            }
            Jugador jugadoractual = ObtenerJugadorActual();
            if (jugadoractual.verificarTerritorio(t))
            {
                if (origenSeleccionado != null) origenSeleccionado.EstaSeleccionado = false;
                t.EstaSeleccionado = true;
                origenSeleccionado = t;
                Debug.WriteLine("Origen seleccionado: " + origenSeleccionado.Nombre);
            }
        }

        public void AsignarDestinoSeleccionado(Territorio t)
        {
            if (t == origenSeleccionado) return;
            if (destinoSeleccionado == t)
            {
                t.EstaSeleccionado = false;
                destinoSeleccionado = null;
                return;
            }
            Jugador jugadoractual = ObtenerJugadorActual();
            if (jugadoractual.verificarTerritorio(t))
            {
                if (destinoSeleccionado != null) destinoSeleccionado.EstaSeleccionado = false;
                t.EstaSeleccionado = true;
                destinoSeleccionado = t;
                Debug.WriteLine("Destino seleccionado: " + destinoSeleccionado.Nombre);
            }
        }

        internal bool verificarTransferenciaPosible(int cantidad)
        {
            return origenSeleccionado != null
                && cantidad >= 1
                && cantidad < origenSeleccionado.Tropas;
        }

        internal void cancelarTrasnferencia()
        {
            if (destinoSeleccionado != null) destinoSeleccionado.EstaSeleccionado = false;
            destinoSeleccionado = null;

            if (origenSeleccionado != null) origenSeleccionado.EstaSeleccionado = false;
            origenSeleccionado = null;
        }

        public bool verificarOrigenValido()
        {
            return origenSeleccionado != null && origenSeleccionado.Tropas >= 2;
        }

        public bool TransferenciaTropas(int cantidad)
        {
            if (origenSeleccionado != null && destinoSeleccionado != null)
            {
                if (verificarTransferenciaPosible(cantidad) && verificarConexionEntreTerritorios())
                {
                    origenSeleccionado.Tropas -= cantidad;
                    destinoSeleccionado.Tropas += cantidad;
                    cancelarTrasnferencia();
                    return true;
                }
                cancelarTrasnferencia();
            }
            return false;
        }

        public bool AsignarDestinoAtaque(Territorio t)
        {
            if (origenSeleccionado == null) return false;
            if (!PuedeAtacarA(origenSeleccionado, t)) return false;

            if (destinoSeleccionado != null) destinoSeleccionado.EstaSeleccionado = false;
            t.EstaSeleccionado = true;
            destinoSeleccionado = t;
            return true;
        }

        public void GenerarCarta(Territorio t, Jugador jugador)
        {
            if (jugador.contarCartasActivas() < 6 && !jugador.verificarCartaCreada(t))
            {
                int t1po = generadorAleatorio.Next(0, 3);
                TipoCarta tipoCarta = (TipoCarta)t1po;
                jugador.AgregarCarta(new Carta(tipoCarta, t.Nombre));
            }
        }

        public bool verificarConexionEntreTerritorios()
        {
            if (origenSeleccionado.Conquistador != destinoSeleccionado.Conquistador)
                return false;

            ListaEnlazada<string> visitados = new ListaEnlazada<string>();
            return vericarConexionRecursiva(origenSeleccionado, destinoSeleccionado, origenSeleccionado.Conquistador, visitados);
        }

        private bool vericarConexionRecursiva(Territorio actual, Territorio destino, string dueño, ListaEnlazada<string> visitados)
        {
            if (actual.Nombre == destino.Nombre) return true;

            visitados.Añadir(actual.Nombre);
            foreach (var nombreAdj in actual.Adyacentes.Enumerar())
            {
                Territorio adj = listaTerritorios.BuscarPorCondición(j => j.Nombre == nombreAdj);
                if (adj == null) continue;

                if (adj.Conquistador == dueño && !visitados.Buscar(adj.Nombre))
                    if (vericarConexionRecursiva(adj, destino, dueño, visitados)) return true;
            }
            return false;
        }
    }
}
