using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace PerudoBot.Database.Data
{
    public class DiscordPlayer
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public ulong UserId { get; set; }
        public ulong GuildId { get; set; }
        public virtual Player Player { get; set; }
        public bool IsAdministrator { get; set; }
        public bool IsBot { get; set; }
        public string BotKey { get; set; }
    }
}