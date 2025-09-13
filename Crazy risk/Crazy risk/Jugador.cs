using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Crazy_risk
{
    public class Jugador
    {
        public string nombre { get; private set; }
        public string color { get; private set; }

        public int fase { get; private set; }
        public ListaEnlazada<string> territorios_Conquistados { get; set; }

        // añadir atributo de cartas, debe ser una lista de objetos de carta

        public int tropasDisponibles {  get;  private set; }
        public Jugador(string nombre, string color, int fase, ListaEnlazada<string> territorios_Conquistados, int tropasDisponibles) {
            this.nombre = nombre;
            this.color = color;
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

    }
}
