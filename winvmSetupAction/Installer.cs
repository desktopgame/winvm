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
                AddPath(EnvironmentVariableTarget.Machine, dllDir);
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
            RemovePath(EnvironmentVariableTarget.Machine, dllDir);
        }

        private void InitializeComponent()
        {

        }

        private static void AddPath(EnvironmentVariableTarget target, string path)
        {
            var envVarPath = Environment.GetEnvironmentVariable("Path",
                              EnvironmentVariableTarget.Machine);
            File.AppendAllText(path + Path.DirectorySeparatorChar + "env_Path.text", envVarPath + "\n");
            if (!envVarPath.Contains(path))
            {
                var newVar = envVarPath + ";" + path;
                Environment.SetEnvironmentVariable("Path", newVar, EnvironmentVariableTarget.Machine);
            }
        }

        private static void RemovePath(EnvironmentVariableTarget target, string path)
        {
            var envVarPath = Environment.GetEnvironmentVariable("Path",
                              EnvironmentVariableTarget.Machine);
            if (envVarPath.Contains(path))
            {
                var index = envVarPath.IndexOf(path);
                var newVar = envVarPath.Remove(index, path.Length);
                newVar = newVar.Replace(";;", ";");
                Environment.SetEnvironmentVariable("Path", newVar, EnvironmentVariableTarget.Machine);
            }
        }
    }
}
