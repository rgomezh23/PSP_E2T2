using PraktikaIndibiduala;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.DataFormats;

namespace PSP_E2T2
{
    public partial class Form1 : Form
    {
        private List<string> chats = new List<string>();
        public List<string> erabiltzaileak = new List<string>();

        private Dictionary<string, string> chatIPs;


        // Bezero socket-a.
        public TcpClient client = null;

        public NetworkStream str = null;

        public StreamReader sr = null;
        public StreamWriter sw = null;

        private ProgramServer server;

        private Form2 form2;

        private bool isConnected = false;

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

        public void AddErabiltzaile(string izena)
        {
            string ip = GetLocalIPAddress();

            if (erabiltzaileak.Contains(izena))
            {
                MessageBox.Show("Izena hori beste erabiltzaile du. Ipini beste bat.", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (!string.IsNullOrEmpty(izena) && !chats.Contains(izena))
            {
                erabiltzaileak.Add(izena);
                chatIPs[izena] = ip; // Asocia el chat con la IP
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
                // Configuración de conexión
                string serverAddress = "192.168.202.33";
                int serverPort = 13000;

                client = new TcpClient();
                await client.ConnectAsync(serverAddress, serverPort);

                str = client.GetStream();
                sr = new StreamReader(str, Encoding.UTF8);
                sw = new StreamWriter(str, Encoding.UTF8) { AutoFlush = true };

                isConnected = true;
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

            if (form2 == null || form2.IsDisposed)
            {
                form2 = new Form2(this);
                form2.Show();
            }
            else
            {
                form2.Focus();
            }

            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Solicitar nombre del nuevo chat
            string nuevoChat = Microsoft.VisualBasic.Interaction.InputBox("Ingrese el nombre del nuevo chat:", "Nuevo Chat");

            if (!string.IsNullOrEmpty(nuevoChat))
            {
                comboBox1.SelectedIndexChanged -= comboBox1_SelectedIndexChanged; // Desactivar el evento
                AddChat(nuevoChat);
                comboBox1.SelectedItem = nuevoChat;
                comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged; // Reactivar el evento
            }
            else
            {
                MessageBox.Show("El nombre del chat no puede estar vacío.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Solicitar el nombre del usuario
            string izena = Microsoft.VisualBasic.Interaction.InputBox("Sartu zure izena:", "Izena");

            if (!string.IsNullOrEmpty(izena))
            {
                AddErabiltzaile(izena);
                ConnectToServer(); // Intentar conectar al servidor

                // Mostrar el formulario Form2 después de agregar el usuario
                if (form2 == null || form2.IsDisposed)
                {
                    form2 = new Form2(this);
                    form2.Show();
                }
                else
                {
                    form2.Focus();
                }

                this.Hide(); // Ocultar el formulario actual
            }
            else
            {
                MessageBox.Show("Izena ezin da hutsik egon.", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
