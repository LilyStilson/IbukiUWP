using IbukiBooruLibrary.Booru;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

using muxc = Microsoft.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Ibuki {
    public class Tag {
        public Tag(string tag_name) {
            this.tag_name = tag_name;
        }

        public string tag_name { get; set; }
        public string tag_display => tag_name.Replace("_", " ");
    }

    public class Tags {
        private static List<Tag> SetTagsFromString(string value, string separator = " ") {
            List<Tag> result = new List<Tag>();
            foreach (string tag in value.Split(separator).ToList()) {
                if (tag != "")
                    result.Add(new Tag(tag));
            }
            return result;
        }

        private static List<Tag> SetTagsFromArray(string[] value) {
            List<Tag> result = new List<Tag>();
            foreach(string tag in value) {
                if (tag != "")
                    result.Add(new Tag(tag));
            }
            return result;
        }

        public static List<Tag> ProcessTags(JToken value, string separator = " ") {
            return (value.Type == JTokenType.Array)
                ? SetTagsFromArray((string[])value.Values<string>())
                : SetTagsFromString(value.ToString(), separator);
        }

        public List<Tag> CopyrightTags { get; set; }
        public List<Tag> CharacterTags { get; set; }
        public List<Tag> SpeciesTags { get; set; }
        public List<Tag> ArtistTags { get; set; }
        public List<Tag> LoreTags { get; set; }
        public List<Tag> GeneralTags { get; set; }
        public List<Tag> MetaTags { get; set; }
    }

    public class BooruPost {
        public int ID { get; set; }
        public string PreviewImageURL { get; set; }
        public string LargeImageURL { get; set; }
        public Tags PostTags { get; set; }
    }

    //public class AppSettings {
    //    public int LIMIT { get; set; } = 20;
    //    public BooruData Booru { get; set; }
    //}
    
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page {
        private readonly List<(string Tag, Type Page)> _pages = new List<(string Tag, Type Page)> {
            ("dashboard", typeof(DashboardPage)),
            ("settings", typeof(SettingsPage)),
            ("viewer", typeof(ImageViewerPage))
        };

        public MainPage() {
            this.InitializeComponent();

            /// Cache main page, there is not much happens there
            NavigationCacheMode = NavigationCacheMode.Required;

            // Hide default title bar.
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            //UpdateTitleBarLayout(coreTitleBar);

            ApplicationViewTitleBar viewTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            viewTitleBar.ButtonBackgroundColor = Colors.Transparent;
            viewTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            // Set XAML element as a draggable region.
            Window.Current.SetTitleBar(AppTitleBar);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e) {
            DashboardPage.RefreshAll();
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e) {
            // You can also add items in code.
            //NavView.MenuItems.Add(new muxc.NavigationViewItemSeparator());
            /*NavView.MenuItems.Add(new muxc.NavigationViewItem
            {
                Content = "My content",
                Icon = new SymbolIcon((Symbol)0xF1AD),
                Tag = "content"
            });*/
            //_pages.Add(("content", typeof(MyContentPage)));

            // Add handler for NavViewContentFrame navigation.
            NavViewContentFrame.Navigated += On_Navigated;

            // NavView doesn't load any page by default, so load home page.
            //NavView.SelectedItem = NavView.MenuItems[0];
            // If navigation occurs on SelectionChanged, this isn't needed.
            // Because we use ItemInvoked to navigate, we need to call Navigate
            // here to load the home page.
            NavView_Navigate("dashboard", new EntranceNavigationTransitionInfo());

            // Add keyboard accelerators for backwards navigation.
            var goBack = new KeyboardAccelerator { Key = VirtualKey.GoBack };
            goBack.Invoked += BackInvoked;
            this.KeyboardAccelerators.Add(goBack);

            // ALT routes here
            /*var altLeft = new KeyboardAccelerator
            {
                Key = VirtualKey.Left,
                Modifiers = VirtualKeyModifiers.Menu
            };
            altLeft.Invoked += BackInvoked;
            this.KeyboardAccelerators.Add(altLeft);*/
        }

        private void NavView_ItemInvoked(muxc.NavigationView sender,
                                         muxc.NavigationViewItemInvokedEventArgs args) {
            if(args.IsSettingsInvoked == true) {
                NavView_Navigate("settings", args.RecommendedNavigationTransitionInfo);
            } else if(args.InvokedItemContainer != null) {
                var navItemTag = args.InvokedItemContainer.Tag.ToString();
                NavView_Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);
            }
        }

        // NavView_SelectionChanged is not used in this example, but is shown for completeness.
        // You will typically handle either ItemInvoked or SelectionChanged to perform navigation,
        // but not both.
        private void NavView_SelectionChanged(muxc.NavigationView sender,
                                              muxc.NavigationViewSelectionChangedEventArgs args) {
            if(args.IsSettingsSelected == true) {
                NavView_Navigate("settings", args.RecommendedNavigationTransitionInfo);
            } else if(args.SelectedItemContainer != null) {
                var navItemTag = args.SelectedItemContainer.Tag.ToString();
                NavView_Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);
            }
        }

        private void NavView_Navigate(string navItemTag, NavigationTransitionInfo transitionInfo) {
            Type _page = null;
            if(navItemTag == "settings") {
                _page = typeof(SettingsPage);
            } else {
                var item = _pages.FirstOrDefault(p => p.Tag.Equals(navItemTag));
                _page = item.Page;
            }
            // Get the page type before navigation so you can prevent duplicate
            // entries in the backstack.
            var preNavPageType = NavViewContentFrame.CurrentSourcePageType;

            // Only navigate if the selected page isn't currently loaded.
            if(!(_page is null) && !Type.Equals(preNavPageType, _page)) {
                NavViewContentFrame.Navigate(_page, null, transitionInfo);
            }
        }

        private void NavView_BackRequested(muxc.NavigationView sender,
                                           muxc.NavigationViewBackRequestedEventArgs args) {
            On_BackRequested();
        }

        private void BackInvoked(KeyboardAccelerator sender,
                                 KeyboardAcceleratorInvokedEventArgs args) {
            On_BackRequested();
            args.Handled = true;
        }

        private bool On_BackRequested() {
            if(!NavViewContentFrame.CanGoBack)
                return false;

            // Don't go back if the nav pane is overlayed.
            if(NavView.IsPaneOpen && (NavView.DisplayMode == muxc.NavigationViewDisplayMode.Compact || NavView.DisplayMode == muxc.NavigationViewDisplayMode.Minimal))
                return false;

            NavViewContentFrame.GoBack();
            return true;
        }

        private void On_Navigated(object sender, NavigationEventArgs e) {
            NavView.IsBackEnabled = NavViewContentFrame.CanGoBack;

            if(NavViewContentFrame.SourcePageType == typeof(SettingsPage)) {
                // SettingsItem is not part of NavView.MenuItems, and doesn't have a Tag.
                NavView.SelectedItem = (muxc.NavigationViewItem)NavView.SettingsItem;
                AppTitleBarHeader.Text = "Settings";
            } else if(NavViewContentFrame.SourcePageType == typeof(ImageViewerPage)) {
                // BigImage is not part of NavView.MenuItems, and doesn't have a Tag.
                //NavView.SelectedItem = (muxc.NavigationViewItem)NavView.BigImage;
                AppTitleBarHeader.Text = $"ID: {(e.Parameter as Image).Tag}";
            } else if(NavViewContentFrame.SourcePageType != null) {
                var item = _pages.FirstOrDefault(p => p.Page == e.SourcePageType);

                NavView.SelectedItem = NavView.MenuItems.OfType<muxc.NavigationViewItem>().First(n => n.Tag.Equals(item.Tag));

                AppTitleBarHeader.Text = ((muxc.NavigationViewItem)NavView.SelectedItem)?.Content?.ToString();
            }
        }

        private void NavViewContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e) {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
    }
}
