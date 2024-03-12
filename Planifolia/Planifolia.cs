using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Globalization;
namespace Planifolia
{
    public partial class Planifolia : Form
    {
        public Planifolia()
        {
            InitializeComponent();
            
        }
        private void dropFile(object sender, DragEventArgs e)
        {
            var file = e.Data.GetData(DataFormats.FileDrop);
            string[] ohio = file as string[];
            var versionInfo = FileVersionInfo.GetVersionInfo(ohio[0]);
            string version = versionInfo.FileVersion;
            label2.Text = "Binary version: " + version;
            getBinaryType(ohio[0]);
        }

        private void getBinaryType(string filepath)
        {

            var versionInfo = FileVersionInfo.GetVersionInfo(filepath);
            if (versionInfo.FileDescription == "ROBLOX Game")
            {
                label1.Text = "Binary type: RobloxApp or RobloxPlayer";
            }
            if (versionInfo.FileDescription == "ROBLOX Game Client")
            {
                label1.Text = "Binary type: RobloxPlayerBeta";
            }
            else
            {
                label1.Text = "Binary type: " + versionInfo.FileDescription;
                patchFile(filepath);
            }

        }
        private void Planifolia_Load(object sender, EventArgs e)
        {
           
        }
        private void patchFile(string filePath)
        {
            var file = File.ReadAllText(filePath);
            string hexString = "829B006A038B";
            int length = hexString.Length;
            byte[] bytes = new byte[length / 2];

            for (int i = 0; i < length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }

            char[] chars = Encoding.GetEncoding(932).GetChars(bytes);
            var ohio = (char)Int32.Parse("829B006A008B", NumberStyles.AllowHexSpecifier);

            File.WriteAllText(filePath, file.Replace(chars[0] + chars[1] + chars[2] + chars[4] + chars[5] + chars[6], ohio));
        }
    }
}
