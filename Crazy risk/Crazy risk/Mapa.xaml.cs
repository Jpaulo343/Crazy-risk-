using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Crazy_risk
{
    public partial class Mapa : Page
    {
        public Juego juego { get; private set; }
        public string jugadorLocal { get; private set; }
        public Conexion conexion { get; private set; }

        private bool soyServidor;

        public Mapa(string nombre1, string nombre2, string local, Conexion conexion)
        {
            InitializeComponent();
            jugadorLocal = local;
            this.conexion = conexion;
            juego = new Juego(nombre1, nombre2);

            DetectarSiSoyServidor(nombre1, local);

            NombreJugadorText1.Text = juego.listaJugadores.ObtenerEnIndice(0).Nombre;
            NombreJugadorText2.Text = juego.listaJugadores.ObtenerEnIndice(1).Nombre;
            NombreJugadorText3.Text = juego.listaJugadores.ObtenerEnIndice(2).Nombre;

            ColorJugador1.Fill = juego.listaJugadores.ObtenerEnIndice(0).Color;
            ColorJugador2.Fill = juego.listaJugadores.ObtenerEnIndice(1).Color;
            ColorJugador3.Fill = juego.listaJugadores.ObtenerEnIndice(2).Color;

            PanelEstado.DataContext = juego.ObtenerJugadorActual();
            CuadrosCartas.DataContext = juego.ObtenerJugadorActual();
            BtnTransferir.DataContext = juego.ObtenerJugadorActual();

            actualizarTextoGuia();

            _ = EscucharMensajesAsync();
            _ = EnviarEstadoAsync(); // internamente solo envía si soyServidor

            foreach (var t in juego.listaTerritorios.Enumerar())
            {
                var pathObjeto = this.FindName(t.Nombre) as System.Windows.Shapes.Path;
                if (pathObjeto != null)
                {
                    pathObjeto.DataContext = t;
                    pathObjeto.PreviewMouseRightButtonDown += Territorio_ClickDerecho;
                }
            }
        }

        private void DetectarSiSoyServidor(string nombre1, string local)
        {
            // Patrón de navegación que usas: si nombre1 == local, eres el servidor.
            soyServidor = (nombre1 == local);
        }

        private async Task EscucharMensajesAsync()
        {
            if (conexion == null) return;
            try
            {
                while (conexion.EstaConectado)
                {
                    var msg = await conexion.Recibir(); // "COMANDO|param1|param2..."
                    if (string.IsNullOrWhiteSpace(msg)) break;
                    await Dispatcher.InvokeAsync(() => ProcesarMensaje(msg));
                }
            }
            catch
            {
                // Log opcional
            }
        }

        private void ProcesarMensaje(string msg)
        {
            var parts = msg.Split('|');
            var cmd = parts[0];

            switch (cmd)
            {
                case "PLACE":
                    {
                        string nombreT = parts[1];
                        var t = juego.listaTerritorios.BuscarPorCondición(tt => tt.Nombre == nombreT);
                        if (t != null)
                        {
                            juego.AñadirTropas(t);
                            actualizarInterfaz();
                        }
                        break;
                    }

                case "NEXT":
                    {
                        juego.AvanzarFase();
                        actualizarInterfaz();
                        break;
                    }

                case "STATE":
                    {
                        string json = msg.Substring("STATE|".Length);
                        var estado = JsonSerializer.Deserialize<EstadoJuegoDTO>(json);
                        if (estado != null)
                        {
                            AplicarSnapshot(estado);
                            actualizarInterfaz();
                        }
                        break;
                    }
            }
        }

        private async Task EnviarEstadoAsync()
        {
            if (conexion?.EstaConectado == true && soyServidor)
            {
                var snap = CrearSnapshot();
                string json = JsonSerializer.Serialize(snap);
                await conexion.Enviar("STATE|" + json);
            }
        }

        private EstadoJuegoDTO CrearSnapshot()
        {
            return new EstadoJuegoDTO
            {
                TurnoActual = juego.ObtenerJugadorActual().Nombre,
                RondaInicial = juego.rondaInicial,
                Jugadores = juego.listaJugadores.Enumerar()
                    .Select(j => new JugadorDTO
                    {
                        Nombre = j.Nombre,
                        TropasDisponibles = j.TropasDisponibles,
                        Fase = j.Fase
                    }).ToArray(),
                Territorios = juego.listaTerritorios.Enumerar()
                    .Select(t => new TerritorioDTO
                    {
                        Nombre = t.Nombre,
                        Conquistador = t.Conquistador,
                        Tropas = t.Tropas
                    }).ToArray()
            };
        }

        private void AplicarSnapshot(EstadoJuegoDTO s)
        {
            foreach (var jd in s.Jugadores)
            {
                var jlocal = juego.listaJugadores.Enumerar().FirstOrDefault(x => x.Nombre == jd.Nombre);
                if (jlocal != null)
                {
                    jlocal.TropasDisponibles = jd.TropasDisponibles;
                    jlocal.Fase = jd.Fase;
                }
            }

            foreach (var td in s.Territorios)
            {
                var t = juego.listaTerritorios.BuscarPorCondición(x => x.Nombre == td.Nombre);
                if (t != null)
                {
                    t.Conquistador = td.Conquistador;
                    t.Tropas = td.Tropas;
                }
            }

            foreach (var j in juego.listaJugadores.Enumerar())
                j.territorios_Conquistados = new ListaEnlazada<string>();

            foreach (var t in juego.listaTerritorios.Enumerar())
            {
                var owner = t.Conquistador;
                if (!string.IsNullOrEmpty(owner))
                {
                    var j = juego.listaJugadores.BuscarPorCondición(x => x.Nombre == owner);
                    if (j != null) j.territorios_Conquistados.Añadir(t.Nombre);
                }
            }

            juego.ForzarRondaInicial(s.RondaInicial);
            juego.ForzarTurnoPorNombre(s.TurnoActual);
        }

        // CLICK IZQUIERDO TERRITORIO
        private async void Territorio_ClickIzquierdo(object sender, MouseButtonEventArgs e)
        {
            if (!CondiciónParaJugar()) return;

            var pathClickeado = sender as System.Windows.Shapes.Path;
            if (pathClickeado == null) return;

            var territorioClickeado = pathClickeado.DataContext as Territorio;
            if (territorioClickeado == null) return;

            bool jugadorCambiado = false;
            Jugador jugador = juego.ObtenerJugadorActual();
            int faseActual = jugador.Fase;

            // FASE 1 / RONDA INICIAL
            if (juego.rondaInicial || faseActual == 1)
            {
                jugadorCambiado = juego.AñadirTropas(territorioClickeado);
                actualizarInterfaz();

                if (conexion?.EstaConectado == true)
                {
                    await conexion.Enviar($"PLACE|{territorioClickeado.Nombre}");
                    if (jugadorCambiado) await conexion.Enviar("NEXT");
                    if (soyServidor) _ = EnviarEstadoAsync();
                }
                return;
            }

            // FASE 2: ATAQUE
            if (faseActual == 2)
            {
                if (juego.origenSeleccionado == null)
                {
                    if (!jugador.verificarTerritorio(territorioClickeado) || territorioClickeado.Tropas < 2)
                    {
                        MessageBox.Show("Elige un territorio TUYO con al menos 2 tropas para atacar.");
                        return;
                    }
                    juego.AsignarOrigenSeleccionado(territorioClickeado);
                    actualizarInterfaz();
                    return;
                }

                // Cambiar ORIGEN si tocas uno propio
                if (jugador.verificarTerritorio(territorioClickeado))
                {
                    if (territorioClickeado.Tropas < 2)
                    {
                        MessageBox.Show("Ese territorio no tiene suficientes tropas (≥ 2).");
                        return;
                    }
                    juego.AsignarOrigenSeleccionado(territorioClickeado);
                    actualizarInterfaz();
                    return;
                }

                // Intentar marcar DESTINO (enemigo adyacente)
                if (!juego.AsignarDestinoAtaque(territorioClickeado))
                {
                    MessageBox.Show("El destino debe ser ENEMIGO y ADYACENTE al origen seleccionado.");
                    return;
                }

                var (resumen, conquistado) = juego.AtacarUnaVez();
                MessageBox.Show(resumen);

                if (juego.origenSeleccionado == null || juego.origenSeleccionado.Tropas < 2)
                    juego.cancelarTrasnferencia();

                if (conexion?.EstaConectado == true)
                    await EnviarEstadoAsync();

                verificarFinJuego();
                actualizarInterfaz();
                if (conexion?.EstaConectado == true && soyServidor) _ = EnviarEstadoAsync();
                return;
            }

            // FASE 3: TRANSFERENCIA (elegir origen)
            if (faseActual == 3)
            {
                juego.AsignarOrigenSeleccionado(territorioClickeado);
                actualizarInterfaz();
                return;
            }
            else
            {
                juego.deseleccionarTerritorios();
            }

            // Selección visual fuera de fases
            juego.selecionarTerritorio(territorioClickeado);
            if (jugadorCambiado) actualizarInterfaz();
        }

        // CLICK DERECHO TERRITORIO
        private async void Territorio_ClickDerecho(object sender, MouseButtonEventArgs e)
        {
            if (!CondiciónParaJugar()) return;

            var pathClickeado = sender as System.Windows.Shapes.Path;
            if (pathClickeado == null) return;
            Territorio? territorioClickeado = pathClickeado.DataContext as Territorio;
            int fase = juego.ObtenerJugadorActual().Fase;

            if (fase == 2)
            {
                if (juego.origenSeleccionado == null)
                {
                    MessageBox.Show("Primero elige el ORIGEN (click izquierdo sobre uno tuyo con ≥2 tropas).");
                    return;
                }

                if (!juego.AsignarDestinoAtaque(territorioClickeado))
                {
                    MessageBox.Show("El destino debe ser ENEMIGO y ADYACENTE al origen seleccionado.");
                    return;
                }

                var (resumen, conquistado) = juego.AtacarUnaVez();
                MessageBox.Show(resumen);

                if (juego.origenSeleccionado == null || juego.origenSeleccionado.Tropas < 2)
                    juego.cancelarTrasnferencia();

                if (conexion?.EstaConectado == true)
                    await EnviarEstadoAsync();

                verificarFinJuego();
                actualizarInterfaz();
                return;
            }

            if (fase == 3)
            {
                if (territorioClickeado == juego.origenSeleccionado) return;
                juego.AsignarDestinoSeleccionado(territorioClickeado!);
                return;
            }
        }

        private void verificarFinJuego()
        {
            Jugador ganador = juego.VerificarVictoria();
            if (ganador != null)
            {
                MessageBox.Show($"¡El ganador es {ganador.Nombre}!");
                Application.Current.Shutdown();
            }
        }

        private void Carta_ClickIzquierdo(object sender, MouseButtonEventArgs e)
        {
            if (!CondiciónParaJugar()) return;
            var borderCarta = sender as Border;
            if (borderCarta == null) return;
            Carta? cartaClickeada = borderCarta.DataContext as Carta;
            if (cartaClickeada != null) juego.seleccionarCartas(cartaClickeada);
        }

        private bool CondiciónParaJugar()
        {
            return (jugadorLocal == juego.ObtenerJugadorActual().Nombre);
        }

        private async void avanzarFase_Click(object sender, RoutedEventArgs e)
        {
            if (!CondiciónParaJugar()) return;

            juego.AvanzarFase();
            actualizarInterfaz();

            if (conexion != null && conexion.EstaConectado)
                await conexion.Enviar("NEXT");
        }

        private void IntecambiarCartas_Click(object sender, RoutedEventArgs e)
        {
            if (!CondiciónParaJugar()) return;

            bool completado = juego.IntercambiarCartas();
            if (!completado)
            {
                MessageBox.Show("Debes seleccionar 3 cartas para intercambiar");
            }
            else
            {
                MessageBox.Show("Intercambio completado, has recibido tropas");
            }
        }

        private void actualizarInterfaz()
        {
            PanelEstado.DataContext = juego.ObtenerJugadorActual();
            CuadrosCartas.DataContext = juego.ObtenerJugadorActual();
            BtnTransferir.DataContext = juego.ObtenerJugadorActual();
            actualizarTextoGuia();
        }

        private void ConfirmarTransferencia_Click(object sender, RoutedEventArgs e)
        {
            if (!CondiciónParaJugar()) return;

            if (juego.origenSeleccionado == null || juego.origenSeleccionado.Tropas < 2) return;

            int cantidad = (int)CantidadSlider.Value;
            bool completado = juego.TransferenciaTropas(cantidad);
            if (completado)
            {
                MessageBox.Show("Se han transferido " + cantidad + " tropas");
                juego.AvanzarFase();
                actualizarInterfaz();
            }
            else
            {
                MessageBox.Show("No se ha podido completar la transferencia, los territorios no estan conectados");
            }
            TrasnefirTopasPopup.IsOpen = false;
        }

        private void CancelarTransferencia_Click(object sender, RoutedEventArgs e)
        {
            if (!CondiciónParaJugar()) return;

            juego.cancelarTrasnferencia();
            TrasnefirTopasPopup.IsOpen = false;
        }

        private void BtnTransferir_Click(object sender, RoutedEventArgs e)
        {
            if (!CondiciónParaJugar()) return;

            if (juego.origenSeleccionado == null || juego.destinoSeleccionado == null)
            {
                MessageBox.Show("Debes seleccionar un territorio de origen (izq click) y uno de destino (der click).");
                return;
            }

            if (!juego.verificarOrigenValido())
            {
                MessageBox.Show("Debes seleccionar un territorio de origen que tenga al menos dos tropas");
                return;
            }

            TransferirTitulo.Text = $"Mover tropas de {juego.origenSeleccionado.Nombre} a {juego.destinoSeleccionado.Nombre}";
            CantidadSlider.Maximum = juego.origenSeleccionado.Tropas - 1;
            CantidadSlider.Value = 1;

            TrasnefirTopasPopup.IsOpen = true;
        }

        private void actualizarTextoGuia()
        {
            TextoGuía.Text = "Turno de " + juego.ObtenerJugadorActual().Nombre;
            int fase = juego.ObtenerJugadorActual().Fase;

            if (fase == 1) TextoGuía.Text += ": Coloca tus tropas";
            else if (fase == 2) TextoGuía.Text += ": Ataca un territorio enemigo";
            else TextoGuía.Text += ": Reubica tus tropas";
        }
    }
}
