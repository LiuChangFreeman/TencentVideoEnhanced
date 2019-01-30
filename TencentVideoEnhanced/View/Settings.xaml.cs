using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TencentVideoEnhanced.Model;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace TencentVideoEnhanced.View
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Settings : Page
    {
        private LocalObjectStorageHelper LocalObjectStorageHelper = new LocalObjectStorageHelper();
        private bool PageLoaded = false;

        public Settings()
        {
            this.InitializeComponent();
            Init();
        }

        private void Init()
        {
            Loading.IsActive = true;
            SettingsPresenter.Content = App.Rules;
            Loading.IsActive = false;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            PageLoaded = true;
        }

        private async void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            //开关在初始化的时候会自动触发，原因不明
            if (!PageLoaded)
            {
                return;
            }
            //等待数据绑定同步
            await Task.Delay(1);
            LocalObjectStorageHelper.Save("settings", App.Rules.GetSettings());
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            LocalObjectStorageHelper.Save("settings", App.Rules.GetSettings());
        }

        private async void ResetSettings_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog md = new MessageDialog("重置默认设置吗？", "消息提示");
            md.Commands.Add(new UICommand("确定", cmd => { }, "重置"));
            md.Commands.Add(new UICommand("取消", cmd => { }));
            var result = await md.ShowAsync();
            if (result.Id as string == "重置")
            {
                StorageFile JsonRules = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Data/rules.json"));
                string StringRules = await FileIO.ReadTextAsync(JsonRules);
                App.Rules = JsonConvert.DeserializeObject<Rules>(StringRules);
                await LocalObjectStorageHelper.SaveFileAsync("rules", App.Rules);
                LocalObjectStorageHelper.Save("settings", App.Rules.GetSettings());
            }
            md = new MessageDialog("是否重启应用？", "重启应用后设置才能生效");
            md.Commands.Add(new UICommand("确定", cmd => { }, "重启"));
            md.Commands.Add(new UICommand("取消", cmd => { }));
            result = await md.ShowAsync();
            if (result.Id as string == "重启")
            {
                await CoreApplication.RequestRestartAsync(string.Empty);
            }
        }
    }
}
