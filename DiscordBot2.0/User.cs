using DiscordBot2._0;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot2._0
{
    public class User
    {
        public OffencesRecord offencesRecord { get; private set; }
        public string name { get; set; }
        public decimal id { get; private set; }

        public User(string name, decimal id)
        {
            this.name = name;
            this.id = id;
            this.offencesRecord = new OffencesRecord();
        }

        public User(string name, decimal id, OffencesRecord offencesRecord)
        {
            this.name = name;
            this.id = id;
            this.offencesRecord = offencesRecord;
        }

        private DateTime ParseToDateTime(string str)
        {
            if (str != "00/00/00" && str != "/") return (DateTime.ParseExact(str, "dd/MM/yyyy", null));
            else return DateTime.Now;
        }
    }
}
