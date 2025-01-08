using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PraktikaIndibiduala
{
    public class ProgramServer
    {
        private readonly int port = 13000; // Puerto del servidor
        private IPAddress localAddr; // Dirección IP del servidor
        private TcpListener server; // Listener del servidor
        private ConcurrentDictionary<string, List<string>> clienteHistorial = new ConcurrentDictionary<string, List<string>>(); // Historial por cliente
        private ConcurrentDictionary<string, TcpClient> clientesActivos = new ConcurrentDictionary<string, TcpClient>(); // Clientes activos
        private bool isRunning = true; // Control del bucle del servidor

        public ProgramServer()
        {
            // Obtener la dirección IP local del servidor
            this.localAddr = GetLocalIPAddress();
            this.server = new TcpListener(this.localAddr, this.port);
        }

        // Método para obtener la dirección IP local del servidor
        private IPAddress GetLocalIPAddress()
        {
            string localIP = string.Empty;

            // Obtiene la dirección IP local
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    // Filtra las direcciones IPv4
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo la IP local: {ex.Message}");
            }

            return IPAddress.Parse(localIP);
        }

        // En ProgramServer
        public async Task StartServerAsync()
        {
            try
            {
                // Inicia el servidor
                this.server.Start();
                Console.WriteLine($"Servidor de chat iniciado en {this.localAddr}:{this.port}. Esperando clientes...");

                // Bucle principal de aceptación de clientes
                while (isRunning)
                {
                    if (!this.server.Pending())
                    {
                        await Task.Delay(100); // Evitar uso intensivo de CPU
                        continue;
                    }

                    TcpClient client = await this.server.AcceptTcpClientAsync();
                    string clientId = GetClientId(client);

                    // Añadir cliente a los activos
                    clientesActivos[clientId] = client;
                    Console.WriteLine($"Nuevo cliente conectado: {clientId}");

                    // Enviar historial si existe
                    if (clienteHistorial.TryGetValue(clientId, out List<string> historial))
                    {
                        await EnviarHistorialAsync(client, historial);
                    }

                    _ = HandleClientAsync(client, clientId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en el servidor: {ex.Message}");
            }
        }


        private async Task HandleClientAsync(TcpClient client, string clientId)
        {
            try
            {
                using (NetworkStream stream = client.GetStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
                {
                    string message;
                    while ((message = await reader.ReadLineAsync()) != null)
                    {
                        Console.WriteLine($"Mensaje recibido de {clientId}: {message}");

                        if (message.ToUpper() == "ATERA")
                        {
                            Console.WriteLine($"Cliente {clientId} se ha desconectado.");
                            break; // El cliente se desconectó
                        }

                        // Guardar mensaje en el historial del cliente
                        clienteHistorial.AddOrUpdate(
                            clientId,
                            new List<string> { message },
                            (key, existingList) =>
                            {
                                existingList.Add(message);
                                return existingList;
                            });

                        // Enviar mensaje a todos los demás clientes conectados
                        await BroadcastMessageAsync($"{clientId}: {message}", clientId);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error con cliente {clientId}: {ex.Message}");
            }
            finally
            {
                // Eliminar cliente de los activos
                clientesActivos.TryRemove(clientId, out _);
                client.Close();
                Console.WriteLine($"Cliente desconectado: {clientId}");
            }
        }


        private async Task BroadcastMessageAsync(string message, string senderId)
        {
            foreach (var kvp in clientesActivos)
            {
                string clientId = kvp.Key;
                TcpClient client = kvp.Value;

                if (clientId == senderId || !client.Connected) continue;

                try
                {
                    using (NetworkStream stream = client.GetStream())
                    using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
                    {
                        await writer.WriteLineAsync(message);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error enviando mensaje a {clientId}: {ex.Message}");
                }
            }
        }

        private async Task EnviarHistorialAsync(TcpClient client, List<string> historial)
        {
            try
            {
                using (NetworkStream stream = client.GetStream())
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
                {
                    await writer.WriteLineAsync("== Historial de mensajes ==");
                    foreach (var mensaje in historial)
                    {
                        await writer.WriteLineAsync(mensaje);
                    }
                    await writer.WriteLineAsync("== Fin del historial ==");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando historial: {ex.Message}");
            }
        }

        private string GetClientId(TcpClient client)
        {
            return ((IPEndPoint)client.Client.RemoteEndPoint).ToString();
        }

        public void StopServer()
        {
            try
            {
                isRunning = false;
                foreach (var kvp in clientesActivos)
                {
                    kvp.Value.Close();
                }
                this.server.Stop();
                Console.WriteLine("Servidor detenido.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deteniendo el servidor: {ex.Message}");
            }
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

            Console.WriteLine("Presiona <ENTER> para salir...");
            Console.ReadLine();
        }
    }
}
