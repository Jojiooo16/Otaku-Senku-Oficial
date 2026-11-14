using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace projeto_delivery_2D1
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnentrar_Click(object sender, EventArgs e)
        {
            try
            {
                
                Conexao banco = new Conexao();
                MySqlConnection conexaoAberta = banco.ConectarBD();
                string sql = "SELECT * FROM usuarios WHERE email = @email AND senha = @senha";

                MySqlCommand cmd = new MySqlCommand(sql, conexaoAberta);
                cmd.Parameters.AddWithValue("@email", txtemail.Text);
                cmd.Parameters.AddWithValue("@senha", txtsenha.Text);

                MySqlDataReader leitor = cmd.ExecuteReader();

                if (leitor.Read())
                {
                    Form1 telaPrincipal = new Form1();
                    telaPrincipal.Show();
                    this.Hide();

                }
                else
                {
                    MessageBox.Show("E-mail ou senha incorretos!");

                }
                conexaoAberta.Close();

            }
            catch (Exception)
            {

                MessageBox.Show("erro conexão não estabelecida.");
            }
        }
    }
}
