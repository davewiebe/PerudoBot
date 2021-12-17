using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace PerudoBot.Database.Data
{
    public class Player : MetadataEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual DiscordPlayer DiscordPlayer { get; set; }
        public int Points { get; set; }
        public virtual ICollection<GamePlayer> GamesPlayed { get; set; }
        public virtual ICollection<PlayerElo> PlayerElos { get; set; }

    }
}