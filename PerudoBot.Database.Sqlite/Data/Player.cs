using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace PerudoBot.Database.Data
{
    public class Player : MetadataEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual DiscordPlayer DiscordPlayer { get; set; }
        public int TotalPoints { get; set; }
        public int UsedPoints { get; set; }

        [NotMapped]
        public int AvailablePoints => TotalPoints - UsedPoints;

        public virtual ICollection<GamePlayer> GamesPlayed { get; set; }
        public virtual ICollection<PlayerElo> PlayerElos { get; set; }

    }
}