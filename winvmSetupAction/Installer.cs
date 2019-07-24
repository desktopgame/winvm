using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace winvmSetupAction
{
    [System.ComponentModel.RunInstaller(true)]
    class Installer : System.Configuration.Install.Installer
    {

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void Install(System.Collections.IDictionary stateSaver)
        {
            base.Install(stateSaver);
            var addToPath = bool.Parse(this.Context.Parameters["addToPath"]);
            if(addToPath)
            {
                File.WriteAllText("install.text", "true");
            } else
            {
                File.WriteAllText("install.text", "false");
            }
            System.Windows.Forms.MessageBox.Show("テスト");
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void Commit(System.Collections.IDictionary savedState)
        {
            base.Commit(savedState);
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void Rollback(System.Collections.IDictionary savedState)
        {
            base.Rollback(savedState);
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            base.Uninstall(savedState);
            File.Delete("install.text");
        }

        private void InitializeComponent()
        {

        }
    }
}
