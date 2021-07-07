using MusCat.Core.Interfaces.Networking;
using MusCat.Core.Util;
using MusCat.Util;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;

namespace MusCat.ViewModels
{
    class VideosViewModel : BindableBase, IDialogAware
    {
        private readonly IVideoLinkWebLoader _videoLinkWebLoader;

        private string _performer;
        private string _song;

        private CircularCollection<string> _circularLinks;

        private string _currentLink;
        public string CurrentLink
        {
            get { return _currentLink; }
            set { SetProperty(ref _currentLink, value); }
        }

        private string _nextLink;
        public string NextLink
        {
            get { return _nextLink; }
            set { SetProperty(ref _nextLink, value); }
        }

        private string _prevLink;
        public string PrevLink
        {
            get { return _prevLink; }
            set { SetProperty(ref _prevLink, value); }
        }

        private string[] _videoLinks;
        public string[] VideoLinks
        {
            get { return _videoLinks; }
            set
            {
                SetProperty(ref _videoLinks, value);

                _circularLinks = new CircularCollection<string>(_videoLinks);

                UpdateLinks();
            }
        }

        public DelegateCommand PrevLinkCommand { get; }
        public DelegateCommand NextLinkCommand { get; }


        public VideosViewModel(IVideoLinkWebLoader videoLinkWebLoader)
        {
            Guard.AgainstNull(videoLinkWebLoader);
            _videoLinkWebLoader = videoLinkWebLoader;

            PrevLinkCommand = new DelegateCommand(() =>
            {
                _circularLinks.Prev();
                UpdateLinks();
            });

            NextLinkCommand = new DelegateCommand(() =>
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


        #region IDialogAware implementation

        public string Title => $"{_performer} - {_song}";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog() => true;

        public void OnDialogOpened(IDialogParameters parameters)
        {
            _performer = parameters.GetValue<string>("performer");
            _song = parameters.GetValue<string>("song");

            _videoLinkWebLoader
                .LoadVideoLinksAsync(_performer, _song)
                .ContinueWith(v => VideoLinks = v.Result);
        }

        public void OnDialogClosed()
        {
        }

        #endregion
    }
}
