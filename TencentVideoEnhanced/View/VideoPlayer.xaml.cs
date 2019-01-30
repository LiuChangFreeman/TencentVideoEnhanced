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
using Windows.ApplicationModel.Core;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace TencentVideoEnhanced.View
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class VideoPlayer : Page
    {
        private Uri UriSearch = new Uri("https://v.qq.com/x/search");
        private bool InitSuccess = false;
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
                    TintOpacity = 0.5
                };
            }
            Init();
        }

        private void Init()
        {
            SystemNavigationManager SystemNavigationManager = SystemNavigationManager.GetForCurrentView();
            SystemNavigationManager.BackRequested += BackRequested;
            SystemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string Url= e.Parameter as string;
            if (CurrentUrl != ""&& Url == "resume from main page")
            {
                return;
            }
            if (CurrentUrl != Url)
            {
                if (CurrentUrl == ""&& Url == "resume from main page")
                {
                    Url = DefaultVideoUrl;
                }
                //当新的视频需要加载时
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

        private string TransferTemplate(string format)
        {
            //替换回String.Format标准格式
            var temp = format.Replace("{{", "_-_").Replace("}}", "-_-");
            temp = temp.Replace("{", "{{").Replace("}", "}}");
            temp = temp.Replace("_-_", "{").Replace("-_-", "}");
            return temp;
        }

        private async void AdaptWebViewWithWindow()
        {
            //窗口改变大小时，重新调整元素的宽高
            string template = "var width=document.body.clientWidth;var elements = document.getElementsByClassName('{{0}}');if (elements.length > 0){elements[0].style.width=width+'px';}";
            template = TransferTemplate(template);
            var script = string.Format(template, "container_inner");
            await MainWebView.InvokeScriptAsync("eval", new string[] { script });
            template = "var elements = document.getElementsByClassName('{{0}}');if (elements.length > 0){elements[0].style.height='100%';}";
            template = TransferTemplate(template);
            script = string.Format(template, "mod_player");
            await MainWebView.InvokeScriptAsync("eval", new string[] { script });
            template = "var width=document.body.clientWidth;var elements = document.getElementsByClassName('{{0}}');if (elements.length > 0){elements[0].style.width=width-320+'px';}";
            template = TransferTemplate(template);
            script = string.Format(template, "mod_player_section");
            await MainWebView.InvokeScriptAsync("eval", new string[] { script });
            var height = MainWebView.ActualHeight;
            template = "var elements = document.getElementsByClassName('{{0}}');if (elements.length > 0){elements[0].style.height='{{1}}px';}";
            template = TransferTemplate(template);
            script = string.Format(template, "mod_player_section", height);
            await MainWebView.InvokeScriptAsync("eval", new string[] { script });
            script = string.Format(template, "scroll_wrap", height);
            await MainWebView.InvokeScriptAsync("eval", new string[] { script });
        }

        private void NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            Loading.IsActive = true;
            Blur.Visibility = Visibility.Visible;
            Information.Visibility = Visibility.Visible;
            Information.Text = "等待网页响应......";
        }

        private void DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            AdaptWebViewWithWindow();
            Information.Text = "正在加载内容......";
        }

        private async void NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            foreach (RulesItem item in App.Rules.rules.eval)
            {
                if (item.status)
                {
                    EvalScripts(item.value);
                }
            }
            foreach (RulesItem item in App.Rules.rules.compact.video)
            {
                if (item.status)
                {
                    RemoveElementsByClassName(item.value);
                }
            }
            await Task.Delay(1000);
            InitSuccess = true;
            Loading.IsActive = false;
            Blur.Visibility = Visibility.Collapsed;
            Information.Visibility = Visibility.Collapsed;
        }

        private async void EvalScripts(string script)
        {
            await MainWebView.InvokeScriptAsync("eval", new string[] { script });
        }

        private void RemoveElementsByClassName(string ClassName)
        {
            string template = "while(true){var elements = document.getElementsByClassName('{{0}}');if(elements.length>0){for(var i=0;i<elements.length;i++){elements[i].parentNode.removeChild(elements[i]);} }else{break;} }";
            template = TransferTemplate(template);
            string script = string.Format(template, ClassName);
            EvalScripts(script);
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
            args.Handled = true;
            string Url = args.Uri.ToString();
            if (Url.Contains("cover") || Url.Contains("page"))
            {
                MainWebView.Navigate(args.Uri);
            }
        }

        private new void SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!InitSuccess)
            {
                return;
            }
            AdaptWebViewWithWindow();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            MainWebView.Refresh();
        }

        private async void FrameDOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            //移除开通VIP窗口，避免内购认证失败。弹出窗口中有一个iframe，很幸运可以方便地解决问题，否则就难办了
            string script = "var vip=document.getElementsByClassName('tvip_bd');if(vip.length>0){vip[0].style.visibility=\"hidden\";}";
            await MainWebView.InvokeScriptAsync("eval", new string[] { script });
        }

        private new void Loaded(object sender, RoutedEventArgs e)
        {
            Loading.IsActive = false;
            Blur.Visibility = Visibility.Collapsed;
            Information.Visibility = Visibility.Collapsed;
        }

        private void Go_Click(object sender, RoutedEventArgs e)
        {
            Loading.IsActive = false;
            Blur.Visibility = Visibility.Collapsed;
            Information.Visibility = Visibility.Collapsed;
        }
    }
}
