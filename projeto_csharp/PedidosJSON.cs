using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace projeto_delivery_2D1
{
    // Classe que representa o cliente do pedido
    public class ClienteJson
    {
        [JsonPropertyName("nome")]
        public string Nome { get; set; }
    }

    // Classe que representa um item do pedido
    public class ItemJson
    {
        [JsonPropertyName("produto")]
        public string Produto { get; set; }

        [JsonPropertyName("quantidade")]
        public int Quantidade { get; set; }

        [JsonPropertyName("preco")]
        public decimal Preco { get; set; }
    }

    // Classe que representa o pedido completo
    public class PedidoJson
    {
        [JsonPropertyName("pedidoId")]
        public string PedidoId { get; set; }

        [JsonPropertyName("cliente")]
        public ClienteJson Cliente { get; set; }

        [JsonPropertyName("total")]
        public decimal Total { get; set; }

        [JsonPropertyName("itens")]
        public List<ItemJson> Itens { get; set; }
    }

    public partial class FrmPedidos : Form
    {
        private void btnImportar_Click(object sender, EventArgs e)
        {
            try
            {
                // Caminho onde o site salva o arquivo (por padrão, a pasta Downloads).
                // Se o arquivo não estiver lá, o usuário escolhe onde ele está.
                string caminhoArquivo = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Downloads",
                    "pedido.json"
                );

                if (!File.Exists(caminhoArquivo))
                {
                    using (OpenFileDialog dialogo = new OpenFileDialog())
                    {
                        dialogo.Title = "Selecione o arquivo JSON do pedido";
                        dialogo.Filter = "Arquivo JSON (*.json)|*.json";

                        if (dialogo.ShowDialog() != DialogResult.OK)
                        {
                            return;
                        }

                        caminhoArquivo = dialogo.FileName;
                    }
                }

                // Lê todo o conteúdo do arquivo JSON
                string textoJson = File.ReadAllText(caminhoArquivo);

                // Converte o JSON em um objeto da classe PedidoJson
                JsonSerializerOptions opcoes =
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                PedidoJson pedidoDoSite =
                    JsonSerializer.Deserialize<PedidoJson>(
                        textoJson,
                        opcoes
                    );

                // Verifica se o pedido foi lido corretamente
                if (pedidoDoSite == null)
                {
                    MessageBox.Show(
                        "Não foi possível ler o pedido.",
                        "Aviso",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    return;
                }

                if (pedidoDoSite.Cliente == null)
                {
                    MessageBox.Show(
                        "O pedido não possui dados do cliente.",
                        "Aviso",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    return;
                }

                Conexao banco = new Conexao();

                using (MySqlConnection conn = banco.ConectarBD())
                using (MySqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int idCliente;
                        string sqlCliente =
                            "SELECT id_cliente FROM clientes WHERE nome = @nome LIMIT 1";

                        using (MySqlCommand cmdCliente =
                            new MySqlCommand(sqlCliente, conn, transaction))
                        {
                            cmdCliente.Parameters.AddWithValue(
                                "@nome",
                                pedidoDoSite.Cliente.Nome
                            );

                            object resultadoCliente = cmdCliente.ExecuteScalar();

                            if (resultadoCliente == null || resultadoCliente == DBNull.Value)
                            {
                                string sqlNovoCliente =
                                    "INSERT INTO clientes (nome) VALUES (@nome)";

                                using (MySqlCommand cmdNovoCliente =
                                    new MySqlCommand(sqlNovoCliente, conn, transaction))
                                {
                                    cmdNovoCliente.Parameters.AddWithValue(
                                        "@nome",
                                        pedidoDoSite.Cliente.Nome
                                    );
                                    cmdNovoCliente.ExecuteNonQuery();
                                    idCliente = Convert.ToInt32(cmdNovoCliente.LastInsertedId);
                                }
                            }
                            else
                            {
                                idCliente = Convert.ToInt32(resultadoCliente);
                            }
                        }

                        string sqlPedido = @"
                            INSERT INTO pedidos
                            (
                                id_cliente,
                                valor_total,
                                status_pedido
                            )
                            VALUES
                            (
                                @id_cliente,
                                @total,
                                'Carrinho'
                            )";

                        int idPedido;
                        using (MySqlCommand cmdPedido =
                            new MySqlCommand(sqlPedido, conn, transaction))
                        {
                            cmdPedido.Parameters.AddWithValue("@id_cliente", idCliente);
                            cmdPedido.Parameters.AddWithValue("@total", pedidoDoSite.Total);
                            cmdPedido.ExecuteNonQuery();
                            idPedido = Convert.ToInt32(cmdPedido.LastInsertedId);
                        }

                        foreach (ItemJson item in pedidoDoSite.Itens ?? new List<ItemJson>())
                        {
                            if (item == null)
                            {
                                throw new Exception("O pedido contém um item inválido.");
                            }

                            string sqlProduto =
                                "SELECT id_produto FROM produtos WHERE nome = @nome LIMIT 1";

                            int idProduto;
                            using (MySqlCommand cmdProduto =
                                new MySqlCommand(sqlProduto, conn, transaction))
                            {
                                cmdProduto.Parameters.AddWithValue("@nome", item.Produto);
                                object resultadoProduto = cmdProduto.ExecuteScalar();

                                if (resultadoProduto == null || resultadoProduto == DBNull.Value)
                                {
                                    throw new Exception(
                                        "O produto '" + item.Produto + "' não foi encontrado no banco."
                                    );
                                }

                                idProduto = Convert.ToInt32(resultadoProduto);
                            }

                            string sqlItem = @"
                                INSERT INTO itens_pedido
                                (
                                    id_pedido,
                                    id_produto,
                                    quantidade,
                                    preco_unitario
                                )
                                VALUES
                                (
                                    @id_pedido,
                                    @id_produto,
                                    @quantidade,
                                    @preco_unitario
                                )";

                            using (MySqlCommand cmdItem =
                                new MySqlCommand(sqlItem, conn, transaction))
                            {
                                cmdItem.Parameters.AddWithValue("@id_pedido", idPedido);
                                cmdItem.Parameters.AddWithValue("@id_produto", idProduto);
                                cmdItem.Parameters.AddWithValue("@quantidade", item.Quantidade);
                                cmdItem.Parameters.AddWithValue("@preco_unitario", item.Preco);
                                cmdItem.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }

                // Informa que deu certo
                MessageBox.Show(
                    "Pedido " + pedidoDoSite.PedidoId +
                    " importado com sucesso!",
                    "Sucesso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                // Atualiza a tabela da tela com o pedido que acabou de entrar
                ListarPedidos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao importar o pedido: " + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
