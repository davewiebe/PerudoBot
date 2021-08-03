using PerudoBot.Database.Data;
using System.Collections.Generic;

namespace PerudoBot.GameService
{
    public interface IGameObject
    {
        public void OnEndOfRound();
        public void OnEndOfGame();
        public void Bid(int playerId, int quantity, int pips);
        public PlayerData GetCurrentPlayer();
        public int GetCurrentRoundNumber();
        public bool BidValidate(int playerId, int quantity, int pips);
        public List<PlayerData> GetAllPlayers();
        public LiarResult Liar(int playerId);
        public bool CreateGame();
        public bool AddPlayer(int playerId, string name);
        public bool SetModeSuddenDeath();
        public bool SetModeVariable();
        public RoundStatus StartNewRound();
        public void ShufflePlayers();
        public Bid GetPreviousBid();
        string GetGameMode();
        public bool HasBots();
        public List<int> GetAllDice();
        public void ReversePlayerOrder();
    }
}
