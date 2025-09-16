using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
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
        public Nodo<T>? Head { get; protected set; }
        public Nodo<T>? Tail { get; protected set; }
        private static Random generadorAleatorio = new Random();

        public int size { get; protected set; }

        /*
        Añade el dato del parametro a la lista, lo añade en la cabeza de la lista 
        */
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

        /*
        Esta función retorna true si el dato del parametro está en la lista, retorna false si no
        */
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

        /*
        Esta función retorna el objeto que se encuentra en un indice de la lista
        */
        public T ObtenerEnIndice(int indice)
        {
            if (indice < 0 || indice >= size)
                throw new IndexOutOfRangeException();

            Nodo<T> actual = Head!;
            for (int i = 0; i < indice; i++)
            {
                actual = actual.Next!;
            }

            return actual.Value;
        }


        /*
         Esta función retorna un elemento aleatorio de la lista, y lo elimina

         En este proyecto se usa para asignar el color de los jugadores y distribuir los territorios entre los jugadores
         */
        public T Seleccionar_Y_Eliminar_Random()
        {   
            if (Head != null)
            {
                int indice = generadorAleatorio.Next(0, size);

                if (indice == 0)
                {
                    T dato = Head!.Value;
                    Head = Head.Next;
                    if (Head == null)
                    {
                        Tail = null;
                    }
                    size--;
                    return dato;
                }


                Nodo<T> actual = Head!;
                for (int i = 0; i < indice - 1; i++)
                {
                    actual = actual.Next!;
                }

                T datoEliminado = actual.Next!.Value;
                actual.Next = actual.Next!.Next;
                if (actual.Next == null)
                {
                    Tail = actual;
                }
                size--;

                return datoEliminado;

            }
            else
            {
                throw new InvalidOperationException("La lista está vacía.");
            }

        }

        /*
        Elimina el dato enviado a traves del parametro de la lista, retorna true si se eliminó, false si no
        */
        public bool Eliminar(T dato)
        {
            if (Head == null)
            {
                return false;
            }

            if (Head.Value!.Equals(dato))
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
                if (actual.Next.Value!.Equals(dato))
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
            return false;
        }

        /*
         retorna un objeto iterable que contiene todos los datos
         Permite que la lista se pueda recorrer con un foreach
         */
        public IEnumerable<T> Enumerar()
        {
            Nodo<T> actual = Head!;
            while (actual != null)
            {
                yield return actual.Value;   
                actual = actual.Next!;   
            }
        }

        /*
         Esta función permite pasar una función lambda como parametro de la forma (j => j.propiedad == dato_A_Comparar) 
        */
        public T? BuscarPorCondición(Func<T, bool> criterio)
        {
            Nodo<T> actual = Head;
            while (actual != null)
            {
                if (criterio(actual.Value))
                    return actual.Value;

                actual = actual.Next!;
            }
            return default;
        }

    }



    public class ListaTerritorios : ListaEnlazada<Territorio> 
    {

        /*
         Esta función verifica si todos los territorios tienen el mismo dueño retorna el nombre del ganador, si no, retorna null
        */
        public string VerificarVictoria() 
        {
            string primerPropietario = this.Head!.Value.Conquistador;
            Nodo<Territorio> actual = this.Head;
            while (actual != null)
            {
                if (actual.Value.Conquistador != primerPropietario)
                {
                    return null!;
                }
                actual = actual.Next!;
            }
            return primerPropietario;
        }

        /*
        Esta función permite crear una copia de una lista (que puede ser modificada sin modificar la lista original y
        sin duplicar los objetos multiples veces, de tal forma que, si se modifica un objeto de una copia se cambia 
        tambien en la lista original)
        */
        public ListaTerritorios CopiarLista()
        {
            ListaTerritorios copia = new ListaTerritorios();

            Nodo<Territorio> actual = Head!;
            while (actual != null)
            {
                copia.Añadir(actual.Value);
                actual = actual.Next!;
            }

            return copia;
        }
    }

}

