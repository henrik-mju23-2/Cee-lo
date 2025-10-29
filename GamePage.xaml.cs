using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Cee_lo
{
    public sealed partial class GamePage : Page
    {
        private Random random = new Random();

        // Public so result page or debugging can access them; navigation passes values instead of creating new page instances.
        public int BankPoints = 0;    // meaning: points in UtanPengar
        public int PlayerPoints = 0;

        // track player's pair kicker when player rolls first (so bank can respond in the same round)
        private int? playerPairValue = null;
        private int? bankPairValue = null; // for pair+{2..5} comparison

        private bool bankStartsNextRound = true; // true = bank-first (default), false = player-first

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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Initialize UI state for UtanPengar points game
            CasinoChip5Button.Visibility = Visibility.Collapsed;
            CasinoChip25Button.Visibility = Visibility.Collapsed;
            CasinoChip50Button.Visibility = Visibility.Collapsed;
            CasinoChip100Button.Visibility = Visibility.Collapsed;
            CasinoChip5Button.IsHitTestVisible = false;
            CasinoChip25Button.IsHitTestVisible = false;
            CasinoChip50Button.IsHitTestVisible = false;
            CasinoChip100Button.IsHitTestVisible = false;

            BankStakeTextBlock.Visibility = Visibility.Collapsed;
            PlayerStakeTextBlock.Visibility = Visibility.Collapsed;

            BankPointsTextBlock.Text = $"Poäng: {BankPoints}";
            PlayerPointsTextBlock.Text = $"Poäng: {PlayerPoints}";
        }

        private async void GamePage_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(3000);

            // Changed message
            InfoTextBlock1.Text = "Banken rullar först";
            InfoTextBlock1.Visibility = Visibility.Visible;
            await Task.Delay(3000);
            InfoTextBlock1.Visibility = Visibility.Collapsed;

            await BankRollAsync();
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

        private void ResetDiceSlots()
        {
            var defaultImage = new BitmapImage(new Uri("ms-appx:///Assets/DieSlot.png"));

            // Player dice
            DieSlot1.Background = new ImageBrush { ImageSource = defaultImage };
            DieSlot2.Background = new ImageBrush { ImageSource = defaultImage };
            DieSlot3.Background = new ImageBrush { ImageSource = defaultImage };

            // Bank dice
            BankDieSlot1.Background = new ImageBrush { ImageSource = defaultImage };
            BankDieSlot2.Background = new ImageBrush { ImageSource = defaultImage };
            BankDieSlot3.Background = new ImageBrush { ImageSource = defaultImage };
        }

        private async Task BankRollAsync(bool isResponseToPlayer = false)
        {
            ResetDiceSlots();

            InfoTextBlock1.Text = isResponseToPlayer ? "Banken rullar som svar..." : "Banken rullar nu...";
            InfoTextBlock1.Visibility = Visibility.Visible;
            await Task.Delay(3000);
            InfoTextBlock1.Visibility = Visibility.Collapsed;

            int[] dice = RollDice();
            Array.Sort(dice);
            UpdateDiceButtons(dice, BankDieSlot1, BankDieSlot2, BankDieSlot3);

            await EvaluateBankRollAsync(dice);
        }

        private async Task EvaluateBankRollAsync(int[] dice)
        {
            Array.Sort(dice);
            string diceText = string.Join("-", dice);
            UpdateInfoTextBlock2(dice);

            bool roundEnds = false;

            // --- AUTOMATIC WINS ---
            if (dice.SequenceEqual(new[] { 4, 5, 6 }) ||
                (dice[0] == dice[1] && dice[1] == dice[2]) ||
                (dice[0] == dice[1] && dice[2] == 6) ||
                (dice[1] == dice[2] && dice[0] == 6) ||
                (dice[0] == dice[2] && dice[1] == 6))
            {
                BankPoints++;
                InfoTextBlock1.Text = $"{diceText}, Automatisk vinst för banken!";
                BankPointsTextBlock.Text = $"Poäng: {BankPoints}";
                roundEnds = true;
            }
            // --- AUTOMATIC LOSSES ---
            else if (dice.SequenceEqual(new[] { 1, 2, 3 }) ||
                     (dice[0] == dice[1] && dice[2] == 1) ||
                     (dice[1] == dice[2] && dice[0] == 1) ||
                     (dice[0] == dice[2] && dice[1] == 1))
            {
                PlayerPoints++;
                InfoTextBlock1.Text = $"{diceText}, Automatisk förlust för banken!";
                PlayerPointsTextBlock.Text = $"Poäng: {PlayerPoints}";
                roundEnds = true;
            }
            // --- PAIR + KICKER (2..5) ---
            else if (HasPairWithKicker(dice, out int pairVal, out int kicker) && kicker >= 2 && kicker <= 5)
            {
                if (!playerPairValue.HasValue)
                {
                    bankPairValue = kicker;
                    BankStakeTextBlock.Text = $"Bankens nummer: {bankPairValue}";
                    InfoTextBlock1.Text = "Spelare 1:s tur att rulla!";
                    InfoTextBlock1.Visibility = Visibility.Visible;

                    DieButton.Opacity = 1;
                    DieButton.IsHitTestVisible = true;
                    return;
                }
                else
                {
                    int playerKicker = playerPairValue.Value;
                    int bankKicker = kicker;

                    if (bankKicker > playerKicker)
                    {
                        BankPoints++;
                        InfoTextBlock1.Text = $"{diceText}, Banken vinner omgången!";
                        BankPointsTextBlock.Text = $"Poäng: {BankPoints}";
                    }
                    else if (bankKicker < playerKicker)
                    {
                        PlayerPoints++;
                        InfoTextBlock1.Text = $"{diceText}, Spelaren vinner omgången!";
                        PlayerPointsTextBlock.Text = $"Poäng: {PlayerPoints}";
                    }
                    else
                    {
                        InfoTextBlock1.Text = $"{diceText}, Oavgjort!";
                    }

                    playerPairValue = null;
                    bankPairValue = null;
                    roundEnds = true;
                }
            }
            // --- INVALID ROLL: REROLL ---
            else
            {
                InfoTextBlock1.Text = $"{diceText}, Banken får rulla om...";
                InfoTextBlock1.Visibility = Visibility.Visible;
                await Task.Delay(3000);

                bool isPlayerFirstResponse = playerPairValue.HasValue;
                await BankRollAsync(isResponseToPlayer: isPlayerFirstResponse);
                return;
            }

            // --- ROUND END HANDLING ---
            if (roundEnds)
            {
                ResetDiceSlots();
                InfoTextBlock1.Visibility = Visibility.Visible;
                await Task.Delay(3000);

                InfoTextBlock1.Text = "Nästa runda...";
                InfoTextBlock1.Visibility = Visibility.Visible;
                await Task.Delay(3000);

                await EndBankTurnAsync();
            }
        }

        private async Task EndBankTurnAsync()
        {
            await Task.Delay(3000);

            bankStartsNextRound = !bankStartsNextRound;

            if (bankStartsNextRound)
            {
                InfoTextBlock1.Text = "Banken börjar nästa runda!";
                InfoTextBlock1.Visibility = Visibility.Visible;
                await Task.Delay(2000);
                await BankRollAsync();
            }
            else
            {
                InfoTextBlock1.Text = "Spelare 1:s tur att börja!";
                InfoTextBlock1.Visibility = Visibility.Visible;
                await Task.Delay(2000);

                DieButton.Opacity = 1;
                DieButton.IsHitTestVisible = true;
            }
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
            Array.Sort(dice);
            UpdateDiceButtons(dice, DieSlot1, DieSlot2, DieSlot3);
            await EvaluatePlayerRollAsync(dice);

            Trace.WriteLine($"BankPoints: {BankPoints}");
            Trace.WriteLine($"PlayerPoints: {PlayerPoints}");
        }

        private async Task EvaluatePlayerRollAsync(int[] dice)
        {
            Array.Sort(dice);
            string diceText = string.Join("-", dice);
            UpdateInfoTextBlock2(dice);

            bool roundEnds = false;

            // --- AUTOMATIC WINS ---
            if (dice.SequenceEqual(new[] { 4, 5, 6 }) ||
                (dice[0] == dice[1] && dice[1] == dice[2]) ||
                (dice[0] == dice[1] && dice[2] == 6) ||
                (dice[1] == dice[2] && dice[0] == 6) ||
                (dice[0] == dice[2] && dice[1] == 6))
            {
                PlayerPoints++;
                InfoTextBlock1.Text = $"{diceText}, Automatisk vinst för spelaren!";
                PlayerPointsTextBlock.Text = $"Poäng: {PlayerPoints}";
                roundEnds = true;
            }
            // --- AUTOMATIC LOSSES ---
            else if (dice.SequenceEqual(new[] { 1, 2, 3 }) ||
                     (dice[0] == dice[1] && dice[2] == 1) ||
                     (dice[1] == dice[2] && dice[0] == 1) ||
                     (dice[0] == dice[2] && dice[1] == 1))
            {
                BankPoints++;
                InfoTextBlock1.Text = $"{diceText}, Automatisk förlust för spelaren!";
                BankPointsTextBlock.Text = $"Poäng: {BankPoints}";
                roundEnds = true;
            }
            // --- PAIR + KICKER (2..5) ---
            else if (HasPairWithKicker(dice, out int pairVal, out int kicker) && kicker >= 2 && kicker <= 5)
            {
                PlayerStakeTextBlock.Visibility = Visibility.Visible;
                PlayerStakeTextBlock.Text = $"Spelare 1:s nummer: {kicker}";

                if (bankPairValue.HasValue)
                {
                    int bankKicker = bankPairValue.Value;

                    if (kicker > bankKicker)
                    {
                        PlayerPoints++;
                        InfoTextBlock1.Text = $"{diceText}, Spelaren vinner omgången!";
                        PlayerPointsTextBlock.Text = $"Poäng: {PlayerPoints}";
                    }
                    else if (kicker < bankKicker)
                    {
                        BankPoints++;
                        InfoTextBlock1.Text = $"{diceText}, Banken vinner omgången!";
                        BankPointsTextBlock.Text = $"Poäng: {BankPoints}";
                    }
                    else
                    {
                        InfoTextBlock1.Text = $"{diceText}, Oavgjort!";
                    }

                    bankPairValue = null;
                    roundEnds = true;
                }
                else
                {
                    playerPairValue = kicker;
                    InfoTextBlock1.Text = $"{diceText}, Banken rullar nu...";
                    InfoTextBlock1.Visibility = Visibility.Visible;

                    DieButton.IsHitTestVisible = false;
                    DieButton.Opacity = 0.5;

                    await Task.Delay(2000);
                    await BankRollAsync(isResponseToPlayer: true);
                    return;
                }
            }
            else
            {
                InfoTextBlock1.Text = $"{diceText}, Spelaren får rulla om...";
                DieButton.IsHitTestVisible = true;
                DieButton.Opacity = 1;
                return;
            }

            if (roundEnds)
            {
                ResetDiceSlots();
                InfoTextBlock1.Visibility = Visibility.Visible;
                await Task.Delay(3000);

                InfoTextBlock1.Text = "Nästa runda...";
                InfoTextBlock1.Visibility = Visibility.Visible;
                await Task.Delay(3000);

                await EndBankTurnAsync();
            }
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
            // Navigate to ResultPage with only bank and player points
            Frame.Navigate(typeof(ResultPage), new object[] { BankPoints, PlayerPoints });
        }

        private void UpdateInfoTextBlock2(int[] dice)
        {
            Array.Sort(dice);
            string diceText = string.Join("-", dice);

            string info2 = "";

            if (dice.SequenceEqual(new[] { 4, 5, 6 }))
            {
                info2 = $"{diceText}, automatisk vinst";
            }
            else if (dice.SequenceEqual(new[] { 1, 2, 3 }))
            {
                info2 = $"{diceText}, automatisk förlust";
            }
            else if (dice[0] == dice[1] && dice[1] == dice[2])
            {
                info2 = $"{diceText}, automatisk vinst";
            }
            else if ((dice[0] == dice[1] && dice[2] == 6) || (dice[1] == dice[2] && dice[0] == 6) || (dice[0] == dice[2] && dice[1] == 6))
            {
                info2 = $"{diceText}, automatisk vinst";
            }
            else if ((dice[0] == dice[1] && dice[2] == 1) || (dice[1] == dice[2] && dice[0] == 1) || (dice[0] == dice[2] && dice[1] == 1))
            {
                info2 = $"{diceText}, automatisk förlust";
            }
            else if (HasPairWithKicker(dice, out int pairVal, out int kicker) && kicker >= 2 && kicker <= 5)
            {
                int target = kicker + 1;
                if (target > 6) target = 6;
                info2 = $"{diceText}, slå en {target}:a eller högre för att vinna";
            }
            else
            {
                info2 = $"{diceText}, ogiltigt resultat";
            }

            InfoTextBlock2.Text = info2;
        }

        // Empty handlers preserved (existing XAML wiring)
        private void BankDieSlot1_Click(object sender, RoutedEventArgs e) { }
        private void BankDieSlot2_Click(object sender, RoutedEventArgs e) { }
        private void BankDieSlot3_Click(object sender, RoutedEventArgs e) { }
        private void DieSlot1_Click(object sender, RoutedEventArgs e) { }
        private void DieSlot2_Click(object sender, RoutedEventArgs e) { }
        private void DieSlot3_Click(object sender, RoutedEventArgs e) { }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e) { }
        private void TextBlock_SelectionChanged_1(object sender, RoutedEventArgs e) { }
        private void BankPointsTextBlock_SelectionChanged(object sender, RoutedEventArgs e) { }
        private void PlayerPointsTextBlock_SelectionChanged_1(object sender, RoutedEventArgs e) { }
        private void InfoTextBlock_SelectionChanged(object sender, RoutedEventArgs e) { }

        private void CasinoChip5Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CasinoChip25Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CasinoChip50Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CasinoChip100Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
