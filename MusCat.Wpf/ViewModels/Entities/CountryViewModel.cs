using AutoMapper;
using MusCat.Core.Entities;
using MusCat.Infrastructure.Validators;
using System.ComponentModel;
using System.Linq;

namespace MusCat.ViewModels.Entities
{
    class CountryViewModel : ViewModelBase, IDataErrorInfo
    {
        public byte Id { get; set; }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged();
            }
        }

        private int _performerCount;
        public int PerformerCount
        {
            get { return _performerCount; }
            set
            {
                _performerCount = value;
                RaisePropertyChanged();
            }
        }

        public string PerformerInfo => $"{Name} ({PerformerCount})";


        #region IDataErrorInfo methods

        private readonly CountryValidator _validator = new CountryValidator();

        public string Error => this["Name"];

        public string this[string columnName]
        {
            get
            {
                var result = _validator.Validate(Mapper.Map<Country>(this));

                var error = result.Errors
                                  .First(e => e.PropertyName == columnName)
                                  .ErrorMessage;
                return error;
            }
        }

        #endregion
    }
}
