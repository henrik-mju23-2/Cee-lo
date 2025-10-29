using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cee_lo.Cee_loGame
{
    // Holds the current state of the game, including scores and pair/kicker information.
    // This class acts as a centralized place to track points and round order.
    public class GameState
    {
        // Tracks the total points of the bank (dealer). 
        // Incremented when the bank wins a round.
        public int BankPoints { get; set; } = 0;

        // Tracks the total points of the player. 
        // Incremented when the player wins a round.
        public int PlayerPoints { get; set; } = 0;


        // Holds the kicker value when the player rolls a pair plus a kicker.
        // Null if the player has not rolled a pair with a kicker yet.
        // Used for comparing against the bank's pair+kicker roll.
        public int? PlayerPairValue { get; set; } = null;

        // Holds the kicker value when the bank rolls a pair plus a kicker.
        // Null if the bank has not rolled a pair with a kicker yet.
        // Used for comparing against the player's pair+kicker roll.
        public int? BankPairValue { get; set; } = null;

        // Determines which side starts the next round.
        // True = bank starts first (default), false = player starts first.
        // Used to alternate the starting player between rounds.
        public bool BankStartsNextRound { get; set; } = true;
    }
}
