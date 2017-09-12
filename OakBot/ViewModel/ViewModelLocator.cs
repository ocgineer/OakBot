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

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

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
                SimpleIoc.Default.Register<IWebSocketEventService, WebSocketEventService>();
                SimpleIoc.Default.Register<ITwitchPubSubService, TwitchPubSubService>();
                SimpleIoc.Default.Register<IBinFileService, BinFileService>();
            }

            // Register ViewModels instanciated on use by a View
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<GiveawaysViewModel>();

            SimpleIoc.Default.Register<ExampleViewModel>(true);

            // Register ViewModels create instance Immediately as no view is using them yet
            SimpleIoc.Default.Register<SubWelcomeModel>(true);
        }

        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
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

        public ExampleViewModel Example
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ExampleViewModel>();
            }
        }
        
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}