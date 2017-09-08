using System;

using System.Data.SQLite;
using System.IO;

using OakBot.Models;
using OakBot.Services;


namespace OakBot.ViewModel
{
    public class SubDB
    {
        private SubsService svc = new SubsService();

        public SubDB()
        {
            
        }        

        public void AddSub(Sub newSub)
        {
            svc.Add(newSub);
        }

        public void UpdateSub(Sub existingSub)
        {
            svc.Update(existingSub);
        }

        public Sub GetSub(string id)
        {
            return svc.GetById(id);
        }        
    }
}
