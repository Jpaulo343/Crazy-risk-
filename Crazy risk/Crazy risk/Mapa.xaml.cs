using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
<<<<<<< Updated upstream
using System.Threading.Tasks;
using System.Windows.Threading;
=======
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
        public Conexion conexion { get; private set; }
        public Mapa(string nombre1, string nombre2, string local, Conexion conexion)
=======

        public Mapa(string Nombre1, string Nombre2, string local)
>>>>>>> Stashed changes
        {
            InitializeComponent();
            jugadorLocal = local;
            this.conexion = conexion;
            juego = new Juego(nombre1, nombre2);
            soyServidor = (nombre1 == jugadorLocal);

            NombreJugadorText1.Text = juego.listaJugadores.ObtenerEnIndice(0).Nombre;
            NombreJugadorText2.Text = juego.listaJugadores.ObtenerEnIndice(1).Nombre;
            NombreJugadorText3.Text = juego.listaJugadores.ObtenerEnIndice(2).Nombre;

            ColorJugador1.Fill = juego.listaJugadores.ObtenerEnIndice(0).Color;
            ColorJugador2.Fill = juego.listaJugadores.ObtenerEnIndice(1).Color;
            ColorJugador3.Fill = juego.listaJugadores.ObtenerEnIndice(2).Color;

            PanelEstado.DataContext = juego.ObtenerJugadorActual();
            CuadrosCartas.DataContext = juego.ObtenerJugadorActual();
            BtnTransferir.DataContext = juego.ObtenerJugadorActual();

<<<<<<< Updated upstream
            DetectarSiSoyServidor(nombre1, local);

            // ... toda tu inicialización (textos, colores, DataContext, foreach de territorios)
            actualizarTextoGuia();

            // Empieza a escuchar mensajes
            _ = EscucharMensajesAsync();

            // ✅ MUY IMPORTANTE: si soy servidor, envío un snapshot inicial
            _ = EnviarEstadoAsync();
=======
>>>>>>> Stashed changes
            foreach (var t in juego.listaTerritorios.Enumerar())
            {
                var pathObjeto = this.FindName(t.Nombre) as System.Windows.Shapes.Path;
                if (pathObjeto != null)
                {
                    pathObjeto.DataContext = t;
                    pathObjeto.PreviewMouseRightButtonDown += Territorio_ClickDerecho; 
                }
            }
<<<<<<< Updated upstream
            actualizarTextoGuia();
            _ = EscucharMensajesAsync();
        }
        // Al final de cada constructor de Mapa:
        // no await
        private bool soyServidor; // el que “creó partida” será server

        // Llama a esto en el constructor para decidir si eres server
        private void DetectarSiSoyServidor(string nombre1, string local)
        {
            // En tus llamadas actuales:
            //  - Servidor navega con (alias, "JugadorRemoto", alias, conexion)
            //  - Cliente  navega con ("JugadorRemoto", alias, alias, conexion)
            // Por lo tanto, si nombre1 == local => soy servidor
            soyServidor = (nombre1 == local);
        }


        private async Task EscucharMensajesAsync()
        {
            if (conexion == null) return;
            try
            {
                while (conexion.EstaConectado)
                {
                    var msg = await conexion.Recibir(); // string "COMANDO|param1|param2..."
                    if (string.IsNullOrWhiteSpace(msg)) break;
                    await Dispatcher.InvokeAsync(() => ProcesarMensaje(msg));
                }
            }
            catch { /* opcional: log */ }
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

        // Construye un snapshot “plano” del juego (solo lo necesario para sincronizar)
        private EstadoJuegoDTO CrearSnapshot()
        {
            return new EstadoJuegoDTO
            {
                TurnoActual = juego.ObtenerJugadorActual().Nombre,
                RondaInicial = juego.rondaInicial, // si es public; si no, crea un getter
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

        // Aplica el snapshot recibido (ajusta estado local para que coincida)
        private void AplicarSnapshot(EstadoJuegoDTO s)
        {
            // Jugadores
            foreach (var jd in s.Jugadores)
            {
                var jlocal = juego.listaJugadores.Enumerar()
                               .FirstOrDefault(x => x.Nombre == jd.Nombre);
                if (jlocal != null)
                {
                    jlocal.TropasDisponibles = jd.TropasDisponibles;
                    jlocal.Fase = jd.Fase;
                }
            }

            // Territorios
            foreach (var td in s.Territorios)
            {
                var t = juego.listaTerritorios.BuscarPorCondición(x => x.Nombre == td.Nombre);
                if (t != null)
                {
                    t.Conquistador = td.Conquistador;
                    t.Tropas = td.Tropas;
                }
            }
            // Reconstruir listas de territorios por jugador según el snapshot
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
            // Ronda/Turno (clave para que el cliente pueda jugar)
            juego.ForzarRondaInicial(s.RondaInicial);
            juego.ForzarTurnoPorNombre(s.TurnoActual);
        }


        /*
         Esta funcion maneja el evento de click izquierdo en un territorio del mapa.
         Si el jugador local es el jugador actual, permite seleccionar o deseleccionar el territorio clickeado
         y, si no es la ronda inicial, intenta añadir tropas al territorio seleccionado.
         */
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

            // --- FASE 1 / RONDA INICIAL: colocar tropas ---
            if (juego.rondaInicial || faseActual == 1)
            {
                jugadorCambiado = juego.AñadirTropas(territorioClickeado); // puede avanzar fase adentro
                actualizarInterfaz();

                // Enviar acción al rival
                if (conexion?.EstaConectado == true)
                {
                    await conexion.Enviar($"PLACE|{territorioClickeado.Nombre}");
                    if (jugadorCambiado)
                        await conexion.Enviar("NEXT");

                    // Si usas servidor autoritario + snapshot, mantén esta línea.
                    // Si NO lo tienes implementado, bórrala.
                    if (soyServidor) _ = EnviarEstadoAsync();
                }
                return;
            }

            // --- FASE 2: ataque (selección de origen/destino y ataque) ---
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

                // Cambiar ORIGEN si toca uno propio
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

                // Si ya tienes protocolo de sincronización de ataque/snapshot, envíalo aquí:
                if (conexion?.EstaConectado == true && soyServidor) _ = EnviarEstadoAsync();

                return;
            }

            // --- FASE 3: transferencia (elegir origen) ---
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

            // Selección visual por fuera de las fases anteriores
            juego.selecionarTerritorio(territorioClickeado);
            if (jugadorCambiado) actualizarInterfaz();
        }

        /*
         Esta función maneja el evento de click derecho en un territorio del mapa.
         Si el jugador local es el jugador actual y está en la fase 3, permite seleccionar el territorio clickeado como destino
         para una posible transferencia de tropas.
         */
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


=======

            juego.ObtenerJugadorActual().AgregarCarta(new Carta(TipoCarta.Infanteria, "alaasdsadasdasska"));
            juego.ObtenerJugadorActual().AgregarCarta(new Carta(TipoCarta.Caballeria, "japon"));
            juego.ObtenerJugadorActual().AgregarCarta(new Carta(TipoCarta.Caballeria, "camerun"));
            juego.ObtenerJugadorActual().AgregarCarta(new Carta(TipoCarta.Artilleria, "westUs"));
            juego.ObtenerJugadorActual().AgregarCarta(new Carta(TipoCarta.Caballeria, "brasil"));
            juego.ObtenerJugadorActual().AgregarCarta(new Carta(TipoCarta.Caballeria, "peru"));

            actualizarTextoGuia();
        }
>>>>>>> Stashed changes
        /*
         Esta función maneja el evento de click izquierdo en una carta.
         Si el jugador local es el jugador actual, permite seleccionar la carta clickeada para posibles acciones posteriores.
         */
        private void Territorio_ClickIzquierdo(object sender, MouseButtonEventArgs e)
        {
            if (!CondiciónParaJugar()) return;

            var path = sender as System.Windows.Shapes.Path;
            if (path == null) return;

            var territorio = path.DataContext as Territorio;
            if (territorio == null) return;

            bool jugadorCambiado = false;
            var jugador = juego.ObtenerJugadorActual();
            int fase = jugador.Fase;

            if (juego.rondaInicial || fase == 1)
            {
                jugadorCambiado = juego.AñadirTropas(territorio);
                if (jugadorCambiado) actualizarInterfaz();
                return;
            }

            if (fase == 2)
            {

                if (juego.origenSeleccionado == null)
                {
                    if (!jugador.verificarTerritorio(territorio) || territorio.Tropas < 2)
                    {
                        MessageBox.Show("Elige un territorio TUYO con al menos 2 tropas para atacar.");
                        return;
                    }
                    juego.AsignarOrigenSeleccionado(territorio);
                    return;
                }

                if (jugador.verificarTerritorio(territorio))
                {
                    if (territorio.Tropas < 2)
                    {
                        MessageBox.Show("Ese territorio no tiene suficientes tropas (≥2).");
                        return;
                    }
                    juego.AsignarOrigenSeleccionado(territorio);
                    return;
                }

                if (!juego.AsignarDestinoAtaque(territorio))
                {
                    MessageBox.Show("El destino debe ser ENEMIGO y ADYACENTE al origen seleccionado.");
                    return;
                }

                var (resumen, conquistado) = juego.AtacarUnaVez();
                MessageBox.Show(resumen);

                if (juego.origenSeleccionado == null || juego.origenSeleccionado.Tropas < 2)
                    juego.cancelarTrasnferencia();

                actualizarInterfaz();
                return;
            }

            if (fase == 3)
            {
                juego.AsignarOrigenSeleccionado(territorio);
                return;
            }

            juego.deseleccionarTerritorios();
        }
        /*
        Esta función maneja el evento de click derecho en un territorio del mapa.
        Si el jugador local es el jugador actual y está en la fase 3, permite seleccionar el territorio clickeado como destino
        para una posible transferencia de tropas.
        */
        private void Territorio_ClickDerecho(object sender, MouseButtonEventArgs e)
        {
            if (!CondiciónParaJugar()) return;
            e.Handled = true; 
            Debug.WriteLine(">> RightClick territorio");

            var path = sender as System.Windows.Shapes.Path;
            if (path == null) return;
            var territorio = path.DataContext as Territorio;
            if (territorio == null) return;

            var jugador = juego.ObtenerJugadorActual();
            int fase = jugador.Fase;

            if (fase == 2)
            {
                if (juego.origenSeleccionado == null)
                {
                    MessageBox.Show("Primero elige el ORIGEN (click izquierdo sobre uno tuyo con ≥2 tropas).");
                    return;
                }

                if (!juego.AsignarDestinoAtaque(territorio))
                {
                    MessageBox.Show("El destino debe ser ENEMIGO y ADYACENTE al origen seleccionado.");
                    return;
                }

                var (resumen, conquistado) = juego.AtacarUnaVez();
                MessageBox.Show(resumen);

                if (juego.origenSeleccionado == null || juego.origenSeleccionado.Tropas < 2)
                    juego.cancelarTrasnferencia();

                actualizarInterfaz();
                return;
            }

            if (fase == 3)
            {
                if (territorio == juego.origenSeleccionado) return;
                juego.AsignarDestinoSeleccionado(territorio);
                return;
            }
        }

        private void Carta_ClickIzquierdo(object sender, MouseButtonEventArgs e)
        {
            if (!CondiciónParaJugar()) return;
<<<<<<< Updated upstream
            var borderCarta = sender as Border;
            if (borderCarta == null) return;
            Carta? cartaClickeada = borderCarta.DataContext as Carta;

            juego.seleccionarCartas(cartaClickeada!);

        }

        /*
         Esta función verifica si el jugador local es el jugador actual y si es su turno para jugar.
         Retorna true si el jugador local puede jugar, de lo contrario retorna false.
         */
        private bool CondiciónParaJugar()
        {
            return (jugadorLocal == juego.ObtenerJugadorActual().Nombre);
        }



        /* Esta función maneja el evento de click en el botón de avanzar fase.
         Verifica si el jugador local puede jugar y avanza a la siguiente fase si es posible.
         */
        private async void avanzarFase_Click(object sender, RoutedEventArgs e)
        {
            if (!CondiciónParaJugar()) return;
            juego.AvanzarFase();                      // cambia fase/turno local. :contentReference[oaicite:4]{index=4}
            actualizarInterfaz();
            if (conexion != null && conexion.EstaConectado)
                await conexion.Enviar("NEXT");        // espejo en el otro lado
        }

=======

            var borderCarta = sender as Border;
            if (borderCarta == null) return;

            var carta = borderCarta.DataContext as Carta;
            Debug.WriteLine("Carta clickeada");
            Debug.WriteLine(carta);
            if (carta != null) juego.seleccionarCartas(carta);
        }
>>>>>>> Stashed changes
        /* Esta función maneja el evento de click en el botón de intercambiar cartas.
         Verifica si el jugador local puede jugar y realiza el intercambio de cartas si es posible.
         */
        private void IntecambiarCartas_Click(object sender, RoutedEventArgs e)
        {
            if (!CondiciónParaJugar()) return;
<<<<<<< Updated upstream
            bool completado=juego.IntercambiarCartas();
=======

            bool completado = juego.IntercambiarCartas();
>>>>>>> Stashed changes
            if (!completado)
            {
                MessageBox.Show("Debes seleccionar 3 cartas para intercambiar");
            }
<<<<<<< Updated upstream
            else 
            {
                MessageBox.Show("Intercambio completado, has recibido tropas");
=======
            else
            {
                MessageBox.Show("Intercambio completado, has recibido tropas");
                actualizarInterfaz();
>>>>>>> Stashed changes
            }
        }
        /* Esta función maneja el evento de click en el botón de avanzar fase.
         Verifica si el jugador local puede jugar y avanza a la siguiente fase si es posible.
         */
        private void avanzarFase_Click(object sender, RoutedEventArgs e)
        {
            if (!CondiciónParaJugar()) return;

            juego.AvanzarFase();
            PanelEstado.DataContext = juego.ObtenerJugadorActual();
            CuadrosCartas.DataContext = juego.ObtenerJugadorActual();
            BtnTransferir.DataContext = juego.ObtenerJugadorActual();
            actualizarTextoGuia();
        }
        /*
         Esta función actualiza la interfaz de usuario para reflejar el estado actual del juego.
         Actualiza los datos del panel de estado, los cuadros de cartas y el botón de transferencia
         con la información del jugador actual, y también actualiza el texto de la guía.
         */
        private void actualizarInterfaz()
        {
            PanelEstado.DataContext = juego.ObtenerJugadorActual();
            CuadrosCartas.DataContext = juego.ObtenerJugadorActual();
            BtnTransferir.DataContext = juego.ObtenerJugadorActual();
            actualizarTextoGuia();
        }
<<<<<<< HEAD
=======


        /* Esta función maneja el evento de click en el botón de confirmar transferencia de tropas.
         Verifica si el jugador local puede jugar, obtiene la cantidad de tropas a transferir del slider,
         realiza la transferencia y cierra el popup de transferencia de tropas.
         */
        private void ConfirmarTransferencia_Click(object sender, RoutedEventArgs e)
        {
            if (!CondiciónParaJugar()) return;

            if (juego.origenSeleccionado.Tropas < 2) return;
            int cantidad = (int)CantidadSlider.Value;
            bool completado = juego.TransferenciaTropas(cantidad);
            if (completado) 
            {
<<<<<<< Updated upstream
                MessageBox.Show("Se han transferido "+cantidad+" tropas");
                juego.AvanzarFase();
                actualizarInterfaz();
=======
                if (juego.origenSeleccionado.Tropas < 2) return;
                int cantidad = (int)CantidadSlider.Value;
                juego.TransferenciaTropas(cantidad);
                TrasnefirTopasPopup.IsOpen = false;
>>>>>>> Stashed changes
            }
            else
            {
                MessageBox.Show("No se ha podido completar la transferencia, los territorios no estan conectados");
            }
            TrasnefirTopasPopup.IsOpen = false;
        }

        /* Esta función maneja el evento de click en el botón de cancelar transferencia de tropas.
         Verifica si el jugador local puede jugar, cancela la transferencia y cierra el popup de transferencia de tropas.
         */
        private void CancelarTransferencia_Click(object sender, RoutedEventArgs e)
        {
            if (!CondiciónParaJugar()) return;

            juego.cancelarTrasnferencia();
            TrasnefirTopasPopup.IsOpen = false;
        }


>>>>>>> d1e6eef3d76f700a370c683d988048761494e142
        /* Esta función permite solo se ejecuta con el boton de transferir que aparece cuando el jugador está en al fase 3
        * verifica que se hayan seleccionado dos territorios para hacer que aparezca un popup que permite trasnferir las tropas*/
        private void BtnTransferir_Click(object sender, RoutedEventArgs e)
        {
            if (!CondiciónParaJugar()) return;

            if (juego.origenSeleccionado == null || juego.destinoSeleccionado == null)
            {
                MessageBox.Show("Debes seleccionar un territorio de origen (izq click) y uno de destino (der click).");
                return;
            }
<<<<<<< Updated upstream
            if(!juego.verificarOrigenValido())
=======
            if (!juego.verificarOrigenValido())
>>>>>>> Stashed changes
            {
                MessageBox.Show("Debes seleccionar un territorio de origen que tenga al menos dos tropas");
                return;
            }

            TransferirTitulo.Text = $"Mover tropas de {juego.origenSeleccionado.Nombre} a {juego.destinoSeleccionado.Nombre}";
            CantidadSlider.Maximum = juego.origenSeleccionado.Tropas - 1;
            CantidadSlider.Value = 1;

            TrasnefirTopasPopup.IsOpen = true;
        }
        /* Esta función maneja el evento de click en el botón de confirmar transferencia de tropas.
         Verifica si el jugador local puede jugar, obtiene la cantidad de tropas a transferir del slider,
         realiza la transferencia y cierra el popup de transferencia de tropas.
         */
        private void ConfirmarTransferencia_Click(object sender, RoutedEventArgs e)
        {
            if (!CondiciónParaJugar()) return;

            int cantidad = (int)CantidadSlider.Value; 
            juego.TransferenciaTropas(cantidad);
            TrasnefirTopasPopup.IsOpen = false;
            actualizarInterfaz();
        }
        /* Esta función maneja el evento de click en el botón de cancelar transferencia de tropas.
         Verifica si el jugador local puede jugar, cancela la transferencia y cierra el popup de transferencia de tropas.
         */
        private void CancelarTransferencia_Click(object sender, RoutedEventArgs e)
        {
            if (!CondiciónParaJugar()) return;

            juego.cancelarTrasnferencia();
            TrasnefirTopasPopup.IsOpen = false;
            actualizarInterfaz();
        }


        private void actualizarTextoGuia()
        {
            TextoGuía.Text = "Turno de " + juego.ObtenerJugadorActual().Nombre;
            int fase = juego.ObtenerJugadorActual().Fase;

            if (fase == 1)
                TextoGuía.Text += ": Coloca tus tropas";
            else if (fase == 2)
                TextoGuía.Text += ": Ataca un territorio enemigo";
            else
                TextoGuía.Text += ": Reubica tus tropas";
        }

        private bool CondiciónParaJugar()
        {
 
            return true;
        }
    }
}
