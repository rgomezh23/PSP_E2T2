using TxatServer;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PSP_E2T2
{
    public partial class Form1 : Form
    {
        public List<string> erabiltzaileak = new List<string>(); // Erabiltzaileen zerrenda gordetzeko.
        public TcpClient client = null; // Bezero TCP konektorea.
        public NetworkStream str = null; // Datu-fluxua sare bidez bidaltzeko.
        public StreamReader sr = null; // Datuak irakurtzeko.
        public StreamWriter sw = null; // Datuak idazteko.
        private ProgramServer server; // Zerbitzariaren erreferentzia, gero konektatu ahal izateko.
        private Form2 form2; // Txataren leihoa.
        private bool isConnected = false; // Konexio-egoera gordetzen du.
        public event Action<string> OnMessageReceived; // Mezuak jasotzean ekintzak egiteko erabiliko den aldagaia.

        public Form1()
        {
            InitializeComponent();
            erabiltzaileak = new List<string>(); // Erabiltzaileen zerrenda hasieratzen du.
        }

        public bool getConnected()
        {
            return isConnected; // Konexio-egoera itzultzen du.
        }

        public StreamReader getStreamReader()
        {
            return sr; // StreamReader objektua itzultzen du.
        }

        public StreamWriter getStreamWriter()
        {
            return sw; // StreamWriter objektua itzultzen du.
        }

        public TcpClient GetClient()
        {
            return client; // TCP bezeroa itzultzen du.
        }

        // Gailuaren tokiko IP helbidea lortzeko metodoa.
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
                MessageBox.Show($"Errorea IP lortzean: {ex.Message}", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return localIP;
        }


        // Zerbitzariaren mezuei entzuteko metodoa.
        private async Task ListenForServerMessages()
        {
            try
            {
                string message;
                while ((message = await sr.ReadLineAsync()) != null)
                {
                    // Mezu bat jasotzean ekitaldia aktibatzen du.
                    OnMessageReceived?.Invoke(message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errorea zerbitzariaren mezuak jasotzean: {ex.Message}", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Metodo hau deitu behar da bezeroa zerbitzarira konektatu ondoren.
        private async void ConnectToServer()
        {
            if (isConnected)
            {
                MessageBox.Show("Dagoeneko zerbitzarira konektatuta zaude.", "Konexioa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // Konexio-konfigurazioa.
                string serverAddress = GetLocalIPAddress(); // Edo zerbitzariaren IP-a.
                int serverPort = 13000;

                client = new TcpClient();
                await client.ConnectAsync(serverAddress, serverPort);

                str = client.GetStream();
                sr = new StreamReader(str, Encoding.UTF8);
                sw = new StreamWriter(str, Encoding.UTF8) { AutoFlush = true };

                // Zerbitzariaren mezuei entzuteko prozesua hasten du.
                _ = ListenForServerMessages();

                isConnected = true;
                MessageBox.Show("Zerbitzariarekin konexioa ezarri da.", "Konexioa", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errorea zerbitzarira konektatzean: {ex.Message}", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            // Erabiltzailearen izena eskatzen du.
            string izena = Microsoft.VisualBasic.Interaction.InputBox("Sartu zure izena:", "Izena");

            if (string.IsNullOrEmpty(izena))
            {
                MessageBox.Show("Izena ezin da hutsik egon.", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            
            ConnectToServer(); // Zerbitzarira konektatzen saiatzen da.

            // Formulario berria irekitzen du erabiltzailea gehitu ondoren.
            if (form2 == null || form2.IsDisposed)
            {
                form2 = new Form2(this); // Form1 parametro bezala pasatzen du.
                form2.Show();
            }
            else
            {
                form2.Focus();
            }

            this.Hide(); // Uneko leihoa ezkutatzen du, txat-aren leihoa irekiko duelako.
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Programa amatatzen.", "Itxaron...", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Aplikazioa ixten du.
            Application.Exit();
        }
    }
}
