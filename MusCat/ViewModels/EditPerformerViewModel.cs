using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using MusCat.Entities;
using MusCat.Repositories.Base;
using MusCat.Services;
using MusCat.Utils;
using MusCat.Views;

namespace MusCat.ViewModels
{
    class EditPerformerViewModel : INotifyPropertyChanged
    {
        public UnitOfWork UnitOfWork { get; set; }

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

        public byte? SelectedCountryId { get; set; }
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
            SavePerformerCommand = new RelayCommand(async() => await SavePerformerInformation());

            // load and set all necessary information to edit performer
            PerformerView = p;

            // load countries and genres just here without any overdesigned repository classes
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

        public async Task SavePerformerInformation()
        {
            UnitOfWork.PerformerRepository.Edit(Performer);
            await UnitOfWork.SaveAsync();
        }

        #region working with images

        private string ChooseImageSavePath()
        {
            var filepaths = FileLocator.MakePerformerImagePathlist(Performer);

            if (filepaths.Count == 1)
            {
                return filepaths[0];
            }

            var choice = new ChoiceWindow();
            choice.SetChoiceList(filepaths);
            choice.ShowDialog();

            return choice.ChoiceResult;
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
            var ofd = new OpenFileDialog();
            var result = ofd.ShowDialog();
            if (!result.HasValue || result.Value != true)
            {
                return;
            }

            var filepath = ChooseImageSavePath();

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

        public void LoadPerformerImageFromClipboard()
        {
            if (!Clipboard.ContainsImage())
            {
                MessageBox.Show("No image in clipboard!");
                return;
            }

            var filepath = ChooseImageSavePath();

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
                    var encoder = new JpegBitmapEncoder();
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

        #endregion

        #region INotifyPropertyChanged event and method

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
