﻿using Cee_lo;
using System;
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
    public sealed partial class ResultPage : Page
    {
        public ResultPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is int[] scores && scores.Length == 2)
            {
                int bankPoints = scores[0];
                int playerPoints = scores[1];
                BankTextBlock.Text = $"Bank Poäng: {bankPoints}";
                PlayerTextBlock.Text = $"Spelare 1 Poäng: {playerPoints}";

                if (bankPoints > playerPoints) 
                {
                    ResultatTextBlock.Text = "Banken vann!";
                }
                else if(playerPoints > bankPoints)
                {
                    ResultatTextBlock.Text = "Spelare 1 vann!";
                }
                else
                {
                    ResultatTextBlock.Text = "Oavgjort.";
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