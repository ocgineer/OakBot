using OakBot.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OakBot.Models
{
    public class Subs
    {
        [DbColumn(IsIdentity = true, IsPrimary = true)]
        public long UserID { get; set; }
        [DbColumn]
        public string Name { get; set; }
        [DbColumn]
        public string Tier { get; set; }
        [DbColumn]
        public string New { get; set; }
    }
}
