using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using AutoMapper;
using MusCat.Core.Entities;
using MusCat.Core.Util;
using MusCat.Infrastructure.Services;
using MusCat.Util;
using MusCat.ViewModels.Entities;
using MusCat.Views;
using MusCat.Core.Interfaces.Networking;
using MusCat.Application.Interfaces;
using Clipboard = System.Windows.Clipboard;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;


namespace MusCat.ViewModels
{
    class EditPerformerViewModel : ViewModelBase
    {
        private readonly IPerformerService _performerService;
        private readonly IWebLoader _bioLoader;

        private PerformerViewModel _performer;
        public PerformerViewModel Performer
        {
            get { return _performer; }
            set
            {
                _performer = value;
                SelectedCountryId = Performer.Country?.Id;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<Country> _countries;
        public ObservableCollection<Country> Countries
        {
            get { return _countries; }
            set
            {
                _countries = value;
                RaisePropertyChanged();
            }
        }

        public int? SelectedCountryId { get; set; }

        public ObservableCollection<Genre> Genres { get; set; }
        
        // commands
        public RelayCommand LoadImageFromFileCommand { get; private set; }
        public RelayCommand LoadImageFromClipboardCommand { get; private set; }
        public RelayCommand LoadBioCommand { get; private set; }
        public RelayCommand SavePerformerCommand { get; private set; }


        public EditPerformerViewModel(IPerformerService performerService, IWebLoader bioLoader)
        {
            Guard.AgainstNull(performerService);
            Guard.AgainstNull(bioLoader);

            _performerService = performerService;
            _bioLoader = bioLoader;

            LoadImageFromFileCommand = new RelayCommand(LoadPerformerImageFromFile);
            LoadImageFromClipboardCommand = new RelayCommand(LoadPerformerImageFromClipboard);
            LoadBioCommand = new RelayCommand(async() => await LoadBioAsync());
            SavePerformerCommand = new RelayCommand(async() => await SavePerformerAsync());
        }

        private async Task SavePerformerAsync()
        {
            var performer = Mapper.Map<Performer>(Performer);
            performer.CountryId = SelectedCountryId;

            var result = await _performerService.UpdatePerformerAsync(Performer.Id, performer);

            if (result.Type != ResultType.Ok)
            {
                MessageBox.Show(performer.Error);
                Performer.Name = "Unknown";
                return;
            }

            Performer.Country = Mapper.Map<Country>(result.Data.Country);
            Performer.ImagePath = FileLocator.GetPerformerImagePath(performer);
            RaisePropertyChanged("Performer");
        }

        private async Task LoadBioAsync()
        {
            try
            {
                Performer.Info = await _bioLoader.LoadBioAsync(Performer.Name);
                RaisePropertyChanged("Performer");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #region working with images

        private string ChooseImageSavePath()
        {
            var filepaths = FileLocator.MakePerformerImagePathlist(Mapper.Map<Performer>(Performer));

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

        private void LoadPerformerImageFromFile()
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            Performer.ImagePath = filepath;

            RaisePropertyChanged("Performer");
        }

        private void LoadPerformerImageFromClipboard()
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

            Performer.ImagePath = filepath;

            RaisePropertyChanged("Performer");
        }

        #endregion
    }
}
