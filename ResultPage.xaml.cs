using Cee_lo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Cee_lo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ResultPage : Page
    {
        public ResultPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is object[] arr && arr.Length >= 3 && arr[0] is string modeStr)
            {
                var mode = modeStr;
                int bank = Convert.ToInt32(arr[1]);
                int player = Convert.ToInt32(arr[2]);

                // show mode and numbers using Swedish labels
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