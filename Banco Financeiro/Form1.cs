using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace SimuladorBancario
{
    public partial class Form1 : Form
    {
        private string connectionString = "Data Source=banco_financeiro.db;Version=3;";

        // --- DECLARAÇÃO DOS COMPONENTES ---
        private DataGridView dgvContas;
        private TextBox txtNome, txtAgencia, txtConta, txtSaldo;
        private Button btnSalvar, btnAtualizar, btnExcluir, btnDeposito, btnTransferir;
        private Label lblNome, lblAgencia, lblConta, lblSaldo;

        public Form1()
        {
            InitializeComponent(); // Agora ele vai existir abaixo
            ConfigurarInterface();
            CriarBancoDeDados();
            AtualizarGrid();
        }

        // --- MÉTODO QUE CRIA OS BOTÕES E CAMPOS NA TELA ---
        private void InitializeComponent()
        {
            this.dgvContas = new DataGridView();
            this.txtNome = new TextBox();
            this.txtAgencia = new TextBox();
            this.txtConta = new TextBox();
            this.txtSaldo = new TextBox();
            this.btnSalvar = new Button();
            this.btnAtualizar = new Button();
            this.btnExcluir = new Button();
            this.btnDeposito = new Button();
            this.btnTransferir = new Button();
            
            // Labels
            this.lblNome = new Label { Text = "Nome:", Top = 10, Left = 10, Width = 50 };
            this.lblAgencia = new Label { Text = "Agência:", Top = 40, Left = 10, Width = 60 };
            this.lblConta = new Label { Text = "Conta:", Top = 70, Left = 10, Width = 50 };
            this.lblSaldo = new Label { Text = "Saldo:", Top = 100, Left = 10, Width = 50 };

            // Posicionamento dos Campos
            txtNome.SetBounds(80, 10, 200, 25);
            txtAgencia.SetBounds(80, 40, 100, 25);
            txtConta.SetBounds(80, 70, 100, 25);
            txtSaldo.SetBounds(80, 100, 100, 25);

            // Botões do CRUD
            btnSalvar.Text = "Salvar"; btnSalvar.SetBounds(300, 10, 100, 30);
            btnAtualizar.Text = "Atualizar"; btnAtualizar.SetBounds(300, 45, 100, 30);
            btnExcluir.Text = "Excluir"; btnExcluir.SetBounds(300, 80, 100, 30);

            // Botões de Operação
            btnDeposito.Text = "Depósito"; btnDeposito.SetBounds(420, 10, 100, 30);
            btnTransferir.Text = "Transferir"; btnTransferir.SetBounds(420, 45, 100, 30);

            // Grid
            dgvContas.SetBounds(10, 150, 520, 200);
            dgvContas.SelectionChanged += dgvContas_SelectionChanged;

            // Eventos dos Botões
            btnSalvar.Click += btnSalvar_Click;
            btnAtualizar.Click += btnAtualizar_Click;
            btnExcluir.Click += btnExcluir_Click;
            btnDeposito.Click += btnDeposito_Click;
            btnTransferir.Click += btnTransferir_Click;

            // Configuração do Formulário
            this.ClientSize = new Size(550, 370);
            this.Controls.AddRange(new Control[] { 
                dgvContas, txtNome, txtAgencia, txtConta, txtSaldo, 
                btnSalvar, btnAtualizar, btnExcluir, btnDeposito, btnTransferir,
                lblNome, lblAgencia, lblConta, lblSaldo 
            });
        }

        private void ConfigurarInterface()
        {
            this.Text = "Sistema Bancário Pro";
            this.BackColor = Color.Black;
            this.ForeColor = Color.Cyan;
            this.Font = new Font("Segoe UI", 9, FontStyle.Bold);

            dgvContas.BackgroundColor = Color.FromArgb(20, 20, 20);
            dgvContas.DefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
            dgvContas.DefaultCellStyle.ForeColor = Color.Cyan;
            dgvContas.DefaultCellStyle.SelectionBackColor = Color.Cyan;
            dgvContas.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvContas.GridColor = Color.Cyan;
            dgvContas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvContas.MultiSelect = false;
            dgvContas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            foreach (Control c in this.Controls) {
                if (c is Button) {
                    c.BackColor = Color.FromArgb(40, 40, 40);
                    c.ForeColor = Color.Cyan;
                }
            }
        }

        private void CriarBancoDeDados()
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = @"CREATE TABLE IF NOT EXISTS Contas (
                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                Nome TEXT,
                                Agencia TEXT,
                                NumeroConta TEXT,
                                Saldo REAL)";
                using (var cmd = new SQLiteCommand(sql, conn)) { cmd.ExecuteNonQuery(); }
            }
        }

        private void AtualizarGrid()
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                SQLiteDataAdapter da = new SQLiteDataAdapter("SELECT * FROM Contas", conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvContas.DataSource = dt;

                // Formata a coluna "Saldo" para o padrão de moeda local (R$)
                dgvContas.Columns["Saldo"].DefaultCellStyle.Format = "C2"; 
            }
        }

        private void btnSalvar_Click(object sender, EventArgs e)
        {
            if (ValidarCampos())
            {
                ExecutarComando("INSERT INTO Contas (Nome, Agencia, NumeroConta, Saldo) VALUES (@nome, @ag, @num, @saldo)", 
                    cmd => {
                        cmd.Parameters.AddWithValue("@nome", txtNome.Text);
                        cmd.Parameters.AddWithValue("@ag", txtAgencia.Text);
                        cmd.Parameters.AddWithValue("@num", txtConta.Text);
                        cmd.Parameters.AddWithValue("@saldo", double.Parse(txtSaldo.Text));
                    });
                AtualizarGrid(); LimparCampos();
            }
        }

        private void btnAtualizar_Click(object sender, EventArgs e)
        {
            if (dgvContas.SelectedRows.Count > 0 && ValidarCampos())
            {
                string id = dgvContas.SelectedRows[0].Cells["Id"].Value.ToString();
                ExecutarComando(@"UPDATE Contas SET Nome=@nome, Agencia=@ag, NumeroConta=@num, Saldo=@saldo WHERE Id=@id",
                    cmd => {
                        cmd.Parameters.AddWithValue("@nome", txtNome.Text);
                        cmd.Parameters.AddWithValue("@ag", txtAgencia.Text);
                        cmd.Parameters.AddWithValue("@num", txtConta.Text);
                        cmd.Parameters.AddWithValue("@saldo", double.Parse(txtSaldo.Text));
                        cmd.Parameters.AddWithValue("@id", id);
                    });
                AtualizarGrid(); LimparCampos();
            }
        }

        private void btnExcluir_Click(object sender, EventArgs e)
        {
            if (dgvContas.SelectedRows.Count > 0)
            {
                string id = dgvContas.SelectedRows[0].Cells["Id"].Value.ToString();
                if (MessageBox.Show("Excluir conta?", "Aviso", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    ExecutarComando("DELETE FROM Contas WHERE Id = @id", cmd => cmd.Parameters.AddWithValue("@id", id));
                    AtualizarGrid(); LimparCampos();
                }
            }
        }

        private void btnDeposito_Click(object sender, EventArgs e)
        {
            if (dgvContas.SelectedRows.Count > 0)
            {
                string id = dgvContas.SelectedRows[0].Cells["Id"].Value.ToString();
                string input = Microsoft.VisualBasic.Interaction.InputBox("Valor:", "Depósito");
                if (double.TryParse(input, out double valor))
                {
                    ExecutarComando("UPDATE Contas SET Saldo = Saldo + @v WHERE Id = @id", cmd => {
                        cmd.Parameters.AddWithValue("@v", valor);
                        cmd.Parameters.AddWithValue("@id", id);
                    });
                    AtualizarGrid();
                }
            }
        }

        private void btnTransferir_Click(object sender, EventArgs e)
        {
            if (dgvContas.SelectedRows.Count > 0)
            {
                int idRemetente = Convert.ToInt32(dgvContas.SelectedRows[0].Cells["Id"].Value);
                double saldoRemetente = Convert.ToDouble(dgvContas.SelectedRows[0].Cells["Saldo"].Value);
                string nomeDestino = Microsoft.VisualBasic.Interaction.InputBox("Para quem?", "Transferir");
                string valorInput = Microsoft.VisualBasic.Interaction.InputBox("Valor:", "Transferir");

                if (double.TryParse(valorInput, out double valor) && valor <= saldoRemetente)
                {
                    using (var conn = new SQLiteConnection(connectionString))
                    {
                        conn.Open();
                        using (var trans = conn.BeginTransaction())
                        {
                            try {
                                new SQLiteCommand($"UPDATE Contas SET Saldo = Saldo - {valor.ToString().Replace(',', '.')} WHERE Id = {idRemetente}", conn).ExecuteNonQuery();
                                int rows = new SQLiteCommand($"UPDATE Contas SET Saldo = Saldo + {valor.ToString().Replace(',', '.')} WHERE Nome = '{nomeDestino}'", conn).ExecuteNonQuery();
                                if (rows > 0) { trans.Commit(); MessageBox.Show("Sucesso!"); }
                                else { trans.Rollback(); MessageBox.Show("Destinatário não encontrado."); }
                            } catch { trans.Rollback(); }
                        }
                    }
                    AtualizarGrid();
                }
            }
        }

        private void dgvContas_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvContas.SelectedRows.Count > 0)
            {
                txtNome.Text = dgvContas.SelectedRows[0].Cells["Nome"].Value.ToString();
                txtAgencia.Text = dgvContas.SelectedRows[0].Cells["Agencia"].Value.ToString();
                txtConta.Text = dgvContas.SelectedRows[0].Cells["NumeroConta"].Value.ToString();
                txtSaldo.Text = dgvContas.SelectedRows[0].Cells["Saldo"].Value.ToString();
            }
        }

        private void ExecutarComando(string sql, Action<SQLiteCommand> preparar)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(sql, conn)) { preparar(cmd); cmd.ExecuteNonQuery(); }
            }
        }

        private bool ValidarCampos() => !string.IsNullOrWhiteSpace(txtNome.Text) && double.TryParse(txtSaldo.Text, out _);

        private void LimparCampos() { txtNome.Clear(); txtAgencia.Clear(); txtConta.Clear(); txtSaldo.Clear(); }
    }
}