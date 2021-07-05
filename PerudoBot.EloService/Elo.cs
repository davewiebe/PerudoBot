using System;
using System.Collections.Generic;
using System.Linq;

namespace PerudoBot.EloService.Elo
{
    public class EloPlayer
    {
        public float EloChangeDeferedRounding = 0f;
        public int PlayerId;
        public int Place = 0;
        public int EloPre = 0;
        public int EloPost = 0;
        public int EloChange = 0;
    }

    public class EloMatch
    {
        private List<EloPlayer> _players = new List<EloPlayer>();

        public void AddPlayer(int playerId, int place, int Elo)
        {
            EloPlayer player = new EloPlayer
            {
                PlayerId = playerId,
                Place = place,
                EloPre = Elo
            };

            _players.Add(player);
        }

        public int GetEloRating(int playerId) =>
            _players.FirstOrDefault(p => p.PlayerId == playerId)?.EloPost ?? 1500;

        public int GetEloChange(int playerId) =>
            _players.FirstOrDefault(p => p.PlayerId == playerId)?.EloChange ?? 0;

        public void CalculateElos(int initialK = 20)
        {
            int n = _players.Count;
            //float K = initialK / (float)(n - 1);
            float K = initialK;
            if (_players.Count > 5)
            {
                K = initialK * (float)(5.0 / n);
            }

            for (int i = 0; i < n; i++)
            {
                int curPlace = _players[i].Place;
                int curElo = _players[i].EloPre;

                for (int j = 0; j < n; j++)
                {
                    if (i != j)
                    {
                        int opponentPlace = _players[j].Place;
                        int opponentElo = _players[j].EloPre;

                        //work out S
                        float S;
                        if (curPlace < opponentPlace)
                            S = 1.0F;
                        else if (curPlace == opponentPlace)
                            S = 0.5F;
                        else
                            S = 0.0F;

                        //work out EA
                        float EA = 1 / (1.0f + (float)Math.Pow(10.0f, (opponentElo - curElo) / 400.0f));

                        //calculate Elo change vs this one opponent, add it to our change bucket
                        _players[i].EloChangeDeferedRounding += K * (S - EA);
                    }
                }
                //add accumulated change to initial Elo for final Elo
                _players[i].EloChange = (int)Math.Round(_players[i].EloChangeDeferedRounding);
                _players[i].EloPost = _players[i].EloPre + _players[i].EloChange;
            }
        }
    }
}