using System.Net.Sockets;

namespace PSP_E2T2
{
    public partial class Form2 : Form
    {
        private Form1 form1;

        public TcpClient client = null;

        public NetworkStream str = null;

        public StreamReader sr = null;
        public StreamWriter sw = null;


        public Form2(Form1 form1)
        {
            InitializeComponent();
            this.form1 = form1;

            // Suscribirse al evento OnMessageReceived de Form1
            this.form1.OnMessageReceived += Form1_OnMessageReceived;
        }

        // Método que maneja los mensajes recibidos desde Form1
        private void Form1_OnMessageReceived(string mensaje)
        {
            // Mostrar el mensaje solo si es sobre la unión de un cliente o un mensaje enviado en el chat
            richTextBox1.Invoke((MethodInvoker)(() =>
            {
                // Solo mostrar si contiene "se ha unido" o un mensaje de chat
                if (mensaje.Contains("batu da") || mensaje.Contains(":"))
                {
                    richTextBox1.AppendText(mensaje + "\n");
                }
            }));
        }


        // Evento para enviar un mensaje cuando se presiona el botón
        private async void button1_Click_1(object sender, EventArgs e)
        {
            if (form1.erabiltzaileak.Count > 0)
            {
                string izena = form1.erabiltzaileak[form1.erabiltzaileak.Count - 1]; // Último usuario agregado
                string mensaje = $"{izena}: {richTextBox2.Text}";

                try
                {
                    // Enviar el mensaje al servidor
                    await form1.sw.WriteLineAsync(mensaje);

                    // Limpiar el contenido de richTextBox2
                    richTextBox2.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ezin izan da mezua bidali: {ex.Message}", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Ezin da mezurik bidali ez dagoelako beste pertsonarik.", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        // Evento para regresar al Form1
        private async void button2_Click(object sender, EventArgs e)
        {
            // Mostrar form1
            form1.Show();
            this.Hide();


            bool connected = form1.getConnected();
            client = form1.GetClient();
            sr = form1.getStreamReader();
            sw = form1.getStreamWriter();

            if (client != null && connected)
            {
                MessageBox.Show("Deskonexio mezua bidaltzen...", "Deskonexioa", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Enviar el mensaje de desconexión al servidor
                await sw.WriteLineAsync("ATERA");
                await Task.Delay(100);

                // Cerrar los streams y el cliente
                sw.Close();
                sr.Close();
                client.Close();

                MessageBox.Show("Bezeroa deskonektatuta.", "Desconexioa", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Ez dago bezero konektaturik.", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void richTextBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Bloquea el procesamiento de la tecla Enter
            }
        }

        private void richTextBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true; // Anula el evento para evitar el salto de línea
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form3 form3 = new Form3();
            form3.Show();
            this.Hide();
        }
    }
}