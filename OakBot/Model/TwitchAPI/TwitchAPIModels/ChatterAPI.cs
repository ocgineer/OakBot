using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OakBot.Model
{
    public class ChatterAPI
    {
        public string Username { get; private set; }

        public UserType Type { get; private set; }

        public ChatterAPI(string username, UserType type)
        {
            Username = username;
            Type = type;
        }

        public override string ToString()
        {
            return Username;
        }
    }
}
