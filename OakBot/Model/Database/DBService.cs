using OakBot.Model;
using OakBot.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OakBot.Model
{    
    public class DBService
    {

        public DBService()
        {
           
        }

        public class SubService : BaseService<Sub>
        {
            public SubService(string Connection) : base(Connection)
            {

            }
        }

        public class BDayService : BaseService<BDay>
        {
            public BDayService(string Connection) : base(Connection)
            {

            }
        }

        public class AutoCastService : BaseService<Caster>
        {
            public AutoCastService(string Connection) : base(Connection)
            {

            }
        }

    }
}
