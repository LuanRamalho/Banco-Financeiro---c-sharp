using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace SimuladorBancario
{
    // Classe de Modelo sem o campo Id
    public class Conta
    {
        public string Nome { get; set; }
        public string Agencia { get; set; }
        public string NumeroConta { get; set; }
        public double Saldo { get; set; }
    }

    public partial class Form1 : Form
    {
        private string jsonFilePath = "banco_financeiro.json";
        private List<Conta> listaContas = new List<Conta>();

        // --- DECLARAÇÃO DOS COMPONENTES ---
        private DataGridView dgvContas;
        private TextBox txtNome, txtAgencia, txtConta, txtSaldo;
        private Button btnSalvar, btnAtualizar, btnExcluir, btnDeposito, btnTransferir;
        private Label lblNome, lblAgencia, lblConta, lblSaldo;

        public Form1()
        {
            InitializeComponent();
            ConfigurarInterface();
            CarregarDados();
            AtualizarGrid();
        }

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
            
            this.lblNome = new Label { Text = "Nome:", Top = 10, Left = 10, Width = 50 };
            this.lblAgencia = new Label { Text = "Agência:", Top = 40, Left = 10, Width = 60 };
            this.lblConta = new Label { Text = "Conta:", Top = 70, Left = 10, Width = 50 };
            this.lblSaldo = new Label { Text = "Saldo:", Top = 100, Left = 10, Width = 50 };

            txtNome.SetBounds(80, 10, 200, 25);
            txtAgencia.SetBounds(80, 40, 100, 25);
            txtConta.SetBounds(80, 70, 100, 25);
            txtSaldo.SetBounds(80, 100, 100, 25);

            btnSalvar.Text = "Salvar"; btnSalvar.SetBounds(300, 10, 100, 30);
            btnAtualizar.Text = "Atualizar"; btnAtualizar.SetBounds(300, 45, 100, 30);
            btnExcluir.Text = "Excluir"; btnExcluir.SetBounds(300, 80, 100, 30);

            btnDeposito.Text = "Depósito"; btnDeposito.SetBounds(420, 10, 100, 30);
            btnTransferir.Text = "Transferir"; btnTransferir.SetBounds(420, 45, 100, 30);

            dgvContas.SetBounds(10, 150, 520, 200);
            dgvContas.SelectionChanged += dgvContas_SelectionChanged;

            btnSalvar.Click += btnSalvar_Click;
            btnAtualizar.Click += btnAtualizar_Click;
            btnExcluir.Click += btnExcluir_Click;
            btnDeposito.Click += btnDeposito_Click;
            btnTransferir.Click += btnTransferir_Click;

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

        private void CarregarDados()
        {
            if (File.Exists(jsonFilePath))
            {
                string jsonString = File.ReadAllText(jsonFilePath);
                listaContas = JsonSerializer.Deserialize<List<Conta>>(jsonString) ?? new List<Conta>();
            }
        }

        private void SalvarDados()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(listaContas, options);
            File.WriteAllText(jsonFilePath, jsonString);
        }

        private void AtualizarGrid()
        {
            dgvContas.DataSource = null;
            dgvContas.DataSource = listaContas.ToList();
            if (dgvContas.Columns["Saldo"] != null)
                dgvContas.Columns["Saldo"].DefaultCellStyle.Format = "C2";
        }

        private void btnSalvar_Click(object sender, EventArgs e)
        {
            if (ValidarCampos())
            {
                var novaConta = new Conta
                {
                    Nome = txtNome.Text,
                    Agencia = txtAgencia.Text,
                    NumeroConta = txtConta.Text,
                    Saldo = double.Parse(txtSaldo.Text)
                };

                listaContas.Add(novaConta);
                SalvarDados();
                AtualizarGrid(); 
                LimparCampos();
            }
        }

        private void btnAtualizar_Click(object sender, EventArgs e)
        {
            if (dgvContas.SelectedRows.Count > 0 && ValidarCampos())
            {
                // Como não há ID, usamos o número da conta original da linha selecionada para busca
                string contaOriginal = dgvContas.SelectedRows[0].Cells["NumeroConta"].Value.ToString();
                var conta = listaContas.FirstOrDefault(c => c.NumeroConta == contaOriginal);
                
                if (conta != null)
                {
                    conta.Nome = txtNome.Text;
                    conta.Agencia = txtAgencia.Text;
                    conta.NumeroConta = txtConta.Text;
                    conta.Saldo = double.Parse(txtSaldo.Text);

                    SalvarDados();
                    AtualizarGrid();
                    LimparCampos();
                }
            }
        }

        private void btnExcluir_Click(object sender, EventArgs e)
        {
            if (dgvContas.SelectedRows.Count > 0)
            {
                string contaNum = dgvContas.SelectedRows[0].Cells["NumeroConta"].Value.ToString();
                if (MessageBox.Show("Excluir conta?", "Aviso", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    listaContas.RemoveAll(c => c.NumeroConta == contaNum);
                    SalvarDados();
                    AtualizarGrid();
                    LimparCampos();
                }
            }
        }

        private void btnDeposito_Click(object sender, EventArgs e)
        {
            if (dgvContas.SelectedRows.Count > 0)
            {
                string contaNum = dgvContas.SelectedRows[0].Cells["NumeroConta"].Value.ToString();
                string input = Microsoft.VisualBasic.Interaction.InputBox("Valor:", "Depósito");
                
                if (double.TryParse(input, out double valor))
                {
                    var conta = listaContas.FirstOrDefault(c => c.NumeroConta == contaNum);
                    if (conta != null)
                    {
                        conta.Saldo += valor;
                        SalvarDados();
                        AtualizarGrid();
                    }
                }
            }
        }

        private void btnTransferir_Click(object sender, EventArgs e)
        {
            if (dgvContas.SelectedRows.Count > 0)
            {
                string contaRemetente = dgvContas.SelectedRows[0].Cells["NumeroConta"].Value.ToString();
                var remetente = listaContas.FirstOrDefault(c => c.NumeroConta == contaRemetente);

                string nomeDestino = Microsoft.VisualBasic.Interaction.InputBox("Nome exato do destinatário:", "Transferir");
                string valorInput = Microsoft.VisualBasic.Interaction.InputBox("Valor:", "Transferir");

                if (double.TryParse(valorInput, out double valor) && remetente != null && valor <= remetente.Saldo)
                {
                    var destino = listaContas.FirstOrDefault(c => c.Nome.Equals(nomeDestino, StringComparison.OrdinalIgnoreCase));

                    if (destino != null)
                    {
                        remetente.Saldo -= valor;
                        destino.Saldo += valor;
                        SalvarDados();
                        MessageBox.Show("Transferência realizada!");
                    }
                    else { MessageBox.Show("Destinatário não encontrado."); }
                    AtualizarGrid();
                }
                else { MessageBox.Show("Saldo insuficiente ou dados inválidos."); }
            }
        }

        private void dgvContas_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvContas.SelectedRows.Count > 0)
            {
                txtNome.Text = dgvContas.SelectedRows[0].Cells["Nome"].Value?.ToString();
                txtAgencia.Text = dgvContas.SelectedRows[0].Cells["Agencia"].Value?.ToString();
                txtConta.Text = dgvContas.SelectedRows[0].Cells["NumeroConta"].Value?.ToString();
                txtSaldo.Text = dgvContas.SelectedRows[0].Cells["Saldo"].Value?.ToString();
            }
        }

        private bool ValidarCampos() => !string.IsNullOrWhiteSpace(txtNome.Text) && double.TryParse(txtSaldo.Text, out _);

        private void LimparCampos() { txtNome.Clear(); txtAgencia.Clear(); txtConta.Clear(); txtSaldo.Clear(); }
    }
}
