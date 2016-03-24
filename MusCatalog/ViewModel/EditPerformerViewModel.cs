using Microsoft.Win32;
using MusCatalog.Model;
using MusCatalog.View;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MusCatalog.ViewModel
{
    class EditPerformerViewModel : INotifyPropertyChanged
    {
        public PerformerViewModel PerformerView { get; set; }
        public Performer Performer
        {
            get { return PerformerView.Performer; }
            set
            {
                PerformerView.Performer = value;
                RaisePropertyChanged("Performer");
            }
        }

        public byte? SelectedCountryID { get; set; }
        public Country SelectedCountry { get; set; }
        public ObservableCollection<Country> Countries { get; set; }
        public ObservableCollection<Genre> Genres { get; set; }

        // commands
        public RelayCommand LoadImageFromFileCommand { get; private set; }
        public RelayCommand LoadImageFromClipboardCommand { get; private set; }
        public RelayCommand SavePerformerCommand { get; private set; }

        public EditPerformerViewModel(PerformerViewModel p)
        {
            // setting up commands
            LoadImageFromFileCommand = new RelayCommand(LoadPerformerImageFromFile);
            LoadImageFromClipboardCommand = new RelayCommand(LoadPerformerImageFromClipboard);
            SavePerformerCommand = new RelayCommand(SavePerformerInformation);

            // load and set all necessary information to edit performer
            PerformerView = p;

            using (var context = new MusCatEntities())
            {
                Countries = new ObservableCollection<Country>();
                Genres = new ObservableCollection<Genre>();

                foreach (var country in context.Countries)
                {
                    Countries.Add(country);
                }

                foreach (var genre in context.Genres)
                {
                    Genres.Add(genre);
                }
            }
        }

        private string ChooseImageSavePath()
        {
            var filepaths = FileLocator.MakePathImagePerformer(Performer);
            
            if (filepaths.Count > 1)
            {
                ChoiceWindow choice = new ChoiceWindow();
                choice.SetChoiceList(filepaths);
                choice.ShowDialog();

                return choice.ChoiceResult;
            }

            return filepaths[0];
        }
        
        public void SavePerformerInformation()
        {
            using (var context = new MusCatEntities())
            {
                context.Entry(context.Performers.Find(Performer.ID)).CurrentValues.SetValues(Performer);
                context.SaveChanges();
            }
        }

        private void PrepareFileForSaving(string filepath)
        {
            // ensure that necessary directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(filepath));

            // first check if file already exists
            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }
        }

        public void LoadPerformerImageFromFile()
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

                    RaisePropertyChanged("Performer");
                    PerformerView.RaisePropertyChanged("Performer");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        public void LoadPerformerImageFromClipboard()
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
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            RaisePropertyChanged("Performer");
            PerformerView.RaisePropertyChanged("Performer");
        }

        #region INotifyPropertyChanged event and method

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
