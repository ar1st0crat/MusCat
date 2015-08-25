using MusCatalog.View;
using System.Windows;

namespace MusCatalog
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void AppInitialize( object sender, StartupEventArgs e)
        {
            FileLocator.Initialize();
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
