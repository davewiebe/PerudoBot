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
        public static PlayerObject ToPlayerObject(this GamePlayer gamePlayer)
        {
            return new PlayerObject
            {
                NumberOfDice = gamePlayer.NumberOfDice,
                IsBot = gamePlayer.Player.IsBot,
                Name = gamePlayer.Player.Name,
                UserId = gamePlayer.Player.UserId
            };
        }
    }
}
