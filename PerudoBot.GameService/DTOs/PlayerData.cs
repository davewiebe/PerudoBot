﻿using System;
using System.Collections.Generic;

namespace PerudoBot.GameService
{
    public class PlayerData
    {
        public string Name { get; set; }
        public int PlayerId { get; set; }
        public List<int> Dice { get; set; }
        public int NumberOfDice { get; internal set; }
        public int TurnOrder { get; internal set; }
        public int Rank { get; internal set; }

        public PlayerData()
        {
        }

        public bool IsEliminated => NumberOfDice == 0;

        internal Dictionary<string, string> PlayerMetadata { get; set; }

        public string GetPlayerMetadata(string key)
        {
            return PlayerMetadata.GetValueOrDefault(key);
        }

        internal Dictionary<string, string> GamePlayerMetadata { get; set; }
        public string GetGamePlayerMetadata(string key)
        {
            return GamePlayerMetadata.GetValueOrDefault(key);
        }
    }
}