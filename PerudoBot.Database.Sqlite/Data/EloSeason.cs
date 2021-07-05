using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace PerudoBot.Database.Data
{
    public class EloSeason
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public string SeasonName { get; set; }
    }
}