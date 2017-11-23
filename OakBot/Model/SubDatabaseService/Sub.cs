using OakBot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OakBot.Model
{
    public class Sub
    {
        [DbColumn(IsPrimary = true)]
        public string UserID { get; set; }
        [DbColumn]
        public string Name { get; set; }
        [DbColumn]
        public int Tier { get; set; }
        [DbColumn]
        public bool New { get; set; }
    }
}
