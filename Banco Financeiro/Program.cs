using System;
using System.Windows.Forms;

namespace SimuladorBancario
{
    internal static class Program
    {
        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Habilita estilos visuais modernos para os controles
            Application.EnableVisualStyles();
            
            // Define a renderização de texto padrão
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Inicia o formulário principal (Form1)
            Application.Run(new Form1());
        }
    }
}