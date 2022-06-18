using System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using TencentVideoEnhanced.Model;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Core;
using System.Threading;
using Microsoft.Web.WebView2.Core;
using Windows.Storage;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace TencentVideoEnhanced.View
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class VideoPlayer : Page
    {
        private bool InitSuccess = false;
        private string CurrentUrl = "";
        private string DefaultVideoUrl = "https://v.qq.com/";
        private CoreWebView2 coreWebView;
        private ContentDialog dialog;

        public VideoPlayer()
        {
            this.InitializeComponent();
            Init();
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5))
            {
                Blur.Background = new AcrylicBrush
                {
                    BackgroundSource = AcrylicBackgroundSource.Backdrop,
                    TintColor = Colors.Transparent,
                    TintOpacity = 0.5
                };
            } 
        }

        private async void Init()
        {
            SystemNavigationManager SystemNavigationManager = SystemNavigationManager.GetForCurrentView();
            SystemNavigationManager.BackRequested += BackRequested;
            SystemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            Window.Current.SetTitleBar(TitleArea);
            await MainWebView.EnsureCoreWebView2Async();
            coreWebView = MainWebView.CoreWebView2;
            coreWebView.DOMContentLoaded += DOMContentLoaded;
            coreWebView.ContainsFullScreenElementChanged += ContainsFullScreenElementChanged;
            coreWebView.NewWindowRequested += NewWindowRequested;
            coreWebView.NavigationCompleted += NavigationCompleted;
            coreWebView.NavigationStarting += NavigationStarting;
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            string showTeachingTipRefresh = localSettings.Values["ShowTeachingTipRefresh"] as string;
            if (showTeachingTipRefresh != "false")
            {
                TeachingTip.IsOpen = true;
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await MainWebView.EnsureCoreWebView2Async();
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
                coreWebView.Navigate(Url);
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

        private async void AdaptWebViewWithWindow()
        {
            //窗口改变大小时，重新调整元素的宽高
            string template = "var width=document.body.clientWidth;var elements = document.getElementsByClassName('{{0}}');if (elements.length > 0){elements[0].style.width=width+'px';}";
            template = Utils.TransferTemplate(template);
            var script = string.Format(template, "container-main");
            await coreWebView.ExecuteScriptAsync(script);
            template = "var elements = document.getElementsByClassName('{{0}}');if (elements.length > 0){elements[0].style.height='100%';}";
            template = Utils.TransferTemplate(template);
            script = string.Format(template, "container-main");
            await coreWebView.ExecuteScriptAsync(script);
            template = "var width=document.body.clientWidth;var elements = document.getElementsByClassName('{{0}}');if (elements.length > 0){elements[0].style.width=width-320+'px';}";
            template = Utils.TransferTemplate(template);
            script = string.Format(template, "container-main__left");
            await coreWebView.ExecuteScriptAsync(script);
            var height = MainWebView.ActualHeight;
            template = "var elements = document.getElementsByClassName('{{0}}');if (elements.length > 0){elements[0].style.height='{{1}}px';}";
            template = Utils.TransferTemplate(template);
            script = string.Format(template, "player__wrapper", height);
            await coreWebView.ExecuteScriptAsync(script);
            script = string.Format(template, "container-main", height);
            await coreWebView.ExecuteScriptAsync(script);
        }

        private void DOMContentLoaded(CoreWebView2 sender, CoreWebView2DOMContentLoadedEventArgs args)
        {
            foreach (RulesItem item in App.Rules.rules.compact.video)
            {
                if (item.status)
                {
                    RemoveElementsByClassName(item.value);
                }
            }
            AdaptWebViewWithWindow();
        }


        private void injectWebview()
        {
            foreach (RulesItem item in App.Rules.rules.eval)
            {
                if (item.status)
                {
                    try
                    {
                        EvalScripts(item.value);
                    }
                    catch (Exception)
                    {
                        ;
                    }
                }
            }
            foreach (RulesItem item in App.Rules.rules.compact.video)
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
                    coreWebView.Reload();
                }
            }
        }

        private void NavigationCompleted(CoreWebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            injectWebview();
            InitSuccess = true;
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
            RulesItem TimeLine = Utils.GetRulesItemById("X006");
            if (TimeLine.status)
            {
                Thread thread = new Thread(new ThreadStart(AddToTimeLine));
                thread.Start();
            }
        }

        private async void EvalScripts(string script)
        {
            await coreWebView.ExecuteScriptAsync(script);
        }

        private void RemoveElementsByClassName(string ClassName)
        {
            string template = "while(true){var elements = document.getElementsByClassName('{{0}}');if(elements.length>0){for(var i=0;i<elements.length;i++){elements[i].parentNode.removeChild(elements[i]);} }else{break;} }";
            template = Utils.TransferTemplate(template);
            string script = string.Format(template, ClassName);
            EvalScripts(script);
        }

        private void ContainsFullScreenElementChanged(CoreWebView2 sender, object args)
        {
            var applicationView = ApplicationView.GetForCurrentView();
            if (sender.ContainsFullScreenElement)
            {
                applicationView.TryEnterFullScreenMode();
                Refresh.Visibility = Visibility.Collapsed;
            }
            else if (applicationView.IsFullScreenMode)
            {
                applicationView.ExitFullScreenMode();
                Refresh.Visibility = Visibility.Visible;
            }
        }

        private async void NewWindowRequested(CoreWebView2 sender, CoreWebView2NewWindowRequestedEventArgs args)
        {
            await MainWebView.EnsureCoreWebView2Async();
            string Url = args.Uri.ToString();
            if (Url.Contains("v.qq.com") &&(Url.Contains("cover") || Url.Contains("page")))
            {
                args.Handled = true;
                coreWebView.Navigate(Url);
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

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await MainWebView.EnsureCoreWebView2Async();
            coreWebView.Reload();
        }


        private async Task<Activity> GetActivity()
        {
            Activity Activity = new Activity();
            string template = "var metas = document.getElementsByTagName('meta');for(var i=0;i<metas.length;i++){if(metas[i].getAttribute('itemprop')=='{{0}}'){metas[i].content;break;} }";
            template = Utils.TransferTemplate(template);
            var script = string.Format(template, "url");
            Activity.url = await coreWebView.ExecuteScriptAsync(script);
            script = string.Format(template, "image");
            Activity.image = await coreWebView.ExecuteScriptAsync(script);
            template = "var metas = document.getElementsByTagName('meta');for(var i=0;i<metas.length;i++){if(metas[i].name=='{{0}}'){metas[i].content;break;} }";
            template = Utils.TransferTemplate(template);
            script = string.Format(template, "title");
            Activity.title = await coreWebView.ExecuteScriptAsync(script);
            script = string.Format(template, "description");
            Activity.description = await coreWebView.ExecuteScriptAsync(script);
            return Activity;
        }

        private async void AddToTimeLine()
        {
            RulesItem TimeLine = Utils.GetRulesItemById("X006");
            int TimeDelay = int.Parse(TimeLine.value)*60*1000;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                Activity PrevousActivity =await GetActivity();
                await Task.Delay(TimeDelay);
                try
                {
                    Activity NowActivity = await GetActivity();
                    if (NowActivity == PrevousActivity)
                    {
                        Utils.AddToTimeLine(NowActivity);
                    }
                }
                catch (Exception)
                {
                    ;
                }
            });
        }

        private void TeachingTip_CloseButtonClick(Microsoft.UI.Xaml.Controls.TeachingTip sender, object args)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["ShowTeachingTipRefresh"] = "false";
        }
    }
}
