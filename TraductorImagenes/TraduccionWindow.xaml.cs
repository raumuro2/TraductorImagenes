using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TraductorImagenes
{
    /// <summary>
    /// Lógica de interacción para TraduccionWindow.xaml
    /// </summary>
    public partial class TraduccionWindow : Window
    {
        public TraduccionWindow(string response)
        {
            InitializeComponent();
            txt_Response.Text = response;
        }

        private void btn_atras_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mw = new MainWindow();
            mw.Show();
            this.Close();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                Point mousePos = e.GetPosition(this);

                this.DragMove();

                // Verifica que la ventana no salga de la pantalla
                double screenWidth = SystemParameters.PrimaryScreenWidth;
                double screenHeight = SystemParameters.PrimaryScreenHeight;

                if (this.Left < 0) this.Left = 0;
                if (this.Top < 0) this.Top = 0;
                if (this.Left + this.Width > screenWidth) this.Left = screenWidth - this.Width;
                if (this.Top + this.Height > screenHeight) this.Top = screenHeight - this.Height;
            }
        }

        private void btn_cerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
