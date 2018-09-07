using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using AutoMapper;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces.Data;
using MusCat.Core.Interfaces.Domain;
using MusCat.Core.Util;
using MusCat.Infrastructure.Services;
using MusCat.Infrastructure.Services.Networking;
using MusCat.Util;
using MusCat.ViewModels.Entities;
using MusCat.Views;
using Clipboard = System.Windows.Clipboard;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace MusCat.ViewModels
{
    class EditPerformerViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPerformerService _performerService;

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

        public byte? SelectedCountryId { get; set; }

        public ObservableCollection<Genre> Genres { get; set; }
        
        // commands
        public RelayCommand LoadImageFromFileCommand { get; private set; }
        public RelayCommand LoadImageFromClipboardCommand { get; private set; }
        public RelayCommand LoadBioCommand { get; private set; }
        public RelayCommand SavePerformerCommand { get; private set; }


        public EditPerformerViewModel(IPerformerService performerService, IUnitOfWork unitOfWork)
        {
            Guard.AgainstNull(performerService);
            Guard.AgainstNull(unitOfWork);
            
            _performerService = performerService;
            _unitOfWork = unitOfWork;

            LoadImageFromFileCommand = new RelayCommand(LoadPerformerImageFromFile);
            LoadImageFromClipboardCommand = new RelayCommand(LoadPerformerImageFromClipboard);
            LoadBioCommand = new RelayCommand(async() => await LoadBioAsync());
            SavePerformerCommand = new RelayCommand(async() => await SavePerformerAsync());
        }

        public async Task LoadCountriesAsync()
        {
            Countries = new ObservableCollection<Country>(
                await _unitOfWork.CountryRepository.GetAllAsync());
        }

        private async Task SavePerformerAsync()
        {
            var performer = Mapper.Map<Performer>(Performer);
            performer.CountryId = SelectedCountryId;

            await _performerService.UpdatePerformerAsync(performer);

            Performer.Country = (await _performerService.GetCountryAsync(Performer.Id)).Data;

            Performer.ImagePath = FileLocator.GetPerformerImagePath(performer);

            RaisePropertyChanged("Performer");
        }

        private async Task LoadBioAsync()
        {
            var bioLoader = new LastfmDataLoader();

            try
            {
                Performer.Info = await bioLoader.LoadBioAsync(Performer.Name);
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
