using Prism.Mvvm;

namespace MusCat.ViewModels
{
    /// <summary>
    /// View model for elements in the index panel
    /// </summary>
    class IndexViewModel : BindableBase
    {
        public string Text { get; set; }

        private bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            set { SetProperty(ref _isActive, value); }
        }
    }
}
