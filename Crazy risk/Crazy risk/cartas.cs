using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum TipoCarta
{
    Infanteria,
    Caballeria,
    Artilleria
}

public class Carta
{
    private TipoCarta _tipo;
    private string _territorioAsociado;
    private bool _usada;
    private readonly string _rutaImagen;
    public TipoCarta Tipo => _tipo;
    public string TerritorioAsociado => _territorioAsociado;
    public bool Usada => _usada;
    public string RutaImagen => _rutaImagen;


    public Carta(TipoCarta tipo, string territorioAsociado = "")
    {
        _tipo = tipo;
        _territorioAsociado = territorioAsociado;
        _usada = false;

        if (tipo == TipoCarta.Infanteria)
            _rutaImagen = "/imagenes/infanteria.png";
        else if (tipo == TipoCarta.Caballeria)
            _rutaImagen = "/imagenes/caballeria.png";
        else // Artilleria
            _rutaImagen = "/imagenes/artilleria.png";
    }

    public void MarcarUsada()
    {
        _usada = true;
    }

    public override string ToString()
    {
        return $"{_tipo} ({_territorioAsociado}){(_usada ? " [USADA]" : "")}";
    }
}
