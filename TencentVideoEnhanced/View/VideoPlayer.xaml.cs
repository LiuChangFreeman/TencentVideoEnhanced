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
using Newtonsoft.Json;
using TencentVideoEnhanced.Model;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Core;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace TencentVideoEnhanced.View
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class VideoPlayer : Page
    {
        private Uri UriSearch = new Uri("https://v.qq.com/x/search");
        private Rules Rules;
        private bool InitSuccess = false;
        private LocalObjectStorageHelper LocalObjectStorageHelper = new LocalObjectStorageHelper();
        private string CurrentUrl = "";
        private string DefaultVideoUrl = "https://v.qq.com/x/cover/ocjepullqnzm7d9/x00262vbmzt.html";
        //默认视频为《创造101》

        public VideoPlayer()
        {
            this.InitializeComponent();
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5))
            {
                Blur.Background = new AcrylicBrush
                {
                    BackgroundSource = AcrylicBackgroundSource.Backdrop,
                    TintColor = Colors.Transparent,
                    TintOpacity = 0.1
                };
            }
            SystemNavigationManager SystemNavigationManager = SystemNavigationManager.GetForCurrentView();
            SystemNavigationManager.BackRequested += BackRequested;
            SystemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string Url= e.Parameter as string;
            if (Url=="resume from main page"&& CurrentUrl != "")
            {
                return;
            }
            if (CurrentUrl != Url)
            {
                if (CurrentUrl == "")
                {
                    Url = DefaultVideoUrl;
                }
                //当新的视频需要加载时
                Loading.IsActive = true;
                Blur.Visibility = Visibility.Visible;
                Rules = LocalObjectStorageHelper.Read<Rules>("rules");
                MainWebView.Navigate(new Uri(Url));
                CurrentUrl = Url;
            }
        }

        private void BackRequested(object sender, BackRequestedEventArgs e)
        {
            //返回主页面
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
            e.Handled = true;
        }

        private string transfer(string format)
        {
            //替换回String.Format标准格式
            var temp = format.Replace("{{", "_-_").Replace("}}", "-_-");
            temp = temp.Replace("{", "{{").Replace("}", "}}");
            temp = temp.Replace("_-_", "{").Replace("-_-", "}");
            return temp;
        }

        private async void AdaptViewForWindow()
        {
            //窗口改变大小时，重新调整元素的宽高
            string template = "var width=document.body.clientWidth;var elements = document.getElementsByClassName('{{0}}');if (elements.length > 0){elements[0].style.width=width+'px';}";
            template = transfer(template);
            var script = string.Format(template, "container_inner");
            await MainWebView.InvokeScriptAsync("eval", new string[] { script });
            template = "var elements = document.getElementsByClassName('{{0}}');if (elements.length > 0){elements[0].style.height=\"100%\";}";
            template = transfer(template);
            script = string.Format(template, "mod_player");
            await MainWebView.InvokeScriptAsync("eval", new string[] { script });
            template = "var width=document.body.clientWidth;var elements = document.getElementsByClassName('{{0}}');if (elements.length > 0){elements[0].style.width=width-320+'px';}";
            template = transfer(template);
            script = string.Format(template, "mod_player_section");
            await MainWebView.InvokeScriptAsync("eval", new string[] { script });
            var height = MainWebView.ActualHeight;
            template = "var elements = document.getElementsByClassName('{{0}}');if (elements.length > 0){elements[0].style.height=\"{{1}}px\";}";
            template = transfer(template);
            script = string.Format(template, "mod_player_section", height);
            await MainWebView.InvokeScriptAsync("eval", new string[] { script });
            script = string.Format(template, "scroll_wrap", height);
            await MainWebView.InvokeScriptAsync("eval", new string[] { script });
        }

        private void NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            foreach (RulesData item in Rules.rules.remove.NavigationCompleted)
            {
                if (item.status)
                {
                    RemoveElementsByClassName(item.value);
                }
            }
            foreach (RulesData item in Rules.rules.eval.NavigationCompleted)
            {
                if (item.status)
                {
                    ClickElementByClassName(item.value);
                }
            }
            AdaptViewForWindow();
            InitSuccess = true;
            Loading.IsActive = false;
            Blur.Visibility = Visibility.Collapsed;
        }

        private void DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            foreach (RulesData item in Rules.rules.remove.DOMContentLoaded)
            {
                if (item.status)
                {
                    RemoveElementsByClassName(item.value);
                }
            }
            foreach (RulesData item in Rules.rules.eval.DOMContentLoaded)
            {
                if (item.status)
                {
                    ClickElementByClassName(item.value);
                }
            }
        }

        private async void RemoveElementsByClassName(string ClassName)
        {
            string template = "while(true){var element = document.getElementsByClassName('{{0}}');if(element.length>0){element[0].parentNode.removeChild(element[0]);}else{break;} }";
            template = transfer(template);
            string script = string.Format(template, ClassName);
            await MainWebView.InvokeScriptAsync("eval", new string[] { script });
        }

        private async void ClickElementByClassName(string ClassName)
        {
            string template = "var elements = document.getElementsByClassName('{{0}}');if (elements.length > 0){elements[0].click();}";
            template = transfer(template);
            string script = string.Format(template, ClassName);
            await MainWebView.InvokeScriptAsync("eval", new string[] { script });
        }

        private void ContainsFullScreenElementChanged(WebView sender, object args)
        {
            var applicationView = ApplicationView.GetForCurrentView();

            if (sender.ContainsFullScreenElement)
            {
                applicationView.TryEnterFullScreenMode();
            }
            else if (applicationView.IsFullScreenMode)
            {
                applicationView.ExitFullScreenMode();
            }
        }

        private void NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            MainWebView.Navigate(args.Uri);
            args.Handled = true;
            Loading.IsActive = true;
            Blur.Visibility = Visibility.Visible;
        }

        private async void SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!InitSuccess)
            {
                return;
            }
            await Task.Delay(100);
            AdaptViewForWindow();
        }
    }
}
