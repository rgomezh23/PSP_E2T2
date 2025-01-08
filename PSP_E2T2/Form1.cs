using PraktikaIndibiduala;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace PSP_E2T2
{
    public partial class Form1 : Form
    {
        private List<string> chats;
        private Dictionary<string, string> chatIPs;
        // Bezero socket-a.
        TcpClient client = null;

        NetworkStream str = null;

        StreamReader sr = null;
        StreamWriter sw = null;

        private ProgramServer server;

        public Form1()
        {
            InitializeComponent();
            chats = new List<string>();
            chatIPs = new Dictionary<string, string>();
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

        public void AddChat(string chat)
        {
            string ip = GetLocalIPAddress();

            if (!string.IsNullOrEmpty(chat) && !chats.Contains(chat))
            {
                chats.Add(chat);
                chatIPs[chat] = ip; // Asocia el chat con la IP
                comboBox1.Items.Add(chat);
            }
        }

        private async void ConnectToServer()
        {
            try
            {
                if (client != null && client.Connected)
                {
                    MessageBox.Show("Ya estás conectado al servidor.", "Conexión", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Asegúrate de que el servidor esté activo antes de conectar
                string serverAddress = "192.168.202.33"; // Dirección IP del servidor
                int serverPort = 13000; // Puerto del servidor

                // Conectar el cliente al servidor
                client = new TcpClient();
                await client.ConnectAsync(serverAddress, serverPort);

                // Inicializar StreamReader y StreamWriter para leer y escribir mensajes
                NetworkStream stream = client.GetStream();
                sr = new StreamReader(stream, Encoding.UTF8);
                sw = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                MessageBox.Show("Conexión establecida con el servidor.", "Conexión", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar al servidor: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedChat = comboBox1.SelectedItem.ToString();
            string ip = chatIPs.ContainsKey(selectedChat) ? chatIPs[selectedChat] : "IP desconocida";

            MessageBox.Show($"Has seleccionado el chat: {selectedChat}\nIP: {ip}", "Chat Seleccionado");

            // Abre un nuevo formulario con la información del chat
            Form2 form2 = new Form2(this);
            form2.Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string nuevoChat = Microsoft.VisualBasic.Interaction.InputBox("Ingrese el nombre del nuevo chat:", "Nuevo Chat");

            if (!string.IsNullOrEmpty(nuevoChat))
            {
                AddChat(nuevoChat);
                comboBox1.SelectedItem = nuevoChat;
                ConnectToServer();
            }
            else
            {
                MessageBox.Show("El nombre del chat no puede estar vacío.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (client != null && client.Connected)
            {
                MessageBox.Show("Enviando mensaje de desconexión...", "Desconexión", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Enviar el mensaje de desconexión al servidor
                await sw.WriteLineAsync("ATERA");

                // Cerrar los streams y el cliente
                sw.Close();
                sr.Close();
                client.Close();

                MessageBox.Show("Cliente desconectado.", "Desconexión", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("No hay cliente conectado.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Salir de la aplicación
            Application.Exit();
        }
    }
}
