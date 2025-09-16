using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Crazy_risk
{

    /*
     Se define la calse DiccionarioColor_Nombre
        Esta clase permite asignar un color a cada jugador de tal forma que la interfaz de WFP pueda encontrar el color que debe 
        tener cada territorio a partir del nombre del dueño 
     */
    public static class DiccionarioColor_Nombre
    {
        private static Dictionary<string, Brush> diccionario = new Dictionary<string, Brush>();

        public static void regisrar(string jugador, Brush brush)
        {
            if (string.IsNullOrEmpty(jugador) || brush == null) { return; }
            diccionario[jugador] = brush;
        }

        public static Brush obtener(string jugador) 
        {
            if (string.IsNullOrEmpty(jugador)) 
            {
             return Brushes.LightGray;
            }

            if (diccionario.TryGetValue(jugador, out Brush brush))
            {
                return brush;
            }

            return Brushes.LightGray;

        }
    }
}
