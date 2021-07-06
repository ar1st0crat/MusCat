using Prism.Ioc;
using Prism.Services.Dialogs;
using System.Windows;

namespace MusCat
{
    /// <summary>
    /// Our dialog service returns windows without Owners
    /// </summary>
    public class CustomDialogService : DialogService
    {
        public CustomDialogService(IContainerExtension containerExtension) : base(containerExtension)
        {
        }

        protected override void ConfigureDialogWindowProperties(IDialogWindow window, FrameworkElement dialogContent, IDialogAware viewModel)
        {
            var windowStyle = Dialog.GetWindowStyle(dialogContent);
            if (windowStyle != null)
                window.Style = windowStyle;

            window.Content = dialogContent;
            window.DataContext = viewModel;

            window.Owner = null;
        }
    }
}
