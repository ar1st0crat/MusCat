using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AutoMapper;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces.Data;
using MusCat.Core.Services;
using MusCat.Utils;
using MusCat.ViewModels.Entities;

namespace MusCat.ViewModels
{
    class CountriesViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CountryService _countryService;

        private ObservableCollection<CountryViewModel> _countrylist;
        public ObservableCollection<CountryViewModel> Countrylist
        {
            get { return _countrylist; }
            set
            {
                _countrylist = value;
                RaisePropertyChanged();
            }
        }

        public int SelectedCountryIndex { get; set; }
        public string CountryInput { get; set; }

        public ICommand AddCommand { get; private set; }
        public ICommand RemoveCommand { get; private set; }
        public ICommand ReplaceCommand { get; private set; }
        public ICommand OkCommand { get; private set; }

        private bool? _dialogResult;
        public bool? DialogResult
        {
            get { return _dialogResult; }
            set
            {
                _dialogResult = value;
                RaisePropertyChanged();
            }
        }


        public CountriesViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _countryService = new CountryService(_unitOfWork);
            
            AddCommand = new RelayCommand(async () => await AddCountryAsync());
            RemoveCommand = new RelayCommand(async () => await RemoveCountryAsync());
            ReplaceCommand = new RelayCommand(async () => await UpdateCountryAsync());
            OkCommand = new RelayCommand(() => { DialogResult = true; });

            LoadCountriesAsync();
        }

        public async Task LoadCountriesAsync()
        {
            Countrylist = new ObservableCollection<CountryViewModel>();

            var countryModels = (await _unitOfWork.CountryRepository.GetAllAsync()).ToList();

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
            
            Countrylist.Add(Mapper.Map<Country, CountryViewModel>(result.Data));
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

            Countrylist[SelectedCountryIndex] = Mapper.Map<Country, CountryViewModel>(result.Data);
        }
    }
}
