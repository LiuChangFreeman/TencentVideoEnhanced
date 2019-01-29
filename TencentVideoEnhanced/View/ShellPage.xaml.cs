using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI;
using TencentVideoEnhanced.Model;
using TencentVideoEnhanced.View;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json;
using Windows.ApplicationModel.Core;


// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace TencentVideoEnhanced.View
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ShellPage : Page
    {
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
            ContentFrame.Navigate(typeof(Search));
        }

        private void Hambuger_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SplitView.IsPaneOpen = !SplitView.IsPaneOpen;
        }

        private void Settings_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(Settings));
        }

        private void Video_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //从播放页回到设置页会栈溢出，原因不明。解决办法是进入视频页之前，先进入搜索页
            if (ContentFrame.Content is Settings)
            {
                ContentFrame.Navigate(typeof(Search));
            }
            Frame.Navigate(typeof(VideoPlayer), "resume from main page");
        }

        private void Search_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(Search));
        }
    }
}
