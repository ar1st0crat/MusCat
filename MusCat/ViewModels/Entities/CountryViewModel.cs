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

        public int PerformerCount { get; set; }

        public string Info => 
            string.Format("{0} ({1})", Name, PerformerCount);
    }
}
