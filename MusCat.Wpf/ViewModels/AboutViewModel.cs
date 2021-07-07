using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;

namespace MusCat.ViewModels
{
    public class AboutViewModel : BindableBase, IDialogAware
    {
        public string Title => "MusCat help";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog() => true;

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }

        public void OnDialogClosed()
        {
        }
    }
}
