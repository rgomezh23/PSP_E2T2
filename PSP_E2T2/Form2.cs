using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PSP_E2T2
{
    public partial class Form2 : Form
    {
        // Lista para almacenar los mensajes (simulando un chat)
        private List<string> mensajes;

        public Form2()
        {
            InitializeComponent();
            mensajes = new List<string>();  // Lista para los mensajes
        }

        // Evento de clic en el botón de enviar mensaje
        private void button1_Click(object sender, EventArgs e)
        {
            //string mensaje = textBox2.Text.Trim();
            
            //if (!string.IsNullOrEmpty(mensaje))
            //{
            //    // Agregar el mensaje a la lista de mensajes
            //    mensajes.Add("Tú: " + mensaje);

            //    // Mostrar los mensajes en el TextBox (historial de chat)
            //    //textBox1.Text = string.Join(Environment.NewLine, mensajes);

            //    // Limpiar el TextBox donde el usuario escribe
            //    //textBox2.Clear();
            //}
        }
    }
}
