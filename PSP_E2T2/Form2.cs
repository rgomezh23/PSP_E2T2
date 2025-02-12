using System.Net.Sockets;

namespace PSP_E2T2
{
    public partial class Form2 : Form
    {
        // Aldagaiak sortuko ditugu, gero erabili ahal izateko.
        private Form1 form1;
        public TcpClient client = null;
        public NetworkStream str = null;
        public StreamReader sr = null;
        public StreamWriter sw = null;

        public Form2(Form1 form1)
        {
            InitializeComponent();
            this.form1 = form1;

            // OnMessageReceived-ra suskribitzen da, saioa hasi ondoren.
            this.form1.OnMessageReceived += Form1_OnMessageReceived;
        }

        // Form1-tik jasotako nezuak kudeatzen ditu.
        private void Form1_OnMessageReceived(string mensaje)
        {
            // Erakutsi mezua bezero bat sartzeari edo txatean bidalitako mezu bati buruzkoa bada soilik.
            richTextBox1.Invoke((MethodInvoker)(() =>
            {
                // Erakutsi "sartu da" edo txat-mezu bat badu soilik.
                if (mensaje.Contains("sartu da") || mensaje.Contains(":"))
                {
                    richTextBox1.AppendText(mensaje + "\n");
                }
            }));
        }

        //Botoia klikatu eta jarraian mezua bidaliko da.
        private async void button1_Click_1(object sender, EventArgs e)
        {
            if (form1.erabiltzaileak.Count > 0)
            {
                string izena = form1.erabiltzaileak[form1.erabiltzaileak.Count - 1]; // Azken erabiltzailea.
                string mensaje = $"{izena}: {richTextBox2.Text}";

                try
                {
                    // Mezua zerbitzarira bidali.
                    await form1.sw.WriteLineAsync(mensaje);

                    // Txat-an mezua bistaratzen du.
                    richTextBox1.AppendText(mensaje + "\n");

                    // Mezua idazteko erabiltzen den testu hutsunea garbitzen du.
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

        // Saioa hasteko leihora bueltatzen du.
        private async void button2_Click(object sender, EventArgs e)
        {
            form1.Show();
            this.Hide();

            bool connected = form1.getConnected();
            client = form1.GetClient();
            sr = form1.getStreamReader();
            sw = form1.getStreamWriter();

            if (client != null && connected)
            {
                MessageBox.Show("Enviando mensaje de desconexión...", "Desconexión", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Zerbitzarira bezero hau deskonektatzeko eskatzen dio.
                await sw.WriteLineAsync("ATERA");
                await Task.Delay(100);

                // Eta konexioa kudeatzeko aldagaiak itzaltzen ditu.
                sw.Close();
                sr.Close();
                client.Close();

                MessageBox.Show("Cliente desconectado.", "Desconexión", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("No hay cliente conectado.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void richTextBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
            }
        }

        private void richTextBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
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
