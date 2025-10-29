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
        // DiceRoller handles dice generation, updating dice images, and resetting dice slots.
        private DiceRoller diceRoller = new DiceRoller();

        // GameState holds points, pair/kicker values, and round start order.
        private GameState gameState = new GameState();

        public GamePage()
        {
            this.InitializeComponent();

            // Initial UI setup: hide blur overlay, info text, disable dice button
            BlurRectangle.Visibility = Visibility.Collapsed;
            InfoTextBlock1.Visibility = Visibility.Collapsed;
            DieButton.Opacity = 0.5;
            DieButton.IsHitTestVisible = false;

            // Disable player's dice slots to prevent clicking before roll
            DieSlot1.IsHitTestVisible = false;
            DieSlot2.IsHitTestVisible = false;
            DieSlot3.IsHitTestVisible = false;

            // Disable bank's dice slots (clicking is not needed)
            BankDieSlot1.IsHitTestVisible = false;
            BankDieSlot2.IsHitTestVisible = false;
            BankDieSlot3.IsHitTestVisible = false;

            // When page is loaded, trigger the initial game sequence
            this.Loaded += GamePage_Loaded;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Hide all casino chip buttons (not used in current game mode)
            CasinoChip5Button.Visibility = Visibility.Collapsed;
            CasinoChip25Button.Visibility = Visibility.Collapsed;
            CasinoChip50Button.Visibility = Visibility.Collapsed;
            CasinoChip100Button.Visibility = Visibility.Collapsed;

            CasinoChip5Button.IsHitTestVisible = false;
            CasinoChip25Button.IsHitTestVisible = false;
            CasinoChip50Button.IsHitTestVisible = false;
            CasinoChip100Button.IsHitTestVisible = false;

            // Hide bank and player stake text blocks
            BankStakeTextBlock.Visibility = Visibility.Collapsed;
            PlayerStakeTextBlock.Visibility = Visibility.Collapsed;

            // Initialize score display with current points from GameState
            BankPointsTextBlock.Text = $"Poäng: {gameState.BankPoints}";
            PlayerPointsTextBlock.Text = $"Poäng: {gameState.PlayerPoints}";
        }

        // Triggered after page is fully loaded
        private async void GamePage_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(3000); // initial delay before first roll

            InfoTextBlock1.Text = "Banken rullar först"; // display initial info
            InfoTextBlock1.Visibility = Visibility.Visible;
            await Task.Delay(3000); // show message for 3 seconds
            InfoTextBlock1.Visibility = Visibility.Collapsed;

            await BankRollAsync(); // start the bank's turn
        }

        /// <summary>
        /// Handles the bank's dice roll, including optional response to player.
        /// </summary>
        private async Task BankRollAsync(bool isResponseToPlayer = false)
        {
            // Reset all dice slots to default images
            diceRoller.ResetDiceSlots(DieSlot1, DieSlot2, DieSlot3, BankDieSlot1, BankDieSlot2, BankDieSlot3);

            // Show appropriate info depending on whether bank is responding
            InfoTextBlock1.Text = isResponseToPlayer ? "Banken rullar som svar..." : "Banken rullar nu...";
            InfoTextBlock1.Visibility = Visibility.Visible;
            await Task.Delay(3000);
            InfoTextBlock1.Visibility = Visibility.Collapsed;

            // Roll and sort dice, then update bank's dice images
            int[] dice = diceRoller.RollDice();
            Array.Sort(dice);
            diceRoller.UpdateDiceButtons(dice, BankDieSlot1, BankDieSlot2, BankDieSlot3);

            // Evaluate the outcome of the bank's roll
            await EvaluateBankRollAsync(dice);
        }

        /// <summary>
        /// Evaluates the result of the bank's roll and updates points, UI, and game state.
        /// Handles automatic wins/losses, pair+kicker, and rerolls.
        /// </summary>
        private async Task EvaluateBankRollAsync(int[] dice)
        {
            Array.Sort(dice);
            string diceText = string.Join("-", dice);

            // Update secondary info text with detailed dice info
            InfoTextBlock2.Text = DiceEvaluator.GetDiceInfoText(dice);

            bool roundEnds = false; // flag for ending the round

            // --- Automatic wins ---
            if (DiceEvaluator.IsAutomaticWin(dice))
            {
                gameState.BankPoints++;
                InfoTextBlock1.Text = $"{diceText}, Automatisk vinst för banken!";
                BankPointsTextBlock.Text = $"Poäng: {gameState.BankPoints}";
                roundEnds = true;
            }
            // --- Automatic losses ---
            else if (DiceEvaluator.IsAutomaticLoss(dice))
            {
                gameState.PlayerPoints++;
                InfoTextBlock1.Text = $"{diceText}, Automatisk förlust för banken!";
                PlayerPointsTextBlock.Text = $"Poäng: {gameState.PlayerPoints}";
                roundEnds = true;
            }
            // --- Pair + kicker rolls ---
            else if (DiceEvaluator.HasPairWithKicker(dice, out int pairVal, out int kicker) && kicker >= 2 && kicker <= 5)
            {
                if (!gameState.PlayerPairValue.HasValue) // if player hasn't rolled pair yet
                {
                    // Bank sets its kicker value and informs player to roll
                    gameState.BankPairValue = kicker;
                    BankStakeTextBlock.Text = $"Bankens nummer: {gameState.BankPairValue}";
                    InfoTextBlock1.Text = "Spelare 1:s tur att rulla!";
                    InfoTextBlock1.Visibility = Visibility.Visible;

                    // Enable player's dice button
                    DieButton.Opacity = 1;
                    DieButton.IsHitTestVisible = true;
                    return; // wait for player's roll
                }
                else
                {
                    // Compare player's kicker vs bank's kicker
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

                    // Reset pair values after round ends
                    gameState.PlayerPairValue = null;
                    gameState.BankPairValue = null;
                    roundEnds = true;
                }
            }
            // --- Invalid roll, reroll ---
            else
            {
                InfoTextBlock1.Text = $"{diceText}, Banken får rulla om...";
                InfoTextBlock1.Visibility = Visibility.Visible;
                await Task.Delay(3000);

                bool isPlayerFirstResponse = gameState.PlayerPairValue.HasValue;
                await BankRollAsync(isResponseToPlayer: isPlayerFirstResponse);
                return;
            }

            // Handle round end: reset dice and prepare next round
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

        /// <summary>
        /// Ends the bank's turn and decides which player starts the next round.
        /// </summary>
        private async Task EndBankTurnAsync()
        {
            await Task.Delay(3000);

            // Toggle who starts next round
            gameState.BankStartsNextRound = !gameState.BankStartsNextRound;

            if (gameState.BankStartsNextRound)
            {
                InfoTextBlock1.Text = "Banken börjar nästa runda!";
                InfoTextBlock1.Visibility = Visibility.Visible;
                await Task.Delay(2000);
                await BankRollAsync(); // bank rolls first
            }
            else
            {
                InfoTextBlock1.Text = "Spelare 1:s tur att börja!";
                InfoTextBlock1.Visibility = Visibility.Visible;
                await Task.Delay(2000);

                // Enable player's dice button
                DieButton.Opacity = 1;
                DieButton.IsHitTestVisible = true;
            }
        }

        /// <summary>
        /// Handles player dice button click.
        /// Rolls dice and evaluates result.
        /// </summary>
        private async void DieButton_Click(object sender, RoutedEventArgs e)
        {
            // Disable button to prevent double click
            DieButton.Opacity = 0.5;
            DieButton.IsHitTestVisible = false;

            int[] dice = diceRoller.RollDice();
            Array.Sort(dice);

            // Update player's dice images
            diceRoller.UpdateDiceButtons(dice, DieSlot1, DieSlot2, DieSlot3);

            // Evaluate result of player's roll
            await EvaluatePlayerRollAsync(dice);

            // Debug logging
            Trace.WriteLine($"BankPoints: {gameState.BankPoints}");
            Trace.WriteLine($"PlayerPoints: {gameState.PlayerPoints}");
        }

        /// <summary>
        /// Evaluates the player's roll, updates points, handles pair+kicker comparisons and rerolls.
        /// </summary>
        private async Task EvaluatePlayerRollAsync(int[] dice)
        {
            Array.Sort(dice);
            string diceText = string.Join("-", dice);
            InfoTextBlock2.Text = DiceEvaluator.GetDiceInfoText(dice);

            bool roundEnds = false;

            // Automatic win
            if (DiceEvaluator.IsAutomaticWin(dice))
            {
                gameState.PlayerPoints++;
                InfoTextBlock1.Text = $"{diceText}, Automatisk vinst för spelaren!";
                PlayerPointsTextBlock.Text = $"Poäng: {gameState.PlayerPoints}";
                roundEnds = true;
            }
            // Automatic loss
            else if (DiceEvaluator.IsAutomaticLoss(dice))
            {
                gameState.BankPoints++;
                InfoTextBlock1.Text = $"{diceText}, Automatisk förlust för spelaren!";
                BankPointsTextBlock.Text = $"Poäng: {gameState.BankPoints}";
                roundEnds = true;
            }
            // Pair + kicker
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
                    // Player rolls first, bank responds next
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
            // Invalid roll: allow reroll
            else
            {
                InfoTextBlock1.Text = $"{diceText}, Spelaren får rulla om...";
                DieButton.IsHitTestVisible = true;
                DieButton.Opacity = 1;
                return;
            }

            if (roundEnds)
            {
                // Reset dice and start next round
                diceRoller.ResetDiceSlots(DieSlot1, DieSlot2, DieSlot3, BankDieSlot1, BankDieSlot2, BankDieSlot3);
                InfoTextBlock1.Visibility = Visibility.Visible;
                await Task.Delay(3000);

                InfoTextBlock1.Text = "Nästa runda...";
                InfoTextBlock1.Visibility = Visibility.Visible;
                await Task.Delay(3000);

                await EndBankTurnAsync();
            }
        }

        // --- Info popup handlers ---
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

        // Navigate to ResultPage passing bank/player points
        private void EndButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ResultPage), new object[] { gameState.BankPoints, gameState.PlayerPoints });
        }

        // --- Empty event handlers preserved for XAML wiring ---
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
