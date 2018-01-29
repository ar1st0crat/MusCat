namespace MusCat.ViewModels.Entities
{
    class CountryViewModel : ViewModelBase
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

        private long _performerCount;
        public long PerformerCount
        {
            get { return _performerCount; }
            set
            {
                _performerCount = value;
                RaisePropertyChanged();
            }
        }

        public string PerformerInfo => 
            string.Format("{0} ({1})", Name, PerformerCount);
    }
}
