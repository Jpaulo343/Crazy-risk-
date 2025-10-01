using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
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
<<<<<<< Updated upstream
<<<<<<< Updated upstream

        public Mapa(string Nombre1, string Nombre2, string local)
=======
<<<<<<< Updated upstream
=======
>>>>>>> Stashed changes
<<<<<<< Updated upstream
=======
>>>>>>> Stashed changes
<<<<<<< Updated upstream
        public Mapa(string Nombre1,string Nombre2,string local)
=======
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes

        public Mapa(string Nombre1, string Nombre2, string local)
>>>>>>> Stashed changes
<<<<<<< Updated upstream
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
        {
            InitializeComponent();
            jugadorLocal = local;
            juego = new Juego(Nombre1, Nombre2);

            NombreJugadorText1.Text = juego.listaJugadores.ObtenerEnIndice(0).Nombre;
            NombreJugadorText2.Text = juego.listaJugadores.ObtenerEnIndice(1).Nombre;
            NombreJugadorText3.Text = juego.listaJugadores.ObtenerEnIndice(2).Nombre;
            ColorJugador1.Fill = juego.listaJugadores.ObtenerEnIndice(0).Color;
            ColorJugador2.Fill = juego.listaJugadores.ObtenerEnIndice(1).Color;
            ColorJugador3.Fill = juego.listaJugadores.ObtenerEnIndice(2).Color;

            PanelEstado.DataContext = juego.ObtenerJugadorActual();
            CuadrosCartas.DataContext = juego.ObtenerJugadorActual();
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
            BtnTransferir.DataContext = juego.ObtenerJugadorActual();

=======
<<<<<<< Updated upstream
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
            BtnTransferir.DataContext = juego.ObtenerJugadorActual();

<<<<<<< Updated upstream
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
<<<<<<< Updated upstream
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
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

            juego.ObtenerJugadorActual().AgregarCarta(new Carta(TipoCarta.Infanteria, "alaasdsadasdasska"));
            juego.ObtenerJugadorActual().AgregarCarta(new Carta(TipoCarta.Caballeria, "japon"));
            juego.ObtenerJugadorActual().AgregarCarta(new Carta(TipoCarta.Caballeria, "camerun"));
            juego.ObtenerJugadorActual().AgregarCarta(new Carta(TipoCarta.Artilleria, "westUs"));
            juego.ObtenerJugadorActual().AgregarCarta(new Carta(TipoCarta.Caballeria, "brasil"));
            juego.ObtenerJugadorActual().AgregarCarta(new Carta(TipoCarta.Caballeria, "peru"));

            actualizarTextoGuia();
        }
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
=======
<<<<<<< Updated upstream
=======
>>>>>>> Stashed changes
<<<<<<< Updated upstream
=======
>>>>>>> Stashed changes
<<<<<<< Updated upstream



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

            var borderCarta = sender as Border;
            if (borderCarta == null) return;

            var carta = borderCarta.DataContext as Carta;
            Debug.WriteLine("Carta clickeada");
            Debug.WriteLine(carta);
            if (carta != null) juego.seleccionarCartas(carta);
        }
        /* Esta función maneja el evento de click en el botón de intercambiar cartas.
         Verifica si el jugador local puede jugar y realiza el intercambio de cartas si es posible.
         */
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
                actualizarInterfaz();
            }
        }
        /* Esta función maneja el evento de click en el botón de avanzar fase.
         Verifica si el jugador local puede jugar y avanza a la siguiente fase si es posible.
         */
        private void avanzarFase_Click(object sender, RoutedEventArgs e)
        {
            if (!CondiciónParaJugar()) return;

<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
=======
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
=======
>>>>>>> Stashed changes
=======
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

            var borderCarta = sender as Border;
            if (borderCarta == null) return;

            var carta = borderCarta.DataContext as Carta;
            Debug.WriteLine("Carta clickeada");
            Debug.WriteLine(carta);
            if (carta != null) juego.seleccionarCartas(carta);
        }
        /* Esta función maneja el evento de click en el botón de intercambiar cartas.
         Verifica si el jugador local puede jugar y realiza el intercambio de cartas si es posible.
         */
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
                actualizarInterfaz();
            }
        }
        /* Esta función maneja el evento de click en el botón de avanzar fase.
         Verifica si el jugador local puede jugar y avanza a la siguiente fase si es posible.
         */
        private void avanzarFase_Click(object sender, RoutedEventArgs e)
        {
            if (!CondiciónParaJugar()) return;

<<<<<<< Updated upstream
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
>>>>>>> Stashed changes
        private void actualizarInterfaz()
        {
            PanelEstado.DataContext = juego.ObtenerJugadorActual();
            CuadrosCartas.DataContext = juego.ObtenerJugadorActual();
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
            BtnTransferir.DataContext = juego.ObtenerJugadorActual();
            actualizarTextoGuia();
        }
        /* Esta función permite solo se ejecuta con el boton de transferir que aparece cuando el jugador está en al fase 3
        * verifica que se hayan seleccionado dos territorios para hacer que aparezca un popup que permite trasnferir las tropas*/
        private void BtnTransferir_Click(object sender, RoutedEventArgs e)
        {
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
=======
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
=======
            BtnTransferir.DataContext = juego.ObtenerJugadorActual();
>>>>>>> Stashed changes
=======
            BtnTransferir.DataContext = juego.ObtenerJugadorActual();
>>>>>>> Stashed changes
=======
            BtnTransferir.DataContext = juego.ObtenerJugadorActual();
>>>>>>> Stashed changes
            actualizarTextoGuia();
        }
<<<<<<< HEAD
=======

=======
<<<<<<< Updated upstream
<<<<<<< Updated upstream
=======
            BtnTransferir.DataContext = juego.ObtenerJugadorActual();
>>>>>>> Stashed changes
=======
            BtnTransferir.DataContext = juego.ObtenerJugadorActual();
>>>>>>> Stashed changes
=======
            BtnTransferir.DataContext = juego.ObtenerJugadorActual();
>>>>>>> Stashed changes
            actualizarTextoGuia();
        }
<<<<<<< HEAD
=======

>>>>>>> Stashed changes
=======
<<<<<<< Updated upstream
=======
            BtnTransferir.DataContext = juego.ObtenerJugadorActual();
>>>>>>> Stashed changes
=======
            BtnTransferir.DataContext = juego.ObtenerJugadorActual();
>>>>>>> Stashed changes
=======
            BtnTransferir.DataContext = juego.ObtenerJugadorActual();
>>>>>>> Stashed changes
            actualizarTextoGuia();
        }
<<<<<<< HEAD
=======

>>>>>>> Stashed changes

<<<<<<< Updated upstream
        /*
          Actualiza el texto de la guía para reflejar el turno y la fase del jugador actual.
=======
<<<<<<< Updated upstream
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
        /* Esta función maneja el evento de click en el botón de confirmar transferencia de tropas.
         Verifica si el jugador local puede jugar, obtiene la cantidad de tropas a transferir del slider,
         realiza la transferencia y cierra el popup de transferencia de tropas.
         */
        private void ConfirmarTransferencia_Click(object sender, RoutedEventArgs e)
        {
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
            if (!CondiciónParaJugar()) return;

            int cantidad = (int)CantidadSlider.Value; 
            juego.TransferenciaTropas(cantidad);
            TrasnefirTopasPopup.IsOpen = false;
            actualizarInterfaz();
        }
=======
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
            if (CondiciónParaJugar())
            {
                if (juego.origenSeleccionado.Tropas < 2) return;
                int cantidad = (int)CantidadSlider.Value;
                juego.TransferenciaTropas(cantidad);
                TrasnefirTopasPopup.IsOpen = false;
            }
        }

<<<<<<< Updated upstream
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
        /* Esta función maneja el evento de click en el botón de cancelar transferencia de tropas.
         Verifica si el jugador local puede jugar, cancela la transferencia y cierra el popup de transferencia de tropas.
         */
        private void CancelarTransferencia_Click(object sender, RoutedEventArgs e)
        {
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
            if (!CondiciónParaJugar()) return;

            juego.cancelarTrasnferencia();
            TrasnefirTopasPopup.IsOpen = false;
            actualizarInterfaz();
        }


=======
            if (CondiciónParaJugar())
            {
                juego.cancelarTrasnferencia();

                TrasnefirTopasPopup.IsOpen = false;
            }
        }


=======
            if (CondiciónParaJugar())
            {
                juego.cancelarTrasnferencia();

                TrasnefirTopasPopup.IsOpen = false;
            }
        }


>>>>>>> Stashed changes
=======
            if (CondiciónParaJugar())
            {
                juego.cancelarTrasnferencia();

                TrasnefirTopasPopup.IsOpen = false;
            }
        }


>>>>>>> Stashed changes
>>>>>>> d1e6eef3d76f700a370c683d988048761494e142
        /* Esta función permite solo se ejecuta con el boton de transferir que aparece cuando el jugador está en al fase 3
        * verifica que se hayan seleccionado dos territorios para hacer que aparezca un popup que permite trasnferir las tropas*/
        private void BtnTransferir_Click(object sender, RoutedEventArgs e)
        {
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
        /* Esta función maneja el evento de click en el botón de confirmar transferencia de tropas.
         Verifica si el jugador local puede jugar, obtiene la cantidad de tropas a transferir del slider,
         realiza la transferencia y cierra el popup de transferencia de tropas.
<<<<<<< Updated upstream
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
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


<<<<<<< Updated upstream
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
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
