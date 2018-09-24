using System;
using System.Collections;
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

namespace RemoteCodeAnalyzerClient
{
    /// <summary>
    /// Interaction logic for PropertyWindow.xaml
    /// </summary>
    public partial class PropertyWindow : Window
    {
        ArrayList  propertiesList { get; set; }
        public PropertyWindow(ArrayList propertiesList)
        {
            InitializeComponent();
            this.propertiesList = propertiesList;

            PropertyList.ItemsSource = propertiesList;
        }
    }
}
