using Cee_lo.Cee_loGame;
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
        private DiceRoller diceRoller = new DiceRoller();
        private GameState gameState = new GameState();

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

            // Hide casino chips
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

            BankPointsTextBlock.Text = $"Poäng: {gameState.BankPoints}";
            PlayerPointsTextBlock.Text = $"Poäng: {gameState.PlayerPoints}";
        }

        private async void GamePage_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(3000);

            InfoTextBlock1.Text = "Banken rullar först";
            InfoTextBlock1.Visibility = Visibility.Visible;
            await Task.Delay(3000);
            InfoTextBlock1.Visibility = Visibility.Collapsed;

            await BankRollAsync();
        }

        private async Task BankRollAsync(bool isResponseToPlayer = false)
        {
            diceRoller.ResetDiceSlots(DieSlot1, DieSlot2, DieSlot3, BankDieSlot1, BankDieSlot2, BankDieSlot3);

            InfoTextBlock1.Text = isResponseToPlayer ? "Banken rullar som svar..." : "Banken rullar nu...";
            InfoTextBlock1.Visibility = Visibility.Visible;
            await Task.Delay(3000);
            InfoTextBlock1.Visibility = Visibility.Collapsed;

            int[] dice = diceRoller.RollDice();
            Array.Sort(dice);
            diceRoller.UpdateDiceButtons(dice, BankDieSlot1, BankDieSlot2, BankDieSlot3);

            await EvaluateBankRollAsync(dice);
        }

        private async Task EvaluateBankRollAsync(int[] dice)
        {
            Array.Sort(dice);
            string diceText = string.Join("-", dice);
            InfoTextBlock2.Text = DiceEvaluator.GetDiceInfoText(dice);

            bool roundEnds = false;

            if (DiceEvaluator.IsAutomaticWin(dice))
            {
                gameState.BankPoints++;
                InfoTextBlock1.Text = $"{diceText}, Automatisk vinst för banken!";
                BankPointsTextBlock.Text = $"Poäng: {gameState.BankPoints}";
                roundEnds = true;
            }
            else if (DiceEvaluator.IsAutomaticLoss(dice))
            {
                gameState.PlayerPoints++;
                InfoTextBlock1.Text = $"{diceText}, Automatisk förlust för banken!";
                PlayerPointsTextBlock.Text = $"Poäng: {gameState.PlayerPoints}";
                roundEnds = true;
            }
            else if (DiceEvaluator.HasPairWithKicker(dice, out int pairVal, out int kicker) && kicker >= 2 && kicker <= 5)
            {
                if (!gameState.PlayerPairValue.HasValue)
                {
                    gameState.BankPairValue = kicker;
                    BankStakeTextBlock.Text = $"Bankens nummer: {gameState.BankPairValue}";
                    InfoTextBlock1.Text = "Spelare 1:s tur att rulla!";
                    InfoTextBlock1.Visibility = Visibility.Visible;

                    DieButton.Opacity = 1;
                    DieButton.IsHitTestVisible = true;
                    return;
                }
                else
                {
                    int playerKicker = gameState.PlayerPairValue.Value;
                    int bankKicker = kicker;

                    if (bankKicker > playerKicker)
                    {
                        gameState.BankPoints++;
                        InfoTextBlock1.Text = $"{diceText}, Banken vinner omgången!";
                        BankPointsTextBlock.Text = $"Poäng: {gameState.BankPoints}";
                    }
                    else if (bankKicker < playerKicker)
                    {
                        gameState.PlayerPoints++;
                        InfoTextBlock1.Text = $"{diceText}, Spelaren vinner omgången!";
                        PlayerPointsTextBlock.Text = $"Poäng: {gameState.PlayerPoints}";
                    }
                    else
                    {
                        InfoTextBlock1.Text = $"{diceText}, Oavgjort!";
                    }

                    gameState.PlayerPairValue = null;
                    gameState.BankPairValue = null;
                    roundEnds = true;
                }
            }
            else
            {
                InfoTextBlock1.Text = $"{diceText}, Banken får rulla om...";
                InfoTextBlock1.Visibility = Visibility.Visible;
                await Task.Delay(3000);

                bool isPlayerFirstResponse = gameState.PlayerPairValue.HasValue;
                await BankRollAsync(isResponseToPlayer: isPlayerFirstResponse);
                return;
            }

            if (roundEnds)
            {
                diceRoller.ResetDiceSlots(DieSlot1, DieSlot2, DieSlot3, BankDieSlot1, BankDieSlot2, BankDieSlot3);
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

            gameState.BankStartsNextRound = !gameState.BankStartsNextRound;

            if (gameState.BankStartsNextRound)
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

        private async void DieButton_Click(object sender, RoutedEventArgs e)
        {
            DieButton.Opacity = 0.5;
            DieButton.IsHitTestVisible = false;

            int[] dice = diceRoller.RollDice();
            Array.Sort(dice);
            diceRoller.UpdateDiceButtons(dice, DieSlot1, DieSlot2, DieSlot3);
            await EvaluatePlayerRollAsync(dice);

            Trace.WriteLine($"BankPoints: {gameState.BankPoints}");
            Trace.WriteLine($"PlayerPoints: {gameState.PlayerPoints}");
        }

        private async Task EvaluatePlayerRollAsync(int[] dice)
        {
            Array.Sort(dice);
            string diceText = string.Join("-", dice);
            InfoTextBlock2.Text = DiceEvaluator.GetDiceInfoText(dice);

            bool roundEnds = false;

            if (DiceEvaluator.IsAutomaticWin(dice))
            {
                gameState.PlayerPoints++;
                InfoTextBlock1.Text = $"{diceText}, Automatisk vinst för spelaren!";
                PlayerPointsTextBlock.Text = $"Poäng: {gameState.PlayerPoints}";
                roundEnds = true;
            }
            else if (DiceEvaluator.IsAutomaticLoss(dice))
            {
                gameState.BankPoints++;
                InfoTextBlock1.Text = $"{diceText}, Automatisk förlust för spelaren!";
                BankPointsTextBlock.Text = $"Poäng: {gameState.BankPoints}";
                roundEnds = true;
            }
            else if (DiceEvaluator.HasPairWithKicker(dice, out int pairVal, out int kicker) && kicker >= 2 && kicker <= 5)
            {
                PlayerStakeTextBlock.Visibility = Visibility.Visible;
                PlayerStakeTextBlock.Text = $"Spelare 1:s nummer: {kicker}";

                if (gameState.BankPairValue.HasValue)
                {
                    int bankKicker = gameState.BankPairValue.Value;

                    if (kicker > bankKicker)
                    {
                        gameState.PlayerPoints++;
                        InfoTextBlock1.Text = $"{diceText}, Spelaren vinner omgången!";
                        PlayerPointsTextBlock.Text = $"Poäng: {gameState.PlayerPoints}";
                    }
                    else if (kicker < bankKicker)
                    {
                        gameState.BankPoints++;
                        InfoTextBlock1.Text = $"{diceText}, Banken vinner omgången!";
                        BankPointsTextBlock.Text = $"Poäng: {gameState.BankPoints}";
                    }
                    else
                    {
                        InfoTextBlock1.Text = $"{diceText}, Oavgjort!";
                    }

                    gameState.BankPairValue = null;
                    roundEnds = true;
                }
                else
                {
                    gameState.PlayerPairValue = kicker;
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
                diceRoller.ResetDiceSlots(DieSlot1, DieSlot2, DieSlot3, BankDieSlot1, BankDieSlot2, BankDieSlot3);
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
            Frame.Navigate(typeof(ResultPage), new object[] { gameState.BankPoints, gameState.PlayerPoints });
        }

        // Empty handlers preserved
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

        private void CasinoChip5Button_Click(object sender, RoutedEventArgs e) { }
        private void CasinoChip25Button_Click(object sender, RoutedEventArgs e) { }
        private void CasinoChip50Button_Click(object sender, RoutedEventArgs e) { }
        private void CasinoChip100Button_Click(object sender, RoutedEventArgs e) { }
    }
}