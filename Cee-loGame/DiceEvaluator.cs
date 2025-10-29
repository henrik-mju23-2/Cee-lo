using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cee_lo.Cee_loGame
{
    public class DiceEvaluator
    {
        // Checks if the dice contain a pair and a kicker (the remaining die).
        // <param name="dice">Array of three dice values</param>
        // <param name="pairValue">Outputs the value of the pair if found</param>
        // <param name="kicker">Outputs the remaining die (kicker) if pair is found</param>
        // <returns>True if a pair exists, false otherwise</returns>
        public static bool HasPairWithKicker(int[] dice, out int pairValue, out int kicker)
        {
            pairValue = 0;
            kicker = 0;

            // Check first two dice
            if (dice[0] == dice[1]) { pairValue = dice[0]; kicker = dice[2]; return true; }

            // Check last two dice
            if (dice[1] == dice[2]) { pairValue = dice[1]; kicker = dice[0]; return true; }

            // Check first and last dice
            if (dice[0] == dice[2]) { pairValue = dice[0]; kicker = dice[1]; return true; }

            // No pair found
            return false;
        }

        // Determines if a dice roll is an automatic win.
        // Automatic wins are 4-5-6 or any triples, or a pair plus a 6.
        // <param name="dice">Array of three dice values</param>
        // <returns>True if roll is automatic win, false otherwise</returns>
        public static bool IsAutomaticWin(int[] dice)
        {
            Array.Sort(dice);

            return dice.SequenceEqual(new[] { 4, 5, 6 }) ||          // Straight 4-5-6
                   (dice[0] == dice[1] && dice[1] == dice[2]) ||    // Triple
                   (dice[0] == dice[1] && dice[2] == 6) ||          // Pair + 6
                   (dice[1] == dice[2] && dice[0] == 6) ||          // Pair + 6
                   (dice[0] == dice[2] && dice[1] == 6);            // Pair + 6
        }

        // Determines if a dice roll is an automatic loss.
        // Automatic losses are 1-2-3 or a pair plus a 1.
        // <param name="dice">Array of three dice values</param>
        // <returns>True if roll is automatic loss, false otherwise</returns>
        public static bool IsAutomaticLoss(int[] dice)
        {
            Array.Sort(dice);

            return dice.SequenceEqual(new[] { 1, 2, 3 }) ||          // Straight 1-2-3
                   (dice[0] == dice[1] && dice[2] == 1) ||          // Pair + 1
                   (dice[1] == dice[2] && dice[0] == 1) ||          // Pair + 1
                   (dice[0] == dice[2] && dice[1] == 1);            // Pair + 1
        }

        // Generates a human-readable string describing the result of a dice roll.
        // Includes information on automatic wins/losses or pair + kicker scenarios.
        // <param name="dice">Array of three dice values</param>
        // <returns>A descriptive string for UI display</returns>
        public static string GetDiceInfoText(int[] dice)
        {
            Array.Sort(dice); // Sort dice for consistency in display
            string diceText = string.Join("-", dice); // e.g., "2-3-5"
            string info2 = "";

            if (IsAutomaticWin(dice))
                info2 = $"{diceText}, automatisk vinst"; // Automatic win message
            else if (IsAutomaticLoss(dice))
                info2 = $"{diceText}, automatisk förlust"; // Automatic loss message
            else if (HasPairWithKicker(dice, out int pairVal, out int kicker) && kicker >= 2 && kicker <= 5)
            {
                // Pair + kicker case: describe what number player needs to beat
                int target = kicker + 1;
                if (target > 6) target = 6; // Max die value is 6
                info2 = $"{diceText}, slå en {target}:a eller högre för att vinna";
            }
            else
                info2 = $"{diceText}, ogiltigt resultat"; // Invalid roll

            return info2;
        }
    }
}
