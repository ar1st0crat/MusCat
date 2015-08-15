using System.Collections.Generic;
using System.Windows;

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
