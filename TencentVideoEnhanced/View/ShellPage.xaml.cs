using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.Foundation.Metadata;
using Windows.UI;
using TencentVideoEnhanced.Model;


// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace TencentVideoEnhanced.View
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ShellPage : Page
    {
        private SolidColorBrush Selected= new SolidColorBrush(Color.FromArgb(255, 23, 101, 168));
        private SolidColorBrush Normal= new SolidColorBrush(Colors.Transparent);

        public ShellPage()
        {
            this.InitializeComponent();
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5))
            {
                SplitView.PaneBackground = new AcrylicBrush
                {
                    BackgroundSource = AcrylicBackgroundSource.Backdrop,
                    TintColor = Colors.Transparent,
                    TintOpacity = 0.1
                };
            }
            RulesItem DefaultPage = Utils.GetRulesItemById("X002");
            if (DefaultPage.status)
            {
                History.Background = Selected;
                ContentFrame.Navigate(typeof(History));
            }
            else
            {
                Search.Background = Selected;
                ContentFrame.Navigate(typeof(Search));
            }
        }

        private void Hambuger_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SplitView.IsPaneOpen = !SplitView.IsPaneOpen;
        }

        private void Settings_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Search.Background = Normal;
            History.Background = Normal;
            Settings.Background = Selected;
            ContentFrame.Navigate(typeof(Settings));
        }

        private void Video_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //从播放页回到设置页会栈溢出，原因不明。解决办法是进入视频页之前，先进入搜索页
            if (ContentFrame.Content is Settings)
            {
                RulesItem DefaultPage = Utils.GetRulesItemById("X002");
                if (DefaultPage.status)
                {
                    Search.Background = Normal;
                    History.Background = Selected;
                    Settings.Background = Normal;
                    ContentFrame.Navigate(typeof(History));
                }
                else
                {
                    Search.Background = Selected;
                    History.Background = Normal;
                    Settings.Background = Normal;
                    ContentFrame.Navigate(typeof(Search));
                }

            }
            Frame.Navigate(typeof(VideoPlayer), "resume from main page");
        }

        private void Search_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Search.Background = Selected;
            History.Background = Normal;
            Settings.Background = Normal;
            ContentFrame.Navigate(typeof(Search));
        }

        private void History_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Search.Background = Normal;
            History.Background = Selected;
            Settings.Background = Normal;
            ContentFrame.Navigate(typeof(History));
        }
    }
}
