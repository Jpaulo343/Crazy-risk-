using System;
using System.Collections.Generic;
using System.IO;
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
            Iniciar_Juego();

            //Esto lo cambierá despues para que se detecte automaticamente, está así por ahora, por pruebasW
            NombreJugadorText1.Text = juego.listaJugadores.ObtenerEnIndice(0).Nombre;
            NombreJugadorText2.Text = juego.listaJugadores.ObtenerEnIndice(1).Nombre;
            NombreJugadorText3.Text = juego.listaJugadores.ObtenerEnIndice(2).Nombre;
            ColorJugador1.Fill = juego.listaJugadores.ObtenerEnIndice(0).Color;
            ColorJugador2.Fill = juego.listaJugadores.ObtenerEnIndice(1).Color;
            ColorJugador3.Fill = juego.listaJugadores.ObtenerEnIndice(2).Color;
            PanelEstado.DataContext = juego.ObtenerJugadorActual();


            //cambia la fase para comprobar que la interfaz funciona
            Task.Delay(3000).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() => juego.ObtenerJugadorActual().Fase = 2);
            });

        }


        /*
         Esta función crea la lista de jugadores para posteriormente crear el objeto de "juego",
         en el cual se ejecutará toda la lógica del juego
         */
        private void Iniciar_Juego()
        {
            ListaEnlazada<Brush> colores = new ListaEnlazada<Brush>();

            Brush orangeBrush = Brushes.LightSalmon;
            Brush greenBrush = Brushes.LightGreen;

            Brush lightBlueBrush = Brushes.MediumPurple;


            colores.Añadir(orangeBrush);
            colores.Añadir(greenBrush);
            colores.Añadir(lightBlueBrush);

            string nombre1 = "jugador1";
            string nombre2 = "jugador2";
            string nombre3 = "jugador neutral";


            DiccionarioColor_Nombre.regisrar(nombre3, colores.Seleccionar_Y_Eliminar_Random());
            DiccionarioColor_Nombre.regisrar(nombre2, colores.Seleccionar_Y_Eliminar_Random());
            DiccionarioColor_Nombre.regisrar(nombre1, colores.Seleccionar_Y_Eliminar_Random());

            ListaTerritorios territorios = cargarDatosTerritorios();
            Random generadorAleatorio = new Random();


            ListaEnlazada<string> territorios1 = new ListaEnlazada<string>();
            Jugador jugador1 = new Jugador(nombre1,DiccionarioColor_Nombre.obtener(nombre1) ,1, territorios1, 40);



            ListaEnlazada<string> territorios2 = new ListaEnlazada<string>();
            Jugador jugador2 = new Jugador(nombre2, DiccionarioColor_Nombre.obtener(nombre2), 1, territorios2, 40);
            

            ListaEnlazada<string> territorios3 = new ListaEnlazada<string>();
            Jugador jugador3 = new Jugador(nombre3, DiccionarioColor_Nombre.obtener(nombre3), 1, territorios3, 40);

            ListaEnlazada<Jugador> jugadores = new ListaEnlazada<Jugador>();

            jugadores.Añadir(jugador3);
            jugadores.Añadir(jugador2);
            jugadores.Añadir(jugador1);
            RepartirTerritorios(territorios,jugadores, generadorAleatorio);

            juego = new Juego(territorios, jugadores, generadorAleatorio);
        }


        /*
        Esta función se encarga de tomar todos los territorios y asignarles un dueño de manera aleatoria
        */
        public static void RepartirTerritorios( ListaTerritorios territorios,ListaEnlazada<Jugador> jugadores, Random generadorNumeros)
        {
            int indiceJugador = 0;
            ListaTerritorios territoriosDisponibles = territorios.CopiarLista();
            while (territorios.size > 0)
            {

                Territorio territorio = territorios.Seleccionar_Y_Eliminar_Random();

                Jugador jugador = jugadores.ObtenerEnIndice(indiceJugador);

                jugador.AsignarTerritorio(territorio);

                indiceJugador = (indiceJugador + 1) % jugadores.size;
            }
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

            foreach (var t in territorios.Enumerar()) 
            {
                System.Windows.Shapes.Path pathObjeto = this.FindName(t.Nombre) as System.Windows.Shapes.Path;
                if (pathObjeto != null)
                {
                    pathObjeto.DataContext = t;

                }
            }
            return territorios;
        }

    }
}
