using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace PerudoBot.Database.Data
{
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual DiscordPlayer DiscordPlayer { get; set; }

        public ICollection<GamePlayer> GamesPlayed { get; set; }
        public ICollection<PlayerElo> PlayerElos { get; set; }
    }
}