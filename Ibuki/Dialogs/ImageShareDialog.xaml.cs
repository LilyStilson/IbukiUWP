using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Ibuki.Dialogs {
    public sealed partial class ImageShareDialog : ContentDialog {
        string ImageFileUrl;

        public ImageShareDialog(string fileUrlToShare) {
            this.InitializeComponent();

            ImageFileUrl = fileUrlToShare;
        }

        private void CopyImageButton_Click(object sender, RoutedEventArgs e) {
            // Create data package - our container
            DataPackage Package = new DataPackage();

            // Add current image to this package and send it to System's clipboard
            Package.SetBitmap(RandomAccessStreamReference.CreateFromUri(new Uri(ImageFileUrl)));
            Clipboard.SetContent(Package);

            // Finally, hide the dialog
            Hide();
        }

        private void CopyLinkButton_Click(object sender, RoutedEventArgs e) {
            // Create data package - our container
            DataPackage Package = new DataPackage();

            // Add current image url as text to this package and send it to System's clipboard
            Package.SetText(ImageFileUrl);
            Clipboard.SetContent(Package);

            // Finally, hide the dialog
            Hide();
        }

        private void MoreOptionsButton_Click(object sender, RoutedEventArgs e) {
            //
        }
    }
}
