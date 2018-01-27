using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AutoMapper;
using MusCat.Core.Entities;
using MusCat.Core.Services;
using MusCat.Utils;
using MusCat.ViewModels.Entities;

namespace MusCat.ViewModels
{
    class CountriesViewModel : ViewModelBase
    {
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


        public CountriesViewModel(CountryService countryService)
        {
            _countryService = countryService;
            
            AddCommand = new RelayCommand(AddCountry);
            RemoveCommand = new RelayCommand(RemoveCountry);
            ReplaceCommand = new RelayCommand(UpdateCountry);
            OkCommand = new RelayCommand(() => { DialogResult = true; });

            LoadCountriesAsync();
        }

        public async Task LoadCountriesAsync()
        {
            var countryModels = await _countryService.GetAllCountriesAsync();
            Countrylist = new ObservableCollection<CountryViewModel>();
            Mapper.Map(countryModels, Countrylist);
        }

        public void AddCountry()
        {
            var result = _countryService.AddCountry(CountryInput);

            if (result.Type != ResultType.Ok)
            {
                MessageBox.Show(result.Error);
                return;
            }
            
            Countrylist.Add(Mapper.Map<Country, CountryViewModel>(result.Data));
        }

        public void RemoveCountry()
        {
            var selectedCountry = Countrylist[SelectedCountryIndex];
            var result = _countryService.RemoveCountry(selectedCountry.Id);

            if (result.Type != ResultType.Ok)
            {
                MessageBox.Show(result.Error);
                return;
            }

            Countrylist.RemoveAt(SelectedCountryIndex);
        }

        public void UpdateCountry()
        {
            if (SelectedCountryIndex < 0)
            {
                MessageBox.Show("Choose the country first");
                return;
            }

            var selectedCountry = Countrylist[SelectedCountryIndex];
            var result = _countryService.UpdateCountry(selectedCountry.Id, CountryInput);

            if (result.Type != ResultType.Ok)
            {
                MessageBox.Show(result.Error);
                return;
            }

            Countrylist[SelectedCountryIndex] = Mapper.Map<Country, CountryViewModel>(result.Data);
        }
    }
}
