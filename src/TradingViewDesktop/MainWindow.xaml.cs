using CefSharp;
using MahApps.Metro.Controls;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace TradingViewDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        System.Windows.Threading.DispatcherTimer _adblockTimer = new System.Windows.Threading.DispatcherTimer();

        public static INI credentials = new INI("Credentials.ini");
        public MainWindow()
        {
            if (File.Exists("debug.log")){ File.Delete("debug.log"); }
            foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.png").Where(item => item.EndsWith(".png")))
            {
                File.Delete(file);
            }

            InitializeComponent();
            _adblockTimer.Tick += adblockTimer_Tick;
            _adblockTimer.Interval = new TimeSpan(0, 0, 1);
            _adblockTimer.Start();
        }

        private void adblockTimer_Tick(object sender, EventArgs e)
        {
            ClearAds();
        }

        private bool HasUsernameBeenChecked = false;
        private bool IsToolbarVisible = false;
        private void MainBrowser_Loaded(object sender, FrameLoadEndEventArgs e)
        {
            if (!HasUsernameBeenChecked)
            {
                if (credentials.Read("username") == "" || credentials.Read("password") == "")
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        ShowNotification(@"Could not read credentials from the file (`credentials.ini`)");
                    });
                }
                HasUsernameBeenChecked = true;
            }
            ClearAds();
            if (IsToolbarVisible) { return; }
            else
            {
                RevealBrowser();
                RevealToolbar();
                Login();
                IsToolbarVisible = true;
            }
        }

        private async Task ClearAds()
        {
            if (!blockAdsCheckbox.IsChecked.Value) { return; }
            MainBrowser.ExecuteScriptAsync(@"document.getElementsByClassName(""closeButton-10VUlhi4-  closeButtonAdv-2pjmC0Yh-  js-toast__close"")[0].click();");
            await Task.Delay(800);
            MainBrowser.ExecuteScriptAsync(@"document.getElementsByClassName(""js-dialog__action-click js-dialog__no-drag tv-button tv-button--link tv-button--no-padding i-float_left"")[0].click();");
        }

        private void RevealBrowser()
        {
            this.Dispatcher.Invoke(() =>
            {
                loadingStackPanel.Visibility = Visibility.Hidden;
                MainBrowser.Visibility = Visibility.Visible;
            });
        }

        private void RevealToolbar()
        {
            this.Dispatcher.Invoke(() =>
            {
                this.Toolbar.Visibility = Visibility.Visible;
            });
        }

       private void _InstantLogin(object sender, FrameLoadEndEventArgs e)
       {
            MainBrowser.ExecuteScriptAsync($@"document.getElementsByClassName(""tv-control-material-input tv-signin-dialog__input tv-control-material-input__control"")[0].setAttribute(""value"", ""{credentials.Read("username")}"");");
            MainBrowser.ExecuteScriptAsync($@"document.getElementsByClassName(""tv-control-material-input tv-signin-dialog__input tv-control-material-input__control"")[1].setAttribute(""value"", ""{credentials.Read("password")}"");");
            MainBrowser.ExecuteScriptAsync(@"document.getElementsByClassName(""tv-button tv-button--no-border-radius tv-button--size_large tv-button--primary_ghost tv-button--loader"")[0].click();");
            MainBrowser.FrameLoadEnd -= _InstantLogin;
       }

        private void Login()
        {
            this.Dispatcher.Invoke(() =>
            {
                MainBrowser.Address = "https://uk.tradingview.com/#signin";
                MainBrowser.FrameLoadEnd += _InstantLogin;
                try
                {
                    MainBrowser.ExecuteScriptAsync(@"document.getElementsByClassName(""tv-dialog__error tv-dialog__error--dark"")[0].click();");
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            });
        }

        private void SignOut()
        {

        }

        private void ScreenshotButton_Click(object sender, RoutedEventArgs e)
        {
            ScreenCapture sc = new ScreenCapture();
            string _captureName = $@"/captures/{DateTime.Now.ToShortDateString()+DateTime.Now.ToLongTimeString()}.png".Replace("/","_").Replace(":","_");
            sc.CaptureWindowToFile(new WindowInteropHelper(this).Handle, _captureName, ImageFormat.Png);

            BitmapImage bImage = new BitmapImage(new Uri(_captureName, uriKind:UriKind.Relative));
            Clipboard.SetImage(bImage);
            ShowNotification("Chart copied to clipboard");
        }

        private void OpenProfileMenu(object sender, FrameLoadEndEventArgs e)
        {

            MainBrowser.FrameLoadEnd -= OpenProfileMenu;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            MainBrowser.Address = "https://uk.tradingview.com/#signin";
            ShowNotification("Browser reset");
        }

        private bool IsNotificationShowing = false;
        private async Task ShowNotification(string message, int duration = 5000)
        {
            snackbarNotification.IsActive = false;

            IsNotificationShowing = true;
            snackbarNotification.Message.Content = message;
            snackbarNotification.IsActive = true;
            await Task.Delay(duration);
            snackbarNotification.IsActive = false;
            IsNotificationShowing = false;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainBrowser.CanGoBack)
            {
                MainBrowser.Back();
            }
            else
            {
                ShowNotification("Cannot go back");
            }
        }

        private void Forwardbutton_Click(object sender, RoutedEventArgs e)
        {
            if (MainBrowser.CanGoForward)
            {
                MainBrowser.Forward();
            }
            else
            {
                ShowNotification("Cannot go forward");
            }
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            MainBrowser.Address = $"https://uk.tradingview.com/u/{credentials.Read("username")}";
        }

        private void SignoutButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MetroWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }
    }
}
