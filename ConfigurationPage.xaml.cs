using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Gaming.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Cee_lo
{
    public sealed partial class ConfigurationPage : Page
    {
        public ConfigurationPage()
        {
            this.InitializeComponent();
            BlurRectangle.Visibility = Visibility.Collapsed;
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

        private void UtanPengarButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(GamePage), "UtanPengar");
        }

        private void MedBankButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(GamePage), "MedBank");
        }

        private void UtanBankButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(GamePage), "UtanBank");
        }

    }
}