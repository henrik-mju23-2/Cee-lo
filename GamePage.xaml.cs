using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Cee_lo
{
    public sealed partial class GamePage : Page
    {
        private Random random = new Random();
        public int BankPoints = 0;
        public int PlayerPoints = 0;
        private int? bankPairValue = null; // for comparing with player later

        public GamePage()
        {
            this.InitializeComponent();

            BlurRectangle.Visibility = Visibility.Collapsed;
            InfoTextBlock1.Visibility = Visibility.Collapsed;
            DieButton.Opacity = 0.5;
            DieButton.IsHitTestVisible = false;

            DieSlot1.IsHitTestVisible = false;
            DieSlot2.IsHitTestVisible = false;
            DieSlot3.IsHitTestVisible = false;

            BankDieSlot1.IsHitTestVisible = false;
            BankDieSlot2.IsHitTestVisible = false;
            BankDieSlot3.IsHitTestVisible = false;

            this.Loaded += GamePage_Loaded;
        }

        private async void GamePage_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(3000);
            InfoTextBlock1.Text = "Banken rullar tärningarna...";
            InfoTextBlock1.Visibility = Visibility.Visible;
            await Task.Delay(3000);
            InfoTextBlock1.Visibility = Visibility.Collapsed;

            await BankRollAsync();
        }

        private async Task BankRollAsync()
        {
            int[] dice = RollDice();
            UpdateDiceButtons(dice, BankDieSlot1, BankDieSlot2, BankDieSlot3);

            await EvaluateBankRollAsync(dice);
        }

        private int[] RollDice()
        {
            return new int[] { random.Next(1, 7), random.Next(1, 7), random.Next(1, 7) };
        }

        private void UpdateDiceButtons(int[] dice, Button slot1, Button slot2, Button slot3)
        {
            slot1.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri($"ms-appx:///Assets/Die{dice[0]}.png")) };
            slot2.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri($"ms-appx:///Assets/Die{dice[1]}.png")) };
            slot3.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri($"ms-appx:///Assets/Die{dice[2]}.png")) };
        }

        private async Task EvaluateBankRollAsync(int[] dice)
        {
            Array.Sort(dice);
            string diceText = string.Join("-", dice);

            if (dice.SequenceEqual(new[] { 4, 5, 6 }))
            {
                BankPoints++;
                InfoTextBlock1.Text = "4-5-6, Automatisk vinst för banken!";
                InfoTextBlock1.Visibility = Visibility.Visible;
                BankPointsTextBlock.Text = $"Poäng: {BankPoints}";
                await EndBankTurnAsync();
            }
            else if (dice.SequenceEqual(new[] { 1, 2, 3 }))
            {
                PlayerPoints++;
                InfoTextBlock1.Text = "1-2-3, Automatisk förlust för banken!";
                InfoTextBlock1.Visibility = Visibility.Visible;
                PlayerPointsTextBlock.Text = $"Poäng: {PlayerPoints}";
                await EndBankTurnAsync();
            }
            else if (dice[0] == dice[1] && dice[1] == dice[2])
            {
                BankPoints++;
                InfoTextBlock1.Text = $"{diceText}, Automatisk vinst för banken!";
                InfoTextBlock1.Visibility = Visibility.Visible;
                BankPointsTextBlock.Text = $"Poäng: {BankPoints}";
                await EndBankTurnAsync();
            }
            else if ((dice[0] == dice[1] && dice[2] == 6) || (dice[1] == dice[2] && dice[0] == 6) || (dice[0] == dice[2] && dice[1] == 6))
            {
                BankPoints++;
                InfoTextBlock1.Text = $"{diceText}, Automatisk vinst för banken!";
                InfoTextBlock1.Visibility = Visibility.Visible;
                BankPointsTextBlock.Text = $"Poäng: {BankPoints}";
                await EndBankTurnAsync();
            }
            else if ((dice[0] == dice[1] && dice[2] == 1) || (dice[1] == dice[2] && dice[0] == 1) || (dice[0] == dice[2] && dice[1] == 1))
            {
                PlayerPoints++;
                InfoTextBlock1.Text = $"{diceText}, Automatisk förlust för banken!";
                InfoTextBlock1.Visibility = Visibility.Visible;
                PlayerPointsTextBlock.Text = $"Poäng: {PlayerPoints}";
                await EndBankTurnAsync();
            }
            else if (HasPairWithKicker(dice, out int pairValue, out int kicker) && kicker >= 2 && kicker <= 5)
            {
                bankPairValue = kicker;
                InfoTextBlock1.Text = "Spelare 1:s tur att rulla!";
                InfoTextBlock1.Visibility = Visibility.Visible;
                DieButton.Opacity = 1;
                DieButton.IsHitTestVisible = true;
            }
            else
            {
                InfoTextBlock1.Text = $"{diceText}, Banken får rulla om...";
                InfoTextBlock1.Visibility = Visibility.Visible;
                await Task.Delay(2000);
                await BankRollAsync();
            }
        }

        private async Task EndBankTurnAsync()
        {
            await Task.Delay(3000);
            InfoTextBlock1.Text = "Spelare 1:s tur att rulla!";
            DieButton.Opacity = 1;
            DieButton.IsHitTestVisible = true;
        }

        private bool HasPairWithKicker(int[] dice, out int pairValue, out int kicker)
        {
            pairValue = 0;
            kicker = 0;
            if (dice[0] == dice[1]) { pairValue = dice[0]; kicker = dice[2]; return true; }
            if (dice[1] == dice[2]) { pairValue = dice[1]; kicker = dice[0]; return true; }
            if (dice[0] == dice[2]) { pairValue = dice[0]; kicker = dice[1]; return true; }
            return false;
        }

        private async void DieButton_Click(object sender, RoutedEventArgs e)
        {
            DieButton.Opacity = 0.5;
            DieButton.IsHitTestVisible = false;

            int[] dice = RollDice();
            UpdateDiceButtons(dice, DieSlot1, DieSlot2, DieSlot3);
            await EvaluatePlayerRollAsync(dice);

            Trace.WriteLine($"BankPoints: {BankPoints}");
            Trace.WriteLine($"PlayerPoints: {PlayerPoints}");
        }

        private async Task EvaluatePlayerRollAsync(int[] dice)
        {
            Array.Sort(dice);
            string diceText = string.Join("-", dice);

            if (dice.SequenceEqual(new[] { 4, 5, 6 }))
            {
                PlayerPoints++;
                InfoTextBlock1.Text = "4-5-6, Automatisk vinst för spelaren!";
                InfoTextBlock1.Visibility = Visibility.Visible;
                PlayerPointsTextBlock.Text = $"Poäng: {PlayerPoints}";
            }
            else if (dice.SequenceEqual(new[] { 1, 2, 3 }))
            {
                BankPoints++;
                InfoTextBlock1.Text = "1-2-3, Automatisk förlust för spelaren!";
                InfoTextBlock1.Visibility = Visibility.Visible;
                BankPointsTextBlock.Text = $"Poäng: {BankPoints}";
            }
            else if (dice[0] == dice[1] && dice[1] == dice[2])
            {
                PlayerPoints++;
                InfoTextBlock1.Text = $"{diceText}, Automatisk vinst för spelaren!";
                InfoTextBlock1.Visibility = Visibility.Visible;
                PlayerPointsTextBlock.Text = $"Poäng: {PlayerPoints}";
            }
            else if ((dice[0] == dice[1] && dice[2] == 6) || (dice[1] == dice[2] && dice[0] == 6) || (dice[0] == dice[2] && dice[1] == 6))
            {
                PlayerPoints++;
                InfoTextBlock1.Text = $"{diceText}, Automatisk vinst för spelaren!";
                InfoTextBlock1.Visibility = Visibility.Visible;
                PlayerPointsTextBlock.Text = $"Poäng: {PlayerPoints}";
            }
            else if ((dice[0] == dice[1] && dice[2] == 1) || (dice[1] == dice[2] && dice[0] == 1) || (dice[0] == dice[2] && dice[1] == 1))
            {
                BankPoints++;
                InfoTextBlock1.Text = $"{diceText}, Automatisk förlust för spelaren!";
                InfoTextBlock1.Visibility = Visibility.Visible;
                BankPointsTextBlock.Text = $"Poäng: {BankPoints}";
            }
            else if (HasPairWithKicker(dice, out int pairValue, out int kicker) && kicker >= 2 && kicker <= 5)
            {
                if (bankPairValue.HasValue)
                {
                    if (kicker > bankPairValue)
                    {
                        PlayerPoints++;
                        InfoTextBlock1.Text = $"{diceText}, Spelaren vinner omgången!";
                        PlayerPointsTextBlock.Text = $"Poäng: {PlayerPoints}";
                    }
                    else if (kicker < bankPairValue)
                    {
                        BankPoints++;
                        InfoTextBlock1.Text = $"{diceText}, Banken vinner omgången!";
                        BankPointsTextBlock.Text = $"Poäng: {BankPoints}";
                    }
                    else
                    {
                        InfoTextBlock1.Text = $"{diceText}, Oavgjort!";
                    }
                }
            }
            else
            {
                InfoTextBlock1.Text = $"{diceText}, Spelaren får rulla om...";
                InfoTextBlock1.Visibility = Visibility.Visible;
                await Task.Delay(2000);
                DieButton.Opacity = 1;
                DieButton.IsHitTestVisible = true;
                return;
            }

            InfoTextBlock1.Visibility = Visibility.Visible;
            await Task.Delay(3000);
            await BankRollAsync();
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            InformationPopUp.IsOpen = true;
            BlurRectangle.Visibility = Visibility.Visible;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            InformationPopUp.IsOpen = false;
            BlurRectangle.Visibility = Visibility.Collapsed;
        }

        private void EndButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ResultPage), new int[] { BankPoints, PlayerPoints });
        }


        private void BankPointsTextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void PlayerPointsTextBlock_SelectionChanged_1(object sender, RoutedEventArgs e)
        {

        }

        private void BankDieSlot1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BankDieSlot2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BankDieSlot3_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DieSlot1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DieSlot2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DieSlot3_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void TextBlock_SelectionChanged_1(object sender, RoutedEventArgs e)
        {

        }

        private void InfoTextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}
