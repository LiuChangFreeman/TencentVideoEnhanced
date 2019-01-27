using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TencentVideoEnhanced.Model
{
    public class Rules : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string _time;
        public string time
        {
            get { return _time; }
            set { _time = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("time")); }
        }
        private RulesContent _rules;
        public RulesContent rules
        {
            get { return _rules; }
            set { _rules = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("rules")); }
        }
    }
    public class RulesContent : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private RulesItem _eval;
        public RulesItem eval
        {
            get { return _eval; }
            set { _eval = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("eval")); }
        }
        private RulesItem _remove;
        public RulesItem remove
        {
            get { return _remove; }
            set { _remove = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("remove")); }
        }
        private RulesItem _remove2;
        public RulesItem remove2
        {
            get { return _remove2; }
            set { _remove2 = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("remove2")); }
        }
    }
    public class RulesItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private List<RulesData> _DOMContentLoaded;
        public List<RulesData> DOMContentLoaded
        {
            get { return _DOMContentLoaded; }
            set { _DOMContentLoaded = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DOMContentLoaded")); }
        }
        private List<RulesData> _NavigationCompleted;
        public List<RulesData> NavigationCompleted
        {
            get { return _NavigationCompleted; }
            set { _NavigationCompleted = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NavigationCompleted")); }
        }
    }
    public class RulesData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string _value;
        public string value
        {
            get { return _value; }
            set { _value = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("value")); }
        }
        private string _description;
        public string description
        {
            get { return _description; }
            set { _description = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("description")); }
        }
        private bool _status;
        public bool status
        {
            get { return _status; }
            set { _status = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("status")); }
        }
    }
}
