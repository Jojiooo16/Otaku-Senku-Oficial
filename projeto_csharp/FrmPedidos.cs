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
    public partial class FrmPedidos : Form
    {
        public FrmPedidos()
        {
            InitializeComponent();
        }

        // Lista os pedidos já salvos no banco (as vendas importadas do site)
        private void ListarPedidos()
        {
            try
            {
                Conexao banco = new Conexao();
                MySqlConnection conn = banco.ConectarBD();
                string sql = @"SELECT p.id_pedido AS id, c.nome AS cliente, p.valor_total AS total, p.status_pedido AS status
                               FROM pedidos p
                               INNER JOIN clientes c ON c.id_cliente = p.id_cliente
                               ORDER BY p.id_pedido DESC";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvPedidos.DataSource = dt;
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao buscar pedidos: " + ex.Message);
            }
        }

        private void FrmPedidos_Load(object sender, EventArgs e)
        {
            ListarPedidos();
        }
    }
}
