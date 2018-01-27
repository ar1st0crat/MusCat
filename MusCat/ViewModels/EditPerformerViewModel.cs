using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using AutoMapper;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces.Data;
using MusCat.Core.Services;
using MusCat.Infrastructure.Services;
using MusCat.Infrastructure.Services.Networking;
using MusCat.Utils;
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
        private readonly PerformerService _performerService;

        public PerformerViewModel Performer { get; set; }

        public ObservableCollection<Country> Countries { get; set; }
        public ObservableCollection<Genre> Genres { get; set; }
        public byte? SelectedCountryId { get; set; }

        // commands
        public RelayCommand LoadImageFromFileCommand { get; private set; }
        public RelayCommand LoadImageFromClipboardCommand { get; private set; }
        public RelayCommand LoadBioCommand { get; private set; }
        public RelayCommand SavePerformerCommand { get; private set; }

        public EditPerformerViewModel(PerformerViewModel performer, IUnitOfWork unitOfWork)
        {
            var countryService = new CountryService(unitOfWork);
            Countries = new ObservableCollection<Country>(countryService.GetAllCountriesAsync().Result);

            Performer = performer;
            _unitOfWork = unitOfWork;
            _performerService = new PerformerService(unitOfWork);

            LoadImageFromFileCommand = new RelayCommand(LoadPerformerImageFromFile);
            LoadImageFromClipboardCommand = new RelayCommand(LoadPerformerImageFromClipboard);
            LoadBioCommand = new RelayCommand(async() => await LoadBioAsync());
            SavePerformerCommand = new RelayCommand(async() => await SavePerformerAsync());

            SelectedCountryId = Performer.Country?.Id;
        }

        private async Task SavePerformerAsync()
        {
            await _performerService.UpdatePerformerAsync(
                        Performer.Id, Performer.Name, Performer.Info, SelectedCountryId);
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
        }

        #endregion
    }
}
