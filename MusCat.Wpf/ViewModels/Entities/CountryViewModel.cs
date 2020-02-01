using AutoMapper;
using MusCat.Application.Validators;
using MusCat.Core.Entities;
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

                if (!result.Errors.Any(e => e.PropertyName == columnName))
                {
                    return string.Empty;
                }

                var error = result.Errors
                                  .First(e => e.PropertyName == columnName)
                                  .ErrorMessage;
                return error;
            }
        }

        #endregion
    }
}
