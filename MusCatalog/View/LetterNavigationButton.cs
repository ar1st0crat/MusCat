using System.Windows.Controls;
using System.Windows.Media;

namespace MusCatalog.View
{
    /// <summary>
    /// The button with a letter content
    /// The button is used in the upper navigation panel of the main window
    /// </summary>
    public class LetterNavigationButton: Button
    {
        FontFamily letterFont = new FontFamily("Stencil");
        bool bSelected = false;
        const int width = 32;
        const int height = 32;
        const double scaleCoeff = 1.5;

        public LetterNavigationButton(string text, int w = width, int h = height)
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
            Width *= scaleCoeff;
            Height *= scaleCoeff;
            bSelected = true;
        }

        /// <summary>
        /// The button can be deselected (for example, when another button is selected)
        /// and get its original size
        /// </summary>
        public void DeSelect()
        {
            if (bSelected)
            {
                Width /= scaleCoeff;
                Height /= scaleCoeff;
                bSelected = false;
            }
        }
    }
}
