using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MusCat.ViewModels
{
    class ChoiceWindowViewModel : BindableBase, IDialogAware
    {
        public string Prompt { get; private set; }

        public IEnumerable<string> Options { get; private set; }
        public string SelectedOption { get; set; }
        
        public DelegateCommand OkCommand { get; }
        public DelegateCommand CancelCommand { get; }

        public ChoiceWindowViewModel()
        {
            OkCommand = new DelegateCommand(() => 
            {
                var parameters = new DialogParameters
                {
                    { "choice", SelectedOption }
                };

                RequestClose.Invoke(new DialogResult(ButtonResult.OK, parameters));
            });

            CancelCommand = new DelegateCommand(() =>
            { 
                RequestClose.Invoke(new DialogResult(ButtonResult.Cancel));
            });
        }


        #region IDialogAware implementation

        public string Title => "Options";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog() => true;

        public void OnDialogOpened(IDialogParameters parameters)
        {
            Options = parameters.GetValue<IEnumerable<string>>("options");
            SelectedOption = Options.FirstOrDefault();

            Prompt = parameters.ContainsKey("prompt") ?
                     parameters.GetValue<string>("prompt") :
                     "Please choose one of the following possible paths. The folder will be created automatically";
        }

        public void OnDialogClosed()
        {
        }

        #endregion
    }
}
