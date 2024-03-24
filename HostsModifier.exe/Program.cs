using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace HostsModifier
{
    class Program
    {
        static void Main(string[] args)
        {
            string hostsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts");
            if (args.Length > 0)
            {
                
                FileInfo fileInfo = new FileInfo(hostsFile);
                if (!File.Exists(hostsFile)) { File.Create(hostsFile); }
                if (fileInfo.IsReadOnly)  { File.SetAttributes(hostsFile, FileAttributes.Normal); }
                if (File.ReadAllText(hostsFile).Contains("# BEGIN VANILLA HOSTS"))
                {
                    string str = File.ReadAllText(hostsFile);
                    int index = str.IndexOf("# BEGIN VANILLA HOSTS");
                    string result = str.Substring(0, index);
                    File.WriteAllText(hostsFile, result);
                }

                //if (File.Exists(hostsFile + ".bak")) {   File.Replace(hostsFile + ".bak", hostsFile, hostsFile + ".bak");   }
                //                                         throws an error, don't use this even if it looks better

                if (File.Exists(hostsFile + ".bak"))
                {
                    File.Delete(hostsFile);
                    File.Copy(hostsFile + ".bak", hostsFile);
                    File.Delete(hostsFile + ".bak");
                }
                File.Copy(hostsFile, hostsFile + ".bak");
                using (StreamWriter w = File.AppendText(hostsFile))
                {
                    w.WriteLine("");
                    w.WriteLine("# BEGIN VANILLA HOSTS");
                    w.WriteLine("127.0.0.1 www.roblox.com");
                    w.WriteLine("127.0.0.1 roblox.com");
                    w.WriteLine("127.0.0.1 api.roblox.com");
                    w.WriteLine("127.0.0.1 assetgame.roblox.com");
                    w.WriteLine("127.0.0.1 clientsettings.api.roblox.com");
                    w.WriteLine("127.0.0.1 versioncompatibility.api.roblox.com");
                    w.WriteLine("127.0.0.1 ephemeralcounters.api.roblox.com");
                    w.WriteLine("127.0.0.1 clientsettingscdn.roblox.com");
                }

            }
            else
            {
                File.Delete(hostsFile);
                File.Copy(hostsFile + ".bak", hostsFile);
            }
        }
    }
}
