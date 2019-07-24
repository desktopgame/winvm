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
        private Dictionary<string, RegValue> attribute;

        public RegEntry(string key)
        {
            this.Key = key;
            this.children = new List<RegEntry>();
            this.attribute = new Dictionary<string, RegValue>();
        }
        public static RegEntry Error(string key)
        {
            var ret = new RegEntry(key);
            ret.IsError = true;
            return ret;
        }
        public void Add(RegEntry e)
        {
            e.parent = this;
            children.Add(e);
        }
        
        public string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(FullPath());
            foreach(var kv in attribute)
            {
                sb.AppendLine("    " + kv.Key + " " + kv.Value.Value);
            }
            return sb.ToString();
        }

        public void Put(string key, RegValue v)
        {
            attribute[key] = v;
        }

        public RegValue Get(string key)
        {
            return attribute[key];
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
