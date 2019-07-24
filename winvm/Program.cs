using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using Microsoft.Extensions.CommandLineUtils;
using System.Text;

namespace winvm
{
    class Program
    {
        static readonly string ROOT = "SYSTEM\\CurrentControlSet\\Enum\\HID";
        static void Main(string[] args)
        {
            var app = new CommandLineApplication(throwOnUnexpectedArg: false);

            app.Name = nameof(Program);
            app.Description = "winvm";
            app.HelpOption("-h|--help");
            // オプションの指定
            var pathOption = app.Option(
                template: "-p|--path",
                description: "レジストリのパス",
                optionType: CommandOptionType.SingleValue
            );
            var grepKeyOption = app.Option(
                template: "--grepKey",
                description: "絞り込む名前(キー)",
                optionType: CommandOptionType.SingleValue
            );
            var grepAttrOption = app.Option(
                template: "--grepAttr",
                description: "絞り込む名前(属性)",
                optionType: CommandOptionType.SingleValue
            );

            app.OnExecute(() =>
            {
                app.ShowHelp();
                return 1;
            });
            //saveコマンド
            app.Command("save", (command) =>
            {
                command.Description = "レジストリの中身を保存する";
                command.HelpOption("-?|-h|--help");
                var outputArgs = command.Argument("output", "保存先のパス");
                command.OnExecute(() =>
                {
                    if(outputArgs.Value == null)
                    {
                        command.ShowHelp();
                        return 1;
                    }
                    File.WriteAllText(
                        outputArgs.Value,
                        GetRegTextRecursive(GetRegistryPath(pathOption), GetGrepKey(grepKeyOption), GetGrepAttr(grepAttrOption))
                    );
                    return 0;
                });
            });
            //loadコマンド
            app.Command("load", (command) =>
            {
                command.Description = "レジストリの中身を復元する";
                command.HelpOption("-?|-h|--help");
                var inputArgs = command.Argument("input", "読み込み元のパス");
                command.OnExecute(() =>
                {
                    if (inputArgs.Value == null)
                    {
                        command.ShowHelp();
                        return 1;
                    }
                    return 0;
                });
            });
            app.Execute(args);
        }

        static string GetRegistryPath(CommandOption option)
        {
            if (option.HasValue())
            {
                return option.Value();
            }
            return ROOT;
        }

        static string GetGrepKey(CommandOption option)
        {
            if (option.HasValue())
            {
                return option.Value();
            }
            return "";
        }

        static string GetGrepAttr(CommandOption option)
        {
            if (option.HasValue())
            {
                return option.Value();
            }
            return "";
        }

        static string GetRegTextRecursive(string key, string grepKey, string grepAttr)
        {
            var e = GetRegRecursive(key, grepKey, grepAttr);
            var buf = new StringBuilder();

            GetRegTextRecursive(e, buf);
            return buf.ToString();
        }

        static void GetRegTextRecursive(RegEntry e, StringBuilder sb)
        {
            for (int i = 0; i < e.Count; i++)
            {
                sb.AppendLine(e[i].ToString());
                GetRegTextRecursive(e[i], sb);
            }
        }

        static RegEntry GetRegRecursive(string key, string grepKey, string grepAttr)
        {
            return GetRegRecursive(key, key, grepKey, grepAttr);
        }

        static RegEntry GetRegRecursive(string key, string name, string grepKey, string grepAttr)
        {
            try
            {
                var entry = new RegEntry(name);
                RegistryKey rParentKey =
                     Registry.LocalMachine.OpenSubKey(key);
                string[] arySubKeyNames = rParentKey.GetSubKeyNames();
                //すべての属性を読み込む
                var attributes = rParentKey.GetValueNames();
                foreach (var attr in attributes)
                {
                    var value = rParentKey.GetValue(attr);
                    entry.Put(attr, new RegValue(rParentKey.GetValueKind(attr), value));
                }
                rParentKey.Close();
                //サブエントリを読み込む
                foreach (string subKeyName in arySubKeyNames)
                {
                    var subentry = GetRegRecursive(key + "\\" + subKeyName, subKeyName, grepKey, grepAttr);
                    if (subentry.GrepKey(grepKey) || subentry.GrepAttr(grepAttr))
                    {
                        entry.Add(subentry);
                    }
                }
                return entry;
            }
            catch (System.Security.SecurityException se)
            {
                return (RegEntry.Error(key));
            }
        }
    }
}
