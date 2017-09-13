using System;
using System.Collections.ObjectModel;

using GalaSoft.MvvmLight;

using OakBot.Model;

namespace OakBot.ViewModel
{
    /* TODO: Manually add / remove giveaway modules as per user
     * Load back in amount of used modules used on prior run of the bot.
     */

    public class GiveawaysViewModel : ViewModelBase
    {
        #region Fields

        private readonly IChatConnectionService _chatService;
        private readonly IWebSocketEventService _wsEventService;

        private readonly Random _rnd;
        private ObservableCollection<GiveawayViewModel> _modules;

        #endregion

        public GiveawaysViewModel(IChatConnectionService chat, IWebSocketEventService wsevent)
        {
            // Store references to services
            _chatService = chat;
            _wsEventService = wsevent;

            // Use one seeded pseudo-random generator for all giveaway modules
            _rnd = new Random();

            // Set giveaway modules
            _modules = new ObservableCollection<GiveawayViewModel>
            {
                new GiveawayViewModel(1, _rnd, _chatService, _wsEventService),
                new GiveawayViewModel(2, _rnd, _chatService, _wsEventService),
                new GiveawayViewModel(3, _rnd, _chatService, _wsEventService)
            };
        }

        public ObservableCollection<GiveawayViewModel> GiveawayModules
        {
            get
            {
                return _modules;
            }
        }
    }
}
