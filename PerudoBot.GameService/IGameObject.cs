using System.Collections.Generic;

namespace PerudoBot.GameService
{
    public interface IGameObject
    {
        void Bid(int quantity, int pips);
        /*
        public int GetGameNumber();
        public bool RemovePlayer(ulong userId);
        public bool AddPlayer(ulong userId, ulong guildId, string name, bool isBot);
        public void SetModeVariable();
        public void SetModeSuddenDeath();
        public List<PlayerObject> GetPlayers();
        public string GetMode();
        public void Start();
        public RoundStatus StartNewRound();
        public void Terminate();
        public LiarResult Liar();
        public object GetCurrentRoundNumber();
        public PlayerObject GetCurrentPlayer();
        public List<PlayerDice> GetPlayerDice();
        */
    }
}