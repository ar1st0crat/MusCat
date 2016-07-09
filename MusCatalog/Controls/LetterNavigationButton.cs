using System.Windows.Controls;
using System.Windows.Media;

namespace MusCatalog.Controls
{
    /// <summary>
    /// The button with a letter content
    /// The button is used in the upper navigation panel of the main window
    /// </summary>
    public class LetterNavigationButton: Button
    {
        FontFamily letterFont = new FontFamily("Stencil");
        private const double SCALE_COEFF = 1.5;
        bool isSelected;

        public LetterNavigationButton(string text, int w = 32, int h = 32)
        {
            Content = text;
            Background = Brushes.Black;
            Foreground = Brushes.Azure;
            FontSize = 16;
            FontFamily = letterFont;
            Width = w;
            Height = h;
        }

        /// <summary>
        /// The button can be selected and enlarged
        /// </summary>
        public void Select()
        {
            Width *= SCALE_COEFF;
            Height *= SCALE_COEFF;
            isSelected = true;
        }

        /// <summary>
        /// The button can be deselected (for example, when another button is selected)
        /// and get its original size
        /// </summary>
        public void DeSelect()
        {
            if (isSelected)
            {
                Width /= SCALE_COEFF;
                Height /= SCALE_COEFF;
                isSelected = false;
            }
        }
    }
}
