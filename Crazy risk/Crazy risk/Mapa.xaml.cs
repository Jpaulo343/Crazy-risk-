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
        }


        /*
         Esta función crea la lista de jugadores para posteriormente crear el objeto de "juego",
         en el cual se ejecutará toda la lógica del juego
         */
        private void Iniciar_Juego()
        {
            ListaEnlazada<Brush> colores = new ListaEnlazada<Brush>();

            Brush orangeBrush = Brushes.Orange;
            Brush greenBrush = Brushes.Green;

            Brush lightBlueBrush = Brushes.LightBlue;


            colores.Añadir(orangeBrush);
            colores.Añadir(greenBrush);
            colores.Añadir(lightBlueBrush);

            string nombre1 = "jugador1";
            string nombre2 = "jugador2";
            string nombre3 = "jugador neutral";


            DiccionarioColor_Nombre.regisrar(nombre1, colores.Seleccionar_Y_Eliminar_Random());
            DiccionarioColor_Nombre.regisrar(nombre2, colores.Seleccionar_Y_Eliminar_Random());
            DiccionarioColor_Nombre.regisrar(nombre3, colores.Seleccionar_Y_Eliminar_Random());

            ListaTerritorios territorios = cargarDatosTerritorios();
            Random generadorAleatorio = new Random();


            ListaEnlazada<string> territorios1 = new ListaEnlazada<string>();
            Jugador jugador1 = new Jugador(nombre1,DiccionarioColor_Nombre.obtener(nombre1) ,0, territorios1, 40);



            ListaEnlazada<string> territorios2 = new ListaEnlazada<string>();
            Jugador jugador2 = new Jugador(nombre2, DiccionarioColor_Nombre.obtener(nombre2), 0, territorios2, 40);
            

            ListaEnlazada<string> territorios3 = new ListaEnlazada<string>();
            Jugador jugador3 = new Jugador(nombre3, DiccionarioColor_Nombre.obtener(nombre2), 0, territorios3, 40);

            ListaEnlazada<Jugador> jugadores = new ListaEnlazada<Jugador>();
            jugadores.Añadir(jugador1);
            jugadores.Añadir(jugador2);
            jugadores.Añadir(jugador3);
            RepartirTerritorios(territorios,jugadores, generadorAleatorio);

            Console.WriteLine(territorios);

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
