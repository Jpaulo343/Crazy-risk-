using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crazy_risk
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Media;

    public class Territorio : INotifyPropertyChanged
    {
        public string Nombre { get; private set; }
        public int Continente { get; private set; }
        
        private string conquistador;
        private Brush _color;


        public string Conquistador
        {
            get => conquistador;
            set
            {
                if (conquistador != value)
                {
                    conquistador = value;
                    color=DiccionarioColor_Nombre.obtener(Conquistador);
                    OnPropertyChanged();
                }
            }
        }

        public Brush color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;
                    OnPropertyChanged(nameof(_color));
                }
            }
        }

        private int tropas;
        public int Tropas
        {
            get => tropas;
            set
            {
                if (value >= 0)
                {
                    tropas = value;
                    OnPropertyChanged(nameof(Tropas));
                }
            }
        }

        // Método para sumar tropas
        public void AgregarTropas(int cantidad)
        {
            if (cantidad > 0)
            {
                Tropas = Tropas + cantidad;
            }
        }

        // Método para restar tropas
        public void QuitarTropas(int cantidad)
        {
            if (cantidad > 0 && Tropas >= cantidad)
            {
                Tropas = Tropas - cantidad;
            }
        }
        public ListaEnlazada<string> Adyacentes { get; private set; }

        private bool estaSeleccionado;
        public bool EstaSeleccionado
        {
            get {  return estaSeleccionado; }
            set
            {
                if (estaSeleccionado != value)
                {
                    estaSeleccionado = value;
                    OnPropertyChanged();
                }
            }
        }

        public Territorio(string nombre, string estado, ListaEnlazada<string> adyacentes, int tropas, int continente)
        {
            Nombre = nombre;
            Conquistador = estado;
            Adyacentes = adyacentes;
            Tropas = tropas;
            Continente = continente;
            estaSeleccionado = false;
            color = DiccionarioColor_Nombre.obtener(Conquistador);
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }


    /*
     * Esta es una clase intermedia
      La funcion de esta clase es simplemente recibir los datos de "DatosTerritorios.json" 
      pues al pasar los datos de string a objeto el programa no puede pasar directamente los datos a un objeto
      de "ListaEnlazada"
        
    */
    public class TerritorioJson
    {
        public string Nombre { get; set; }
        public string Estado { get; set; }
        public int Tropas { get; set; }
        public int Continente { get; set; }
        public List<string> Adyacentes { get; set; }   
    }
}
