using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PSP_E2T2
{
    public partial class Form1 : Form
    {
        private List<string> chats;

        public Form1()
        {
            InitializeComponent();
            chats = new List<string>();
        }

        public void AddChat(string chat)
        {
            if (!string.IsNullOrEmpty(chat) && !chats.Contains(chat))
            {
                chats.Add(chat);
                comboBox1.Items.Add(chat);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedChat = comboBox1.SelectedItem.ToString();
            MessageBox.Show($"Has seleccionado el chat: {selectedChat}", "Chat Seleccionado");

            // Instantziatu form informazioa gordetzeko.
            Form2 form2 = new Form2(this); 
            form2.Show();

            this.Hide(); 
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string nuevoChat = Microsoft.VisualBasic.Interaction.InputBox("Ingrese el nombre del nuevo chat:", "Nuevo Chat");

            if (!string.IsNullOrEmpty(nuevoChat))
            {
                AddChat(nuevoChat);
                comboBox1.SelectedItem = nuevoChat;
            }
            else
            {
                MessageBox.Show("El nombre del chat no puede estar vacío.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
