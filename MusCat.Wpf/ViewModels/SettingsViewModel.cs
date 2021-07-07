using MusCat.Infrastructure.Services;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MusCat.ViewModels
{
    class SettingsViewModel : BindableBase, IDialogAware
    {
        private ObservableCollection<string> _pathlist; 
        public ObservableCollection<string> Pathlist
        {
            get { return _pathlist; }
            set { SetProperty(ref _pathlist, value); }
        }

        public int SelectedPathIndex { get; set; }

        public DelegateCommand AddCommand { get; }
        public DelegateCommand RemoveCommand { get; }
        public DelegateCommand ReplaceCommand { get; }
        public DelegateCommand OkCommand { get; }


        public SettingsViewModel()
        {
            AddCommand = new DelegateCommand(() =>
            {
                var fbd = new System.Windows.Forms.FolderBrowserDialog();
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Pathlist.Add(fbd.SelectedPath);
                }
            });

            RemoveCommand = new DelegateCommand(() =>
            {
                Pathlist.RemoveAt(SelectedPathIndex);
            });

            ReplaceCommand = new DelegateCommand(() =>
            {
                var fbd = new System.Windows.Forms.FolderBrowserDialog();
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Pathlist[SelectedPathIndex] = fbd.SelectedPath;
                }
            });

            OkCommand = new DelegateCommand(() =>
            {
                FileLocator.Pathlist = Pathlist.ToList();
                FileLocator.SaveConfiguration();
                RequestClose.Invoke(new DialogResult(ButtonResult.OK));
            });

            Pathlist = new ObservableCollection<string>(FileLocator.Pathlist);
        }


        #region IDialogAware implementation

        public string Title => "Settings";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog() => true;

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }

        public void OnDialogClosed()
        {
        }

        #endregion
    }
}
