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
        public Mapa(string Nombre1,string Nombre2,string local)
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
            juego.ObtenerJugadorActual().AgregarCarta(new Carta(TipoCarta.Artilleria, "brasil"));
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
                juego.deseleccionarTerritorios();
                var pathClickeado = sender as System.Windows.Shapes.Path;
                if (pathClickeado == null) return;

                Territorio? territorioClickeado = pathClickeado.DataContext as Territorio;
                bool jugadorCambiado=false;
                if (juego.rondaInicial)
                {
                    jugadorCambiado = juego.AñadirTropas(territorioClickeado!);
                }
                juego.selecionarTerritorio(territorioClickeado!);
                if (jugadorCambiado) 
                {
                    actualizarInterfaz();
                }
            }
        
        }

        private bool CondiciónParaJugar()
        {
            return (jugadorLocal == juego.ObtenerJugadorActual().Nombre);
        }
        private void avanzarFase_Click(object sender, RoutedEventArgs e)
        {
            if (CondiciónParaJugar())
            {
                juego.AvanzarFase();
                actualizarInterfaz();
            }
        }

        private void actualizarInterfaz()
        {
            PanelEstado.DataContext = juego.ObtenerJugadorActual();
            CuadrosCartas.DataContext = juego.ObtenerJugadorActual();
            actualizarTextoGuia();

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
