using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crazy_risk
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Territorio : INotifyPropertyChanged
    {
        public string Nombre { get; private set; }
        public int Continente { get; private set; }
        public string Estado { get; set; }
        public int Tropas { get; set; }
        public ListaEnlazada<string> Adyacentes { get; set; }

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
            Estado = estado;
            Adyacentes = adyacentes;
            Tropas = tropas;
            Continente = continente;
            estaSeleccionado = false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName = null)
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
