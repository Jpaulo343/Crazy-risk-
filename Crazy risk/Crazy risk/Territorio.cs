using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crazy_risk
{
    internal class Territorio
    {

        internal string Nombre { get;  set; }
        internal int Continente { get; private set; }
        /*            
            0: norteamerica
            1: sudamerica
            2: europa
            3: africa
            4: asia
            5: oceanía
        */

        internal string Estado { get; set; } // Puede ser conquistado por "x" jugdor o libre
        internal int Tropas { get; set; }
        internal ListaEnlazada<String> Adyacentes { get; set; }
        public Territorio(string nombre, string estado, ListaEnlazada<String> adyacentes,int tropas, int continente)
        {
            Nombre=nombre;
            Estado=estado;
            Adyacentes=adyacentes;
            Tropas=Tropas;
            Continente = continente;
        }


    }
}
