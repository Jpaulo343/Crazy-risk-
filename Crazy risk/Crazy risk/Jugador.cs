using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media;

namespace Crazy_risk
{
    public class Jugador : INotifyPropertyChanged
    {

        
        public string Nombre { get; private set; }
        public Brush Color { get; private set; }


        private int tropasDisponibles;
        public int TropasDisponibles
        {
            get => tropasDisponibles;
            set { tropasDisponibles = value; OnPropertyChanged(); }
        }
        private int fase;
        public int Fase
        {
            get => fase;
            set { fase = value; OnPropertyChanged(); }
        }

        private ObservableCollection<Carta> cartas = new();
        public ObservableCollection<Carta> Cartas
        {
            get => cartas;
            private set
            {
                cartas = value;
                OnPropertyChanged(nameof(Cartas));
            }
        }

        //Se añaden o quitan cartas de la lista de cartas del jugador
        public void AgregarCarta(Carta carta) => Cartas.Add(carta);
        public void QuitarCarta(Carta carta) => Cartas.Remove(carta);

        public ListaEnlazada<string> territorios_Conquistados { get; set; }

        // añadir atributo de cartas, debe ser una lista de objetos de carta


        public Jugador(string nombre, Brush color, int fase, ListaEnlazada<string> territorios_Conquistados, int tropasDisponibles) {
            this.Nombre = nombre;
            this.Color = color;
            this.fase = fase;
            this.territorios_Conquistados = territorios_Conquistados;
            this.tropasDisponibles = tropasDisponibles;
        }

        // Se elimina el territorio especificado de la lista de territorios conquistados por el jugador
        public void PerderTerritorio(string territorio)
        {
            territorios_Conquistados.Eliminar(territorio);
        }

        // Comprueba si el jugador posee el territorio especificado
        public bool ComprobarTerritorio(string territorio)
        {
            return this.territorios_Conquistados.Buscar(territorio);
        }

        // Asigna un territorio al jugador, actualizando la lista de territorios conquistados y el estado del territorio
        public void AsignarTerritorio(Territorio t)
        {
            territorios_Conquistados.Añadir(t.Nombre);
            t.Conquistador = Nombre; 
            t.Tropas = 1; 
        }

        // Añade la cantidad especificada de tropas disponibles al jugador
        public void AgregarTropas(int cantidad)
        {
            tropasDisponibles += cantidad;
        }

        //Funciones de notificación a la interfaz
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


}
