using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace projeto_delivery_2D1
{
    // 12501190 & 12502545
    // Giovanna Torquato Pedrosa & Sofia Izabela
    // 2D1 
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        private int ObterIdAnime(MySqlConnection conexao, string nomeAnime)
        {
            string sql = "SELECT id_anime FROM animes WHERE nome = @nome";
            MySqlCommand comando = new MySqlCommand(sql, conexao);
            comando.Parameters.AddWithValue("@nome", nomeAnime);
            object resultado = comando.ExecuteScalar();

            if (resultado == null || resultado == DBNull.Value)
            {
                throw new Exception("Selecione um anime válido.");
            }

            return Convert.ToInt32(resultado);
        }

        private void ListarProdutos()
        {
            try
            {
                Conexao banco = new Conexao();
                MySqlConnection conn = banco.ConectarBD();
                string sql = @"SELECT p.id_produto AS id, p.nome, p.preco, a.nome AS categoria
                               FROM produtos p
                               INNER JOIN animes a ON a.id_anime = p.id_anime";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt); // O caminhão descarrega na caixa
                dgvProdutos.DataSource = dt;
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao buscar produtos: " + ex.Message);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ListarProdutos();
        }

        private void btncadastrar_Click(object sender, EventArgs e)
        {
            try
            {
                produto objproduto = new produto();
                objproduto.Nome = txtnome.Text;
                objproduto.preco = double.Parse(txtproduto.Text.Replace(".", ","));
                objproduto.categoria = cmbcategoria.Text;

                Conexao banco = new Conexao();
                MySqlConnection conexaoaberta = banco.ConectarBD();
                int idAnime = ObterIdAnime(conexaoaberta, objproduto.categoria);

                string sql = "INSERT INTO produtos (id_anime, nome, preco) VALUES (@id_anime, @nome, @preco)";

                MySqlCommand comando = new MySqlCommand(sql, conexaoaberta);
                comando.Parameters.AddWithValue("@id_anime", idAnime);
                comando.Parameters.AddWithValue("@nome", objproduto.Nome);
                comando.Parameters.AddWithValue("@preco", objproduto.preco);

                comando.ExecuteNonQuery();

                MessageBox.Show("Cadastro Realizado com Sucesso!!!");

                txtnome.Clear();
                txtproduto.Clear();
                cmbcategoria.SelectedIndex = -1;
                conexaoaberta.Close();
                ListarProdutos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: não realizado. \n{ex.Message}");

            }
        }

        private void dgvProdutos_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dgvProdutos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow linha = dgvProdutos.Rows[e.RowIndex];
                textID.Text = linha.Cells["id"].Value.ToString();
                txtnome.Text = linha.Cells["nome"].Value.ToString();
                txtproduto.Text = linha.Cells["preco"].Value.ToString();
                cmbcategoria.Text = linha.Cells["categoria"].Value.ToString();
            }

        }

        private void btnatualizar_Click(object sender, EventArgs e)
        {
            if (textID.Text == "")
            {
                MessageBox.Show("Selecione um produto na lista primeiro!");
                return;
            }
            try
            {
                Conexao banco = new Conexao();
                MySqlConnection conexaoAberta = banco.ConectarBD();
                int idAnime = ObterIdAnime(conexaoAberta, cmbcategoria.Text);
                string sql = "UPDATE produtos SET nome = @nome, preco = @preco, id_anime = @id_anime WHERE id_produto = @id";
                MySqlCommand cmd = new MySqlCommand(sql, conexaoAberta);

                cmd.Parameters.AddWithValue("@nome", txtnome.Text);
                cmd.Parameters.AddWithValue("@preco", double.Parse(txtproduto.Text));
                cmd.Parameters.AddWithValue("@id_anime", idAnime);
                cmd.Parameters.AddWithValue("@id", int.Parse(textID.Text));

                cmd.ExecuteNonQuery();
                conexaoAberta.Close();

                MessageBox.Show("Produto atualizado com sucesso!");
                ListarProdutos();


            }
            catch (Exception ex)
            {
                MessageBox.Show("erro conexão não estabelecida.");
            }

        }

        private void btnpedidos_Click(object sender, EventArgs e)
        {
            FrmPedidos telaPedidos = new FrmPedidos();
            telaPedidos.Show();
        }

        private void btnexcluir_Click(object sender, EventArgs e)
        {

            if (textID.Text == "")
            {
                MessageBox.Show("Selecione um produto na lista primeiro para excluir!");
                return;
            }

            DialogResult resposta = MessageBox.Show("Tem certeza que deseja excluir este produto?", "Atenção", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (resposta == DialogResult.Yes)
            {

                try
                {
                    Conexao banco = new Conexao();
                    MySqlConnection conexaoAberta = banco.ConectarBD();
                    string sql = "DELETE FROM produtos WHERE id_produto = @id";
                    MySqlCommand cmd = new MySqlCommand(sql, conexaoAberta);
                    cmd.Parameters.AddWithValue("@id", Convert.ToInt32(textID.Text));
                    cmd.ExecuteNonQuery();
                    conexaoAberta.Close();

                    MessageBox.Show("Produto excluído!");
                    textID.Clear();
                    // ... (Limpar as outras caixas)
                    ListarProdutos();

                }
                catch (Exception)
                {

                    MessageBox.Show("erro conexão não estabelecida.");
                }


            }
        }
    }
}
