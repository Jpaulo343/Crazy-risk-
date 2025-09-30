using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics;

namespace Crazy_risk
{
    /// <summary>
    /// Interaction logic for Mapa.xaml
    /// </summary>
    public partial class Mapa : Page
    {
        public Juego juego { get; private set; }
        public string jugadorLocal { get; private set; }
        public Mapa(string Nombre1, string Nombre2, string local)
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
            BtnTransferir.DataContext = juego.ObtenerJugadorActual();
            foreach (var t in juego.listaTerritorios.Enumerar())
            {
                System.Windows.Shapes.Path pathObjeto = this.FindName(t.Nombre) as System.Windows.Shapes.Path;
                if (pathObjeto != null)
                {
                    pathObjeto.DataContext = t;
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



        /*
         Esta funcion maneja el evento de click izquierdo en un territorio del mapa.
         Si el jugador local es el jugador actual, permite seleccionar o deseleccionar el territorio clickeado
         y, si no es la ronda inicial, intenta añadir tropas al territorio seleccionado.
         */
        private void Territorio_ClickIzquierdo(object sender, MouseButtonEventArgs e)
        {
            if (CondiciónParaJugar())
            {
                var pathClickeado = sender as System.Windows.Shapes.Path;
                if (pathClickeado == null) return;

                Territorio? territorioClickeado = pathClickeado.DataContext as Territorio;
                bool jugadorCambiado = false;
                int faseActual = juego.ObtenerJugadorActual().Fase;
                if (juego.rondaInicial || faseActual == 1)
                {
                    jugadorCambiado = juego.AñadirTropas(territorioClickeado!);
                }

                if (faseActual == 3)
                {
                    juego.AsignarOrigenSeleccionado(territorioClickeado);
                    return;
                }
                else juego.deseleccionarTerritorios();
                
                
                juego.selecionarTerritorio(territorioClickeado!);
                if (jugadorCambiado)
                {
                    actualizarInterfaz();
                }
            }
        }

        /*
         Esta función maneja el evento de click derecho en un territorio del mapa.
         Si el jugador local es el jugador actual y está en la fase 3, permite seleccionar el territorio clickeado como destino
         para una posible transferencia de tropas.
         */
        private void Territorio_ClickDerecho(object sender, MouseButtonEventArgs e)
        {
            if (CondiciónParaJugar())
            {

                if (juego.ObtenerJugadorActual().Fase == 3)
                {
                    var pathClickeado = sender as System.Windows.Shapes.Path;
                    if (pathClickeado == null) return;
                    Territorio? territorioClickeado = pathClickeado.DataContext as Territorio;
                    if (territorioClickeado == juego.origenSeleccionado) return;
                    juego.AsignarDestinoSeleccionado(territorioClickeado!);
                    return;
                }

            }
        }

        /*
         Esta función maneja el evento de click izquierdo en una carta.
         Si el jugador local es el jugador actual, permite seleccionar la carta clickeada para posibles acciones posteriores.
         */
        private void Carta_ClickIzquierdo(object sender, MouseButtonEventArgs e)
        {
            if (CondiciónParaJugar())
            {
                var borderCarta = sender as Border;
                if (borderCarta == null) return;
                Carta? cartaClickeada = borderCarta.DataContext as Carta;

                juego.seleccionarCartas(cartaClickeada!);
            }
        }

        /*
         Esta función verifica si el jugador local es el jugador actual y si es su turno para jugar.
         Retorna true si el jugador local puede jugar, de lo contrario retorna false.
         */
        private bool CondiciónParaJugar()
        {

            return true;
            //return (jugadorLocal == juego.ObtenerJugadorActual().Nombre);
        }

        /* Esta función maneja el evento de click en el botón de avanzar fase.
         Verifica si el jugador local puede jugar y avanza a la siguiente fase si es posible.
         */
        private void avanzarFase_Click(object sender, RoutedEventArgs e)
        {
            if (CondiciónParaJugar())
            {
                juego.AvanzarFase();
                actualizarInterfaz();
            }
        }

        /* Esta función maneja el evento de click en el botón de intercambiar cartas.
         Verifica si el jugador local puede jugar y realiza el intercambio de cartas si es posible.
         */
        private void IntecambiarCartas_Click(object sender, RoutedEventArgs e)
        {
            if (CondiciónParaJugar())
            {
                bool completado=juego.IntercambiarCartas();
                if (!completado)
                {
                    MessageBox.Show("Debes seleccionar 3 cartas para intercambiar");
                }
                else 
                {
                    MessageBox.Show("Intercambio completado, has recibido tropas");
                }
            }
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
            BtnTransferir.DataContext= juego.ObtenerJugadorActual();
            actualizarTextoGuia();

        }


        /* Esta función maneja el evento de click en el botón de confirmar transferencia de tropas.
         Verifica si el jugador local puede jugar, obtiene la cantidad de tropas a transferir del slider,
         realiza la transferencia y cierra el popup de transferencia de tropas.
         */
        private void ConfirmarTransferencia_Click(object sender, RoutedEventArgs e)
        {
            if (CondiciónParaJugar())
            {
                int cantidad = (int)CantidadSlider.Value;
                juego.TransferenciaTropas(cantidad);
                TrasnefirTopasPopup.IsOpen = false;
            }
        }

        /* Esta función maneja el evento de click en el botón de cancelar transferencia de tropas.
         Verifica si el jugador local puede jugar, cancela la transferencia y cierra el popup de transferencia de tropas.
         */
        private void CancelarTransferencia_Click(object sender, RoutedEventArgs e)
        {
            if (CondiciónParaJugar())
            {
                juego.cancelarTrasnferencia();

                TrasnefirTopasPopup.IsOpen = false;
            }
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
            if(juego.verificarOrigenValido())
            {
                MessageBox.Show("Debes seleccionar un territorio de origen que tenga almenos dos tropas");
                return;
            }
            TransferirTitulo.Text = $"Mover tropas de {juego.origenSeleccionado.Nombre} a {juego.destinoSeleccionado.Nombre}";
            CantidadSlider.Maximum = juego.origenSeleccionado.Tropas - 1;
            CantidadSlider.Value = 1;

            TrasnefirTopasPopup.IsOpen = true;
        }


        /*
          Actualiza el texto de la guía para reflejar el turno y la fase del jugador actual.
         */
        private void actualizarTextoGuia()
        {
            TextoGuía.Text = "Turno de " + juego.ObtenerJugadorActual().Nombre;
            if (juego.ObtenerJugadorActual().Fase == 1)
            {
                TextoGuía.Text += ": Coloca tus tropas";
            }
            else if (juego.ObtenerJugadorActual().Fase == 2)
            {
                TextoGuía.Text += ": Ataca un territorio enemigo";
            }
            else
            {
                TextoGuía.Text += ": Reubica tus tropas";
            }
        }

    }
}
