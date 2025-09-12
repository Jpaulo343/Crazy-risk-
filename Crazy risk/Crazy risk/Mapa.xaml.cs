using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.IO;
using System.Text.Json;

namespace Crazy_risk
{
    /// <summary>
    /// Interaction logic for Mapa.xaml
    /// </summary>
    public partial class Mapa : Page
    {
        public Mapa()
        {
            InitializeComponent();
            cargarDatosTerritorios();

        }

        private void Territorio_ClickIzquierdo(object sender, MouseButtonEventArgs e)
        {
            
            var pathClickeado = sender as System.Windows.Shapes.Path;

            if (pathClickeado != null)
            {
                var territorioClickeado = pathClickeado.DataContext as Territorio;

                if (territorioClickeado != null)
                {
                    territorioClickeado.EstaSeleccionado = !territorioClickeado.EstaSeleccionado; //cuando se cree el objeto de Juego debe cambiarse
                }
            }
        
        }

        internal void cargarDatosTerritorios()
        {
            string json = File.ReadAllText("DatosTerritorios.json");
            var territoriosJson = JsonSerializer.Deserialize<List<TerritorioJson>>(json);

            ListaEnlazada<Territorio> territorios = new();

            foreach (var t in territoriosJson)
            {
                var listaAdyacentes = new ListaEnlazada<string>();
                foreach (var nombreAdj in t.Adyacentes)
                {
                    listaAdyacentes.añadir(nombreAdj);
                }

                Territorio territorio = new Territorio(t.Nombre, t.Estado, listaAdyacentes, t.Tropas, t.Continente);
                territorios.añadir(territorio);
            }

            foreach (var t in territorios.Enumerar()) 
            {
                System.Windows.Shapes.Path pathObjeto = this.FindName(t.Nombre) as System.Windows.Shapes.Path;
                if (pathObjeto != null)
                {
                    pathObjeto.DataContext = t;

                }
            }
        }
    }
}
