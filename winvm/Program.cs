using System;
using Microsoft.Win32;

namespace winvm
{
    class Program
    {
        static void Main(string[] args)
        {
            string baseKeyName =
      @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\";

            // すべてのサブ・キー名を取得する
            RegistryKey rParentKey =
              Registry.LocalMachine.OpenSubKey(baseKeyName);

            string[] arySubKeyNames = rParentKey.GetSubKeyNames();

            rParentKey.Close();

            foreach (string subKeyName in arySubKeyNames)
            {
                Console.WriteLine(subKeyName);
                // 出力例：
                // AcroRd32.exe
                // atltracetool.exe
                // ……以下略
            }
        }
    }
}
