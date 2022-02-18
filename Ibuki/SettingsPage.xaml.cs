﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Ibuki {
    public sealed partial class SettingsPage : Page {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public SettingsPage() {
            this.InitializeComponent();

            System.Diagnostics.Debug.WriteLine("Settings opened. Fetching...");
            //ImgLimitSlider.Value = MainPage.currentSetings.IMG_LIMIT;

            System.Diagnostics.Debug.WriteLine("Fetched settings. Ready to edit.");
        }

        private void ClearSettingsButton_Click(object sender, RoutedEventArgs e) {

        }
    }
}
