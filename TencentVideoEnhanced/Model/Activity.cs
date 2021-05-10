using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TencentVideoEnhanced.Model
{
    class Activity
    {
        public string url { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string image { get; set; }
        public static bool operator ==(Activity a, Activity b)
        {
            return a.url == b.url;
        }
        public static bool operator !=(Activity a, Activity b)
        {
            return a.url != b.url;
        }
    }
}
