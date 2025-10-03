using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// EstadoJuego.cs
namespace Crazy_risk
{
    public class EstadoJuegoDTO
    {
        public string TurnoActual { get; set; }
        public bool RondaInicial { get; set; }
        public JugadorDTO[] Jugadores { get; set; }
        public TerritorioDTO[] Territorios { get; set; }
    }

    public class JugadorDTO
    {
        public string Nombre { get; set; }
        public int TropasDisponibles { get; set; }
        public int Fase { get; set; }
    }

    public class TerritorioDTO
    {
        public string Nombre { get; set; }
        public string Conquistador { get; set; }
        public int Tropas { get; set; }
    }
}
