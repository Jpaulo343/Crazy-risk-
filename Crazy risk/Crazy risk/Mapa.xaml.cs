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
        public Mapa()
        {
            InitializeComponent();

            juego = new Juego("Jugador1", "Jugador2");

            //Esto lo cambierá despues para que se detecte automaticamente, está así por ahora, por pruebas
            NombreJugadorText1.Text = juego.listaJugadores.ObtenerEnIndice(0).Nombre;
            NombreJugadorText2.Text = juego.listaJugadores.ObtenerEnIndice(1).Nombre;
            NombreJugadorText3.Text = juego.listaJugadores.ObtenerEnIndice(2).Nombre;
            ColorJugador1.Fill = juego.listaJugadores.ObtenerEnIndice(0).Color;
            ColorJugador2.Fill = juego.listaJugadores.ObtenerEnIndice(1).Color;
            ColorJugador3.Fill = juego.listaJugadores.ObtenerEnIndice(2).Color;
            PanelEstado.DataContext = juego.ObtenerJugadorActual();
            foreach (var t in juego.listaTerritorios.Enumerar())
            {
                System.Windows.Shapes.Path pathObjeto = this.FindName(t.Nombre) as System.Windows.Shapes.Path;
                if (pathObjeto != null)
                {
                    pathObjeto.DataContext = t;
                    Debug.WriteLine($"Asignado {t.Nombre} al Path {pathObjeto.Name}");
                }
            }

            //cambia la fase para comprobar que la interfaz funciona
            Task.Delay(3000).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() => juego.ObtenerJugadorActual().Fase = 2);
            });

        }


        private void Territorio_ClickIzquierdo(object sender, MouseButtonEventArgs e)
        {
            
            var pathClickeado = sender as System.Windows.Shapes.Path;

            if (pathClickeado != null)
            {
                var territorioClickeado = pathClickeado.DataContext as Territorio;

                if (territorioClickeado != null)
                {
                    territorioClickeado.EstaSeleccionado = !territorioClickeado.EstaSeleccionado; 
                }
            }
        
        }

    }
}
