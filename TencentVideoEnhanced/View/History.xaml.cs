using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using TencentVideoEnhanced.Model;
using Windows.UI.Core;
using Windows.Foundation.Metadata;
using Windows.UI;
using Microsoft.Web.WebView2.Core;

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
        private CoreWebView2 coreWebView;
        private ContentDialog dialog;

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

        private async void Init()
        {
            Window.Current.SetTitleBar(TitleArea);
            SystemNavigationManager.BackRequested += BackRequested;
            SystemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            await HistoryWebView.EnsureCoreWebView2Async();
            coreWebView = HistoryWebView.CoreWebView2;
            coreWebView.NewWindowRequested += NewWindowRequested;
            coreWebView.NavigationCompleted += NavigationCompleted;
            coreWebView.NavigationStarting += NavigationStarting;
            Loading.IsActive = true;
            Blur.Visibility = Visibility.Visible;
            coreWebView.Navigate(Url);
        }

        private async void BackRequested(object sender, BackRequestedEventArgs e)
        {
            //返回主页面
            await HistoryWebView.EnsureCoreWebView2Async();
            if (coreWebView.CanGoBack)
            {
                Loading.IsActive = true;
                Blur.Visibility = Visibility.Visible;
                coreWebView.GoBack();
            }
            e.Handled = true;
        }

        private async void NewWindowRequested(CoreWebView2 sender, CoreWebView2NewWindowRequestedEventArgs args)
        {
            string Url = args.Uri.ToString();
            args.Handled = true;
            if (Url.Contains("v.qq.com") && (Url.Contains("cover") || Url.Contains("page")))
            {
                var CurrentFrame = Window.Current.Content as Frame;
                var MainPage = CurrentFrame.Content as MainPage;
                MainPage.MainFrame.Navigate(typeof(VideoPlayer), Url);
            }
            else
            {
                await HistoryWebView.EnsureCoreWebView2Async();
                coreWebView.Navigate(Url);
            }
        }

        private async void NavigationStarting(CoreWebView2 sender, CoreWebView2NavigationStartingEventArgs args)
        {
            Loading.IsActive = true;
            Blur.Visibility = Visibility.Visible;
            if (dialog==null)
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
                    coreWebView.Reload();
                }
            }
        }

        private async void NavigationCompleted(CoreWebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
        {

            foreach (RulesItem item in App.Rules.rules.compact.history)
            {
                if (item.status)
                {
                    try
                    {
                        RemoveElementsByClassName(item.value);
                    }
                    catch (Exception)
                    {
                        ;
                    }
                    
                }
            }

            var width = HistoryWebView.ActualHeight;
            string template = "var elements = document.getElementsByClassName('{{0}}');if (elements.length > 0){elements[0].style.width='{{1}}';elements[0].style.float='left';};document.body.style.overflowX='hidden';";
            template = Utils.TransferTemplate(template);
            string script = string.Format(template, "site_main", width);
            await coreWebView.ExecuteScriptAsync(script);

            template = "var elements = document.getElementsByClassName('{{0}}');if (elements.length > 0){elements[0].style.position='relative';elements[0].style.left='100px';elements[0].style.margin='0px';}";
            template = Utils.TransferTemplate(template);
            script = string.Format(template, "mod_search");
            await coreWebView.ExecuteScriptAsync(script);

            //使所有链接都触发新窗口
            script = "var elements = document.getElementsByTagName('a');for(var i=0;i<elements.length;i++){elements[i].target='_blank';}";
            await coreWebView.ExecuteScriptAsync(script);

            Loading.IsActive = false;
            Blur.Visibility = Visibility.Collapsed;
            try
            {
                if (dialog != null)
                {
                    dialog.Hide();
                    dialog=null;
                }
            }
            catch (Exception)
            {
                ;
            }
        }

        private async void AdaptWebViewWithWindow()
        {
            await HistoryWebView.EnsureCoreWebView2Async();
            var width = HistoryWebView.ActualHeight;
            string template = "var elements = document.getElementsByClassName('{{0}}');if (elements.length > 0){elements[0].style.width='{{1}}px';}";
            template = Utils.TransferTemplate(template);
            string script = string.Format(template, "wrapper_main", width);
            await coreWebView.ExecuteScriptAsync(script);
        }

        private async void RemoveElementsByClassName(string ClassName)
        {
            string template = "while(true){var elements = document.getElementsByClassName('{{0}}');if(elements.length>0){for(var i=0;i<elements.length;i++){elements[i].parentNode.removeChild(elements[i]);} }else{break;} }";
            template = Utils.TransferTemplate(template);
            string script = string.Format(template, ClassName);
            await coreWebView.ExecuteScriptAsync(script);
        }

        private void HistoryWebView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdaptWebViewWithWindow();
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await HistoryWebView.EnsureCoreWebView2Async();
            coreWebView.Reload();
        }

        private void Go_Click(object sender, RoutedEventArgs e)
        {
            Loading.IsActive = false;
            Blur.Visibility = Visibility.Collapsed;
        }
    }
}
