using System;
using System.Collections.Generic;
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
        private int fase;
        public int TropasDisponibles
        {
            get => tropasDisponibles;
            set { tropasDisponibles = value; OnPropertyChanged(); }
        }

        public int Fase
        {
            get => fase;
            set { fase = value; OnPropertyChanged(); }
        }

        public ListaEnlazada<string> territorios_Conquistados { get; set; }

        // añadir atributo de cartas, debe ser una lista de objetos de carta


        public Jugador(string nombre, Brush color, int fase, ListaEnlazada<string> territorios_Conquistados, int tropasDisponibles) {
            this.Nombre = nombre;
            this.Color = color;
            this.fase = fase;
            this.territorios_Conquistados = territorios_Conquistados;
            this.tropasDisponibles = tropasDisponibles;
        }

        public void ConquistarTerritorio(string territorio)
        {
            territorios_Conquistados.Añadir(territorio);
        }

        public void PerderTerritorio(string territorio)
        {
            territorios_Conquistados.Eliminar(territorio);
        }

        public bool ComprobarTerritorio(string territorio)
        {
            return this.territorios_Conquistados.Buscar(territorio);
        }

        public void AsignarTerritorio(Territorio t)
        {
            territorios_Conquistados.Añadir(t.Nombre);
            t.Conquistador = Nombre; 
            t.Tropas = 1; 
        }
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
