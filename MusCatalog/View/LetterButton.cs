using System.Windows.Controls;
using System.Windows.Media;

namespace MusCatalog.View
{
    class LetterButton: Button
    {
        FontFamily letterFont = new FontFamily( "Stencil" );
        const int width = 32;
        const int height = 32;
        const double scaleCoeff = 1.5;

        public LetterButton(string text, int w = width, int h = height)
        {
            Content = text;
            Background = Brushes.Black;
            Foreground = Brushes.Azure;
            FontSize = 16;
            FontFamily = letterFont;
            Width = w;
            Height = h;
        }

        public void Select()
        {
            Width *= scaleCoeff;
            Height *= scaleCoeff;
        }

        public void DeSelect()
        {
            Width /= scaleCoeff;
            Height /= scaleCoeff;
        }
    }
}
