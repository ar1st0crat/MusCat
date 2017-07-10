using MusCat.Utils;
using MusCat.View;
using System.Windows;

namespace MusCat
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void AppInitialize( object sender, StartupEventArgs e)
        {
            FileLocator.Initialize();
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
