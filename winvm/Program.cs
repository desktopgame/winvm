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
                var grepKeyOption = command.Option(
                    template: "--grepKey",
                    description: "絞り込む名前(キー)",
                    optionType: CommandOptionType.SingleValue
                );
                var grepAttrOption = command.Option(
                    template: "--grepAttr",
                    description: "絞り込む名前(属性)",
                    optionType: CommandOptionType.SingleValue
                );
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
                    Console.WriteLine("ダンプが終了しました。");
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
                    Load(inputArgs.Value);
                    return 0;
                });
            });
            app.Execute(args);
        }

        static void Load(string path)
        {
            var regs = new List<RegEntry>();
            var lines = File.ReadLines(path).ToArray();
            for(int i=0; i<lines.Length; i++)
            {
                //@line
                var line = lines[i];
                //空行ならスキップ
                if (line.Length == 0)
                {
                    continue;
                }
                if (line != "@name")
                {
                    Console.WriteLine("invalid line: " + line + "<" + i + ">");
                    break;
                }
                //@line
                //key
                i++;
                line = lines[i];
                var key = line;
                var reg = new RegEntry(key);
                //@begin-attr-list
                i++;
                line = lines[i];
                if(line != "@begin-attr-list")
                {
                    Console.WriteLine("invalid line: " + line + "<" + i + ">");
                    break;
                }
                while(true)
                {
                    i++;
                    line = lines[i];
                    if(line == "@end-attr-list")
                    {
                        regs.Add(reg);
                        break;
                    }
                    //@begin-attr
                    if(line != "@begin-attr")
                    {
                        Console.WriteLine("invalid line: " + line + "<" + i + ">");
                        return;
                    }
                    //attr
                    i++;
                    line = lines[i];
                    var attr = line;
                    //type
                    i++;
                    line = lines[i];
                    var type = line;
                    //value
                    i++;
                    line = lines[i];
                    var value = line;
                    //@end-attr
                    i++;
                    line = lines[i];
                    if(line != "@end-attr")
                    {
                        Console.WriteLine("invalid line: " + line + "<" + i + ">");
                        return;
                    }
                    reg.Put(attr, new RegValue((RegistryValueKind)Enum.Parse(typeof(RegistryValueKind), type), value));
                }
            }
            var saveList = new List<string>();
            var discardList = new List<string>();
            foreach (var reg in regs)
            {
                RegistryKey rParentKey =
                 Registry.LocalMachine.OpenSubKey(reg.Key);
                if(rParentKey == null)
                {
                    continue;
                }
                foreach (var key in reg.Keys())
                {
                    var regValue = reg.Get(key);
                    var loadValue = (string)regValue.Value;
                    var realValue = rParentKey.GetValue(key);
                    saveList.Add(reg.Key + " " + key);
                    switch (regValue.Kind)
                    {
                        case RegistryValueKind.Binary:
                            discardList.Add(saveList[saveList.Count - 1]);
                            saveList.RemoveAt(saveList.Count - 1);
                            break;
                        case RegistryValueKind.DWord:
                            int loadIvalue = (int)int.Parse(loadValue);
                            int readIvalue = (int)realValue;
                            //変更されていない
                            if(loadIvalue == readIvalue)
                            {
                                discardList.Add(saveList[saveList.Count - 1]);
                                saveList.RemoveAt(saveList.Count - 1);
                                break;
                            }
                            Console.WriteLine(reg.Key + " " + key);
                            Console.WriteLine(realValue + " -> " + loadIvalue);
                            Console.WriteLine("変更してもよろしいですか？(y/n):");
                            var rline = Console.ReadLine();
                            if(rline.ToLower() == "y" || rline.ToLower() == "yes")
                            {
                                rParentKey.SetValue(key, loadIvalue, RegistryValueKind.DWord);
                            } else
                            {
                                discardList.Add(saveList[saveList.Count - 1]);
                                saveList.RemoveAt(saveList.Count - 1);
                            }
                            break;
                        case RegistryValueKind.ExpandString:
                            discardList.Add(saveList[saveList.Count - 1]);
                            saveList.RemoveAt(saveList.Count - 1);
                            break;
                        case RegistryValueKind.MultiString:
                            discardList.Add(saveList[saveList.Count - 1]);
                            saveList.RemoveAt(saveList.Count - 1);
                            break;
                        case RegistryValueKind.None:
                            discardList.Add(saveList[saveList.Count - 1]);
                            saveList.RemoveAt(saveList.Count - 1);
                            break;
                        case RegistryValueKind.QWord:
                            discardList.Add(saveList[saveList.Count - 1]);
                            saveList.RemoveAt(saveList.Count - 1);
                            break;
                        case RegistryValueKind.String:
                            discardList.Add(saveList[saveList.Count - 1]);
                            saveList.RemoveAt(saveList.Count - 1);
                            break;
                        case RegistryValueKind.Unknown:
                            discardList.Add(saveList[saveList.Count - 1]);
                            saveList.RemoveAt(saveList.Count - 1);
                            break;
                        default:
                            break;
                    }
                }
                rParentKey.Close();
            }
            if(saveList.Count > 0)
            {
                Console.WriteLine("変更されなかった一覧");
                foreach (var e in discardList)
                {
                    Console.WriteLine("    " + e);
                }
                Console.WriteLine("変更された一覧");
                foreach (var e in saveList)
                {
                    Console.WriteLine("     " + e);
                }
            } else
            {
                Console.WriteLine("ダンプファイルが変更されていないか、全ての変更が無視されました。");
            }
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
                entry.Path = key;
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
