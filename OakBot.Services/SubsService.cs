using OakBot.Common;
using OakBot.Models;
using OakBot.Utility;

namespace OakBot.Services
{
    public class SubsService : BaseService<Subs>
    {
        public SubsService()
            :base(Storage.ConnectionString)
        {

        }
    }
}
