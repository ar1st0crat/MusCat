using AutoMapper;
using MusCat.Application.Dto;
using MusCat.Application.Interfaces;
using MusCat.Core.Interfaces.Data;
using MusCat.Core.Util;
using MusCat.ViewModels.Entities;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MusCat.ViewModels
{
    class EditCountriesViewModel : BindableBase, IDialogAware
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICountryService _countryService;

        private ObservableCollection<CountryViewModel> _countrylist;
        public ObservableCollection<CountryViewModel> Countrylist
        {
            get { return _countrylist; }
            set { SetProperty(ref _countrylist, value); }
        }

        private string _countryInput;
        public string CountryInput
        {
            get { return _countryInput; }
            set { SetProperty(ref _countryInput, value); }
        }

        public int SelectedCountryIndex { get; set; }
        
        public DelegateCommand AddCommand { get; }
        public DelegateCommand RemoveCommand { get; }
        public DelegateCommand ReplaceCommand { get; }
        public DelegateCommand OkCommand { get; }


        public EditCountriesViewModel(ICountryService countryService, IUnitOfWork unitOfWork)
        {
            Guard.AgainstNull(countryService);
            Guard.AgainstNull(unitOfWork);

            _countryService = countryService;
            _unitOfWork = unitOfWork;
            
            AddCommand = new DelegateCommand(async () => await AddCountryAsync());
            RemoveCommand = new DelegateCommand(async () => await RemoveCountryAsync());
            ReplaceCommand = new DelegateCommand(async () => await UpdateCountryAsync());
            OkCommand = new DelegateCommand(() => RequestClose.Invoke(new DialogResult(ButtonResult.OK)));
        }

        public async Task LoadCountriesAsync()
        {
            Countrylist = new ObservableCollection<CountryViewModel>();

            var countryModels = (await _unitOfWork.CountryRepository.GetAllAsync()).OrderBy(c => c.Name).ToList();

            foreach (var countryModel in countryModels)
            {
                var country = Mapper.Map<CountryViewModel>(countryModel);
                country.PerformerCount = await _countryService.GetPerformersCountAsync(country.Id);
                Countrylist.Add(country);
            }
        }

        private async Task AddCountryAsync()
        {
            var result = await _countryService.AddCountryAsync(CountryInput);

            if (result.Type != ResultType.Ok)
            {
                MessageBox.Show(result.Error);
                return;
            }
            
            Countrylist.Add(Mapper.Map<CountryDto, CountryViewModel>(result.Data));
        }

        private async Task RemoveCountryAsync()
        {
            var selectedCountry = Countrylist[SelectedCountryIndex];
            var result = await _countryService.RemoveCountryAsync(selectedCountry.Id);

            if (result.Type != ResultType.Ok)
            {
                MessageBox.Show(result.Error);
                return;
            }

            Countrylist.RemoveAt(SelectedCountryIndex);
        }

        private async Task UpdateCountryAsync()
        {
            if (SelectedCountryIndex < 0)
            {
                MessageBox.Show("Choose the country first");
                return;
            }

            var selectedCountry = Countrylist[SelectedCountryIndex];
            var result = await _countryService.UpdateCountryAsync(selectedCountry.Id, CountryInput);

            if (result.Type != ResultType.Ok)
            {
                MessageBox.Show(result.Error);
                return;
            }

            CountryInput = "";

            var updatedCountry = Mapper.Map<CountryViewModel>(result.Data);
            updatedCountry.PerformerCount = selectedCountry.PerformerCount;

            Countrylist[SelectedCountryIndex] = updatedCountry;
        }


        #region IDialogAware implementation

        public string Title => "Countries";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog() => true;

        public void OnDialogOpened(IDialogParameters parameters)
        {
            LoadCountriesAsync();
        }

        public void OnDialogClosed()
        {
        }

        #endregion
    }
}
