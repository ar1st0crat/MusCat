using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MusCat.Entities
{
    public class Country : INotifyPropertyChanged
    {
        public Country()
        {
            Performers = new HashSet<Performer>();
        }

        public byte ID { get; set; }

        [Required]
        [StringLength(20)]
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }
        private string _name;

        public virtual ICollection<Performer> Performers { get; set; }


        #region INotifyPropertyChanged event and method

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}
