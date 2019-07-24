using System;
using System.IO;
using Microsoft.Win32;
using Microsoft.Extensions.CommandLineUtils;

namespace winvm
{
    class Program {
        static readonly string ROOT = "SYSTEM\\CurrentControlSet\\Enum";
        static void Main(string[] args) {
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

            app.OnExecute(() => {
                app.ShowHelp();
                return 1;
            });
            //saveコマンド
            app.Command("save", (command) => {
                command.Description = "レジストリの中身を保存する";
                command.HelpOption("-?|-h|--help");
                var outputArgs = command.Argument("output", "保存先のパス");
                command.OnExecute(() => {
                    File.WriteAllText(outputArgs.Value, OpenRegRecursive(GetRegistryPath(pathOption),0));
                    return 0;
                });
            });
            //loadコマンド
            app.Command("load", (command) => {
                command.Description = "レジストリの中身を復元する";
                command.HelpOption("-?|-h|--help");
                var outputArgs = command.Argument("input", "読み込み元のパス");
                command.OnExecute(() => {
                    var location = outputArgs.Value;
                    Console.WriteLine("Hoge: " + location);
                    return 0;
                });
            });
            app.Execute(args);
        }

        static string GetRegistryPath(CommandOption option) {
            if(option.HasValue()) {
                return option.Value();
            }
            return ROOT;
        }
        
        static string OpenRegRecursive(string key, int depth) {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(key);
            try {
                RegistryKey rParentKey =
                     Registry.LocalMachine.OpenSubKey(key);
                string[] arySubKeyNames = rParentKey.GetSubKeyNames();
                rParentKey.Close();
                foreach (string subKeyName in arySubKeyNames)
                {
                    sb.Append(OpenRegRecursive(key + "\\" + subKeyName, depth + 1));
                }
            } catch(System.Security.SecurityException se) {
                sb.AppendLine("ocurred exception: " + key);
            }
            return sb.ToString();
        }
    }
}
