using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cee_lo.Cee_loGame
{
    public class GameState
    {
        public int BankPoints { get; set; } = 0;
        public int PlayerPoints { get; set; } = 0;

        public int? PlayerPairValue { get; set; } = null;
        public int? BankPairValue { get; set; } = null;

        public bool BankStartsNextRound { get; set; } = true;
    }
}
