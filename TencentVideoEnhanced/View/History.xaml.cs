using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using TencentVideoEnhanced.Model;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using Windows.UI.Popups;
using Windows.UI.Core;
using Windows.Foundation.Metadata;
using Windows.UI;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace TencentVideoEnhanced.View
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class History : Page
    {
        private string Url = "http://v.qq.com/u/history";
        private SystemNavigationManager SystemNavigationManager = SystemNavigationManager.GetForCurrentView();

        public History()
        {
            this.InitializeComponent();
            RulesItem DisallowCache = Utils.GetRulesItemById("X004");
            if (DisallowCache.status)
            {
                NavigationCacheMode = NavigationCacheMode.Disabled;
            }
            else
            {
                NavigationCacheMode = NavigationCacheMode.Required;
            }
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5))
            {
                Blur.Background = new AcrylicBrush
                {
                    BackgroundSource = AcrylicBackgroundSource.Backdrop,
                    TintColor = Colors.Transparent,
                    TintOpacity = 0.1
                };
            }
            Init();
        }

        private void Init()
        {
            SystemNavigationManager.BackRequested += BackRequested;
            SystemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            Loading.IsActive = true;
            Blur.Visibility = Visibility.Visible;
            HistoryWebView.Navigate(new Uri(Url));
        }

        private void BackRequested(object sender, BackRequestedEventArgs e)
        {
            //返回主页面
            if (HistoryWebView.CanGoBack)
            {
                Loading.IsActive = true;
                Blur.Visibility = Visibility.Visible;
                HistoryWebView.GoBack();
            }
            e.Handled = true;
        }

        private void NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            string Url = args.Uri.ToString();
            args.Handled = true;
            if (Url.Contains("cover") || Url.Contains("page"))
            {
                var CurrentFrame = Window.Current.Content as Frame;
                var MainPage = CurrentFrame.Content as MainPage;
                MainPage.MainFrame.Navigate(typeof(VideoPlayer), Url);
            }
            else
            {
                HistoryWebView.Navigate(args.Uri);
            }
        }

        private async void NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            foreach (RulesItem item in App.Rules.rules.compact.history)
            {
                if (item.status)
                {
                    RemoveElementsByClassName(item.value);
                }
            }
            var width = HistoryWebView.ActualHeight;
            string template = "var elements = document.getElementsByClassName('{{0}}');if (elements.length > 0){elements[0].style.width='{{1}}';elements[0].style.float='left';};document.body.style.overflowX='hidden';";
            template = TransferTemplate(template);
            string script = string.Format(template, "wrapper_main",width);
            await HistoryWebView.InvokeScriptAsync("eval", new string[] { script });

            template = "var elements = document.getElementsByClassName('{{0}}');if (elements.length > 0){elements[0].style.position='relative';elements[0].style.left='100px';elements[0].style.margin='0px';}";
            template = TransferTemplate(template);
            script = string.Format(template, "mod_search");
            await HistoryWebView.InvokeScriptAsync("eval", new string[] { script });
            


            Loading.IsActive = false;
            Blur.Visibility = Visibility.Collapsed;
        }

        private async void AdaptWebViewWithWindow()
        {
            var width = HistoryWebView.ActualHeight;
            string template = "var elements = document.getElementsByClassName('{{0}}');if (elements.length > 0){elements[0].style.width='{{1}}px';}";
            template = TransferTemplate(template);
            string script = string.Format(template, "wrapper_main", width);
            await HistoryWebView.InvokeScriptAsync("eval", new string[] { script });
        }

        private async void RemoveElementsByClassName(string ClassName)
        {
            string template = "while(true){var elements = document.getElementsByClassName('{{0}}');if(elements.length>0){for(var i=0;i<elements.length;i++){elements[i].parentNode.removeChild(elements[i]);} }else{break;} }";
            template = TransferTemplate(template);
            string script = string.Format(template, ClassName);
            await HistoryWebView.InvokeScriptAsync("eval", new string[] { script });
        }

        private string TransferTemplate(string format)
        {
            //替换回String.Format标准格式
            var temp = format.Replace("{{", "_-_").Replace("}}", "-_-");
            temp = temp.Replace("{", "{{").Replace("}", "}}");
            temp = temp.Replace("_-_", "{").Replace("-_-", "}");
            return temp;
        }

        private void HistoryWebView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdaptWebViewWithWindow();
        }
    }
}
