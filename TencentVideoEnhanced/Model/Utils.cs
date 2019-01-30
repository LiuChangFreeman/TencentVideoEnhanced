using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TencentVideoEnhanced.Model
{
    class Utils
    {
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
    }
}
