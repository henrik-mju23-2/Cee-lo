using Cee_lo;
using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Cee_lo
{
    public sealed partial class ResultPage : Page
    {
        public ResultPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is object[] arr && arr.Length >= 2)
            {
                int bank = Convert.ToInt32(arr[0]);
                int player = Convert.ToInt32(arr[1]);

                // show points using Swedish labels
                BankTextBlock.Text = $"Bank: {bank}";
                PlayerTextBlock.Text = $"Spelare 1: {player}";

                // Determine winner
                if (bank > player)
                {
                    ResultatTextBlock.Text = "Banken vann!";
                    Rectangle2_Copy.Fill = new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0xD4, 0x17, 0x17));
                    Rectangle2_Copy1.Fill = new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0xD4, 0x17, 0x17));
                    Rectangle2_Copy2.Fill = new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0xD4, 0x17, 0x17));
                    Rectangle2_Copy3.Fill = new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0xD4, 0x17, 0x17));
                }
                else if (player > bank)
                {
                    ResultatTextBlock.Text = "Spelare 1 vann!";
                }
                else
                {
                    ResultatTextBlock.Text = "Oavgjort!";
                }
            }
        }

        private void ResultatPageBackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(StartPage));
        }

        private void ResultatTextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {
        }
    }
}
