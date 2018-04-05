/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:OakBot"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;

using OakBot.Model;

namespace OakBot.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            // Register Services for DI
            if (ViewModelBase.IsInDesignModeStatic)
            {
                // Create design time view services and model
            }
            else
            {
                // Create run time view services and models
                SimpleIoc.Default.Register<IChatConnectionService, ChatConnectionService>();
                SimpleIoc.Default.Register<IChatterDatabaseService, ChatterDatabaseService>();
                SimpleIoc.Default.Register<IWebSocketEventService, WebSocketEventService>();
                SimpleIoc.Default.Register<ITwitchPubSubService, TwitchPubSubService>();
            }

            // Register ViewModels and create instance immedialy
            SimpleIoc.Default.Register<StatusBarViewModel>(true);

            // Register ViewModels instanciated on use by a View
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<ConsoleViewModel>();
            SimpleIoc.Default.Register<GiveawaysViewModel>();


            // Register ViewModels create instance Immediately as no view is using them yet
            SimpleIoc.Default.Register<SubWelcomeModel>(true);
            SimpleIoc.Default.Register<BDayModel>(true);
        }

        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public StatusBarViewModel StatusBar
        {
            get
            {
                return ServiceLocator.Current.GetInstance<StatusBarViewModel>();
            }
        }

        public ConsoleViewModel Console
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ConsoleViewModel>();
            }
        }

        public GiveawaysViewModel Giveaways
        {
            get
            {
                return ServiceLocator.Current.GetInstance<GiveawaysViewModel>();
            }
        }

        public SubWelcomeModel SubWelcome
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SubWelcomeModel>();
            }
        }

        public BDayModel BDay
        {
            get
            {
                return ServiceLocator.Current.GetInstance<BDayModel>();
            }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}