﻿using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using TencentVideoEnhanced.Model;
using Windows.ApplicationModel.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using System.Net.Http;
using System.Net;

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
                App.Rules = await LocalObjectStorageHelper.ReadFileAsync<Rules>("rules_origin");
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

        private async void UpdateSettings_Click(object sender, RoutedEventArgs e)
        {
            Rules Rules;
            using (HttpClient HttpClient = new HttpClient())
            {
                HttpClient.Timeout = TimeSpan.FromSeconds(10);
                var HttpResopnse = await HttpClient.GetAsync(Utils.UpdateRulesUri);
                if (HttpResopnse.StatusCode == HttpStatusCode.OK)
                {
                    string StringRules = await HttpResopnse.Content.ReadAsStringAsync();
                    Rules = JsonConvert.DeserializeObject<Rules>(StringRules);
                    if (Rules == null)
                    {
                        MessageDialog md = new MessageDialog("未知错误", "更新失败");
                        md.Commands.Add(new UICommand("确定", cmd => { }));
                        await md.ShowAsync();
                        return;
                    }
                    if (Rules.version > App.Rules.version)
                    {
                        MessageDialog md = new MessageDialog("请问是否更新？", "已检测到新的更新");
                        md.Commands.Add(new UICommand("确定", cmd => { },"更新"));
                        md.Commands.Add(new UICommand("取消", cmd => { }));
                        var result = await md.ShowAsync();
                        if (result.Id as string == "更新")
                        {
                            LocalObjectStorageHelper LocalObjectStorageHelper = new LocalObjectStorageHelper();
                            await LocalObjectStorageHelper.SaveFileAsync("rules_origin", Rules);
                            await LocalObjectStorageHelper.SaveFileAsync("rules", Rules);
                            var NewSettings = Rules.GetSettings();
                            var OldSettings = App.Rules.GetSettings();
                            foreach (var Item in OldSettings)
                            {
                                if (NewSettings.ContainsKey(Item.Key))
                                {
                                    NewSettings[Item.Key] = Item.Value;
                                }
                            }
                            LocalObjectStorageHelper.Save("settings", NewSettings);
                            Rules.SetSettings(NewSettings);
                            App.Rules = Rules;
                            md = new MessageDialog("某些设置将在重启后生效", "更新成功!");
                            md.Commands.Add(new UICommand("确定", cmd => { }));
                            await md.ShowAsync();
                        }
                    }
                    else
                    {
                        MessageDialog md = new MessageDialog("您无需更新", "已经是最新版本了");
                        md.Commands.Add(new UICommand("确定", cmd => { }));
                        await md.ShowAsync();
                    }
                }
                else
                {
                    MessageDialog md = new MessageDialog("请检查您的网络", "更新失败");
                    md.Commands.Add(new UICommand("确定", cmd => { }));
                    await md.ShowAsync();
                }
            }
        }

        private async void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            //在初始化的时候会自动触发，原因不明
            if (!PageLoaded)
            {
                return;
            }
            //等待数据绑定同步
            await Task.Delay(1);
            await LocalObjectStorageHelper.SaveFileAsync("rules", App.Rules);
        }
    }
}
