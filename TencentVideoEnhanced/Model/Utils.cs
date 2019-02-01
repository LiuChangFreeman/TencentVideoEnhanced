using System;
using System.Net.Http;
using System.Net;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json;
using Windows.ApplicationModel.UserActivities;
using Windows.Storage;
using Windows.UI.Shell;

namespace TencentVideoEnhanced.Model
{
    class Utils
    {
        private static UserActivitySession CurrentSession;
        public static Uri UpdateRulesUri = new Uri("http://aikatsucn.cn/files/rules.json");

        public static string TransferTemplate(string format)
        {
            //替换回String.Format标准格式
            var temp = format.Replace("{{", "_-_").Replace("}}", "-_-");
            temp = temp.Replace("{", "{{").Replace("}", "}}");
            temp = temp.Replace("_-_", "{").Replace("-_-", "}");
            return temp;
        }

        public static RulesItem GetRulesItemById(string id)
        {
            RulesItem result = new RulesItem();
            if (App.Rules!=null)
            {
                foreach (var item in App.Rules.app)
                {
                    if (item.id == id)
                    {
                        result = item;
                        break;
                    }
                }
            }
            return result;
        }

        public async static void GetLatestRules()
        {
            Rules Rules;
            using(HttpClient HttpClient=new HttpClient())
            {
                HttpClient.Timeout = TimeSpan.FromSeconds(10);
                var HttpResopnse=await HttpClient.GetAsync(UpdateRulesUri);
                if (HttpResopnse.StatusCode==HttpStatusCode.OK)
                {
                    string StringRules = await HttpResopnse.Content.ReadAsStringAsync();
                    Rules = JsonConvert.DeserializeObject<Rules>(StringRules);
                    if (Rules==null)
                    {
                        return;
                    }
                    if (Rules.version>App.Rules.version)
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
                    }
                }
            }
        }

        public async static void AddToTimeLine(Activity data)
        {
            UserActivityChannel Channel = UserActivityChannel.GetDefault();
            UserActivity Activity = await Channel.GetOrCreateUserActivityAsync(data.url);
            Activity.ActivationUri = new Uri("tencentvideo:video?url=" + data.url);
            Activity.VisualElements.DisplayText = "腾讯视频";
            StorageFile CardFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Data/activity.json"));
            string CardText = await FileIO.ReadTextAsync(CardFile);
            CardText = TransferTemplate(CardText);
            string ActivityContent = string.Format(CardText,data.image,data.title,data.description);
            Activity.VisualElements.Content = AdaptiveCardBuilder.CreateAdaptiveCardFromJson(ActivityContent);
            await Activity.SaveAsync();
            CurrentSession = Activity.CreateSession();
        }
    }
}
