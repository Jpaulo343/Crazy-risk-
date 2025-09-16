using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media;

namespace Crazy_risk
{
    public class Jugador
    {
        public string Nombre { get; private set; }
        public Brush Color { get; private set; }

        public int Fase { get; private set; }
        public ListaEnlazada<string> territorios_Conquistados { get; set; }

        // añadir atributo de cartas, debe ser una lista de objetos de carta

        public int tropasDisponibles {  get;  private set; }
        public Jugador(string nombre, Brush color, int fase, ListaEnlazada<string> territorios_Conquistados, int tropasDisponibles) {
            this.Nombre = nombre;
            this.Color = color;
            this.Fase = fase;
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
    }



    public static class PlayerColorProvider
    {
        static Dictionary<string, Brush> map = new();

        public static void Register(string playerName, Brush brush)
        {
            if (playerName == null) return;
            map[playerName] = brush;
        }

        public static Brush? GetBrush(string playerName)
        {
            if (playerName == null) return null;
            if (map.TryGetValue(playerName, out var b)) return b;
            return null;
        }
    }


}
