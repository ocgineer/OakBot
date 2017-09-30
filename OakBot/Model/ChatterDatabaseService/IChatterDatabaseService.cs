using System;
using System.Collections.ObjectModel;

namespace OakBot.Model
{
    public interface IChatterDatabaseService
    {
        event EventHandler<ChattersListUpdatedEventArgs> ChattersListUpdated;

        void StartService(string channel);
        void StopService();
    }
}
