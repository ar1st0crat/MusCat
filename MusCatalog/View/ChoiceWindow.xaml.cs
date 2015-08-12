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

namespace MusCatalog.View
{
    /// <summary>
    /// Interaction logic for ChoiceWindow.xaml
    /// </summary>
    public partial class ChoiceWindow : Window
    {
        public string ChoiceResult { get; set; }

        public ChoiceWindow()
        {
            InitializeComponent();
        }

        public void SetChoiceList( List<string> list )
        {
            this.ChoiseListBox.ItemsSource = list;
        }

        private void OKButtonClick(object sender, RoutedEventArgs e)
        {
            ChoiceResult = this.ChoiseListBox.SelectedItem.ToString();
            Close();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            ChoiceResult = "";
            Close();
        }
    }
}
