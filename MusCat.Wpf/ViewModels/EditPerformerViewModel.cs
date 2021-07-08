using AutoMapper;
using MusCat.Application.Interfaces;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces.Networking;
using MusCat.Core.Util;
using MusCat.Infrastructure.Services;
using MusCat.ViewModels.Entities;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Clipboard = System.Windows.Clipboard;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace MusCat.ViewModels
{
    class EditPerformerViewModel : BindableBase, IDialogAware
    {
        private readonly IPerformerService _performerService;
        private readonly IBioWebLoader _bioWebLoader;
        private readonly IDialogService _dialogService;

        private string _initialName;

        private PerformerViewModel _performer;
        public PerformerViewModel Performer
        {
            get { return _performer; }
            set
            {
                SetProperty(ref _performer, value);

                _initialName = _performer.Name;
                SelectedCountryId = _performer.Country?.Id;
            }
        }

        private ObservableCollection<Country> _countries;
        public ObservableCollection<Country> Countries
        {
            get { return _countries; }
            set { SetProperty(ref _countries, value); }
        }

        public int? SelectedCountryId { get; set; }

        public ObservableCollection<Genre> Genres { get; set; }
        
        // commands

        public DelegateCommand LoadImageFromFileCommand { get; }
        public DelegateCommand LoadImageFromClipboardCommand { get; }
        public DelegateCommand LoadBioCommand { get; }
        public DelegateCommand SavePerformerCommand { get; }


        public EditPerformerViewModel(IPerformerService performerService,
                                      IBioWebLoader bioWebLoader,
                                      IDialogService dialogService)
        {
            Guard.AgainstNull(performerService);
            Guard.AgainstNull(bioWebLoader);
            Guard.AgainstNull(dialogService);

            _performerService = performerService;
            _bioWebLoader = bioWebLoader;
            _dialogService = dialogService;

            LoadImageFromFileCommand = new DelegateCommand(LoadPerformerImageFromFile);
            LoadImageFromClipboardCommand = new DelegateCommand(LoadPerformerImageFromClipboard);
            LoadBioCommand = new DelegateCommand(async() => await LoadBioAsync());
            SavePerformerCommand = new DelegateCommand(async() => await SavePerformerAsync());
        }

        private async Task SavePerformerAsync()
        {
            var performer = Mapper.Map<Performer>(Performer);
            performer.CountryId = SelectedCountryId;

            var result = await _performerService.UpdatePerformerAsync(_performer.Id, performer);

            if (result.Type != ResultType.Ok)
            {
                MessageBox.Show(result.Error);
                _performer.Name = _initialName;
                return;
            }

            _performer.Country = Mapper.Map<Country>(result.Data.Country);
            _performer.ImagePath = FileLocator.GetPerformerImagePath(performer);
            RaisePropertyChanged("Performer");
        }

        private async Task LoadBioAsync()
        {
            try
            {
                _performer.Info = await _bioWebLoader.LoadBioAsync(_performer.Name);
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

            var parameters = new DialogParameters
            {
                { "options", filepaths }
            };

            string path = null;

            _dialogService.ShowDialog("ChoiceWindow", parameters, r =>
            {
                path = r.Parameters.GetValue<string>("choice");
            });

            return path;
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

            if (filepath is null)
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

            _performer.ImagePath = filepath;

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

            if (filepath is null)
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

            _performer.ImagePath = filepath;

            RaisePropertyChanged("Performer");
        }

        #endregion


        #region IDialogAware implementation

        public string Title => $"{Performer?.Name} info";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog() => true;

        public void OnDialogOpened(IDialogParameters parameters)
        {
            Performer = parameters.GetValue<PerformerViewModel>("performer");

            if (parameters.ContainsKey("countries"))
            {
                var countries = parameters.GetValue<IEnumerable<Country>>("countries");
                Countries = new ObservableCollection<Country>(countries);
            }
        }

        public void OnDialogClosed()
        {
        }

        #endregion
    }
}
