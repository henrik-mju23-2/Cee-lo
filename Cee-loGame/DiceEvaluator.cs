using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cee_lo.Cee_loGame
{
    public class DiceEvaluator
    {
        public static bool HasPairWithKicker(int[] dice, out int pairValue, out int kicker)
        {
            pairValue = 0;
            kicker = 0;
            if (dice[0] == dice[1]) { pairValue = dice[0]; kicker = dice[2]; return true; }
            if (dice[1] == dice[2]) { pairValue = dice[1]; kicker = dice[0]; return true; }
            if (dice[0] == dice[2]) { pairValue = dice[0]; kicker = dice[1]; return true; }
            return false;
        }

        public static bool IsAutomaticWin(int[] dice)
        {
            Array.Sort(dice);
            return dice.SequenceEqual(new[] { 4, 5, 6 }) ||
                   (dice[0] == dice[1] && dice[1] == dice[2]) ||
                   (dice[0] == dice[1] && dice[2] == 6) ||
                   (dice[1] == dice[2] && dice[0] == 6) ||
                   (dice[0] == dice[2] && dice[1] == 6);
        }

        public static bool IsAutomaticLoss(int[] dice)
        {
            Array.Sort(dice);
            return dice.SequenceEqual(new[] { 1, 2, 3 }) ||
                   (dice[0] == dice[1] && dice[2] == 1) ||
                   (dice[1] == dice[2] && dice[0] == 1) ||
                   (dice[0] == dice[2] && dice[1] == 1);
        }

        public static string GetDiceInfoText(int[] dice)
        {
            Array.Sort(dice);
            string diceText = string.Join("-", dice);
            string info2 = "";

            if (IsAutomaticWin(dice)) info2 = $"{diceText}, automatisk vinst";
            else if (IsAutomaticLoss(dice)) info2 = $"{diceText}, automatisk förlust";
            else if (HasPairWithKicker(dice, out int pairVal, out int kicker) && kicker >= 2 && kicker <= 5)
            {
                int target = kicker + 1;
                if (target > 6) target = 6;
                info2 = $"{diceText}, slå en {target}:a eller högre för att vinna";
            }
            else info2 = $"{diceText}, ogiltigt resultat";

            return info2;
        }
    }
}
