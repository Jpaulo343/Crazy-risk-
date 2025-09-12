using System.Net;
using System.Net.Sockets;
using System.Text;

if (args.Length == 0) {
    Console.WriteLine("Usa: --server [--port 5000]  |  --client [--host 127.0.0.1] [--port 5000]");
    return;
}

int port = 5000;
string host = "127.0.0.1";

for (int i = 0; i < args.Length; i++) {
    if (args[i] == "--port" && i + 1 < args.Length) port = int.Parse(args[++i]);
    if (args[i] == "--host" && i + 1 < args.Length) host = args[++i];
}

if (args[0] == "--server") {
    await RunServerAsync(port);
} else if (args[0] == "--client") {
    await RunClientAsync(host, port);
} else {
    Console.WriteLine("Argumento no reconocido.");
}

static async Task RunServerAsync(int port) {
    var listener = new TcpListener(IPAddress.Loopback, port); 
    listener.Start();
    Console.WriteLine($"[SERVIDOR] Escuchando en 127.0.0.1:{port} ...");

    using var client = await listener.AcceptTcpClientAsync();
    Console.WriteLine("[SERVIDOR] Cliente conectado.");
    using var stream = client.GetStream();

    // Eco simple: lo que recibe, lo devuelve
    var buffer = new byte[1024];
    int read;
    while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0) {
        var text = Encoding.UTF8.GetString(buffer, 0, read);
        Console.WriteLine($"[SERVIDOR] Recibido: {text.Trim()}");
        var reply = Encoding.UTF8.GetBytes($"ECO:{text}");
        await stream.WriteAsync(reply, 0, reply.Length);
    }

    Console.WriteLine("[SERVIDOR] Cliente desconectado.");
    listener.Stop();
}

static async Task RunClientAsync(string host, int port) {
    using var client = new TcpClient();
    await client.ConnectAsync(host, port);
    Console.WriteLine($"[CLIENTE] Conectado a {host}:{port}");
    using var stream = client.GetStream();

    Console.WriteLine("[CLIENTE] Escribe mensajes y ENTER. Ctrl+C para salir.");
    while (true) {
        var line = Console.ReadLine();
        if (line == null) break;
        var data = Encoding.UTF8.GetBytes(line + "\n");
        await stream.WriteAsync(data, 0, data.Length);

        var buffer = new byte[1024];
        int read = await stream.ReadAsync(buffer, 0, buffer.Length);
        var text = Encoding.UTF8.GetString(buffer, 0, read);
        Console.WriteLine($"[CLIENTE] Respuesta: {text.Trim()}");
    }
}
