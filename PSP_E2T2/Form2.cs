using System;
using System.Collections.Generic;
using System.Windows.Forms;

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

        private void button1_Click_1(object sender, EventArgs e)
        {
        
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // form1 originala
            form1.Show();

            this.Hide();
        }
    }
}
