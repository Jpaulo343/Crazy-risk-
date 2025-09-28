using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Crazy_risk
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //Pasar el nombre de ambos jugadores como parametro, y como tercer parametro su nombre otra vez
            var mapaPage = new Mapa("jugador1", "jugador2","jugador1");

            MainFrame.Content = mapaPage;
        }
    }
}