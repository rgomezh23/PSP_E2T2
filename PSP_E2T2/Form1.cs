using TxatServer;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PSP_E2T2
{
    public partial class Form1 : Form
    {
        public List<string> erabiltzaileak = new List<string>(); // Erabiltzaileen zerrenda gordetzeko

        public TcpClient client = null; // TCP bezeroa zerbitzarira konektatzeko
        public NetworkStream str = null; // Sareko korronte datuak trukatzeko
        public StreamReader sr = null; // Datuak irakurtzeko korrontea
        public StreamWriter sw = null; // Datuak idazteko korrontea

        private ProgramServer server; // Zerbitzariaren instantzia
        private Form2 form2; // Bigarren formularioaren instantzia
        private bool isConnected = false; // Konexio egoera adierazten du

        public event Action<string> OnMessageReceived; // Mezu bat jasotzean aktibatzen den ekitaldia

        public Form1()
        {
            InitializeComponent();
            erabiltzaileak = new List<string>(); // Erabiltzaileen zerrenda hasieratu
        }

        public bool getConnected() // Konexio egoera itzultzen du
        {
            return isConnected;
        }

        public StreamReader getStreamReader() // StreamReader instantzia itzultzen du
        {
            return sr;
        }

        public StreamWriter getStreamWriter() // StreamWriter instantzia itzultzen du
        {
            return sw;
        }

        public TcpClient GetClient() // TcpClient instantzia itzultzen du
        {
            return client;
        }

        // Gailuaren IP lokala lortzeko metodoa
        private string GetLocalIPAddress()
        {
            string localIP = string.Empty; // IP helbidea gordetzeko aldagaia
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName()); // Gailuaren host informazioa lortu
                foreach (var ip in host.AddressList) // IP helbideen zerrenda iteratu
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) // IPv4 helbidea bilatu
                    {
                        localIP = ip.ToString(); // IP helbidea gorde
                        break;
                    }
                }
            }
            catch (Exception ex) // Salbuespena harrapatu
            {
                MessageBox.Show($"Errorea IP lortzean: {ex.Message}", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error); // Errorea erakutsi
            }
            return localIP; // IP helbidea itzuli
        }

        public void AddErabiltzaile(string izena) // Erabiltzailea zerrendara gehitzeko metodoa
        {
            if (erabiltzaileak.Contains(izena)) // Izena erabilita badago
            {
                MessageBox.Show("Izena hori beste erabiltzaile batek dauka. Aukeratu beste bat.", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error); // Errorea erakutsi
            }
            else if (!string.IsNullOrEmpty(izena)) // Izena hutsik ez badago
            {
                erabiltzaileak.Add(izena); // Erabiltzailea zerrendara gehitu
            }
        }

        // Zerbitzaritik mezuak jasotzeko metodoa
        private async Task ListenForServerMessages()
        {
            try
            {
                string message;
                while ((message = await sr.ReadLineAsync()) != null) // Mezuak irakurri zerbitzaritik
                {
                    OnMessageReceived?.Invoke(message); // Mezu bat jasotzean ekitaldia aktibatu
                }
            }
            catch (Exception ex) // Salbuespena harrapatu
            {
                MessageBox.Show($"Errorea zerbitzariko mezuak jasotzean: {ex.Message}", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error); // Errorea erakutsi
            }
        }

        private async void button1_Click(object sender, EventArgs e) // "Konektatu" botoia klikatzean exekutatzen da
        {
            string izena = Microsoft.VisualBasic.Interaction.InputBox("Sartu zure izena:", "Izena"); // Erabiltzailearen izena eskatu
            if (!string.IsNullOrEmpty(izena)) // Izena hutsik ez badago
            {
                AddErabiltzaile(izena); // Erabiltzailea zerrendara gehitu
                ConnectToServer(); // Zerbitzariarekin konektatzen saiatu
            }
            else
            {
                MessageBox.Show("Izena ezin da hutsik egon.", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error); // Errorea erakutsi
            }
        }

        private async void ConnectToServer() // Zerbitzariarekin konektatzeko metodoa
        {
            if (isConnected) // Dagoeneko konektatuta badago
            {
                MessageBox.Show("Dagoeneko konektatuta zaude zerbitzarira.", "Konexioa", MessageBoxButtons.OK, MessageBoxIcon.Information); // Informazioa erakutsi
                return;
            }

            try
            {
                string serverAddress = GetLocalIPAddress(); // Zerbitzariaren IP helbidea lortu
                int serverPort = 13000; // Zerbitzariaren portua

                client = new TcpClient(); // TCP bezeroa sortu
                await client.ConnectAsync(serverAddress, serverPort); // Zerbitzariarekin konektatu

                str = client.GetStream(); // Sareko korrontea lortu
                sr = new StreamReader(str, Encoding.UTF8); // Datuak irakurtzeko korrontea sortu
                sw = new StreamWriter(str, Encoding.UTF8) { AutoFlush = true }; // Datuak idazteko korrontea sortu

                string serverMessage = await sr.ReadLineAsync(); // Zerbitzariaren mezua irakurri
                if (serverMessage == "Mesedez, sartu zure erabiltzaile izena:") // Zerbitzariak izena eskatu badu
                {
                    await sw.WriteLineAsync(erabiltzaileak.Last()); // Erabiltzailearen izena bidali zerbitzarira
                    string response = await sr.ReadLineAsync(); // Zerbitzariaren erantzuna irakurri

                    if (response == "Erabiltzaile izena dagoeneko erabilia dago. Deskonektatzen...") // Izena erabilita badago
                    {
                        MessageBox.Show("Erabiltzaile izena erabilita dago. Aukeratu beste bat.", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error); // Errorea erakutsi
                        client.Close(); // Konexioa itxi
                        return;
                    }
                    else if (response == "SERVER_FULL") // Zerbitzaria beteta badago
                    {
                        MessageBox.Show("Zerbitzaria beteta dago. Saiatu berriro geroago.", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error); // Errorea erakutsi
                        client.Close(); // Konexioa itxi
                        return;
                    }
                    else if (response.StartsWith("Bienvenido al chat,")) // Konexioa arrakastatsua bada
                    {
                        isConnected = true; // Konexio egoera eguneratu
                        MessageBox.Show("Konexioa arrakastaz ezarri da zerbitzariarekin.", "Konexioa", MessageBoxButtons.OK, MessageBoxIcon.Information); // Informazioa erakutsi

                        _ = ListenForServerMessages(); // Zerbitzariko mezuak jasotzeko prozesua hasi

                        if (form2 == null || form2.IsDisposed) // Form2 instantziatu edo berrerabili
                        {
                            form2 = new Form2(this); // Form1 parametro gisa pasatu
                            form2.Show(); // Form2 erakutsi
                        }
                        else
                        {
                            form2.Focus(); // Form2 fokuratu
                        }

                        this.Hide(); // Form1 ezkutatu
                    }
                }
            }
            catch (IOException) // Zerbitzariak konexioa itxi badu
            {
                MessageBox.Show("Zerbitzaria beteta dago edo konexioa eten da. Saiatu berriro geroago.", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error); // Errorea erakutsi
            }
            catch (SocketException) // Konexio errorea
            {
                MessageBox.Show("Ezin izan da zerbitzarira konektatu. Ziurtatu zerbitzaria martxan dagoela.", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error); // Errorea erakutsi
            }
            catch (Exception ex) // Bestelako erroreak
            {
                MessageBox.Show($"Errorea zerbitzarira konektatzean: {ex.Message}", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error); // Errorea erakutsi
            }
        }

        private async void button2_Click(object sender, EventArgs e) // "Itxi" botoia klikatzean exekutatzen da
        {
            MessageBox.Show("Programa itzaltzen...", "Informazioa", MessageBoxButtons.OK, MessageBoxIcon.Information); // Informazioa erakutsi
            Application.Exit(); // Aplikazioa itxi
        }

        private void label1_Click(object sender, EventArgs e) // Label1 klikatzean exekutatzen da
        {
            // Ez du ezer egiten
        }

        private void Form1_Load(object sender, EventArgs e) // Form1 kargatzean exekutatzen da
        {
            // Ez du ezer egiten
        }
    }
}