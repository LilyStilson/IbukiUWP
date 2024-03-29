﻿using Ibuki.Classes;
using Ibuki.Dialogs;
using Ibuki.Utils;
using IbukiBooruLibrary.Booru;
using Microsoft.Toolkit.Collections;
using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Ibuki {
    public class BooruSource : IIncrementalSource<BooruPost> {
        private readonly List<BooruPost> _posts;
        private readonly BooruData _booru;
        public string Search { get; set; } = "";

        public BooruSource(BooruData booru) {
            _posts = new List<BooruPost>();
            _booru = booru;
        }

        public async Task<IEnumerable<BooruPost>> CreatePosts(int limit = 20, int page = 1, string search = "") {
            List<BooruPost> result = new List<BooruPost>();

            PasswordCredentials credentials = MainPage.CurrentSettings.GetActiveCredentialsForActiveBooru();
            string AuthString = "";

            if (credentials != null) {
                AuthString = MainPage.CurrentSettings.GetFormatedCredentialsForActiveBooru();
            }

            JObject[] BooruPosts = JsonConvert.DeserializeObject<JObject[]>(await Helpers.GET(Helpers.FormatBooruUri(
                new Uri(_booru.BaseURI, _booru.postEndpoints.GET_Posts),
                new Dictionary<string, string> { { "LIMIT", $"{limit}" }, { "TAGS", search }, { "PAGE", $"{page}" }, { "AUTH", AuthString } }
            )));

            foreach (JObject post in BooruPosts) {
                if(post.SelectToken(_booru.post.ID) != null && post.SelectToken(_booru.post.PreviewImageURL) != null && post.SelectToken(_booru.post.LargeImageURL) != null) {
                    result.Add(new BooruPost {
                        ID = int.Parse(post.SelectToken(_booru.post.ID).ToString()),
                        PreviewImageURL = post.SelectToken(_booru.post.PreviewImageURL).ToString(),
                        LargeImageURL = post.SelectToken(_booru.post.LargeImageURL).ToString(),
                        PostTags = new Tags {
                            CopyrightTags = _booru.post.CopyrightTags != "" ? Tags.ProcessTags(post.SelectToken(_booru.post.CopyrightTags)) : null,
                            CharacterTags = _booru.post.CharacterTags != "" ? Tags.ProcessTags(post.SelectToken(_booru.post.CharacterTags)) : null,
                            SpeciesTags = _booru.post.SpeciesTags != "" ? Tags.ProcessTags(post.SelectToken(_booru.post.SpeciesTags)) : null,
                            ArtistTags = _booru.post.ArtistTags != "" ? Tags.ProcessTags(post.SelectToken(_booru.post.ArtistTags)) : null,
                            GeneralTags = _booru.post.GeneralTags != "" ? Tags.ProcessTags(post.SelectToken(_booru.post.GeneralTags)) : null,
                            LoreTags = _booru.post.LoreTags != "" ? Tags.ProcessTags(post.SelectToken(_booru.post.LoreTags)) : null,
                            MetaTags = _booru.post.MetaTags != "" ? Tags.ProcessTags(post.SelectToken(_booru.post.MetaTags)) : null,
                        }
                    });
                }
            }

#if DEBUG
            System.Diagnostics.Debug.WriteLine($"Successfully loaded {result.Count} posts");
#endif

            return result;
        }

        public async Task<IEnumerable<BooruPost>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default) {
            return await CreatePosts(pageSize, pageIndex, Search);
        }
    }

    public sealed partial class DashboardPage : Page {
        public static bool Initialized { get; set; } = false;
        public Image clickedImage;

        public static BooruData booru { get; set; } //= JsonConvert.DeserializeObject<BooruData>(BooruPresets.Danbooru().GetAwaiter().GetResult());

        public static IncrementalLoadingCollectionEx<BooruSource, BooruPost> BooruCollection/* = new IncrementalLoadingCollectionEx<BooruSource, BooruPost>(new BooruSource(booru), 20,
            onStartLoading: () => {

            },
            onEndLoading: () => {
                System.Diagnostics.Debug.WriteLine($"Finished loading {BooruCollection.Count} items from {BooruCollection.CurrentPageIndex - 1}");
                Initialized = true;
            },
            onError: async (Exception ex) => {
                if (ex.GetType() == typeof(System.Net.Http.HttpRequestException)) {
                    NetworkErrorDialog dlg = new NetworkErrorDialog(ex.Message + '\n' + ex.StackTrace);
                    if (await dlg.ShowAsync() == ContentDialogResult.Primary) {
                        await BooruCollection.RefreshAsync(1);
}
                } else {
                    UnhadledExceptionDialog dlg = new UnhadledExceptionDialog(ex.GetType().FullName + '\n' + ex.Message + '\n' + ex.StackTrace);
}
            }
        )*/;

        public static void RefreshAll() {
            BooruCollection.RefreshAsync();
        }

        public DashboardPage() {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);

            var anim = ConnectedAnimationService.GetForCurrentView().GetAnimation("ToBooroGrid");

            if(anim != null && clickedImage != null) {
                anim.TryStart(clickedImage);
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e) {
            base.OnNavigatingFrom(e);
            if(e.SourcePageType == typeof(ImageViewerPage)) {
                var anim = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ToBigImage", clickedImage);
                anim.Configuration = new DirectConnectedAnimationConfiguration();
            }
        }

        private void GridRefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args) {
            RefreshAll();
        }

        private void ImageGrid_ItemClick(object sender, ItemClickEventArgs e) {
            clickedImage = (ImageGrid.ContainerFromItem(e.ClickedItem) as GridViewItem).FindDescendant<Image>();
            ImageViewerPage.CurrentPost = BooruCollection.FirstOrDefault(post => post.ID.ToString() == clickedImage.Tag.ToString());

            Frame.Navigate(typeof(ImageViewerPage), clickedImage, new SuppressNavigationTransitionInfo());
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) {
            if (booru == null)
                if (MainPage.CurrentSettings.Boorus.Count > 0)
                    booru = MainPage.CurrentSettings.Boorus[MainPage.CurrentSettings.ActiveBooru];
                    
                //JsonConvert.DeserializeObject<BooruData>(await BooruPresets.Danbooru());
            if (BooruCollection == null) {
                BooruCollection = new IncrementalLoadingCollectionEx<BooruSource, BooruPost>(new BooruSource(booru), 20,
                    onStartLoading: () => {

                    },
                    onEndLoading: () => {
                        System.Diagnostics.Debug.WriteLine($"Finished loading {BooruCollection.Count} items from {BooruCollection.CurrentPageIndex - 1}");
                        Initialized = true;
                    },
                    onError: async (Exception ex) => {
                        if (ex.GetType() == typeof(System.Net.Http.HttpRequestException)) {
                            NetworkErrorDialog dlg = new NetworkErrorDialog(ex.Message + '\n' + ex.StackTrace);
                            if (await dlg.ShowAsync() == ContentDialogResult.Primary) {
                                await BooruCollection.RefreshAsync(1);
                            }
                        } else {
                            UnhadledExceptionDialog dlg = new UnhadledExceptionDialog(ex.GetType().FullName + '\n' + ex.Message + '\n' + ex.StackTrace);
                        }
                    }
                );

                ImageGrid.ItemsSource = BooruCollection;
            }
        }
    }
}
