using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;

//[assembly: InternalsVisibleTo("PerudoBotTests.PerudoGameServiceTests")]

namespace PerudoBot.Database.Sqlite.Data
{
    public class Game
    {
        public int Id { get; set; }
        public int State { get; set; }

        public int? PlayerTurnId { get; set; }
        public ulong ChannelId { get; set; }
        public int RoundStartPlayerId { get; internal set; }
        public DateTime DateCreated { get; internal set; }
        public ulong GuildId { get; internal set; }
        public string Winner { get; internal set; }

        public DateTime DateStarted { get; set; }

        public DateTime DateFinished { get; internal set; }
        public bool CanCallExactToJoinAgain { get; internal set; }
        public ulong StatusMessage { get; internal set; }

        public virtual ICollection<GamePlayer> GamePlayers { get; set; }

        public virtual ICollection<Round> Rounds { get; set; }


        [NotMapped]
        public GamePlayer CurrentGamePlayer => GamePlayers.SingleOrDefault(x => x.PlayerId == PlayerTurnId);

        [NotMapped]
        public Round CurrentRound => Rounds.LastOrDefault();

        public int CurrentRoundNumber => CurrentRound?.RoundNumber ?? 0;

    }
}