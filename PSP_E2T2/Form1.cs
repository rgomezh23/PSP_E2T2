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

        // Gailuaren IP lokala lortzeko metodoa
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

        public void AddErabiltzaile(string izena)
        {
            string ip = GetLocalIPAddress();

            if (erabiltzaileak.Contains(izena))
            {
                MessageBox.Show("Izena hori beste erabiltzaile batek dauka. Aukeratu beste bat.", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (!string.IsNullOrEmpty(izena))
            {
                erabiltzaileak.Add(izena);
            }
        }

        // Zerbitzaritik mezuak jasotzeko metodoa
        private async Task ListenForServerMessages()
        {
            try
            {
                string message;
                while ((message = await sr.ReadLineAsync()) != null)
                {
                    // Mezu bat jasotzean ekitaldia aktibatzen du
                    OnMessageReceived?.Invoke(message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errorea zerbitzariko mezuak jasotzean: {ex.Message}", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            // Erabiltzailearen izena eskatu
            string izena = Microsoft.VisualBasic.Interaction.InputBox("Sartu zure izena:", "Izena");

            if (!string.IsNullOrEmpty(izena))
            {
                AddErabiltzaile(izena);
                ConnectToServer(); // Zerbitzariarekin konektatzen saiatu
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
                MessageBox.Show("Dagoeneko konektatuta zaude zerbitzarira.", "Konexioa", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

                // Zerbitzariak erabiltzailearen izena eskatu arte itxaron
                string serverMessage = await sr.ReadLineAsync();
                if (serverMessage == "Mesedez, sartu zure erabiltzaile izena:")
                {
                    // Erabiltzailearen izena bidali zerbitzarira
                    await sw.WriteLineAsync(erabiltzaileak.Last());

                    // Zerbitzariaren erantzuna irakurri
                    string response = await sr.ReadLineAsync();
                    if (response == "Erabiltzaile izena dagoeneko erabilia dago. Deskonektatzen...")
                    {
                        MessageBox.Show("Erabiltzaile izena erabilita dago. Aukeratu beste bat.", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        client.Close();
                        return; // Form2 ez erakutsi izena erabilita badago
                    }
                    else if (response == "SERVER_FULL")
                    {
                        MessageBox.Show("Zerbitzaria beteta dago. Saiatu berriro geroago.", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        client.Close();
                        return; // Form2 ez erakutsi zerbitzaria beteta badago
                    }
                    else if (response.StartsWith("Bienvenido al chat,"))
                    {
                        isConnected = true;
                        MessageBox.Show("Konexioa arrakastaz ezarri da zerbitzariarekin.", "Konexioa", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Zerbitzariko mezuak jasotzeko prozesua hasi
                        _ = ListenForServerMessages();

                        // Konexio arrakastatsuaren ondoren Form2 erakutsi
                        if (form2 == null || form2.IsDisposed)
                        {
                            form2 = new Form2(this); // Form1 parametro gisa pasatu
                            form2.Show();
                        }
                        else
                        {
                            form2.Focus();
                        }

                        this.Hide(); // Uneko formularioa ezkutatu
                    }
                }
            }
            catch (IOException)
            {
                // Zerbitzariak bat-batean konexioa ixtean salbuespena harrapatu
                MessageBox.Show("Zerbitzaria beteta dago edo konexioa eten da. Saiatu berriro geroago.", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (SocketException)
            {
                MessageBox.Show("Ezin izan da zerbitzarira konektatu. Ziurtatu zerbitzaria martxan dagoela.", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errorea zerbitzarira konektatzean: {ex.Message}", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Programa itzaltzen...", "Informazioa", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Aplikaziotik irten
            Application.Exit();
        }
    }
}
