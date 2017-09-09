﻿using OakBot.Common;
using OakBot.Models;
using OakBot.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OakBot.Services
{
    public class SubService : BaseService<Sub>
    {
        public SubService()
            : base(Storage.ConnectionString)
        {

        }

    }
}