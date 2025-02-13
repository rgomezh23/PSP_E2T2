using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TxatServer
{
    public class ProgramServer
    {
        private readonly int port = 13000; // Puerto del servidor
        private IPAddress localAddr; // Dirección IP del servidor
        private TcpListener server; // Listener del servidor
        private ConcurrentDictionary<string, TcpClient> clientesActivos = new ConcurrentDictionary<string, TcpClient>(); // Clientes activos
        private bool isRunning = true; // Control del bucle del servidor
        public event Action<string> OnNewMessageReceived;

        private ConcurrentDictionary<string, string> usuariosConectados = new ConcurrentDictionary<string, string>();

        public ProgramServer()
        {
            // Obtener la dirección IP local del servidor
            this.localAddr = GetLocalIPAddress();
            this.server = new TcpListener(this.localAddr, this.port);
        }

        private IPAddress GetLocalIPAddress()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                        return ip;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo la IP local: {ex.Message}");
            }

            return IPAddress.Loopback; // Dirección de bucle local como respaldo
        }

        public async Task StartServerAsync()
        {
            try
            {
                this.server.Start();
                Console.WriteLine($"Servidor de chat iniciado en {this.localAddr}:{this.port}. Esperando clientes...");

                while (isRunning)
                {
                    if (!this.server.Pending())
                    {
                        await Task.Delay(100);
                        continue;
                    }

                    var client = await this.server.AcceptTcpClientAsync();
                    var clientId = GetClientId(client);

                    clientesActivos[clientId] = client;
                    Console.WriteLine($"Nuevo cliente conectado: {clientId}");

                    _ = HandleClientAsync(client, clientId); // Manejar cliente en un hilo separado
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en el servidor: {ex.Message}");
            }
        }

        // Dentro del HandleClientAsync, en el bloque donde se reciben los mensajes:
        private async Task HandleClientAsync(TcpClient client, string clientId)
        {
            try
            {
                using var stream = client.GetStream();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                // Solicitar el nombre de usuario al cliente
                await writer.WriteLineAsync("Por favor, introduce tu nombre de usuario:");
                string nombreUsuario = await reader.ReadLineAsync();

                // Verificar si el nombre de usuario ya está en uso
                if (usuariosConectados.ContainsKey(nombreUsuario))
                {
                    await writer.WriteLineAsync("El nombre de usuario ya está en uso. Desconectando...");
                    client.Close();
                    return;
                }

                // Agregar el nombre de usuario a la lista de usuarios conectados
                usuariosConectados[nombreUsuario] = clientId;
                clientesActivos[clientId] = client;

                Console.WriteLine($"Nuevo cliente conectado: {nombreUsuario} ({clientId})");
                await writer.WriteLineAsync($"Bienvenido al chat, {nombreUsuario}!");

                string message;
                while ((message = await reader.ReadLineAsync()) != null)
                {
                    Console.WriteLine($"Mensaje recibido de {nombreUsuario}: {message}");

                    if (message.ToUpper() == "ATERA")
                    {
                        Console.WriteLine($"Cliente {nombreUsuario} se ha desconectado.");
                        await writer.WriteLineAsync($"Adiós, {nombreUsuario}!");
                        break;
                    }

                    // Enviar el mensaje con el formato "nombreUsuario: mensaje"
                    await BroadcastMessageToAllClients(nombreUsuario, message);
                    OnNewMessageReceived?.Invoke($"{nombreUsuario}: {message}");
                }
            }
            catch (IOException)
            {
                Console.WriteLine($"El cliente {clientId} cerró la conexión.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error con cliente {clientId}: {ex.Message}");
            }
            finally
            {
                // Eliminar cliente de la lista
                clientesActivos.TryRemove(clientId, out _);
                usuariosConectados.TryRemove(clientId, out _);
                client.Close();
                Console.WriteLine($"Cliente desconectado: {clientId}");
            }
        }

        private async Task BroadcastMessageToAllClients(string clientId, string message)
        {
            // Iterar sobre todos los clientes conectados y enviarles el mensaje
            foreach (var client in clientesActivos.Values)
            {
                try
                {
                    // Verificar si el cliente es el que está enviando el mensaje
                    if (client.Client.RemoteEndPoint.ToString() != clientId)
                    {
                        var writer = new StreamWriter(client.GetStream(), Encoding.UTF8) { AutoFlush = true };
                        await writer.WriteLineAsync(message);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al enviar mensaje a cliente: {ex.Message}");
                }
            }
        }

        private string GetClientId(TcpClient client)
        {
            return ((IPEndPoint)client.Client.RemoteEndPoint).ToString();
        }

        public void StopServer()
        {
            isRunning = false;
            foreach (var kvp in clientesActivos)
                kvp.Value.Close();

            this.server.Stop();
            Console.WriteLine("Servidor detenido.");
        }

        public static async Task Main(string[] args)
        {
            var server = new ProgramServer();
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                server.StopServer();
            };

            await server.StartServerAsync();
        }
    }
}
