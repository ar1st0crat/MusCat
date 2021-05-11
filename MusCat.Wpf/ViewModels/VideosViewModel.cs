using MusCat.Util;
using System.Windows.Input;

namespace MusCat.ViewModels
{
    class VideosViewModel : ViewModelBase
    {
        public string Title { get; set; }

        private CircularCollection<string> _circularLinks;

        private string[] _videoLinks;
        public string[] VideoLinks
        {
            get => _videoLinks;
            set
            {
                _videoLinks = value;
                _circularLinks = new CircularCollection<string>(_videoLinks);
                UpdateLinks();
            }
        }

        private string _currentLink;
        public string CurrentLink
        { 
            get => _currentLink;
            set
            {
                _currentLink = value;
                RaisePropertyChanged("CurrentLink");
            }
        }

        private string _nextLink;
        public string NextLink
        {
            get => _nextLink;
            set
            {
                _nextLink = value;
                RaisePropertyChanged("NextLink");
            }
        }

        private string _prevLink;
        public string PrevLink
        {
            get => _prevLink;
            set
            {
                _prevLink = value;
                RaisePropertyChanged("PrevLink");
            }
        }

        public ICommand PrevLinkCommand { get; private set; }
        public ICommand NextLinkCommand { get; private set; }


        public VideosViewModel()
        {
            PrevLinkCommand = new RelayCommand(() =>
            {
                _circularLinks.Prev();
                UpdateLinks();
            });

            NextLinkCommand = new RelayCommand(() =>
            {
                _circularLinks.Next();
                UpdateLinks();
            });
        }

        public void UpdateLinks()
        {
            PrevLink = _circularLinks.GetPrev();
            CurrentLink = _circularLinks.GetCurrent();
            NextLink = _circularLinks.GetNext();
        }
    }
}
