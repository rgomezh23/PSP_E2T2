using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace TxatServer
{
    public class ProgramServer
    {
        private readonly int port = 13000; // Puerto del servidor
        private IPAddress localAddr; // Dirección IP del servidor
        private TcpListener server; // Listener del servidor
        private ConcurrentDictionary<string, List<TcpClient>> chatsActivos = new ConcurrentDictionary<string, List<TcpClient>>(); // Chats activos
        private ConcurrentDictionary<string, TcpClient> clientesActivos = new ConcurrentDictionary<string, TcpClient>(); // Clientes activos
        private bool isRunning = true; // Control del bucle del servidor
        public event Action<string> OnNewMessageReceived;

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
                var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                // Enviar lista de chats activos al cliente
                await SendChatListToClient(writer);

                string message;
                string erantzuna;
                string clientChat = null;

                // Leer mensajes del cliente
                while ((message = await reader.ReadLineAsync()) != null)
                {
                    Console.WriteLine($"Mensaje recibido de {clientId}: {message}");
                    erantzuna = "BIDALITA";

                    if (erantzuna.ToUpper() == "BIDALITA")
                    {
                        await writer.WriteLineAsync(erantzuna);
                    }

                    // Si el mensaje es "ATERA", desconectar el cliente
                    if (message.ToUpper() == "ATERA")
                    {
                        Console.WriteLine($"Cliente {clientId} se ha desconectado.");
                        break;
                    }

                    // Si el cliente no está en un chat, lo asignamos a uno
                    if (clientChat == null)
                    {
                        clientChat = message; // El primer mensaje del cliente es el chat al que quiere unirse
                        await AddClientToChat(clientChat, client, writer);
                    }
                    else
                    {
                        // Aquí, haces el broadcast de mensaje a todos los clientes
                        await BroadcastMessageToChat(clientChat, $"{clientId}: {message}");

                        // Dispara el evento para enviar el mensaje a Form2 (cliente)
                        OnNewMessageReceived?.Invoke($"{clientId}: {message}");
                    }
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
                client.Close();
                Console.WriteLine($"Cliente desconectado: {clientId}");
            }
        }
        private async Task SendChatListToClient(StreamWriter writer)
        {
            try
            {
                // Enviar la lista de chats activos a un cliente
                var chatList = string.Join(",", chatsActivos.Keys);
                await writer.WriteLineAsync($"Chats disponibles: {chatList}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando lista de chats: {ex.Message}");
            }
        }

        // Método para agregar un cliente al chat
        private async Task AddClientToChat(string chatName, TcpClient client, StreamWriter writer)
        {
            try
            {
                // Verificar si el chat no existe, y crearlo si es necesario
                if (!chatsActivos.ContainsKey(chatName))
                {
                    // Crear un nuevo chat si no existe
                    chatsActivos[chatName] = new List<TcpClient>();
                    Console.WriteLine($"Nuevo chat creado: {chatName}");

                }

                // Agregar el cliente al chat
                chatsActivos[chatName].Add(client);
                Console.WriteLine($"Cliente agregado al chat: {chatName}");

                // Notificar a todos los miembros del chat sobre el nuevo cliente
                await BroadcastMessageToChat(chatName, $"{client.Client.RemoteEndPoint} se ha unido al chat {chatName}");

                // Enviar el historial de mensajes al nuevo cliente (si existe)
                await SendChatHistoryToClient(writer, chatName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error agregando cliente al chat: {ex.Message}");
            }
        }


        private async Task BroadcastMessageToChat(string chatId, string message)
        {
            // Iterar sobre todos los clientes conectados y enviarles el mensaje
            foreach (var client in clientesActivos.Values)
            {
                try
                {
                    var writer = new StreamWriter(client.GetStream(), Encoding.UTF8) { AutoFlush = true };
                    await writer.WriteLineAsync(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al enviar mensaje a cliente: {ex.Message}");
                }
            }
        }


        private async Task SendChatHistoryToClient(StreamWriter writer, string chatName)
        {
            try
            {
                // Enviar el historial de mensajes al cliente
                if (chatsActivos.ContainsKey(chatName))
                {
                    foreach (var client in chatsActivos[chatName])
                    {
                        // Asegurarnos de que el cliente reciba el historial
                        var chatHistory = "== Historial de mensajes ==";
                        foreach (var msg in chatsActivos[chatName])
                        {
                            chatHistory += msg;
                        }

                        await writer.WriteLineAsync(chatHistory);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando historial al cliente: {ex.Message}");
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
