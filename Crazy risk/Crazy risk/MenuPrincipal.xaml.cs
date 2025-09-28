using System.Windows;
using System.Windows.Controls;

namespace Crazy_risk
{
    public partial class MenuPrincipal : Page
    {
        public MenuPrincipal()
        {
            InitializeComponent();
        }

        private void BtnCrear_Click(object sender, RoutedEventArgs e)
        {
            // TODO: lógica para crear la partida (servidor)
            MessageBox.Show("Crear partida (Servidor)");
        }

        private void BtnUnirse_Click(object sender, RoutedEventArgs e)
        {
            // TODO: lógica para unirse como cliente
            MessageBox.Show("Unirse a partida (Cliente)");
        }

        private void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
