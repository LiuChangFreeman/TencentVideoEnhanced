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

        private void HambugerB_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SplitView.IsPaneOpen = !SplitView.IsPaneOpen;
        }

        private void HambugerA_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SplitView.IsPaneOpen = !SplitView.IsPaneOpen;
        }

        private void Settings_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(Settings));
        }

        //private void Home_Tapped(object sender, TappedRoutedEventArgs e)
        //{
        //    ContentFrame.Navigate(typeof(VideoPlayer), "https://v.qq.com/x/cover/nphyo88qm4m6i6s.html");
        //}

        private void Video_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(VideoPlayer), "resume from main page");
        }

        private void Search_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(Search));
        }
    }
}
