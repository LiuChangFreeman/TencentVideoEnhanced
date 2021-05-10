using System.Threading;
using Windows.UI.Xaml.Controls;
using TencentVideoEnhanced.Model;
using TencentVideoEnhanced.View;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace TencentVideoEnhanced
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Init();
        }

        private void Init()
        {
            AutoUpdateRules();
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            var TitleBar = ApplicationView.GetForCurrentView().TitleBar;
            TitleBar.BackgroundColor = Colors.Transparent;
            TitleBar.ButtonBackgroundColor = Colors.Transparent;
            TitleBar.ButtonInactiveForegroundColor = Colors.Transparent;
            TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            MainFrame.Navigate(typeof(ShellPage));
        }

        private void AutoUpdateRules()
        {
            RulesItem AutoUpadte = Utils.GetRulesItemById("X005");
            if (AutoUpadte.status)
            {
                Thread thread = new Thread(new ThreadStart(Utils.GetLatestRules));
                thread.Start();
            }
        }
    }
}
