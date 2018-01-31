using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using MusCat.Infrastructure.Services;
using MusCat.Util;

namespace MusCat.ViewModels
{
    class SettingsViewModel : ViewModelBase
    {
        private ObservableCollection<string> _pathlist; 
        public ObservableCollection<string> Pathlist
        {
            get { return _pathlist; }
            set
            {
                _pathlist = value;
                RaisePropertyChanged();
            }
        }

        public int SelectedPathIndex { get; set; }

        public ICommand AddCommand { get; private set; }
        public ICommand RemoveCommand { get; private set; }
        public ICommand ReplaceCommand { get; private set; }
        public ICommand OkCommand { get; private set; }

        private bool? _dialogResult;
        public bool? DialogResult
        {
            get { return _dialogResult; }
            set
            {
                _dialogResult = value;
                RaisePropertyChanged();
            }
        }


        public SettingsViewModel()
        {
            AddCommand = new RelayCommand(() =>
            {
                var fbd = new FolderBrowserDialog();
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Pathlist.Add(fbd.SelectedPath);
                }
            });

            RemoveCommand = new RelayCommand(() =>
            {
                Pathlist.RemoveAt(SelectedPathIndex);
            });

            ReplaceCommand = new RelayCommand(() =>
            {
                var fbd = new FolderBrowserDialog();
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Pathlist[SelectedPathIndex] = fbd.SelectedPath;
                }
            });

            OkCommand = new RelayCommand(() =>
            {
                FileLocator.Pathlist = Pathlist.ToList();
                FileLocator.SaveConfiguration();
                DialogResult = true;
            });

            Pathlist = new ObservableCollection<string>(FileLocator.Pathlist);
        }
    }
}
