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
        private Random random = new Random();

        public int[] RollDice() => new int[] { random.Next(1, 7), random.Next(1, 7), random.Next(1, 7) };

        public void UpdateDiceButtons(int[] dice, Button slot1, Button slot2, Button slot3)
        {
            slot1.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri($"ms-appx:///Assets/Die{dice[0]}.png")) };
            slot2.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri($"ms-appx:///Assets/Die{dice[1]}.png")) };
            slot3.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri($"ms-appx:///Assets/Die{dice[2]}.png")) };
        }

        public void ResetDiceSlots(params Button[] slots)
        {
            var defaultImage = new BitmapImage(new Uri("ms-appx:///Assets/DieSlot.png"));
            foreach (var slot in slots)
                slot.Background = new ImageBrush { ImageSource = defaultImage };
        }
    }
}
