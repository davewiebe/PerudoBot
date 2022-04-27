using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerudoBot.GameService
{
    public class RoundResult
    {
        public LiarResult LiarResult { get; set; }
        public List<BetResult> BetResults { get; set; }
    }
}
