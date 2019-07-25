using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace winvmSetupAction
{
    [System.ComponentModel.RunInstaller(true)]
    public class Installer : System.Configuration.Install.Installer
    {


        public override void Install(System.Collections.IDictionary stateSaver)
        {
            base.Install(stateSaver);
            var dllPath = Context.Parameters["assemblyPath"];
            var dllDir = Path.GetDirectoryName(dllPath);
            var addToPath = (this.Context.Parameters["addToPath"]);
            if (addToPath == "1")
            {
                var envVarPath = Environment.GetEnvironmentVariable("Path",
                              EnvironmentVariableTarget.Machine);
                File.AppendAllText("env_Path.text", envVarPath + "\n");
                if (!envVarPath.Contains(dllDir))
                {
                    var newVar = envVarPath + ";" + dllDir;
                    Environment.SetEnvironmentVariable("Path", newVar, EnvironmentVariableTarget.Machine);
                }
            }
        }

        public override void Commit(System.Collections.IDictionary savedState)
        {
            base.Commit(savedState);
        }

        public override void Rollback(System.Collections.IDictionary savedState)
        {
            base.Rollback(savedState);
        }

        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            base.Uninstall(savedState);
            var dllPath = Context.Parameters["assemblyPath"];
            var dllDir = Path.GetDirectoryName(dllPath);
            var addToPath = (this.Context.Parameters["addToPath"]);
            var envVarPath = Environment.GetEnvironmentVariable("Path",
                              EnvironmentVariableTarget.Machine);
            if (envVarPath.Contains(dllDir))
            {
                var index = envVarPath.IndexOf(dllDir);
                var newVar = envVarPath.Remove(index, dllDir.Length);
                newVar = newVar.Replace(";;", ";");
                Environment.SetEnvironmentVariable("Path", newVar, EnvironmentVariableTarget.Machine);
            }
        }

        private void InitializeComponent()
        {

        }
    }
}
