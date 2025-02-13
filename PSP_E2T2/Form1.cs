using TxatServer;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PSP_E2T2
{
    public partial class Form1 : Form
    {
        public List<string> erabiltzaileak = new List<string>();

        public TcpClient client = null;

        public NetworkStream str = null;

        public StreamReader sr = null;
        public StreamWriter sw = null;

        private ProgramServer server;

        private Form2 form2;

        private bool isConnected = false;

        public event Action<string> OnMessageReceived;

        public Form1()
        {
            InitializeComponent();
            erabiltzaileak = new List<string>();
        }

        public bool getConnected()
        {
            return isConnected;
        }

        public StreamReader getStreamReader()
        {
            return sr;
        }

        public StreamWriter getStreamWriter()
        {
            return sw;
        }

        public TcpClient GetClient()
        {
            return client;
        }

        // Método para obtener la IP local del dispositivo
        private string GetLocalIPAddress()
        {
            string localIP = string.Empty;

            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener la IP: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return localIP;
        }

        public void AddErabiltzaile(string izena)
        {
            string ip = GetLocalIPAddress();

            if (erabiltzaileak.Contains(izena))
            {
                MessageBox.Show("Izena hori beste erabiltzaile du. Ipini beste bat.", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (!string.IsNullOrEmpty(izena))
            {
                erabiltzaileak.Add(izena);
            }
        }

        // Método para recibir mensajes del servidor

        private async Task ListenForServerMessages()
        {
            try
            {
                string message;
                while ((message = await sr.ReadLineAsync()) != null)
                {
                    // Dispara el evento cuando se recibe un mensaje
                    OnMessageReceived?.Invoke(message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al recibir mensajes del servidor: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            // Solicitar el nombre del usuario
            string izena = Microsoft.VisualBasic.Interaction.InputBox("Sartu zure izena:", "Izena");

            if (!string.IsNullOrEmpty(izena))
            {
                AddErabiltzaile(izena);
                ConnectToServer(); // Intentar conectar al servidor
            }
            else
            {
                MessageBox.Show("Izena ezin da hutsik egon.", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void ConnectToServer()
        {
            if (isConnected)
            {
                MessageBox.Show("Ya estás conectado al servidor.", "Conexión", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                string serverAddress = GetLocalIPAddress();
                int serverPort = 13000;

                client = new TcpClient();
                await client.ConnectAsync(serverAddress, serverPort);

                str = client.GetStream();
                sr = new StreamReader(str, Encoding.UTF8);
                sw = new StreamWriter(str, Encoding.UTF8) { AutoFlush = true };

                // Esperar a que el servidor solicite el nombre de usuario
                string serverMessage = await sr.ReadLineAsync();
                if (serverMessage == "Por favor, introduce tu nombre de usuario:")
                {
                    // Enviar el nombre de usuario al servidor
                    await sw.WriteLineAsync(erabiltzaileak.Last());

                    // Leer la respuesta del servidor
                    string response = await sr.ReadLineAsync();
                    if (response == "El nombre de usuario ya está en uso. Desconectando...")
                    {
                        MessageBox.Show("El nombre de usuario ya está en uso. Por favor, elige otro.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        client.Close();
                        return; // No mostrar el Form2 si el nombre ya está en uso
                    }
                    else if (response == "SERVER_FULL")
                    {
                        MessageBox.Show("El servidor está lleno. Inténtalo de nuevo más tarde.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        client.Close();
                        return; // No mostrar el Form2 si el servidor está lleno
                    }
                    else if (response.StartsWith("Bienvenido al chat,"))
                    {
                        isConnected = true;
                        MessageBox.Show("Conexión establecida con el servidor.", "Conexión", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Iniciar el proceso de escucha de mensajes del servidor
                        _ = ListenForServerMessages();

                        // Mostrar el formulario Form2 después de una conexión exitosa
                        if (form2 == null || form2.IsDisposed)
                        {
                            form2 = new Form2(this); // Pasar Form1 como parámetro
                            form2.Show();
                        }
                        else
                        {
                            form2.Focus();
                        }

                        this.Hide(); // Ocultar el formulario actual
                    }
                }
            }
            catch (IOException)
            {
                // Captura la excepción cuando el servidor cierra la conexión abruptamente
                MessageBox.Show("El servidor está lleno o la conexión se interrumpió. Inténtalo de nuevo más tarde.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (SocketException)
            {
                MessageBox.Show("No se pudo conectar al servidor. Asegúrate de que el servidor esté en ejecución.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar al servidor: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Programa amatatzen.", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Salir de la aplicación
            Application.Exit();
        }
    }
}