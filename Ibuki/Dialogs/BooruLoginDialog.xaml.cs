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
using Ibuki;
using System.Threading.Tasks;
using Ibuki.Utils;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Windows.Security.Credentials;
using Ibuki.Classes;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Ibuki.Dialogs {
    public sealed partial class BooruLoginDialog : ContentDialog {
        private string BooruDisplayName = "";
        private async Task<int> TryLogin() {
            /// Start ProgressBar
            Progress.IsEnabled = true;
            StatusText.Visibility = Visibility.Visible;
            StatusText.Text = "Signing in...";

            Dictionary<string, string> AuthData = new Dictionary<string, string> {
                { "USERNAME", Login.Text }
            };
            if (UseAPIKey.IsChecked == true)
                AuthData.Add("API_KEY", APIKey.Text);
            else
                AuthData.Add("PASSWORD", Password.Text);

            JObject Response = JsonConvert.DeserializeObject<JObject>(await Helpers.GET(Helpers.FormatBooruUri(new Uri(DashboardPage.booru.BaseURI, DashboardPage.booru.userEndpoints.GET_LoginAPIKey), AuthData)));
            Progress.IsEnabled = false;

            JToken UserID;

            if (Response.TryGetValue(DashboardPage.booru.user.ID, out UserID)) {
                StatusText.Text = "Successfully signed in!";
                return UserID.Value<int>();
            } else {
                StatusText.Text = $"Can't sign in: {Response.GetValue("message").Value<string>()}";
                return -1;
            }

        }

        public BooruLoginDialog(string title) {
            this.InitializeComponent();
            DialogTitle.Text = DialogTitle.Text.Replace("{BOORU}", title);
            BooruDisplayName = title;
            Progress.IsEnabled = false;
            StatusText.Visibility = Visibility.Collapsed;
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {
            int UserID = await TryLogin();
            if (UserID != -1) {
                await Task.Delay(2000);
                MainPage.CurrentSettings.Credentials.Add(new PasswordCredentials() {
                    Booru = BooruDisplayName,
                    ID = UserID,
                    IsAPIKey = UseAPIKey.IsChecked == true,
                    Username = Login.Text,
                    Password = UseAPIKey.IsChecked == true ? APIKey.Text : Password.Text,
                });
                MainPage.CurrentSettings.Credentials.Last().IsActive = true;
                System.Diagnostics.Debug.WriteLine(JsonConvert.SerializeObject(MainPage.CurrentSettings));
                Hide();
                DashboardPage.RefreshAll();
            }
        }

        private void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args) {
            if(args.Result == ContentDialogResult.Primary)
                args.Cancel = true;
        }
    }
}
