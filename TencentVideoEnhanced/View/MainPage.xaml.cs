using System;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using TencentVideoEnhanced.Model;
using TencentVideoEnhanced.View;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace TencentVideoEnhanced
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Rules Rules;

        public MainPage()
        {
            this.InitializeComponent();
            Init();
        }

        private async void Init()
        {
            LocalObjectStorageHelper LocalObjectStorageHelper = new LocalObjectStorageHelper();
            if (!LocalObjectStorageHelper.KeyExists("rules"))
            {
                StorageFile JsonRules = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/rules.json"));
                string StringRules = await FileIO.ReadTextAsync(JsonRules);
                Rules = JsonConvert.DeserializeObject<Rules>(StringRules);
                LocalObjectStorageHelper.Save("rules", Rules);
            }
            else
            {
                Rules = LocalObjectStorageHelper.Read("rules",Rules);
            }
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            var TitleBar = ApplicationView.GetForCurrentView().TitleBar;
            TitleBar.BackgroundColor = Colors.Transparent;
            TitleBar.ButtonBackgroundColor = Colors.Transparent;
            TitleBar.ButtonInactiveForegroundColor = Colors.Transparent;
            TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            MainFrame.Navigate(typeof(ShellPage));
        }
    }
}
