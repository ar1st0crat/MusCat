using Microsoft.Win32;
using MusCatalog.Model;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MusCatalog.View
{
    /// <summary>
    /// Interaction logic for EditPerformerWindow.xaml
    /// </summary>
    public partial class EditPerformerWindow : Window
    {
        private Performer performer;

        public EditPerformerWindow( Performer p )
        {
            InitializeComponent();
            performer = p;
            this.DataContext = performer;

            using ( var context = new MusCatEntities() )
            {
                this.CountryList.ItemsSource = context.Countries.ToList();
                this.GenreList.ItemsSource = context.Genres.ToList();

                if (performer.CountryID != null)
                {
                    this.CountryList.SelectedValue = performer.CountryID.Value;
                }
            }
        }

        private string ChooseImageSavePath()
        {
            var filepaths = FileLocator.MakePathImagePerformer(performer);
            
            if (filepaths.Count > 1)
            {
                ChoiceWindow choice = new ChoiceWindow();
                choice.SetChoiceList(filepaths);
                choice.ShowDialog();

                return choice.ChoiceResult;
            }

            return filepaths[0];
        }

        private void PrepareFileForSaving( string filepath )
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filepath));

            // first check if file already exists
            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }
        }

        private void SavePerformerInformation(object sender, RoutedEventArgs e)
        {
            if (this.CountryList.SelectedItem != null)
            {
                performer.CountryID = (byte)this.CountryList.SelectedValue;
                performer.Country = this.CountryList.SelectedItem as Country;
            }

            using (var context = new MusCatEntities())
            {
                context.Entry(context.Performers.Find(performer.ID)).CurrentValues.SetValues(performer);
                context.SaveChanges();
            }
        }

        private void LoadPerformerImageFromFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            var result = ofd.ShowDialog();
            if (result.HasValue && result.Value == true)
            {
                string filepath = ChooseImageSavePath();
                if (filepath == null)
                {
                    return;
                }

                try
                {
                    PrepareFileForSaving(filepath);
                    File.Copy(ofd.FileName, filepath);

                    this.PerformerPhoto.Source = new WriteableBitmap(new BitmapImage(new Uri(filepath)));
                }
                catch (Exception ex)
                {
                    MessageBox.Show( ex.Message );
                }
            }
        }

        private void LoadPerformerImageFromClipboard(object sender, RoutedEventArgs e)
        {
            if (!Clipboard.ContainsImage())
            {
                MessageBox.Show("No image in clipboard!");
                return;
            }

            string filepath = ChooseImageSavePath();
            if (filepath == null)
            {
                return;
            }

            var image = Clipboard.GetImage();
            try
            {
                PrepareFileForSaving(filepath);

                using (var fileStream = new FileStream(filepath, FileMode.Create))
                {
                    BitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    encoder.Save(fileStream);

                    this.PerformerPhoto.Source = new WriteableBitmap( encoder.Frames[0] );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
