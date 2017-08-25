using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using MusCat.Services;
using MusCat.Utils;

namespace MusCat.ViewModels
{
    class SettingsViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<string> _pathlist; 
        public ObservableCollection<string> Pathlist
        {
            get { return _pathlist; }
            set
            {
                _pathlist = value;
                RaisePropertyChanged("Pathlist");
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
                RaisePropertyChanged("DialogResult");
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
                FileLocator.SaveConfigFile();
                DialogResult = true;
            });

            Pathlist = new ObservableCollection<string>(FileLocator.Pathlist);
        }

        #region INotifyPropertyChanged event and method

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
