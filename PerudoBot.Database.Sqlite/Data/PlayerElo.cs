using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace PerudoBot.Database.Data
{
    public class PlayerElo
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public virtual Player Player { get; set; }
        public int EloSeasonId { get; set; }
        public virtual EloSeason EloSeason { get; set; }
        public string GameMode { get; set; }
        public int Rating { get; set; }
    }
}