using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace PerudoBot.Database.Data
{
    public class Player
    {
        public int Id { get; set; }

        [DefaultValue(0)]
        public ulong UserId { get; set; }

        [DefaultValue(0)]
        public ulong GuildId { get; set; }

        public string Name { get; set; }

        public bool IsBot { get; set; }

        public ICollection<GamePlayer> GamesPlayed { get; set; }
    }
}