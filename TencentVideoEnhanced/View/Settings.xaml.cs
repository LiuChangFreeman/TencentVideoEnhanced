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
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
        private Rules rules;
        private LocalObjectStorageHelper localObjectStorageHelper = new LocalObjectStorageHelper();
        private bool loaded = false;

        public Settings()
        {
            this.InitializeComponent();
            Init();
        }

        private void Init()
        {
            rules = localObjectStorageHelper.Read<Rules>("rules");           
            SettingsPresenter.Content = rules;
        }

        private async void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (!loaded)
            {
                return;
            }
            await Task.Delay(1);
            localObjectStorageHelper.Save("rules",rules);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            loaded = true;
        }
    }
}
