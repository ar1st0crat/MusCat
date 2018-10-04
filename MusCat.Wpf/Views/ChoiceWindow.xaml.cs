using System.Collections.Generic;
using System.Windows;

namespace MusCat.Views
{
    /// <summary>
    /// Interaction logic for ChoiceWindow.xaml
    /// </summary>
    public partial class ChoiceWindow : Window
    {
        public string ChoiceResult { get; private set; }

        public ChoiceWindow()
        {
            InitializeComponent();
        }

        public void SetChoiceList(IEnumerable<string> list)
        {
            ChoiseListBox.ItemsSource = list;
            ChoiseListBox.SelectedIndex = 0;
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            ChoiceResult = ChoiseListBox.SelectedItem.ToString();
            Close();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
