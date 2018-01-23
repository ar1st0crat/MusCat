using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces.Data;
using MusCat.Utils;

namespace MusCat.ViewModels
{
    class CountriesViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;

        private ObservableCollection<Country> _countrylist;
        public ObservableCollection<Country> Countrylist
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
            _countrylist = new ObservableCollection<Country>(_unitOfWork.CountryRepository.GetAll());

            AddCommand = new RelayCommand(AddCountry);
            RemoveCommand = new RelayCommand(RemoveCountry);
            ReplaceCommand = new RelayCommand(ReplaceCountry);
            OkCommand = new RelayCommand(() => { DialogResult = true; });
        }

        public void AddCountry()
        {
            if (CountryInput == "")
            {
                MessageBox.Show("You can't add empty country!");
                return;
            }

            if (Countrylist.Select(c => c.Name).Any(n => n == CountryInput))
            {
                MessageBox.Show("The specified country is already in the list!");
                return;
            }

            var country = new Country { Name = CountryInput };
            _unitOfWork.CountryRepository.Add(country);
            _unitOfWork.Save();

            Countrylist.Add(country);
        }

        public void RemoveCountry()
        {
            _unitOfWork.CountryRepository.Delete(Countrylist[SelectedCountryIndex]);
            _unitOfWork.Save();

            Countrylist.RemoveAt(SelectedCountryIndex);
        }

        public void ReplaceCountry()
        {
            Countrylist[SelectedCountryIndex].Name = CountryInput;
            _unitOfWork.CountryRepository.Edit(Countrylist[SelectedCountryIndex]);
            _unitOfWork.Save();
        }
    }
}
