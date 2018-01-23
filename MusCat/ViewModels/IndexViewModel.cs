namespace MusCat.ViewModels
{
    class IndexViewModel : ViewModelBase
    {
        public string Text { get; set; }

        private bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                RaisePropertyChanged();
            }
        }
    }
}
