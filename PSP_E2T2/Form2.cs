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

            // Form1-eko OnMessageReceived gertaerara harpidetu
            this.form1.OnMessageReceived += Form1_OnMessageReceived;
        }

        // Form1-etik jasotako mezuak kudeatzen dituen metodoa
        private void Form1_OnMessageReceived(string mensaje)
        {
            // Mezua erakutsi bezero bat batu den edo txat mezu bat den kasuetan bakarrik
            richTextBox1.Invoke((MethodInvoker)(() =>
            {
                // Erakutsi soilik "sartu da" edo ":" duten mezuak
                if (mensaje.Contains("sartu da") || mensaje.Contains(":"))
                {
                    richTextBox1.AppendText(mensaje + "\n");
                }
            }));
        }

        // Botoia sakatzean mezua bidaltzeko gertaera
        private async void button1_Click_1(object sender, EventArgs e)
        {
            if (form1.erabiltzaileak.Count > 0)
            {
                string izena = form1.erabiltzaileak[form1.erabiltzaileak.Count - 1]; // Azken gehitutako erabiltzailea
                string mensaje = $"{izena}: {richTextBox2.Text}";

                try
                {
                    // Mezua zerbitzarira bidali
                    await form1.sw.WriteLineAsync(mensaje);

                    // richTextBox2 edukia garbitu
                    richTextBox2.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errorea mezua bidaltzean: {ex.Message}", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Ez dago erregistratutako erabiltzailearik mezuak bidaltzeko.", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Form1-era itzultzeko gertaera
        private async void button2_Click(object sender, EventArgs e)
        {
            // Form1 erakutsi
            form1.Show();
            this.Hide();

            bool connected = form1.getConnected();
            client = form1.GetClient();
            sr = form1.getStreamReader();
            sw = form1.getStreamWriter();

            if (client != null && connected)
            {
                MessageBox.Show("Deskonektatzeko mezua bidaltzen...", "Deskonektatzea", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Deskonektatzeko mezua zerbitzarira bidali
                await sw.WriteLineAsync("ATERA");
                await Task.Delay(100);

                // Streamak eta bezeroa itxi
                sw.Close();
                sr.Close();
                client.Close();

                MessageBox.Show("Bezeroa deskonektatu da.", "Deskonektatzea", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Ez dago konektatutako bezerorik.", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void richTextBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Enter teklaren prozesatzea blokeatu
            }
        }

        private void richTextBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true; // Gertaera baliogabetu lerro-jauzia ekiditeko
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
