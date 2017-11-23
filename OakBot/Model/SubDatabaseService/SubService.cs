using OakBot.Model;
using OakBot.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OakBot.Model
{
    public class SubService : BaseService<Sub>
    {
        public SubService()
            : base(Storage.ConnectionString)
        {

        }

    }
}
