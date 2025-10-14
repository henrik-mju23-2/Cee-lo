﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class GamePage : Page
    {
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
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            InformationPopUp.IsOpen = true;
            BlurRectangle.Visibility = Visibility.Visible;
        }

        private void DieButton_Click(object sender, RoutedEventArgs e)
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

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            InformationPopUp.IsOpen = false;
            BlurRectangle.Visibility = Visibility.Collapsed;
        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void EndButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ResultPage));
        }

        private void InfoTextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void PlayerPointsTextBlock_SelectionChanged_1(object sender, RoutedEventArgs e)
        {

        }

        private void BankPointsTextBlock_SelectionChanged(object sender, RoutedEventArgs e)
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
    }
}
