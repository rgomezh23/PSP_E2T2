using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static System.Windows.Forms.DataFormats;

namespace PSP_E2T2
{
    public partial class Form2 : Form
    {
        private Form1 form1; 
        private List<string> mensajes;


        public Form2(Form1 form1)
        {
            InitializeComponent();
            this.form1 = form1; 
            mensajes = new List<string>();
        }

        public async void ReceiveMessages()
        {
            try
            {
                while (true)
                {
                    // Leer el mensaje del servidor
                    string mensaje = await form1.sr.ReadLineAsync();

                    if (!string.IsNullOrEmpty(mensaje))
                    {
                        // Agregar el mensaje al richTextBox1
                        richTextBox1.Invoke((MethodInvoker)(() =>
                        {
                            richTextBox1.AppendText(mensaje + "\n");
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al recibir mensajes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button1_Click_1(object sender, EventArgs e)
{
        if (string.IsNullOrWhiteSpace(richTextBox2.Text))
        {
            MessageBox.Show("El mensaje no puede estar vacío.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Obtener el nombre del usuario
        int listaTamaina = form1.erabiltzaileak.Count;
        string izena = form1.erabiltzaileak[listaTamaina - 1]; // Último usuario agregado
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

        private async void button2_Click(object sender, EventArgs e)
        {
            // form1 originala
            form1.Show();

            this.Hide();
        }
    }
}
