using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Cee_lo.Cee_loGame
{
    public class DiceRoller
    {
        // Random number generator used to simulate dice rolls
        private Random random = new Random();

        /// <summary>
        /// Rolls three six-sided dice and returns the result as an array of integers.
        /// Each value is between 1 and 6 inclusive.
        /// </summary>
        /// <returns>Array of three integers representing dice rolls</returns>
        public int[] RollDice() => new int[] { random.Next(1, 7), random.Next(1, 7), random.Next(1, 7) };

        /// <summary>
        /// Updates the background images of three dice buttons to show the rolled dice.
        /// </summary>
        /// <param name="dice">Array of three integers representing dice values</param>
        /// <param name="slot1">Button for first die</param>
        /// <param name="slot2">Button for second die</param>
        /// <param name="slot3">Button for third die</param>
        public void UpdateDiceButtons(int[] dice, Button slot1, Button slot2, Button slot3)
        {
            // Each die value corresponds to a PNG image in the Assets folder
            slot1.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri($"ms-appx:///Assets/Die{dice[0]}.png")) };
            slot2.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri($"ms-appx:///Assets/Die{dice[1]}.png")) };
            slot3.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri($"ms-appx:///Assets/Die{dice[2]}.png")) };
        }

        /// <summary>
        /// Resets the background images of given dice slots to the default empty slot image.
        /// </summary>
        /// <param name="slots">An array of Button controls representing dice slots to reset</param>
        public void ResetDiceSlots(params Button[] slots)
        {
            // Default dice slot image (blank)
            var defaultImage = new BitmapImage(new Uri("ms-appx:///Assets/DieSlot.png"));

            // Set each provided button to the default image
            foreach (var slot in slots)
                slot.Background = new ImageBrush { ImageSource = defaultImage };
        }
    }
}
