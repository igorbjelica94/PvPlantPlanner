using Common.LicenseHashGenerator;
using System.Windows;

namespace PvPlantPlanner.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

#if RELEASE
            // Pokreće LicenseHashGenerator pre nego što se otvori MainWindow
            var licenseGen = new LicenseHashGenerator();
            if (!licenseGen.CheckLicensgeHashKey()) Environment.Exit(1);
#endif

            // Ovde se nastavlja standardni start WPF-a
            // MainWindow se i dalje otvara automatski ako je StartupUri postavljen u App.xaml
        }
    }

}
