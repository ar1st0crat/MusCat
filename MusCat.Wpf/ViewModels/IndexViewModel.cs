namespace MusCat.ViewModels
{
    /// <summary>
    /// View model for elements in the index panel
    /// </summary>
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
