using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using TencentVideoEnhanced.Model;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Core;
using Microsoft.Web.WebView2.Core;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace TencentVideoEnhanced.View
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Search : Page
    {
        private Uri UriSearch = new Uri("https://v.qq.com/x/search/");
        private SystemNavigationManager SystemNavigationManager = SystemNavigationManager.GetForCurrentView();
        private CoreWebView2 coreWebView;
        private ContentDialog dialog;

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

            RulesItem MoreInformation = Utils.GetRulesItemById("X007");
            if (MoreInformation.status)
            {
                UriSearch = new Uri("https://v.qq.com/?ptag=qqbsc");
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

        private async void Init()
        {
            SystemNavigationManager.BackRequested += BackRequested;
            SystemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            Loading.IsActive = true;
            Blur.Visibility = Visibility.Visible;
            await SearchWebView.EnsureCoreWebView2Async();
            coreWebView = SearchWebView.CoreWebView2;
            coreWebView.NewWindowRequested += NewWindowRequested;
            coreWebView.NavigationCompleted += NavigationCompleted;
            coreWebView.NavigationStarting += NavigationStarting;
            coreWebView.Navigate(UriSearch.ToString());
        }


        private async void NavigationStarting(CoreWebView2 sender, CoreWebView2NavigationStartingEventArgs args)
        {
            Loading.IsActive = true;
            Blur.Visibility = Visibility.Visible;
            if (dialog == null)
            {
                dialog = new ContentDialog();
                dialog.Title = "等待加载中...";
                dialog.PrimaryButtonText = "跳过";
                dialog.SecondaryButtonText = "刷新";
                dialog.Content = "如果加载时间过长，您可以选择跳过等待或者刷新";
                dialog.DefaultButton = ContentDialogButton.Primary;
                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    dialog = null;
                    Loading.IsActive = false;
                    Blur.Visibility = Visibility.Collapsed;
                }
                else if (result == ContentDialogResult.Secondary)
                {
                    dialog = null;
                    await SearchWebView.EnsureCoreWebView2Async();
                    coreWebView.Reload();
                }
            }
        }

        private void NavigationCompleted(CoreWebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
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
            try
            {
                if (dialog != null)
                {
                    dialog.Hide();
                    dialog = null;
                }
            }
            catch (Exception)
            {
                ;
            }
        }

        private async void BackRequested(object sender, BackRequestedEventArgs e)
        {
            await SearchWebView.EnsureCoreWebView2Async();
            //返回主页面
            if (SearchWebView.CanGoBack)
            {
                SearchWebView.GoBack();
            }
            e.Handled = true;
        }

        private async void NewWindowRequested(CoreWebView2 sender, CoreWebView2NewWindowRequestedEventArgs args)
        {
            await SearchWebView.EnsureCoreWebView2Async();
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
                coreWebView.Navigate(Url);
            }
        }

        private async void RemoveElementsByClassName(string ClassName)
        {
            await SearchWebView.EnsureCoreWebView2Async();
            string template = "while(true){var elements = document.getElementsByClassName('{{0}}');if(elements.length>0){for(var i=0;i<elements.length;i++){elements[i].parentNode.removeChild(elements[i]);} }else{break;} }";
            template = Utils.TransferTemplate(template);
            string script = string.Format(template, ClassName);
            await coreWebView.ExecuteScriptAsync(script);
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await SearchWebView.EnsureCoreWebView2Async();
            coreWebView.Reload();
        }
    }
}
