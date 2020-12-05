using PerudoBot.Database.Sqlite.Data;
using System;

namespace PerudoBot.GameService
{
    public class GameObject
    {
        private int _id;

        public GameObject(Game game)
        {
            _id = game.Id;
        }

        public int GetGameNumber()
        {
            return _id;
        }
    }
}