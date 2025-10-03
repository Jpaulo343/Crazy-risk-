using System;
using System.IO;            // <-- agrega
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Crazy_risk
{
    public class Conexion
    {
        private TcpClient cliente;
        private TcpListener servidor;
        private NetworkStream flujo;

        // NUEVO: lector/escritor por líneas
        private StreamReader lector;
        private StreamWriter escritor;

        public bool EstaConectado => cliente != null && cliente.Connected;

        // Modo servidor
        public async Task IniciarServidor(int puerto)
        {
            servidor = new TcpListener(IPAddress.Any, puerto);
            servidor.Start();
            cliente = await servidor.AcceptTcpClientAsync();
            flujo = cliente.GetStream();

            // configurar lectura/escritura por líneas
            lector = new StreamReader(flujo, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 4096, leaveOpen: true);
            escritor = new StreamWriter(flujo, Encoding.UTF8, bufferSize: 4096, leaveOpen: true) { AutoFlush = true, NewLine = "\n" };
        }

        // Modo cliente
        public async Task Conectar(string host, int puerto)
        {
            cliente = new TcpClient();
            await cliente.ConnectAsync(host, puerto);
            flujo = cliente.GetStream();

            lector = new StreamReader(flujo, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 4096, leaveOpen: true);
            escritor = new StreamWriter(flujo, Encoding.UTF8, bufferSize: 4096, leaveOpen: true) { AutoFlush = true, NewLine = "\n" };
        }

        public async Task Enviar(string mensaje)
        {
            if (escritor == null) return;
            await escritor.WriteLineAsync(mensaje);   // <-- SIEMPRE termina en \n
        }

        public async Task<string> Recibir()
        {
            if (lector == null) return null;
            return await lector.ReadLineAsync();      // <-- lee UNA línea completa (hasta \n)
        }

        public void Cerrar()
        {
            try { escritor?.Dispose(); } catch { }
            try { lector?.Dispose(); } catch { }
            try { flujo?.Close(); } catch { }
            try { cliente?.Close(); } catch { }
            try { servidor?.Stop(); } catch { }
        }
    }
}
