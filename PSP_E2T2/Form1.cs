namespace PSP_E2T2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // L�gica que quieres ejecutar cuando el texto cambie en textBox1
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            // L�gica que quieres ejecutar cuando el texto cambie en textBox1
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string usuario = "prueba";
            string contrase�a = "1234";

            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Por favor, introduce tu nombre de usuario para continuar.", "Usuario requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (string.IsNullOrEmpty(textBox2.Text))
            {
                MessageBox.Show("Por favor, introduce tu contrase�a para continuar.", "Contrase�a requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (usuario == textBox1.Text && contrase�a == textBox2.Text)
            {
                MessageBox.Show($"�Bienvenido, {usuario}!\nHas iniciado sesi�n correctamente.", "Inicio de sesi�n exitoso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Mostrar el nuevo formulario
                Form2 nuevoFormulario = new Form2();
                this.Hide(); // Ocultar el formulario actual
                nuevoFormulario.ShowDialog(); // Mostrar el nuevo formulario como modal

                // Volver a mostrar Form1 si es necesario
                this.Show();
            }
            else
            {
                MessageBox.Show("El nombre de usuario o la contrase�a que ingresaste no son correctos. Por favor, verifica tus datos e int�ntalo nuevamente.", "Error de inicio de sesi�n", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
