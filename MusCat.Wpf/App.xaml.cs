using Microsoft.Win32;
using MusCat.Application.Interfaces;
using MusCat.Application.Services;
using MusCat.Core.Interfaces;
using MusCat.Core.Interfaces.Audio;
using MusCat.Core.Interfaces.Data;
using MusCat.Core.Interfaces.Networking;
using MusCat.Core.Interfaces.Radio;
using MusCat.Core.Interfaces.Stats;
using MusCat.Core.Interfaces.Tracklist;
using MusCat.Core.Services;
using MusCat.Infrastructure.Data;
using MusCat.Infrastructure.Services;
using MusCat.Infrastructure.Services.Audio;
using MusCat.Infrastructure.Services.Networking;
using MusCat.Infrastructure.Services.Radio;
using MusCat.Infrastructure.Services.Stats;
using MusCat.Infrastructure.Services.Tracklist;
using MusCat.ViewModels;
using MusCat.Views;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using System.Configuration;
using System.Linq;
using System.Windows;

namespace MusCat
{
    public partial class App : PrismApplication
    {
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MusCatDbContext"].ConnectionString;

            containerRegistry.RegisterSingleton<IUnitOfWork>(() => new UnitOfWork(connectionString));
            containerRegistry.Register<IPerformerService, PerformerService>();
            containerRegistry.Register<IAlbumService, AlbumService>();
            containerRegistry.Register<ISongService, SongService>();
            containerRegistry.Register<ICountryService, CountryService>();
            containerRegistry.Register<IStatsService>(() => new StatsService(connectionString));
            containerRegistry.Register<ISongSelector>(() => new RandomSongSelector(connectionString));
            containerRegistry.Register<IAudioPlayer, AudioPlayer>();
            containerRegistry.Register<IRadioService, RadioService>();
            containerRegistry.Register<ITracklistHelper, Mp3TracklistHelper>();
            containerRegistry.Register<IBioWebLoader, BioWebLoader>();
            containerRegistry.Register<ITracklistWebLoader, TracklistWebLoader>();
            containerRegistry.Register<ILyricsWebLoader, LyricsWebLoader>();
            containerRegistry.Register<IVideoLinkWebLoader, VideoLinkWebLoader>();
            containerRegistry.Register<IRateCalculator, RateCalculator>();
            containerRegistry.Register<MainWindowViewModel>();
            containerRegistry.Register<AlbumWindowViewModel>();
            containerRegistry.Register<StatsViewModel>();
            containerRegistry.Register<EditPerformerViewModel>();
            containerRegistry.Register<EditAlbumViewModel>();
            containerRegistry.Register<EditCountriesViewModel>();

            containerRegistry.Register<IDialogService, CustomDialogService>();
            containerRegistry.RegisterDialog<RadioWindow, RadioWindowViewModel>();
            containerRegistry.RegisterDialog<AlbumWindow, AlbumWindowViewModel>();
            containerRegistry.RegisterDialog<EditAlbumWindow, EditAlbumViewModel>();
            containerRegistry.RegisterDialog<PerformerWindow, EditPerformerViewModel>();
            containerRegistry.RegisterDialog<EditPerformerWindow, EditPerformerViewModel>();
            containerRegistry.RegisterDialog<EditCountriesWindow, EditCountriesViewModel>();
            containerRegistry.RegisterDialog<StatsWindow, StatsViewModel>();
            containerRegistry.RegisterDialog<VideosWindow, VideosViewModel>();
            containerRegistry.RegisterDialog<ChoiceWindow, ChoiceWindowViewModel>();
            containerRegistry.RegisterDialog<SettingsWindow, SettingsViewModel>();
            containerRegistry.RegisterDialog<AboutWindow, AboutViewModel>();
        }

        protected override Window CreateShell()
        {
            Mappings.Init();

            // Setup FileLocator:

            if (FileLocator.MustBeConfigured())
            {
                MessageBox.Show("Please specify folders for MusCat to look for media files");

                // Disable shutdown when the dialog is closed
                Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                var dialogService = Container.Resolve<IDialogService>();

                dialogService.ShowDialog("SettingsWindow", r =>
                {
                    if (r.Result != ButtonResult.OK)
                    {
                        MessageBox.Show("Default path will be used: " + FileLocator.Pathlist.FirstOrDefault());
                    }
                });
            }

            FileLocator.LoadConfiguration();

            InitWebBrowser();

            // Normal shutdown

            Current.ShutdownMode = ShutdownMode.OnLastWindowClose;

            return Container.Resolve<MainWindow>();
        }

        /// <summary>
        /// Call this function before embedding YouTube videos in WPF windows
        /// </summary>
        private static void InitWebBrowser()
        {
            int browserVersion;
            int registryValue;

            using (var wb = new System.Windows.Forms.WebBrowser())
            {
                browserVersion = wb.Version.Major;
            }

            if (browserVersion >= 11)
                registryValue = 11001;
            else if (browserVersion == 10)
                registryValue = 10001;
            else if (browserVersion == 9)
                registryValue = 9999;
            else if (browserVersion == 8)
                registryValue = 8888;
            else
                registryValue = 7000;

            using (RegistryKey Key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", RegistryKeyPermissionCheck.ReadWriteSubTree))
                if (Key.GetValue(System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe") == null)
                    Key.SetValue(System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe", registryValue, RegistryValueKind.DWord);
        }
    }
}
