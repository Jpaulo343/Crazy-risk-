using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Crazy_risk
{
    public partial class MenuPrincipal : Page
    {
        private Conexion conexion;

        public MenuPrincipal()
        {
            InitializeComponent();
        }

        private async void BtnCrear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string alias = TxtAlias.Text;
                int puerto = int.Parse(TxtPuerto.Text);

                conexion = new Conexion();
                await conexion.IniciarServidor(puerto);

                // Handshake: yo digo mi nombre, espero el del cliente
                await conexion.Enviar($"HELLO|{alias}");
                var resp = await conexion.Recibir();                 // "HELLO|<aliasCliente>"
                var parts = resp?.Split('|');
                string aliasCliente = (parts?.Length == 2) ? parts[1] : "Invitado";

                // Ambos juegos usarán exactamente estos dos nombres y este orden:
                // [aliasServidor, aliasCliente]; local = aliasServidor
                NavigationService.Navigate(new Mapa(alias, aliasCliente, alias, conexion));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al crear partida: {ex.Message}");
            }
        }


        private async void BtnUnirse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string alias = TxtAlias.Text;
                string ip = TxtIp.Text;
                int puerto = int.Parse(TxtPuerto.Text);

                conexion = new Conexion();
                await conexion.Conectar(ip, puerto);

                // Handshake: yo envío mi nombre, leo el del servidor
                await conexion.Enviar($"HELLO|{alias}");
                var resp = await conexion.Recibir();                 // "HELLO|<aliasServidor>"
                var parts = resp?.Split('|');
                string aliasServidor = (parts?.Length == 2) ? parts[1] : "Servidor";

                // Mismo orden que el servidor: [aliasServidor, aliasCliente]; local = aliasCliente
                NavigationService.Navigate(new Mapa(aliasServidor, alias, alias, conexion));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al unirse: {ex.Message}");
            }
        }


        private void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
