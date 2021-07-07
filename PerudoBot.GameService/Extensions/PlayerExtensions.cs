using PerudoBot.Database.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerudoBot.GameService.Extensions
{
    public static class PlayerExtensions
    {
        public static PlayerData ToPlayerObject(this GamePlayer gamePlayer)
        {
            return new PlayerData
            {
                NumberOfDice = gamePlayer.NumberOfDice,
                Name = gamePlayer.Player.Name,
                PlayerId = gamePlayer.Player.Id,
                Rank = gamePlayer.Rank,
                TurnOrder = gamePlayer.TurnOrder,
                Dice = gamePlayer.CurrentGamePlayerRound?.Dice
            };
        }
    }
}
