using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Linq;

namespace winvm
{
    class RegEntry
    {
        public string Key { private set; get; }
        public object Value { private set; get; }
        public bool IsError { private set; get; }
        public int Count {
            get { return children.Count; }
        }
        public  RegEntry this[int i] {
            get {
                return children[i];
            }
        }

        private RegEntry parent;
        private List<RegEntry> children;

        public RegEntry(string key, object value)
        {
            this.Key = key;
            this.Value = value;
            this.children = new List<RegEntry>();
        }
        public static RegEntry Error(string key)
        {
            var ret = new RegEntry(key, "");
            ret.IsError = true;
            return ret;
        }
        public void Add(RegEntry e)
        {
            e.parent = this;
            children.Add(e);
        }
        public void Get()
        {
            RegistryKey rParentKey =
                     Registry.LocalMachine.OpenSubKey(FullPath());
            if(rParentKey == null)
            {
                return;
            }
            foreach(var e in children)
            {
                e.Value = rParentKey.GetValue(e.Key);
                e.Get();
            }
            rParentKey.Close();
        }
        
        public string ToString()
        {
            return FullPath() + "  " + Value;
        }

        public string FullPath()
        {
            if(parent == null)
            {
                return Key;
            }
            return parent.FullPath(new RegEntry[] { this});
        }

        private string FullPath(RegEntry[] children)
        {
            if (parent == null)
            {
                var sb = new StringBuilder();
                sb.Append(Key).Append("\\");
                foreach(var a  in children)
                {
                    sb.Append(a.Key).Append("\\");
                }
                sb.Remove(sb.Length - 1, 1);
                return sb.ToString();
            } else
            {
                var li = new List<RegEntry>();
                li.AddRange(children);
                li.Add(this);
                return parent.FullPath(li.ToArray());
            }
        }
    }
}
