using PerudoBot.Database.Data;

namespace PerudoBot.GameService
{
    public class BetResult
    {
        public Player BettingPlayer { get; set; }
        public int BetAmount { get; set; }
        public int BetPips { get; set; }
        public int BetQuantity { get; set; }
        public string BetType { get; set; }
        public bool IsSuccessful { get; set; }
    }
}
