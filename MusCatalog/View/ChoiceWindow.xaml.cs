using System.Collections.Generic;
using System.Windows;

namespace MusCatalog.View
{
    /// <summary>
    /// Interaction logic for ChoiceWindow.xaml
    /// </summary>
    public partial class ChoiceWindow : Window
    {
        private string choiceResult = null;
        public string ChoiceResult
        {
            get { return choiceResult; }
        }

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
            choiceResult = this.ChoiseListBox.SelectedItem.ToString();
            Close();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
