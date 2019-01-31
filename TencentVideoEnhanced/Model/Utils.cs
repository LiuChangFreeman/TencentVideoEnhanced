using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json;
using Windows.Storage;
using Windows.ApplicationModel;

namespace TencentVideoEnhanced.Model
{
    class Utils
    {
        public static Uri UpdateRulesUri = new Uri("http://aikatsucn.cn/files/rules.json");

        public static RulesItem GetRulesItemById(string id)
        {
            RulesItem result = new RulesItem();
            foreach (var item in App.Rules.app)
            {
                if (item.id == id)
                {
                    result=item;
                    break;
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

    }
}
