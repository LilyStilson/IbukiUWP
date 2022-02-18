using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Ibuki.Classes {
    public class LockableAppBarToggleButton : AppBarToggleButton {
        protected override void OnToggle() {
            if (!LockToggle) {
                base.OnToggle();
            }
        }

        public bool LockToggle {
            get { return (bool)GetValue(LockToggleProperty); }
            set { SetValue(LockToggleProperty, value); }
        }

        /// Using a DependencyProperty as the backing store for LockToggle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LockToggleProperty =
            DependencyProperty.Register("LockToggle", typeof(bool), typeof(LockableAppBarToggleButton), new PropertyMetadata(false));
    }
}
