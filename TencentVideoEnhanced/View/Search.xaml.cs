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
    public sealed partial class Search : Page
    {
        private Uri UriSearch = new Uri("https://v.qq.com/x/search");
        private SystemNavigationManager SystemNavigationManager = SystemNavigationManager.GetForCurrentView();

        public Search()
        {
            this.InitializeComponent();
            RulesItem DisallowCache = Utils.GetRulesItemById("X003");
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
            SearchWebView.Navigate(UriSearch);
        }


        private void BackRequested(object sender, BackRequestedEventArgs e)
        {
            //返回主页面
            if (SearchWebView.CanGoBack)
            {
                SearchWebView.GoBack();
            }
            e.Handled = true;
        }

        private void NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            string Url=args.Uri.ToString();
            args.Handled = true;
            if (Url.Contains("v.qq.com") && (Url.Contains("cover") || Url.Contains("page")))
            {
                var CurrentFrame =Window.Current.Content as Frame;
                var MainPage = CurrentFrame.Content as MainPage;
                MainPage.MainFrame.Navigate(typeof(VideoPlayer), Url);
            }
            else
            {
                SearchWebView.Navigate(args.Uri);
            }
        }

        private void NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            foreach (RulesItem item in App.Rules.rules.compact.search)
            {
                if (item.status)
                {
                    RemoveElementsByClassName(item.value);
                }
            }
            Loading.IsActive = false;
            Blur.Visibility = Visibility.Collapsed;
        }

        private async void RemoveElementsByClassName(string ClassName)
        {
            string template = "while(true){var elements = document.getElementsByClassName('{{0}}');if(elements.length>0){for(var i=0;i<elements.length;i++){elements[i].parentNode.removeChild(elements[i]);} }else{break;} }";
            template = Utils.TransferTemplate(template);
            string script = string.Format(template, ClassName);
            await SearchWebView.InvokeScriptAsync("eval", new string[] { script });
        }
    }
}
