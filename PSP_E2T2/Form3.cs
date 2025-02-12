using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PSP_E2T2
{
    public partial class Form3 : Form
    {
        // HTTP bezeroa definitzen da API eskaerak egiteko.
        private HttpClient client = new HttpClient();

        public Form3()
        {
            InitializeComponent();
            // Botoiaren klik-ekintza asinkronoki exekutatzen da.
            button1.Click += async (sender, e) => await FetchAppointments();
        }

        private async Task FetchAppointments()
        {
            // Data aukeratzen da DateTimePicker bidez.
            string date = dateTimePicker1.Value.ToString("yyyy-MM-dd");
            // APIaren URLa osatzen da dataren arabera.
            string url = $"http://localhost:8080/hitzorduak/datarenHitzorduak?date={date}";

            try
            {
                // HTTP eskaera egiten da.
                HttpResponseMessage response = await client.GetAsync(url);

                // Erantzuna edukirik gabekoa bada, mezua erakusten da eta DataGridView-a garbitzen da.
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    MessageBox.Show("Ez dago hitzordurik data horretarako.", "Informazioa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dataGridView1.DataSource = null;
                    return;
                }

                // Erantzuna ondo jaso dela ziurtatzen da.
                response.EnsureSuccessStatusCode();
                // JSON erantzuna irakurri eta deserializatzen da.
                string jsonResponse = await response.Content.ReadAsStringAsync();
                var appointments = JsonSerializer.Deserialize<List<Appointment>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Hitzorduak DataGridView-an bistaratzen dira.
                dataGridView1.DataSource = appointments;
            }
            catch (Exception ex)
            {
                // Errorea gertatuz gero, mezua erakusten da.
                MessageBox.Show($"Errorea datuak eskuratzean: {ex.Message}", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // Appointment klasea definitzen da, API erantzunaren datuak gordetzeko
    public class Appointment
    {
        public int Id { get; set; } // Hitzorduaren identifikadorea
        public int Eserlekua { get; set; } // Eserlekuaren zenbakia
        public int Id_Langilea { get; set; } // Langilearen identifikadorea
        public string Data { get; set; } // Hitzorduaren data
        public string Hasiera_Ordua { get; set; } // Hitzorduaren hasiera-ordua
        public string Amaiera_Ordua { get; set; } // Hitzorduaren amaiera-ordua
        public string Izena { get; set; } // Bezeroaren izena
        public string Telefonoa { get; set; } // Bezeroaren telefono zenbakia
        public string Deskribapena { get; set; } // Hitzorduaren deskribapena
        public string Etxekoa { get; set; } // Etxeko zerbitzua den ala ez
        public decimal Prezio_Totala { get; set; } // Hitzorduaren prezio totala
    }
}
