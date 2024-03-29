﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;

//[assembly: InternalsVisibleTo("PerudoBotTests.PerudoGameServiceTests")]

namespace PerudoBot.Database.Data
{
    public class Game : MetadataEntity
    {
        public int Id { get; set; }
        public int State { get; set; }
        public int GamePlayerTurnId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong GuildId { get; set; }
        public int WinnerPlayerId { get; set; }

        public string Mode { get; set; }

        public virtual ICollection<GamePlayer> GamePlayers { get; set; }

        public virtual ICollection<Round> Rounds { get; set; }


        [NotMapped]
        public GamePlayer CurrentGamePlayer => GamePlayers.SingleOrDefault(x => x.Id == GamePlayerTurnId);

        [NotMapped]
        public Round CurrentRound => Rounds.LastOrDefault();

        public int CurrentRoundNumber => CurrentRound?.RoundNumber ?? 0;

    }
}