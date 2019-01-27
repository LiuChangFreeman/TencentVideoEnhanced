using System;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using TencentVideoEnhanced.Model;
using TencentVideoEnhanced.View;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json;
using Windows.UI.Core;

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
            MainFrame.Navigate(typeof(ShellPage));
        }

        private async void Init()
        {
            LocalObjectStorageHelper localObjectStorageHelper = new LocalObjectStorageHelper();
            if (!localObjectStorageHelper.KeyExists("rules"))
            {
                StorageFile JsonRules = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/rules.json"));
                string StringRules = await FileIO.ReadTextAsync(JsonRules);
                var rules = JsonConvert.DeserializeObject<Rules>(StringRules);
                localObjectStorageHelper.Save("rules", rules);
            }
        }
    }
}
