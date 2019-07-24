using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using Microsoft.Extensions.CommandLineUtils;
using System.Text;

namespace winvm
{
    class Program
    {
        static readonly string ROOT = "SYSTEM\\CurrentControlSet\\Enum";
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
                    File.WriteAllText(outputArgs.Value, GetRegTextRecursive(GetRegistryPath(pathOption)));
                    return 0;
                });
            });
            //loadコマンド
            app.Command("load", (command) =>
            {
                command.Description = "レジストリの中身を復元する";
                command.HelpOption("-?|-h|--help");
                var outputArgs = command.Argument("input", "読み込み元のパス");
                command.OnExecute(() =>
                {
                    var location = outputArgs.Value;
                    Console.WriteLine("Hoge: " + location);
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

        static string GetRegTextRecursive(string key)
        {
            var e = GetRegRecursive(key);
            var buf = new StringBuilder();
            buf.AppendLine(e.FullPath());
            GetRegTextRecursive(e, buf);
            return buf.ToString();
        }

        static void GetRegTextRecursive(RegEntry e, StringBuilder sb)
        {
            for(int i=0; i<e.Count; i++)
            {
                sb.AppendLine(e[i].FullPath());
                GetRegTextRecursive(e[i], sb);
            }
        }

        static RegEntry GetRegRecursive(string key)
        {
            return GetRegRecursive(key, key);
        }

        static RegEntry GetRegRecursive(string key, string name)
        {
            try
            {
                var entry = new RegEntry(name, "v");
                RegistryKey rParentKey =
                     Registry.LocalMachine.OpenSubKey(key);
                string[] arySubKeyNames = rParentKey.GetSubKeyNames();
                rParentKey.Close();
                foreach (string subKeyName in arySubKeyNames)
                {
                    entry.Add(GetRegRecursive(key + "\\" + subKeyName, subKeyName));
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
