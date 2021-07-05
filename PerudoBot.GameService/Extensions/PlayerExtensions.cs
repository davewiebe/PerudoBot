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
                IsBot = gamePlayer.Player.IsBot,
                Name = gamePlayer.Player.Name,
                UserId = gamePlayer.Player.UserId
            };
        }
    }
}
