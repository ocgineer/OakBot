using System;

namespace OakBot.Model
{
    public class Caster
    {
        [DbColumn(IsPrimary = true)]
        public string UserID { get; set; }
        [DbColumn]
        public string Name { get; set; }
        [DbColumn]
        public bool Custom { get; set; }
        [DbColumn]
        public string Message { get; set; }
        
    }
}
