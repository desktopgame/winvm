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

            var addToPath = (this.Context.Parameters["addToPath"]);
            System.Windows.Forms.MessageBox.Show(addToPath.ToString());
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
            File.Delete("install.text");
            System.Windows.Forms.MessageBox.Show("テスト2");
        }

        private void InitializeComponent()
        {

        }
    }
}
