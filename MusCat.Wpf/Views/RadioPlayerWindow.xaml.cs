using MusCat.Core.Entities;
using MusCat.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MusCat.Views
{
    public partial class RadioPlayerWindow : Window
    {
        public RadioPlayerWindow()
        {
            InitializeComponent();
        }

        #region pleasant drag-n-drop in the list of upcoming songs

        private ListBoxItem _dragItem;

        private T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null)
            {
                return null;
            }

            var parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }

            return FindVisualParent<T>(parentObject);
        }

        private void ListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var lbi = FindVisualParent<ListBoxItem>(((DependencyObject)e.OriginalSource));

                if (lbi == null)
                {
                    return;
                }

                var radioContext = DataContext as RadioViewModel;

                var from = radioContext.RadioUpcoming.IndexOf(_dragItem.DataContext as Song);
                var to = radioContext.RadioUpcoming.IndexOf(lbi.DataContext as Song);

                if (from != to)
                {
                    radioContext.MoveUpcomingSong(from, to);
                    _dragItem.DataContext = radioContext.RadioUpcoming[to];
                }
            }
        }

        private void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem)
            {
                _dragItem = sender as ListBoxItem;
            }
        }

        #endregion
    }
}
