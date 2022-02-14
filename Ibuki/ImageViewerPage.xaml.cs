using Ibuki.Dialogs;
using Ibuki.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Ibuki {
    public sealed partial class ImageViewerPage : Page {
        public bool ImageLoaded { get; set; } = false;
        public static BooruPost CurrentPost { get; set; }
        public BooruPost PrevShowedPost { get; set; } = new BooruPost {
            ID = 0
        };

        public static List<string> ImageExt = new List<string>() { "png", "jpg", "jpeg", "tiff", "gif" };
        public static List<string> VideoExt = new List<string>() { "webm", "mp4", "avi", "mov", "zip" };

        public ObservableCollection<Tag> CopyrightTags = new ObservableCollection<Tag>();
        public ObservableCollection<Tag> CharacterTags = new ObservableCollection<Tag>();
        public ObservableCollection<Tag> ArtistTags = new ObservableCollection<Tag>();
        public ObservableCollection<Tag> GeneralTags = new ObservableCollection<Tag>();
        public ObservableCollection<Tag> MetaTags = new ObservableCollection<Tag>();

        public ImageViewerPage() {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;

            ImageLoaded = false;

            ZoomView.ChangeView(0, 0, 1, false);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);

            if(e.NavigationMode == NavigationMode.Back) {
                /// Something seriously gone wrong if this page became a target for "Back" navigation...
            }

            if(PrevShowedPost.ID != CurrentPost.ID) {
                /// If the image we clicked in grid is different from currently cached in page
                
                /// Clear previous image
                BigImage.Source = null;
                ImageLoaded = false;

                CopyrightTags.Clear();
                CharacterTags.Clear();
                ArtistTags.Clear();
                GeneralTags.Clear();
                MetaTags.Clear();

                /// Assign clicked image as a placeholder for animation
                Placeholder.Source = (e.Parameter as Image).Source;

                /// Start transition animation
                ConnectedAnimation anim = ConnectedAnimationService.GetForCurrentView().GetAnimation("ToBigImage");
                if(anim != null) {
                    anim.TryStart(Placeholder);
                }

                BigImage.Source = new BitmapImage(new Uri(CurrentPost.LargeImageURL));

                
                CurrentPost.PostTags.CopyrightTags.ForEach(item => CopyrightTags.Add(item));
                CurrentPost.PostTags.CharacterTags.ForEach(item => CharacterTags.Add(item));
                CurrentPost.PostTags.ArtistTags.ForEach(item => ArtistTags.Add(item));
                CurrentPost.PostTags.GeneralTags.ForEach(item => GeneralTags.Add(item));
                CurrentPost.PostTags.MetaTags.ForEach(item => MetaTags.Add(item));
            } else {
                /// We are opening previously cached image here, so we won't be loading this image anew
                
                ConnectedAnimation anim = ConnectedAnimationService.GetForCurrentView().GetAnimation("ToBigImage");
                if (anim != null) {
                    if (ImageLoaded) {
                        anim.TryStart(BigImage);
                    } else {
                        anim.TryStart(Placeholder);
                    }
                }
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e) {
            base.OnNavigatingFrom(e);

            /// Make connected animation back to grid
            if(e.NavigationMode == NavigationMode.Back) {
                ConnectedAnimation anim;
                if(ImageLoaded) {
                    anim = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ToBooroGrid", BigImage);
                } else {
                    anim = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ToBooroGrid", Placeholder);
                }
                anim.Configuration = new DirectConnectedAnimationConfiguration();
            }
        }

        private void ZoomView_SizeChanged(object sender, SizeChangedEventArgs e) {
            BigImage.Height = ZoomView.ViewportHeight - 64;
        }

        private void BigImage_ImageOpened(object sender, RoutedEventArgs e) {
            Placeholder.Source = null;
            ImageLoaded = true;
            PrevShowedPost = CurrentPost;
        }

        private async void Share_Click(object sender, RoutedEventArgs e) {
            ContentDialog dialog = new ImageShareDialog(CurrentPost.LargeImageURL);
            await dialog.ShowAsync();
        }

        private async void Browser_Click(object sender, RoutedEventArgs e) {
            /// TODO: Fix Danbooru preset!!! 
            await Windows.System.Launcher.LaunchUriAsync(Helpers.FormatBooruUri(new Uri(DashboardPage.booru.BaseURI, DashboardPage.booru.postEndpoints.GET_Post), new Dictionary<string, string> { { "ID", $"{CurrentPost.ID}" } }));
        }
    }
}
