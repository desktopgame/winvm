using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

namespace winvm
{
    class RegValue
    {
        public RegistryValueKind Kind { private set; get; }
        public object Value { private set; get; }

        public RegValue(RegistryValueKind kind, object value)
        {
            this.Kind = kind;
            this.Value = value;
        }
    }
}
