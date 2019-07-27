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
            Window.Current.SetTitleBar(TitleArea);
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

        private async void AdaptWebViewWithWindow()
        {
            //窗口改变大小时，重新调整元素的宽高
            string template = "var width=document.body.clientWidth;var elements = document.getElementsByClassName('{{0}}');if (elements.length > 0){elements[0].style.width=width+'px';}";
            template = Utils.TransferTemplate(template);
            var script = string.Format(template, "container_inner");
            await MainWebView.InvokeScriptAsync("eval", new string[] { script });
            template = "var elements = document.getElementsByClassName('{{0}}');if (elements.length > 0){elements[0].style.height='100%';}";
            template = Utils.TransferTemplate(template);
            script = string.Format(template, "mod_player");
            await MainWebView.InvokeScriptAsync("eval", new string[] { script });
            template = "var width=document.body.clientWidth;var elements = document.getElementsByClassName('{{0}}');if (elements.length > 0){elements[0].style.width=width-320+'px';}";
            template = Utils.TransferTemplate(template);
            script = string.Format(template, "mod_player_section");
            await MainWebView.InvokeScriptAsync("eval", new string[] { script });
            var height = MainWebView.ActualHeight;
            template = "var elements = document.getElementsByClassName('{{0}}');if (elements.length > 0){elements[0].style.height='{{1}}px';}";
            template = Utils.TransferTemplate(template);
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
            foreach (RulesItem item in App.Rules.rules.compact.video)
            {
                if (item.status)
                {
                    RemoveElementsByClassName(item.value);
                }
            }
            AdaptWebViewWithWindow();
            Information.Text = "正在加载内容......";
        }

        private void NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
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
            InitSuccess = true;
            Loading.IsActive = false;
            Blur.Visibility = Visibility.Collapsed;
            Information.Visibility = Visibility.Collapsed;
            Go.Visibility = Visibility.Collapsed;
            RulesItem TimeLine = Utils.GetRulesItemById("X006");
            if (TimeLine.status)
            {
                Thread thread = new Thread(new ThreadStart(AddToTimeLine));
                thread.Start();
            }
        }

        private async void EvalScripts(string script)
        {
            await MainWebView.InvokeScriptAsync("eval", new string[] { script });
        }

        private void RemoveElementsByClassName(string ClassName)
        {
            string template = "while(true){var elements = document.getElementsByClassName('{{0}}');if(elements.length>0){for(var i=0;i<elements.length;i++){elements[i].parentNode.removeChild(elements[i]);} }else{break;} }";
            template = Utils.TransferTemplate(template);
            string script = string.Format(template, ClassName);
            EvalScripts(script);
        }

        private void ContainsFullScreenElementChanged(WebView sender, object args)
        {
            var applicationView = ApplicationView.GetForCurrentView();
            if (sender.ContainsFullScreenElement)
            {
                applicationView.TryEnterFullScreenMode();
                Go.Visibility = Visibility.Collapsed;
                Refresh.Visibility = Visibility.Collapsed;
            }
            else if (applicationView.IsFullScreenMode)
            {
                applicationView.ExitFullScreenMode();
                Go.Visibility = Visibility.Visible;
                Refresh.Visibility = Visibility.Visible;
            }
        }

        private void NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            string Url = args.Uri.ToString();
            if (Url.Contains("v.qq.com") &&(Url.Contains("cover") || Url.Contains("page")))
            {
                args.Handled = true;
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
            Go.Visibility = Visibility.Visible;
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

        private async Task<Activity> GetActivity()
        {
            Activity Activity = new Activity();
            string template = "var metas = document.getElementsByTagName('meta');for(var i=0;i<metas.length;i++){if(metas[i].getAttribute('itemprop')=='{{0}}'){metas[i].content;break;} }";
            template = Utils.TransferTemplate(template);
            var script = string.Format(template, "url");
            Activity.url = await MainWebView.InvokeScriptAsync("eval", new string[] { script });
            script = string.Format(template, "image");
            Activity.image = await MainWebView.InvokeScriptAsync("eval", new string[] { script });
            template = "var metas = document.getElementsByTagName('meta');for(var i=0;i<metas.length;i++){if(metas[i].name=='{{0}}'){metas[i].content;break;} }";
            template = Utils.TransferTemplate(template);
            script = string.Format(template, "title");
            Activity.title = await MainWebView.InvokeScriptAsync("eval", new string[] { script });
            script = string.Format(template, "description");
            Activity.description = await MainWebView.InvokeScriptAsync("eval", new string[] { script });
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
                catch (Exception e)
                {
                    ;
                }
            });
        }
    }
}
