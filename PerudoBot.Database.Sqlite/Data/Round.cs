using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PerudoBot.Database.Data
{
    public abstract class Round
    {
        public Round()
        {
        }

        public int Id { get; set; }
        public int GameId { get; set; }
        public int RoundNumber { get; set; }
        public virtual Game Game { get; set; }
        public ICollection<Action> Actions { get; set; }
        public ICollection<GamePlayerRound> GamePlayerRounds { get; set; }

        public int StartingPlayerId { get; set; } // GamePlayerId

        //Discriminator Column
        public string RoundType { get; private set; }

        public Action LatestAction => Actions.LastOrDefault();

        //private ICollection<RoundMetadata> RoundMetadata { get; set; }

        //public bool AddMetadata(string key, string value)
        //{
        //    if (RoundMetadata.Any(x => x.Key == key)) return false;

        //    var metadata = new RoundMetadata
        //    {
        //        RoundId = Id,
        //        Key = key,
        //        Value = value
        //    };

        //    RoundMetadata.Add(metadata);
        //    return true;
        //}

        //public string GetMetadata(string key)
        //{
        //    var metadata = RoundMetadata.SingleOrDefault(x => x.Key == key);
        //    return metadata?.Value;
        //}
    }

    public class StandardRound : Round
    {
    }

    public class FaceoffRound : Round
    {
    }
}