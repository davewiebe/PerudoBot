using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PerudoBot.Database.Data
{
    public class RoundMetadata
    {
        public RoundMetadata()
        {
        }

        public int Id { get; set; }
        public int RoundId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}