using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Encodings.Web; // Necessário para corrigir os caracteres
using System.Windows.Forms;

namespace SimuladorBancario
{
    public class Conta
    {
        public string Nome { get; set; } = string.Empty;
        public string Agencia { get; set; } = string.Empty;
        public string NumeroConta { get; set; } = string.Empty;
        public double Saldo { get; set; }
    }

    public partial class Form1 : Form
    {
        private string jsonFilePath = "banco_financeiro.json";
        private List<Conta> listaContas = new List<Conta>();
        private string contaSelecionadaNumero = "";

        private FlowLayoutPanel flpContas = null!; 
        private TextBox txtNome = null!, txtAgencia = null!, txtConta = null!, txtSaldo = null!;
        private Button btnSalvar = null!, btnAtualizar = null!, btnExcluir = null!, btnDeposito = null!, btnTransferir = null!;
        private Label lblNome = null!, lblAgencia = null!, lblConta = null!, lblSaldo = null!;
        private Panel pnlTopo = null!;

        public Form1()
        {
            InitializeComponent();
            ConfigurarInterface();
            CarregarDados();
            AtualizarCards();
            this.Resize += (s, e) => AtualizarCards();
        }

        private void InitializeComponent()
        {
            pnlTopo = new Panel { Dock = DockStyle.Top, Height = 145, BackColor = Color.Black };
            flpContas = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(15, 15, 15), Padding = new Padding(10) };
            
            this.txtNome = new TextBox(); this.txtAgencia = new TextBox();
            this.txtConta = new TextBox(); this.txtSaldo = new TextBox();
            
            lblNome = new Label { Text = "Nome:", Top = 10, Left = 10, Width = 60, ForeColor = Color.Cyan };
            lblAgencia = new Label { Text = "Agência:", Top = 40, Left = 10, Width = 60, ForeColor = Color.Cyan };
            lblConta = new Label { Text = "Conta:", Top = 70, Left = 10, Width = 60, ForeColor = Color.Cyan };
            lblSaldo = new Label { Text = "Saldo:", Top = 100, Left = 10, Width = 60, ForeColor = Color.Cyan };

            txtNome.SetBounds(80, 10, 200, 25);
            txtAgencia.SetBounds(80, 40, 120, 25);
            txtConta.SetBounds(80, 70, 120, 25);
            txtSaldo.SetBounds(80, 100, 120, 25);

            btnSalvar = new Button { Text = "Salvar" }; btnSalvar.SetBounds(300, 10, 110, 35);
            btnAtualizar = new Button { Text = "Atualizar" }; btnAtualizar.SetBounds(300, 50, 110, 35);
            btnExcluir = new Button { Text = "Excluir" }; btnExcluir.SetBounds(300, 90, 110, 35);
            btnDeposito = new Button { Text = "Depósito" }; btnDeposito.SetBounds(420, 10, 110, 35);
            btnTransferir = new Button { Text = "Transferir" }; btnTransferir.SetBounds(420, 50, 110, 35);

            btnSalvar.Click += btnSalvar_Click;
            btnAtualizar.Click += btnAtualizar_Click;
            btnExcluir.Click += btnExcluir_Click;
            btnDeposito.Click += btnDeposito_Click;
            btnTransferir.Click += btnTransferir_Click;

            pnlTopo.Controls.AddRange(new Control[] { txtNome, txtAgencia, txtConta, txtSaldo, btnSalvar, btnAtualizar, btnExcluir, btnDeposito, btnTransferir, lblNome, lblAgencia, lblConta, lblSaldo });
            this.Controls.Add(flpContas);
            this.Controls.Add(pnlTopo);
            this.MinimumSize = new Size(600, 500);
            this.Text = "Sistema Bancário Pro";
        }

        private void ConfigurarInterface()
        {
            this.BackColor = Color.Black;
            foreach (Control c in pnlTopo.Controls) if (c is Button b) { b.BackColor = Color.FromArgb(40, 40, 40); b.ForeColor = Color.Cyan; b.FlatStyle = FlatStyle.Flat; }
        }

        private void AtualizarCards()
        {
            flpContas.SuspendLayout();
            flpContas.Controls.Clear();
            int colunas = Math.Max(1, flpContas.Width / 350); 
            int larguraCard = (flpContas.Width / colunas) - 30;
            foreach (var conta in listaContas) flpContas.Controls.Add(CriarCard(conta, larguraCard));
            flpContas.ResumeLayout();
        }

        private Control CriarCard(Conta conta, int largura)
        {
            Panel card = new Panel { Size = new Size(largura, 150), BackColor = (contaSelecionadaNumero == conta.NumeroConta) ? Color.FromArgb(50, 50, 50) : Color.FromArgb(30, 30, 30), BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(10) };

            Label lblNomeCard = new Label { Text = conta.Nome.ToUpper(), Dock = DockStyle.Top, ForeColor = Color.Cyan, Height = 35, Font = new Font("Segoe UI", 11, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
            
            // Labels individuais com posição garantida para não sumir
            Label lblAg = new Label { Text = "AGÊNCIA: " + conta.Agencia, Top = 45, Left = 15, Width = largura - 30, ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            Label lblCt = new Label { Text = "CONTA: " + conta.NumeroConta, Top = 75, Left = 15, Width = largura - 30, ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            Label lblVal = new Label { Text = conta.Saldo.ToString("C2"), Dock = DockStyle.Bottom, ForeColor = Color.SpringGreen, Height = 35, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 12, FontStyle.Bold) };

            void Selecionar(object? s, EventArgs e) {
                contaSelecionadaNumero = conta.NumeroConta;
                txtNome.Text = conta.Nome; txtAgencia.Text = conta.Agencia; txtConta.Text = conta.NumeroConta; txtSaldo.Text = conta.Saldo.ToString();
                AtualizarCards();
            }

            card.Click += Selecionar;
            foreach (Control c in new Control[] { lblNomeCard, lblAg, lblCt, lblVal }) { c.Click += Selecionar; card.Controls.Add(c); }

            return card;
        }

        private void CarregarDados()
        {
            if (File.Exists(jsonFilePath))
            {
                string js = File.ReadAllText(jsonFilePath);
                // Desserialização simples
                listaContas = JsonSerializer.Deserialize<List<Conta>>(js) ?? new List<Conta>();
            }
        }

        private void SalvarDados()
        {
            // CORREÇÃO DOS CARACTERES: RelaxedJsonEscaping evita o \u00E9
            var options = new JsonSerializerOptions { 
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping 
            };
            File.WriteAllText(jsonFilePath, JsonSerializer.Serialize(listaContas, options));
        }

        private void btnSalvar_Click(object? s, EventArgs e) { if (ValidarCampos()) { listaContas.Add(new Conta { Nome = txtNome.Text, Agencia = txtAgencia.Text, NumeroConta = txtConta.Text, Saldo = double.Parse(txtSaldo.Text) }); SalvarDados(); AtualizarCards(); LimparCampos(); } }
        private void btnAtualizar_Click(object? s, EventArgs e) { 
            var c = listaContas.FirstOrDefault(x => x.NumeroConta == contaSelecionadaNumero); 
            if (c != null && ValidarCampos()) { c.Nome = txtNome.Text; c.Agencia = txtAgencia.Text; c.NumeroConta = txtConta.Text; c.Saldo = double.Parse(txtSaldo.Text); SalvarDados(); AtualizarCards(); LimparCampos(); } 
        }
        private void btnExcluir_Click(object? s, EventArgs e) { if (!string.IsNullOrEmpty(contaSelecionadaNumero)) { if (MessageBox.Show("Excluir conta?", "Aviso", MessageBoxButtons.YesNo) == DialogResult.Yes) { listaContas.RemoveAll(x => x.NumeroConta == contaSelecionadaNumero); SalvarDados(); AtualizarCards(); LimparCampos(); } } }
        
        private void btnDeposito_Click(object? s, EventArgs e) { 
            var c = listaContas.FirstOrDefault(x => x.NumeroConta == contaSelecionadaNumero);
            if (c != null) {
                string res = Microsoft.VisualBasic.Interaction.InputBox("Valor do depósito:", "Depósito");
                if (double.TryParse(res, out double v)) { c.Saldo += v; SalvarDados(); AtualizarCards(); }
            }
        }

        private void btnTransferir_Click(object? s, EventArgs e) { 
            var rem = listaContas.FirstOrDefault(x => x.NumeroConta == contaSelecionadaNumero);
            if (rem != null) {
                string destNum = Microsoft.VisualBasic.Interaction.InputBox("Conta destino:", "Transferir");
                string valIn = Microsoft.VisualBasic.Interaction.InputBox("Valor:", "Transferir");
                if (double.TryParse(valIn, out double v) && v <= rem.Saldo) {
                    var dest = listaContas.FirstOrDefault(x => x.NumeroConta == destNum);
                    if (dest != null) { rem.Saldo -= v; dest.Saldo += v; SalvarDados(); MessageBox.Show("Sucesso!"); AtualizarCards(); }
                    else MessageBox.Show("Destinatário não encontrado!");
                }
            }
        }

        private bool ValidarCampos() => !string.IsNullOrWhiteSpace(txtNome.Text) && !string.IsNullOrWhiteSpace(txtAgencia.Text) && !string.IsNullOrWhiteSpace(txtConta.Text) && double.TryParse(txtSaldo.Text, out _);
        private void LimparCampos() { txtNome.Clear(); txtAgencia.Clear(); txtConta.Clear(); txtSaldo.Clear(); contaSelecionadaNumero = ""; }
    }
}
