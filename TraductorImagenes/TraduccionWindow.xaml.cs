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
    }
}
