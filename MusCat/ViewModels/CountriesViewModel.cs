﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MusCat.Entities;
using MusCat.Repositories.Base;
using MusCat.Utils;

namespace MusCat.ViewModels
{
    class CountriesViewModel : INotifyPropertyChanged
    {
        public UnitOfWork UnitOfWork { get; set; }

        private ObservableCollection<Country> _countrylist;
        public ObservableCollection<Country> Countrylist
        {
            get { return _countrylist; }
            set
            {
                _countrylist = value;
                RaisePropertyChanged("Countrylist");
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
                RaisePropertyChanged("DialogResult");
            }
        }


        public CountriesViewModel()
        {
            AddCommand = new RelayCommand(AddCountry);
            RemoveCommand = new RelayCommand(RemoveCountry);
            ReplaceCommand = new RelayCommand(ReplaceCountry);

            OkCommand = new RelayCommand(() =>
            {
                DialogResult = true;
            });
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
            UnitOfWork.CountryRepository.Add(country);
            UnitOfWork.Save();

            Countrylist.Add(country);
        }

        public void RemoveCountry()
        {
            UnitOfWork.CountryRepository.Delete(Countrylist[SelectedCountryIndex]);
            UnitOfWork.Save();

            Countrylist.RemoveAt(SelectedCountryIndex);
        }

        public void ReplaceCountry()
        {
            Countrylist[SelectedCountryIndex].Name = CountryInput;
            UnitOfWork.CountryRepository.Edit(Countrylist[SelectedCountryIndex]);
            UnitOfWork.Save();
        }

        #region INotifyPropertyChanged event and method

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
