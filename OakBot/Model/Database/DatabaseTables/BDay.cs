using System;

namespace OakBot.Model
{
    public class BDay
    {
        [DbColumn(IsPrimary = true)]
        public string UserID { get; set; }
        [DbColumn]
        public string Name { get; set; }
        [DbColumn]
        public string Date { get; set; }        

    }
    
}
