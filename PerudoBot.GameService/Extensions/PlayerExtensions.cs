using PerudoBot.Database.Data;
using PerudoBot.Extensions;
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
                Dice = gamePlayer.CurrentGamePlayerRound?.Dice.ToIntegerDice(),
                HiddenDiceIndeces = gamePlayer.CurrentGamePlayerRound?.Dice.GetHiddenDiceIndeces(),
                PlayerMetadata = gamePlayer.Player.Metadata.ToDictionary(x => x.Key, x => x.Value),
                GamePlayerMetadata = gamePlayer.Metadata.ToDictionary(x => x.Key, x => x.Value)
            };
        }
    }
}
