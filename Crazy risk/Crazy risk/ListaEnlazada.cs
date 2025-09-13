using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Crazy_risk
{
    public class Nodo<T>
    {
        public T Value { get; private set; }
        public Nodo<T>? Next { get; set; }

        public Nodo(T dato)
        {
            Value = dato;
        }
        
    }
    public class ListaEnlazada<T>
    {
        protected Nodo<T>? Head { get; set; }
        protected Nodo<T>? Tail { get; set; }

        protected int size { get; set; }

        public void Añadir(T dato)
        {
            Nodo<T> nodo = new Nodo<T>(dato);
            size++;
            if (Head == null)
            {
                Tail = Head = nodo;
                return;
            }
            else
            {
                Nodo<T> Temp = Head;
                Head = nodo;
                Head.Next = Temp;
                return;
            }
        }

        public bool Buscar(T dato)
        {

            Nodo<T> actual = Head;
            while (actual != null)
            {
                if (actual.Value!.Equals(dato))
                { 
                    return true;
                }

                actual = actual.Next!;
            }
            return false;
        }

        public bool Eliminar(T dato)
        {
            if (Head == null)
            {
                return false;
            }

            if (Head.Value.Equals(dato))
            {
                Head = Head.Next;
                if (Head == null)
                {
                    Tail = null;
                }
                size--;
                return true;
            }

            Nodo<T> actual = Head;
            while (actual.Next != null)
            {
                if (actual.Next.Value.Equals(dato))
                {
                    actual.Next = actual.Next.Next;
                    if (actual.Next == null)
                    {
                        Tail = actual;
                    }
                    size--;
                    return true;
                }
                actual = actual.Next;
            }

            // El nodo no fue encontrado
            return false;
        }


        public IEnumerable<T> Enumerar()
        {
            Nodo<T> actual = Head!;
            while (actual != null)
            {
                yield return actual.Value;   
                actual = actual.Next!;   
            }
        }

    }



    internal class ListaTerritorios : ListaEnlazada<Territorio> 
    {
        public  Territorio BuscarPorNombre(string nombre)
        {
            if (Head != null){
                Nodo<Territorio> Temp = Head!;
                while (Temp != null)
                {
                    if (Temp.Value.Nombre == nombre)
                    {
                        return Temp.Value;
                    }
                    Temp = Temp.Next!;
                }
            }

            return null!;

        }
    }

}

