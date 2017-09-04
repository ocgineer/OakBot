using System.Windows;
using GalaSoft.MvvmLight.Threading;

namespace OakBot
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Override OnStartup to do some additional initializations
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize MVVM Light DispatcherHelper on main thread
            DispatcherHelper.Initialize();
        }
    }
}
