using OakBot.Model;
using OakBot.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OakBot.Model
{
    public class DBService : BaseService<Sub>
    {
        public DBService(string dbConnectionString)
            : base(dbConnectionString)
        {

        }

    }
}
