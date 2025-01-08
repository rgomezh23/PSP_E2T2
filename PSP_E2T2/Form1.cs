using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PSP_E2T2
{
    public partial class Form1 : Form
    {
        // Lista para almacenar los nombres de los chats
        private List<string> chats;

        public Form1()
        {
            InitializeComponent();
            // Inicializar la lista de chats
            chats = new List<string>();
        }

        // Este método se llama cuando se selecciona un chat del ComboBox
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Aquí puedes manejar lo que pasa cuando se selecciona un chat (por ejemplo, cargar el contenido del chat)
        }

        // Este método se llama cuando se hace clic en el botón "Nuevo Chat"
        private void button1_Click(object sender, EventArgs e)
        {
            // Puedes solicitar un nombre para el nuevo chat
            string nuevoChat = Microsoft.VisualBasic.Interaction.InputBox("Ingrese el nombre del nuevo chat:", "Nuevo Chat");

            if (!string.IsNullOrEmpty(nuevoChat))
            {
                chats.Add(nuevoChat);

                comboBox1.Items.Add(nuevoChat);
                
                comboBox1.SelectedItem = nuevoChat;
            }
            else
            {
                MessageBox.Show("El nombre del chat no puede estar vacío.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
