using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;

namespace TencentVideoEnhanced.Model
{
    public class Rules : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _version;
        public int version
        {
            get { return _version; }
            set { _version = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("version")); }
        }

        private RulesContent _rules;
        public RulesContent rules
        {
            get { return _rules; }
            set { _rules = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("rules")); }
        }

        private ObservableCollection<RulesItem> _app;
        public ObservableCollection<RulesItem> app
        {
            get { return _app; }
            set { _app = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("app")); }
        }

        public Dictionary<string, bool> GetSettings()
        {
            Dictionary<string, bool> result= new Dictionary<string, bool>();
            foreach (var item in app)
            {
                result[item.id] = item.status;
            }
            foreach (var item in rules.eval)
            {
                result[item.id] = item.status;
            }
            foreach (var item in rules.compact.video)
            {
                result[item.id] = item.status;
            }
            foreach (var item in rules.compact.search)
            {
                result[item.id] = item.status;
            }
            foreach (var item in rules.compact.history)
            {
                result[item.id] = item.status;
            }
            return result;
        }

        public void SetSettings(Dictionary<string, bool> data)
        {
            foreach (var item in app)
            {
                item.status= data[item.id];
            }
            foreach (var item in rules.eval)
            {
                item.status = data[item.id];
            }
            foreach (var item in rules.compact.video)
            {
                item.status = data[item.id];
            }
            foreach (var item in rules.compact.search)
            {
                item.status = data[item.id];
            }
            foreach (var item in rules.compact.history)
            {
                item.status = data[item.id];
            }
        }

    }
    public class RulesContent : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private ObservableCollection<RulesItem> _eval;
        public ObservableCollection<RulesItem> eval
        {
            get { return _eval; }
            set { _eval = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("eval")); }
        }
        private Compact _compact;
        public Compact compact
        {
            get { return _compact; }
            set { _compact = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("compact")); }
        }
    }
    public class Compact : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<RulesItem> _video;
        public ObservableCollection<RulesItem> video
        {
            get { return _video; }
            set { _video = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("video")); }
        }

        private ObservableCollection<RulesItem> _search;
        public ObservableCollection<RulesItem> search
        {
            get { return _search; }
            set { _search = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("search")); }
        }

        private ObservableCollection<RulesItem> _history;
        public ObservableCollection<RulesItem> history
        {
            get { return _history; }
            set { _history = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("history")); }
        }
        
    }

    public class RulesItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string _id;
        public string id
        {
            get { return _id; }
            set { _id = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("id")); }
        }
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
        private bool _enable;
        public bool enable
        {
            get { return _enable; }
            set { _enable = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("enable")); }
        }
    }
}
