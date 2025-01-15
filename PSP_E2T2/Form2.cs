namespace PSP_E2T2
{
    public partial class Form2 : Form
    {
        private Form1 form1;

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
                if (mensaje.Contains("se ha unido") || mensaje.Contains(":"))
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

                    // Mostrar el mensaje en el propio chat
                    richTextBox1.AppendText(mensaje + "\n");

                    // Limpiar el contenido de richTextBox2
                    richTextBox2.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al enviar el mensaje: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("No hay usuarios registrados para enviar mensajes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        // Evento para regresar al Form1
        private async void button2_Click(object sender, EventArgs e)
        {
            // Mostrar form1
            form1.Show();
            this.Hide();
        }
    }
}
