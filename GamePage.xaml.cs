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
    public enum GameMode
    {
        UtanPengar,
        MedBank,
        UtanBank
    }

    public sealed partial class GamePage : Page
    {
        private Random random = new Random();

        // Public so result page or debugging can access them; navigation passes values instead of creating new page instances.
        public int BankPoints = 0;    // meaning: points in UtanPengar, credits in money modes
        public int PlayerPoints = 0;

        // Stakes / credits state
        private int bankStake = 0;
        private int playerStake = 0;

        private int? bankPairValue = null; // for pair+{2..5} comparison
        private GameMode currentMode = GameMode.UtanPengar;

        // track player's pair kicker when player rolls first (so bank can respond in the same round)
        private int? playerPairValue = null;

        // chip values
        private readonly int[] chipValues = new[] { 5, 25, 50, 100 };

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

            // ensure chip buttons are wired to handlers (these methods are implemented in this file)
            CasinoChip5Button.Click += CasinoChipButton_Click;
            CasinoChip25Button.Click += CasinoChipButton_Click;
            CasinoChip50Button.Click += CasinoChipButton_Click;
            CasinoChip100Button.Click += CasinoChipButton_Click;

            this.Loaded += GamePage_Loaded;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Expecting either a string "UtanPengar"/"MedBank"/"UtanBank" or a GameMode enum
            base.OnNavigatedTo(e);
            if (e.Parameter is string s)
            {
                if (Enum.TryParse<GameMode>(s, out var mode)) currentMode = mode;
            }
            else if (e.Parameter is GameMode gm)
            {
                currentMode = gm;
            }
            else if (e.Parameter is object[] arr && arr.Length >= 1 && arr[0] is string s2)
            {
                Enum.TryParse<GameMode>(s2, out currentMode);
            }

            // Initialize UI state depending on mode
            switch (currentMode)
            {
                case GameMode.UtanPengar:
                    // points game, hide stakes UI
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

                    // Keep BankPoints & PlayerPoints as 0
                    BankPointsTextBlock.Text = $"Poäng: {BankPoints}";
                    PlayerPointsTextBlock.Text = $"Poäng: {PlayerPoints}";
                    break;

                case GameMode.MedBank:
                case GameMode.UtanBank:
                    // money game: initialize credits
                    BankPoints = 500;
                    PlayerPoints = 500;

                    BankPointsTextBlock.Text = $"Krediter: {BankPoints}";
                    PlayerPointsTextBlock.Text = $"Krediter: {PlayerPoints}";

                    // show chip buttons & stake textblocks
                    CasinoChip5Button.Visibility = Visibility.Visible;
                    CasinoChip25Button.Visibility = Visibility.Visible;
                    CasinoChip50Button.Visibility = Visibility.Visible;
                    CasinoChip100Button.Visibility = Visibility.Visible;

                    // initially disabled until it's player's turn to stake
                    SetChipButtonsEnabled(false);

                    BankStakeTextBlock.Visibility = Visibility.Visible;
                    PlayerStakeTextBlock.Visibility = Visibility.Visible;
                    BankStakeTextBlock.Text = "Bank Insats: -";
                    PlayerStakeTextBlock.Text = "Spelare Insats: -";
                    break;
            }
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
            if (!isResponseToPlayer)
            {
                // Starting a fresh bank turn
                bankStake = 0;
                playerStake = 0;
                bankPairValue = null;

                ResetDiceSlots();

                // Only hide player stake at the very start of a round
                PlayerStakeTextBlock.Visibility = Visibility.Collapsed;

                if (currentMode == GameMode.UtanPengar)
                {
                    BankStakeTextBlock.Visibility = Visibility.Visible;
                    PlayerStakeTextBlock.Visibility = Visibility.Visible;
                    BankStakeTextBlock.Text = "";
                    PlayerStakeTextBlock.Text = "";
                }
                else
                {
                    BankStakeTextBlock.Text = "Bank Insats: -";
                    PlayerStakeTextBlock.Text = "Spelare Insats: -";
                }
            }
            else
            {
                // Bank responding to player-first roll
                PlayerStakeTextBlock.Visibility = Visibility.Visible;
                // Keep playerStake & player dice intact
            }

            // Show rolling indicator
            InfoTextBlock1.Text = isResponseToPlayer ? "Banken rullar som svar..." : "Banken rullar nu...";
            InfoTextBlock1.Visibility = Visibility.Visible;
            await Task.Delay(3000); // <-- ensures the message lasts 3 seconds
            InfoTextBlock1.Visibility = Visibility.Collapsed;

            // Bank chooses a stake if in money mode (not UtanPengar)
            if (currentMode != GameMode.UtanPengar)
            {
                var validChips = chipValues.Where(c => c <= BankPoints && c <= PlayerPoints).ToArray();
                if (validChips.Length == 0)
                {
                    await HandleEndOfGameByCredits();
                    return;
                }

                bankStake = validChips[random.Next(validChips.Length)];
                BankPoints -= bankStake;
                BankPointsTextBlock.Text = $"Krediter: {BankPoints}";
                BankStakeTextBlock.Text = $"Bank Insats: {bankStake}";

                InfoTextBlock1.Text = $"Banken satsar {bankStake}";
                InfoTextBlock1.Visibility = Visibility.Visible;
                await Task.Delay(3000);
                InfoTextBlock1.Visibility = Visibility.Collapsed;
            }

            int[] dice = RollDice();
            Array.Sort(dice);
            UpdateDiceButtons(dice, BankDieSlot1, BankDieSlot2, BankDieSlot3);

            // Evaluate bank roll
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
                if (currentMode == GameMode.UtanPengar)
                {
                    BankPoints++;
                    InfoTextBlock1.Text = $"{diceText}, Automatisk vinst för banken!";
                    BankPointsTextBlock.Text = $"Poäng: {BankPoints}";
                }
                else
                {
                    await SettleMoneyRound(winnerIsBank: true);
                    InfoTextBlock1.Text = $"{diceText}, Banken vinner omgången!";
                }
                roundEnds = true;
            }
            // --- AUTOMATIC LOSSES ---
            else if (dice.SequenceEqual(new[] { 1, 2, 3 }) ||
                     (dice[0] == dice[1] && dice[2] == 1) ||
                     (dice[1] == dice[2] && dice[0] == 1) ||
                     (dice[0] == dice[2] && dice[1] == 1))
            {
                if (currentMode == GameMode.UtanPengar)
                {
                    PlayerPoints++;
                    InfoTextBlock1.Text = $"{diceText}, Automatisk förlust för banken!";
                    PlayerPointsTextBlock.Text = $"Poäng: {PlayerPoints}";
                }
                else
                {
                    await SettleMoneyRound(winnerIsBank: false);
                    InfoTextBlock1.Text = $"{diceText}, Spelaren vinner omgången!";
                }
                roundEnds = true;
            }
            // --- PAIR + KICKER (2..5) ---
            else if (HasPairWithKicker(dice, out int pairVal, out int kicker) && kicker >= 2 && kicker <= 5)
            {
                // Bank-first: store bankPairValue
                if (!playerPairValue.HasValue)
                {
                    bankPairValue = kicker;

                    if (currentMode == GameMode.UtanPengar)
                        BankStakeTextBlock.Text = $"Bankens nummer: {bankPairValue}";

                    InfoTextBlock1.Text = "Spelare 1:s tur att rulla!";
                    InfoTextBlock1.Visibility = Visibility.Visible;

                    // Enable player controls
                    if (currentMode == GameMode.UtanPengar)
                    {
                        DieButton.Opacity = 1;
                        DieButton.IsHitTestVisible = true;
                    }
                    else
                    {
                        SetChipButtonsEnabled(true);
                    }

                    return;
                }
                else
                {
                    // Player-first: compare immediately
                    int playerKicker = playerPairValue.Value;
                    int bankKicker = kicker;

                    if (bankKicker > playerKicker)
                    {
                        if (currentMode == GameMode.UtanPengar)
                        {
                            BankPoints++;
                            InfoTextBlock1.Text = $"{diceText}, Banken vinner omgången!";
                            BankPointsTextBlock.Text = $"Poäng: {BankPoints}";
                        }
                        else
                        {
                            await SettleMoneyRound(winnerIsBank: true);
                            InfoTextBlock1.Text = $"{diceText}, Banken vinner omgången!";
                        }
                    }
                    else if (bankKicker < playerKicker)
                    {
                        if (currentMode == GameMode.UtanPengar)
                        {
                            PlayerPoints++;
                            InfoTextBlock1.Text = $"{diceText}, Spelaren vinner omgången!";
                            PlayerPointsTextBlock.Text = $"Poäng: {PlayerPoints}";
                        }
                        else
                        {
                            await SettleMoneyRound(winnerIsBank: false);
                            InfoTextBlock1.Text = $"{diceText}, Spelaren vinner omgången!";
                        }
                    }
                    else
                    {
                        InfoTextBlock1.Text = $"{diceText}, Oavgjort!";
                        if (currentMode != GameMode.UtanPengar)
                        {
                            BankPoints += bankStake;
                            PlayerPoints += playerStake;
                            BankPointsTextBlock.Text = $"Krediter: {BankPoints}";
                            PlayerPointsTextBlock.Text = $"Krediter: {PlayerPoints}";
                        }
                    }

                    // Clear stored playerPairValue now that round resolved
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

                // Preserve player UI if player-first
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

            // Alternate who starts next round
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

                if (currentMode == GameMode.UtanPengar)
                {
                    DieButton.Opacity = 1;
                    DieButton.IsHitTestVisible = true;
                }
                else
                {
                    SetChipButtonsEnabled(true);
                }
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
                if (currentMode == GameMode.UtanPengar)
                {
                    PlayerPoints++;
                    InfoTextBlock1.Text = $"{diceText}, Automatisk vinst för spelaren!";
                    PlayerPointsTextBlock.Text = $"Poäng: {PlayerPoints}";
                }
                else
                {
                    await SettleMoneyRound(winnerIsBank: false);
                    InfoTextBlock1.Text = $"{diceText}, Spelaren vinner omgången!";
                }
                roundEnds = true;
            }
            // --- AUTOMATIC LOSSES ---
            else if (dice.SequenceEqual(new[] { 1, 2, 3 }) ||
                     (dice[0] == dice[1] && dice[2] == 1) ||
                     (dice[1] == dice[2] && dice[0] == 1) ||
                     (dice[0] == dice[2] && dice[1] == 1))
            {
                if (currentMode == GameMode.UtanPengar)
                {
                    BankPoints++;
                    InfoTextBlock1.Text = $"{diceText}, Automatisk förlust för spelaren!";
                    BankPointsTextBlock.Text = $"Poäng: {BankPoints}";
                }
                else
                {
                    await SettleMoneyRound(winnerIsBank: true);
                    InfoTextBlock1.Text = $"{diceText}, Banken vinner omgången!";
                }
                roundEnds = true;
            }
            // --- PAIR + KICKER (2..5) ---
            else if (HasPairWithKicker(dice, out int pairVal, out int kicker) && kicker >= 2 && kicker <= 5)
            {
                // Always show player's number text
                PlayerStakeTextBlock.Visibility = Visibility.Visible;
                if (currentMode == GameMode.UtanPengar)
                    PlayerStakeTextBlock.Text = $"Spelare 1:s nummer: {kicker}";
                else
                    PlayerStakeTextBlock.Text = $"Spelare Insats: {kicker}";

                // Bank has already rolled a pair+kicker
                if (bankPairValue.HasValue)
                {
                    int bankKicker = bankPairValue.Value;

                    if (kicker > bankKicker)
                    {
                        if (currentMode == GameMode.UtanPengar)
                        {
                            PlayerPoints++;
                            InfoTextBlock1.Text = $"{diceText}, Spelaren vinner omgången!";
                            PlayerPointsTextBlock.Text = $"Poäng: {PlayerPoints}";
                        }
                        else
                        {
                            await SettleMoneyRound(winnerIsBank: false);
                            InfoTextBlock1.Text = $"{diceText}, Spelaren vinner omgången!";
                        }
                    }
                    else if (kicker < bankKicker)
                    {
                        if (currentMode == GameMode.UtanPengar)
                        {
                            BankPoints++;
                            InfoTextBlock1.Text = $"{diceText}, Banken vinner omgången!";
                            BankPointsTextBlock.Text = $"Poäng: {BankPoints}";
                        }
                        else
                        {
                            await SettleMoneyRound(winnerIsBank: true);
                            InfoTextBlock1.Text = $"{diceText}, Banken vinner omgången!";
                        }
                    }
                    else
                    {
                        InfoTextBlock1.Text = $"{diceText}, Oavgjort!";
                        if (currentMode != GameMode.UtanPengar)
                        {
                            BankPoints += bankStake;
                            PlayerPoints += playerStake;
                            BankPointsTextBlock.Text = $"Krediter: {BankPoints}";
                            PlayerPointsTextBlock.Text = $"Krediter: {PlayerPoints}";
                        }
                    }

                    // Clear stored bankPairValue now that round resolved
                    bankPairValue = null;
                    roundEnds = true;
                }
                else
                {
                    // Player-first: store kicker and let bank respond
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
            // --- INVALID ROLL ---
            else
            {
                InfoTextBlock1.Text = $"{diceText}, Spelaren får rulla om...";
                DieButton.IsHitTestVisible = true;
                DieButton.Opacity = 1;
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

        private void SetChipButtonsEnabled(bool enabled)
        {
            // Enable chip buttons only if allowed by credit constraints.
            // For player enabling: only enable chips <= playerCredits and <= bankCredits (to avoid leaving bank negative).
            void set(Button b, int chip)
            {
                if (!enabled)
                {
                    b.IsHitTestVisible = false;
                    b.Opacity = 0.5;
                    return;
                }
                bool allowed = chip <= PlayerPoints && chip <= BankPoints + bankStake; // bankStake was previously deducted.
                b.IsHitTestVisible = allowed;
                b.Opacity = allowed ? 1.0 : 0.5;
            }

            set(CasinoChip5Button, 5);
            set(CasinoChip25Button, 25);
            set(CasinoChip50Button, 50);
            set(CasinoChip100Button, 100);
        }

        private async void CasinoChipButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button b)) return;

            int chosen = 0;
            if (b == CasinoChip5Button) chosen = 5;
            else if (b == CasinoChip25Button) chosen = 25;
            else if (b == CasinoChip50Button) chosen = 50;
            else if (b == CasinoChip100Button) chosen = 100;

            // Ensure chosen is allowed
            if (chosen <= 0) return;

            // deduct player stake (reserve)
            playerStake = chosen;
            PlayerPoints -= playerStake;
            PlayerStakeTextBlock.Text = $"Spelare Insats: {playerStake}";
            PlayerPointsTextBlock.Text = $"Krediter: {PlayerPoints}";

            // disable chips
            SetChipButtonsEnabled(false);

            // Now let player roll
            InfoTextBlock1.Text = $"Spelaren satsade {playerStake}. Spelaren rullar...";
            InfoTextBlock1.Visibility = Visibility.Visible;
            await Task.Delay(1000);

            // Give control to DieButton roll (simulate pressing it programmatically or enable)
            DieButton.Opacity = 1;
            DieButton.IsHitTestVisible = true;
        }

        private async Task SettleMoneyRound(bool winnerIsBank)
        {
            // Settlement per chosen interpretation:
            // Winner gets 2 * winnerStake added. Loser loses winnerStake (already represented by reserved stakes).
            // Note: Both bankStake and playerStake may be zero depending on how the round progressed.
            if (winnerIsBank)
            {
                // bank wins => bank gets +2 * bankStake and player loses bankStake (player may have already been deducted their own stake)
                BankPoints += 2 * bankStake;
                PlayerPoints -= bankStake; // remove winnerStake from loser as specified
            }
            else
            {
                PlayerPoints += 2 * playerStake;
                BankPoints -= playerStake;
            }

            // If UtanBank (winner takes all) we interpret winner receives sum of both stakes, loser loses only their stake
            if (currentMode == GameMode.UtanBank)
            {
                // We'll do a clean winner-take-all recompute to be safe:
                if (winnerIsBank)
                {
                    BankPoints += (bankStake + playerStake);
                    // player already had playerStake deducted earlier; no further change.
                }
                else
                {
                    PlayerPoints += (bankStake + playerStake);
                }
            }

            // After settlement update textblocks
            if (currentMode != GameMode.UtanPengar)
            {
                BankPointsTextBlock.Text = $"Krediter: {BankPoints}";
                PlayerPointsTextBlock.Text = $"Krediter: {PlayerPoints}";
            }

            // clear stakes
            bankStake = 0;
            playerStake = 0;
            BankStakeTextBlock.Text = currentMode == GameMode.UtanPengar ? "" : "Bank Insats: -";
            PlayerStakeTextBlock.Text = currentMode == GameMode.UtanPengar ? "" : "Spelare Insats: -";
        }

        private async Task HandleEndOfGameByCredits()
        {
            // Display final winner then navigate to ResultPage after 3 seconds
            if (BankPoints <= 0)
            {
                InfoTextBlock1.Text = "Spelaren har bankrutt - Spelaren förlorade!"; // bank = 0 => player won?
            }
            if (PlayerPoints <= 0)
            {
                InfoTextBlock1.Text = "Banken har bankrutt - Banken förlorade!";
            }
            InfoTextBlock1.Visibility = Visibility.Visible;
            await Task.Delay(3000);

            // Navigate to ResultPage with final numbers and mode
            Frame.Navigate(typeof(ResultPage), new object[] { currentMode.ToString(), BankPoints, PlayerPoints });
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
            // Pass mode and final numbers to ResultPage
            Frame.Navigate(typeof(ResultPage), new object[] { currentMode.ToString(), BankPoints, PlayerPoints });
        }

        private void UpdateInfoTextBlock2(int[] dice)
        {
            Array.Sort(dice); // always sort for consistent messages
            string diceText = string.Join("-", dice);

            string info2 = "";

            // Automatic 4-5-6
            if (dice.SequenceEqual(new[] { 4, 5, 6 }))
            {
                info2 = $"{diceText}, automatisk vinst";
            }
            // Automatic 1-2-3
            else if (dice.SequenceEqual(new[] { 1, 2, 3 }))
            {
                info2 = $"{diceText}, automatisk förlust";
            }
            // Triple or pair+6 or pair+1
            else if (dice[0] == dice[1] && dice[1] == dice[2]) // triple
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
            // Pair + kicker 2..5
            else if (HasPairWithKicker(dice, out int pairVal, out int kicker) && kicker >= 2 && kicker <= 5)
            {
                info2 = $"{diceText}, slå en {pairVal + 1}-a eller högre för att vinna";
            }
            // All other rolls
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

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void TextBlock_SelectionChanged_1(object sender, RoutedEventArgs e)
        {

        }

        private void BankPointsTextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void PlayerPointsTextBlock_SelectionChanged_1(object sender, RoutedEventArgs e)
        {

        }

        private void InfoTextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        // Optional: debug helpers to force-a-round (not used)
    }
}
