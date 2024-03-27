using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.IO.Compression;
using Newtonsoft.Json;
using System.Data.SQLite;
using BlueMystic;

namespace VanillaLauncher.ContentView
{
    public partial class Vanilla : Form
    {
        public class SettingsFile
        {
            public string Username { get; set; }
            public string ID { get; set; }

            public string HostPort { get; set; }

            public string Client { get; set; }
            public string Map { get; set; }

            public string Hat1 { get; set; }
            public string Hat2 { get; set; }
            public string Hat3 { get; set; }

            public string Shirt { get; set; }
            public string Pants { get; set; }

            public string TShirt { get; set; }
            public string AvatarType { get; set; }
            public string HeadColor { get; set; }
            public string TorsoColor { get; set; }
            public string LeftArmColor { get; set; }
            public string LeftLegColor { get; set; }
            public string RightArmColor { get; set; }
            public string RightLegColor { get; set; }
        }

        string CurrentItem { get; set; }
        bool Is2007 { get; set; }
        bool IsRobloxApp { get; set; }
        bool IsRobloxPlayerBeta { get; set; }
        bool IsRCCService { get; set; }
        bool AvatarFetchRequired { get; set; }
        bool isRobloxPlayer { get; set; }
        string GlobalMap { get; set; }
        string GlobalUsername { get; set; }
        string GlobalID { get; set; }
        string GlobalHostPort { get; set; }
        string GlobalHat1 { get; set; }
        string GlobalHat2 { get; set; }
        string GlobalHat3 { get; set; }
        string GlobalShirt { get; set; }
        string GlobalPants { get; set; }
        string GlobalTshirt { get; set; }
        string AvatarTypeStr { get; set; }
        public string HeadColor { get; set; }
        public string TorsoColor { get; set; }
        public string LeftArmColor { get; set; }
        public string LeftLegColor { get; set; }
        public string RightArmColor { get; set; }
        public string RightLegColor { get; set; }

        private DarkModeCS DarkMode = null;

        private Timer timer = new Timer { Interval = 100 }; // This needs debating, for now it works.
        private Process phpCGIProcess = new Process
        {
            StartInfo = {
                FileName = "php-cgi.exe",
                WorkingDirectory = Directory.GetCurrentDirectory() + "\\files\\webserver\\php",
                Arguments = "-b 127.0.0.1:9123",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        public Vanilla()
        {
            InitializeComponent();

            string hostsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts");

            WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            bool administrativeMode = principal.IsInRole(WindowsBuiltInRole.Administrator);

            bool administrativeMode2 = principal.IsInRole(WindowsBuiltInRole.Administrator);
            Process.Start("taskkill.exe", "/F /IM nginx.exe");
            Process.Start("taskkill.exe", "/F /IM php-cgi.exe");
            System.Threading.Thread.Sleep(3000);

            string httpdconf = File.ReadAllText("files\\webserver\\conf\\nginx.conf");
            string currentDirFixed = Directory.GetCurrentDirectory().Replace(@"\", @"/");
            if (!httpdconf.Contains(currentDirFixed))
            {
                File.Delete("files\\webserver\\conf\\nginx.conf");
                File.Copy("files\\webserver\\conf\\nginx.conf.bak", "files\\webserver\\conf\\nginx.conf");
            }

            if (httpdconf.Contains(@"C:/Vanilla/files/webroot"))
            {
                string fixedConf = httpdconf.Replace(@"C:/Vanilla/files/webroot", currentDirFixed + @"/files/webroot");
                File.WriteAllText("files\\webserver\\conf\\nginx.conf", fixedConf);
            }

            Directory.SetCurrentDirectory(Directory.GetCurrentDirectory() + "\\files\\webserver\\");
            timer.Tick += (object sender, EventArgs e) => {
                if (phpCGIProcess.HasExited)
                {
                    phpCGIProcess.Start();
                }
            };

            phpCGIProcess.Start();
            timer.Start();

            Process.Start(Directory.GetCurrentDirectory() + "\\nginx.exe");
            Directory.SetCurrentDirectory("..\\..");
            if (!administrativeMode)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.Verb = "runas";
                startInfo.FileName = Application.ExecutablePath;
                Process.Start(startInfo);
                Process.GetCurrentProcess().Kill();

            }
            if (!File.Exists(Directory.GetCurrentDirectory() + @"\files\vanilla.sqlite"))
            {
                SQLiteConnection.CreateFile(Directory.GetCurrentDirectory() + @"\files\vanilla.sqlite");

                using (var sqlite2 = new SQLiteConnection(@"Data Source=" + Directory.GetCurrentDirectory() + @"\files\vanilla.sqlite"))
                {
                    sqlite2.Open();

                    string sql = "create table characterappearance (torsocolor text, leftlegcolor text, leftarmcolor text, rightlegcolor text, rightarmcolor text, headcolor text, asset1 text, asset2 text, asset3 text, asset4 text, asset5 text, asset6 text, asset7 text, asset8 text, asset9 text, asset10 text, asset11 text, asset12 text, asset13 text, avatartype text, userid text)";
                    SQLiteCommand command = new SQLiteCommand(sql, sqlite2);
                    command.ExecuteNonQuery();

                    string sql2 = "create table badges (badgeId text, obtainedBy text)";
                    SQLiteCommand command2 = new SQLiteCommand(sql2, sqlite2);
                    command2.ExecuteNonQuery();

                    string sql3 = "create table gamepasses (passId text, boughtBy text)";
                    SQLiteCommand command3 = new SQLiteCommand(sql3, sqlite2);
                    command3.ExecuteNonQuery();
                }
            }

            DarkMode = new DarkModeCS(this);
            Application.ApplicationExit += new EventHandler(onshutdown);

            string pathFile = Environment.GetEnvironmentVariable("PATH");
            if (!pathFile.Contains(Directory.GetCurrentDirectory() + @"\files\webserver\php"))
            {
                var name = "PATH";
                var scope = EnvironmentVariableTarget.Machine;
                var oldValue = Environment.GetEnvironmentVariable(name, scope);
                var newValue = oldValue + @";" + Directory.GetCurrentDirectory() + @"\files\webserver\php";
                Environment.SetEnvironmentVariable(name, newValue, scope);
            }

            if (!pathFile.Contains(Directory.GetCurrentDirectory() + @"\files\webserver\openssl"))
            {
                var name = "PATH";
                var scope = EnvironmentVariableTarget.Machine;
                var oldValue = Environment.GetEnvironmentVariable(name, scope);
                var newValue = oldValue + @";" + Directory.GetCurrentDirectory() + @"\files\webserver\openssl";
                Environment.SetEnvironmentVariable(name, newValue, scope);
            }
            string openSslConf = Environment.GetEnvironmentVariable("OPENSSL_CONF");
            if (openSslConf != null)
            {
                var name = "OPENSSL_CONF";
                var scope = EnvironmentVariableTarget.Machine;
                var newValue = Directory.GetCurrentDirectory() + @"\files\webserver\php\extras\ssl\openssl.cnf";
                Environment.SetEnvironmentVariable(name, newValue, scope);

            }
            FileInfo fileInfo = new FileInfo(hostsFile);
            if (!File.Exists(hostsFile))
            {
                MessageBox.Show("Your HOSTS file does not exist! Vanilla will create one for you. Please restart Vanilla.",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                File.Create(hostsFile);
                Process.GetCurrentProcess().Kill();
            }

            if (fileInfo.IsReadOnly)
            {
                MessageBox.Show("Vanilla will not work until you have disabled 'Read-Only' on your HOSTS file! Do this in C:\\Windows\\System32\\drivers\\etc.",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (File.ReadAllText(hostsFile).Contains("# BEGIN VANILLA HOSTS"))
            {
                string str = File.ReadAllText(hostsFile);
                int index = str.IndexOf("# BEGIN VANILLA HOSTS");

                File.WriteAllText(hostsFile, str.Substring(0, index));
            }

            if (File.Exists(hostsFile + ".bak"))
            {
                File.Delete(hostsFile);
                File.Copy(hostsFile + ".bak", hostsFile);
                File.Delete(hostsFile + ".bak");
            }

            File.Copy(hostsFile, hostsFile + ".bak");

            using (StreamWriter w = File.AppendText(hostsFile))
            {
                w.WriteLine(string.Empty);
                w.WriteLine("# BEGIN VANILLA HOSTS");
                w.WriteLine("127.0.0.1 www.roblox.com");
                w.WriteLine("127.0.0.1 roblox.com");
                w.WriteLine("127.0.0.1 api.roblox.com");
                w.WriteLine("127.0.0.1 assetgame.roblox.com");
                w.WriteLine("127.0.0.1 clientsettings.api.roblox.com");
                w.WriteLine("127.0.0.1 versioncompatibility.api.roblox.com");
                w.WriteLine("127.0.0.1 ephemeralcounters.api.roblox.com");
                w.WriteLine("127.0.0.1 clientsettingscdn.roblox.com");
                w.WriteLine("127.0.0.1 gamejoin.roblox.com");
                w.WriteLine("127.0.0.1 apis.roblox.com");
                w.WriteLine("127.0.0.1 auth.roblox.com");
            }

            var files3 = from file in Directory.EnumerateFiles("files\\char\\hats") select file;
            foreach (var file in files3)
            {
                if (file.Contains(".json"))
                {
                    string ohio = file.Substring(file.IndexOf("ts\\") + 3);
                    int index = ohio.IndexOf(".info");
                    string result = ohio.Substring(0, index);
                    listBox0.Items.Add(result);
                    listBox1.Items.Add(result);
                    listBox2.Items.Add(result);
                }
            }

            var files4 = from file in Directory.EnumerateFiles("files\\char\\shirts") select file;
            foreach (var file in files4)
            {
                if (file.Contains(".json"))
                {
                    string ohio = file.Substring(file.IndexOf("ts\\") + 3);
                    int index = ohio.IndexOf(".info");
                    string result = ohio.Substring(0, index);
                    listBox3.Items.Add(result);
                }
            }

            var files5 = from file in Directory.EnumerateFiles("files\\char\\pants") select file;
            foreach (var file in files5)
            {
                if (file.Contains(".json"))
                {
                    string ohio = file.Substring(file.IndexOf("ts\\") + 3);
                    int index = ohio.IndexOf(".info");
                    string result = ohio.Substring(0, index);
                    listBox4.Items.Add(result);
                }
            }

            var files6 = from file in Directory.EnumerateFiles("files\\char\\t-shirts") select file;
            foreach (var file in files6)
            {
                if (file.Contains(".json"))
                {
                    string ohio = file.Substring(file.IndexOf("ts\\") + 3);
                    int index = ohio.IndexOf(".info");
                    string result = ohio.Substring(0, index);
                    listBox5.Items.Add(result);
                }
            }

            var files2 = from file2 in Directory.EnumerateDirectories("clients\\") select file2;
            foreach (var file2 in files2)
            {
                string ohio = file2.Substring(file2.IndexOf("\\") + 1);
                clientBox.Items.Add(ohio);

            }

            var files = from file in Directory.EnumerateFiles("files\\maps", "*", SearchOption.AllDirectories) select file;
            foreach (var file in files)
            {
                mapBox.Items.Add(file);
            }

            string curDir = Directory.GetCurrentDirectory();
            if (File.Exists("files\\settings.json"))
            {
                dynamic val = JsonConvert.DeserializeObject(File.ReadAllText("files\\settings.json"));
                hostPortNew.Text = val["HostPort"];
                idBox.Text = val["ID"];
                userNameBox.Text = val["Username"];
                CurrentItem = val["Client"];
                GlobalMap = val["Map"];
                GlobalHat1 = val["Hat1"];
                GlobalHat2 = val["Hat2"];
                GlobalHat3 = val["Hat3"];
                GlobalShirt = val["Shirt"];
                GlobalPants = val["Pants"];
                GlobalTshirt = val["TShirt"];
                ClientInfo.Text = "selected client: None";
                AvatarTypeStr = val["AvatarType"];
                HeadColor = val["HeadColor"];
                TorsoColor = val["TorsoColor"];
                LeftArmColor = val["LeftArmColor"];
                LeftLegColor = val["LeftLegColor"];
                RightArmColor = val["RightArmColor"];
                RightLegColor = val["RightLegColor"];

                string[] values = { GlobalShirt, GlobalPants, GlobalHat1, GlobalHat2, GlobalHat3, GlobalHat3, GlobalTshirt, CurrentItem, GlobalMap };

                foreach (string goon in values)
                {
                    if (goon == CurrentItem)
                    {
                        var index = clientBox.FindString(CurrentItem);
                        clientBox.SelectedIndex = index;
                    }

                    if (goon == GlobalMap)
                    {
                        var index = mapBox.FindString(GlobalMap);
                        mapBox.SelectedIndex = index;
                    }

                    if (goon == GlobalHat1)
                    {
                        var index = listBox0.FindString(GlobalHat1);
                        listBox0.SelectedIndex = index;
                    }

                    if (goon == GlobalHat2)
                    {
                        var index = listBox1.FindString(GlobalHat2);
                        listBox1.SelectedIndex = index;
                    }

                    if (goon == GlobalHat3)
                    {
                        var index = listBox2.FindString(GlobalHat3);
                        listBox2.SelectedIndex = index;
                    }

                    if (goon == GlobalShirt)
                    {
                        var index = listBox3.FindString(GlobalShirt);
                        listBox3.SelectedIndex = index;
                    }

                    if (goon == GlobalPants)
                    {
                        var index = listBox4.FindString(GlobalPants);
                        listBox4.SelectedIndex = index;
                    }

                    if (goon == GlobalTshirt)
                    {
                        var index = listBox5.FindString(GlobalTshirt);
                        listBox5.SelectedIndex = index;
                    }

                    headColor.Text = val["HeadColor"];
                    torsoColor.Text = val["TorsoColor"];
                    leftArmColor.Text = val["LeftArmColor"];
                    leftLegColor.Text = val["LeftLegColor"];
                    rightArm.Text = val["RightArmColor"];
                    rightLeg.Text = val["RightLegColor"];
                }
            }

            var lines = File.ReadAllLines("files\\splashes.txt");
            var r = new Random();
            var randomLineNumber = r.Next(0, lines.Length - 1);
            splash.Text = lines[randomLineNumber];

            if (!Directory.Exists(@"C:\ProgramData\Roblox\content") && Directory.Exists("clients\\2008M\\RCC"))
            {
                Directory.CreateDirectory(@"C:\ProgramData\Roblox\content");
                CopyDirectory(@"clients\2008M\RCC\content", @"C:\ProgramData\Roblox\content", true);
            }

            if (string.IsNullOrEmpty(AvatarTypeStr))
            {
                AvatarTypeStr = "R6";
            }
        }
        private void button1_Click_1(object sender, EventArgs e)
        {
            if (AvatarTypeStr == "R15")
            {
                AvatarTypeStr = "R6";
                return;
            };

            if (AvatarTypeStr == "R6")
            {
                AvatarTypeStr = "R15";
                return;
            };

        }

        public void SetGlobal(object sender, EventArgs e)
        {
            var textBox = ((TextBox)sender);

            switch (textBox.Name)
            {
                case "hostPortNew":
                    GlobalHostPort = textBox.Text;
                    break;
                case "userNameBox":
                    GlobalUsername = textBox.Text;
                    break;
                case "idBox":
                    GlobalID = textBox.Text;
                    break;
            }
        }

        public void onshutdown(object sender, EventArgs e)
        {
            timer.Stop();

            if (File.Exists("files\\settings.json"))
            {
                File.Delete("files\\settings.json");
            }

            var jsonFile = new SettingsFile
            {
                Username = GlobalUsername,
                ID = GlobalID,
                HostPort = GlobalHostPort,
                Client = CurrentItem,
                Map = GlobalMap,
                Hat1 = GlobalHat1,
                Hat2 = GlobalHat2,
                Hat3 = GlobalHat3,
                Shirt = GlobalShirt,
                Pants = GlobalPants,
                TShirt = GlobalTshirt,
                AvatarType = AvatarTypeStr,
                HeadColor = HeadColor,
                TorsoColor = TorsoColor,
                LeftArmColor = LeftArmColor,
                LeftLegColor = LeftLegColor,
                RightArmColor = RightArmColor,
                RightLegColor = RightLegColor
            };

            File.WriteAllText(@"files\\settings.json", JsonConvert.SerializeObject(jsonFile));

            using (StreamWriter file = File.CreateText(@"files\\settings.json"))
            {
                var jsonSerializer = new JsonSerializer();
                jsonSerializer.Serialize(file, jsonFile);
            }

            // Don't use File.Replace here, it'll cause issues.
            string hostsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts");
            File.Delete(hostsFile);
            File.Copy(hostsFile + ".bak", hostsFile);

            Process.Start("CMD.exe", "/C taskkill /F /IM nginx.exe");
            Process.Start("CMD.exe", "/C taskkill /F /IM php-cgi.exe");

        }

        private void mapChanged(object sender, System.EventArgs e)
        {
            if (mapBox.SelectedItem == null)
            {
                return;
            }
            else
            {
                GlobalMap = mapBox.SelectedItem.ToString();
                if (GlobalMap.Contains(".gz"))
                {
                    Decompress(new FileInfo(GlobalMap));
                }
                else
                {
                    File.Delete("files\\web\\1818");
                    File.Copy(GlobalMap, "files\\web\\1818");
                }

            }
        }

        private void charChanged(object sender, System.EventArgs e)
        {
            var listBox = ((ListBox)sender);

            switch (listBox.Name)
            {
                case "listBox0":
                    {
                        if (listBox.SelectedItem == null)
                        {
                            return;
                        }

                        dynamic val = JsonConvert.DeserializeObject(File.ReadAllText("files\\char\\hats\\" + listBox.SelectedItem.ToString() + ".info.json"));
                        hatName.Text = val["Name"];

                        Hat1Box.SizeMode = PictureBoxSizeMode.StretchImage;
                        Hat1Box.Image = Image.FromFile("files\\char\\hats\\" + listBox.SelectedItem.ToString() + ".thumb.png");
                        Hat1Box.Refresh();

                        GlobalHat1 = listBox.SelectedItem.ToString();
                    }
                    break;
                case "listBox1":
                    {
                        if (listBox.SelectedItem == null)
                        {
                            return;
                        }

                        dynamic val = JsonConvert.DeserializeObject(File.ReadAllText("files\\char\\hats\\" + listBox.SelectedItem.ToString() + ".info.json"));
                        textBox2.Text = val["Name"];

                        Hat2Box.SizeMode = PictureBoxSizeMode.StretchImage;
                        Hat2Box.Image = Image.FromFile("files\\char\\hats\\" + listBox.SelectedItem.ToString() + ".thumb.png");
                        Hat2Box.Refresh();

                        Hat2Box.Visible = true;
                        GlobalHat2 = listBox.SelectedItem.ToString();
                    }
                    break;
                case "listBox2":
                    {
                        if (listBox.SelectedItem == null)
                        {
                            return;
                        }

                        dynamic val = JsonConvert.DeserializeObject(File.ReadAllText("files\\char\\hats\\" + listBox.SelectedItem.ToString() + ".info.json"));
                        textBox3.Text = val["Name"];

                        Hat3Box.SizeMode = PictureBoxSizeMode.StretchImage;
                        Hat3Box.Image = Image.FromFile("files\\char\\hats\\" + listBox.SelectedItem.ToString() + ".thumb.png");
                        Hat3Box.Refresh();

                        Hat3Box.Visible = true;
                        GlobalHat3 = listBox.SelectedItem.ToString();
                    }
                    break;
                case "listBox3":
                    {
                        if (listBox.SelectedItem == null)
                        {
                            return;
                        }

                        dynamic val = JsonConvert.DeserializeObject(File.ReadAllText("files\\char\\shirts\\" + listBox.SelectedItem.ToString() + ".info.json"));
                        textBox4.Text = val["Name"];
                        ShirtBox.SizeMode = PictureBoxSizeMode.StretchImage;
                        ShirtBox.Image = Image.FromFile("files\\char\\shirts\\" + listBox.SelectedItem.ToString() + ".thumb.png");
                        ShirtBox.Refresh();

                        ShirtBox.Visible = true;
                        GlobalShirt = listBox.SelectedItem.ToString();
                    }
                    break;
                case "listBox4":
                    {
                        if (listBox.SelectedItem == null)
                        {
                            return;
                        }

                        dynamic val = JsonConvert.DeserializeObject(File.ReadAllText("files\\char\\pants\\" + listBox.SelectedItem.ToString() + ".info.json"));
                        textBox5.Text = val["Name"];
                        PantsBox.SizeMode = PictureBoxSizeMode.StretchImage;
                        PantsBox.Image = Image.FromFile("files\\char\\pants\\" + listBox.SelectedItem.ToString() + ".thumb.png");
                        PantsBox.Refresh();

                        PantsBox.Visible = true;
                        GlobalPants = listBox.SelectedItem.ToString();
                    }
                    break;
                case "listBox5":
                    {
                        if (listBox.SelectedItem == null)
                        {
                            return;
                        }

                        dynamic val = JsonConvert.DeserializeObject(File.ReadAllText("files\\char\\t-shirts\\" + listBox.SelectedItem.ToString() + ".info.json"));
                        textBox5.Text = val["Name"];
                        TShirtBox.SizeMode = PictureBoxSizeMode.StretchImage;
                        TShirtBox.Image = Image.FromFile("files\\char\\t-shirts\\" + listBox.SelectedItem.ToString() + ".thumb.png");
                        TShirtBox.Refresh();

                        TShirtBox.Visible = true;
                        GlobalTshirt = listBox.SelectedItem.ToString();
                    }
                    break;
            }
        }

        private void clientChanged(object Sender, EventArgs e)
        {
            if (clientBox.SelectedItem == null)
            {
                return;
            }
            else
            {
                var directory = new DirectoryInfo("files\\webroot\\");
                foreach (FileInfo file in directory.GetFiles())
                {
                    file.Delete();
                }

                foreach (DirectoryInfo subDirectory in directory.GetDirectories())
                {
                    subDirectory.Delete(true);
                }

                CurrentItem = clientBox.SelectedItem.ToString();
                ZipFile.ExtractToDirectory("files\\filesets\\" + CurrentItem + ".zip", "files\\webroot");
                ZipFile.ExtractToDirectory("files\\filesets\\common.zip", "files\\webroot");

                if (assetCache.Checked)
                {
                    File.Replace("files\\webroot\\asset\\cacher.php", "files\\webroot\\asset\\index.php", "files\\webroot\\asset\\nocache.php");
                    File.Replace("files\\webroot\\api\\asset\\cacher.php", "files\\webroot\\api\\asset\\index.php", "files\\webroot\\api\\asset\\nocache.php");
                }

                ClientInfo.Text = "selected client: " + CurrentItem;
                if (File.Exists("clients\\" + CurrentItem + "\\client.json"))
                {
                    dynamic val = JsonConvert.DeserializeObject(File.ReadAllText("clients\\" + CurrentItem + "\\client.json"));
                    IsRobloxApp = val["isRobloxApp"] == "true";
                    IsRobloxPlayerBeta = val["isRobloxPlayerBeta"] == "true";
                    IsRCCService = val["isRCCService"] == "true";
                    Is2007 = val["is2007"] == "true";
                    AvatarFetchRequired = val["avatarFetchRequired"] == "true";
                    isRobloxPlayer = val["isRobloxPlayer"] == "true";
                }

                try
                {
                    ScreenShot.Image = Image.FromFile("clients\\" + CurrentItem + "\\photo.png");
                }
                catch
                {
                    ScreenShot.Image = null;
                }
            }

        }

        private void HostButton_Click_1(object sender, EventArgs e)
        {
            string selectedClient = CurrentItem;
            string hostPortstring = hostPortNew.Text;
            if (IsRCCService)
            {
                if (selectedClient == "2015M" || selectedClient == "2015E" || selectedClient == "2014X")
                {
                    Directory.SetCurrentDirectory("clients\\" + selectedClient + "\\RCC\\");
                    Process.Start("CMD.exe", "/C RCCService.exe -console -start -placeid:1818");
                    Directory.SetCurrentDirectory("..\\..\\..");
                }
                else
                {
                    Directory.SetCurrentDirectory("clients\\" + selectedClient + "\\RCC\\");
                    Process.Start("CMD.exe", "/C RCCService.exe -console -start -verbose");
                    Directory.SetCurrentDirectory("..\\..\\..");
                    System.Threading.Thread.Sleep(11000);
                    Extensions.NetworkingEx.Execute(selectedClient);
                }
            }

            if (isRobloxPlayer)
            {
                Directory.SetCurrentDirectory("clients\\" + selectedClient + "\\Player\\");
                Process.Start("RobloxPlayer.exe", "-joinScriptUrl \"http://www.roblox.com/game/gameserver.ashx?port=" + hostPortstring + "\"");
                Directory.SetCurrentDirectory("..\\..\\..");
            }

            if (IsRobloxApp && !IsRCCService)
            {
                Directory.SetCurrentDirectory("clients\\" + selectedClient + "\\Player\\");
                Process.Start("RobloxApp.exe", "-no3d -script \"loadfile('http://www.roblox.com/game/gameserver.ashx?port=" + hostPortstring + "')()\"");
                Directory.SetCurrentDirectory("..\\..\\..");
            }

            if (IsRobloxPlayerBeta && !IsRCCService)
            {
                Directory.SetCurrentDirectory("clients\\" + selectedClient + "\\Player\\");
                Process.Start("RobloxPlayerBeta.exe", "-j \"http://www.roblox.com/game/gameserver.ashx?port=53640\" -t \"0\" -a \"http://roblox.com/goon\"");
                Directory.SetCurrentDirectory("..\\..\\..");
            }

            if (Is2007)
            {
                Directory.SetCurrentDirectory("clients\\" + selectedClient + "\\Player\\");
                Process.Start("Roblox.exe", "-no3d -script \"" + Directory.GetCurrentDirectory() + "\\content\\gameserver.lua\" \"" + Directory.GetCurrentDirectory() + "\\..\\..\\..\\files\\web\\1818");

                Directory.SetCurrentDirectory("..\\..\\..");
            }
        }

        private void JoinButton_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (clientBox.SelectedItem == null)
                {
                    return;
                }
                else
                {
                    string selectedClient = clientBox.SelectedItem.ToString();
                    string ipaddr = IPBox.Text;
                    string port = PortBox.Text;
                    string userName = userNameBox.Text;
                    string ID = idBox.Text;
                    string hat1 = GlobalHat1;
                    string hat2s = GlobalHat2;
                    string hat3s = GlobalHat3;
                    string shirts = GlobalShirt;
                    string pants = GlobalPants;
                    string tshirts = GlobalTshirt;

                    if (IsRobloxApp && !isRobloxPlayer)
                    {
                        string[] values = { shirts, pants, hat1, hat2s, hat3s, tshirts };
                        for (int i = 0; i < values.Length; i++)
                        {
                            if (string.IsNullOrEmpty(values[i]))
                            {
                                values[i] = "0";
                            }
                        }

                        Directory.SetCurrentDirectory("clients\\" + selectedClient + "\\Player\\");
                        Process.Start("RobloxApp.exe", "-build -script \"loadfile('http://www.roblox.com/game/join.ashx?username=" + userName + "&id=" + ID + "&ip=" + ipaddr + "&hat1=" + hat1 + "&hat2=" + hat2s + "&hat3=" + hat3s + "&shirt=" + shirts + "&pants=" + pants + "&tshirt=" + tshirts + "&port=" + port + "&hc=" + HeadColor + "&tc=" + TorsoColor + "&la=" + LeftArmColor + "&ll=" + LeftLegColor + "&ra=" + RightArmColor + "&rl=" + RightLegColor + "')()\"");
                        Directory.SetCurrentDirectory("..\\..\\..");
                    }

                    if (isRobloxPlayer)
                    {
                        Directory.SetCurrentDirectory("clients\\" + selectedClient + "\\Player\\");
                        Process.Start("RobloxPlayer.exe", "-joinScriptUrl \"http://www.roblox.com/game/join.ashx?username=" + userName + "&id=" + ID + "&ip=" + ipaddr + "&hat1=" + hat1 + "&hat2=" + hat2s + "&hat3=" + hat3s + "&shirt=" + shirts + "&pants=" + pants + "&tshirt=" + tshirts + "&port=" + port + "&hc=" + HeadColor + "&tc=" + TorsoColor + "&la=" + LeftArmColor + "&ll=" + LeftLegColor + "&ra=" + RightArmColor + "&rl=" + RightLegColor + "\"");
                        Directory.SetCurrentDirectory("..\\..\\..");
                    }
                    if (IsRobloxPlayerBeta)
                    {

                        Directory.SetCurrentDirectory("clients\\" + selectedClient + "\\Player\\");
                        Process.Start("RobloxPlayerBeta.exe", "-j \"http://www.roblox.com/game/join.ashx?username=" + userName + "&id=" + ID + "&ip=" + ipaddr + "&hat1=" + hat1 + "&hat2=" + hat2s + "&hat3=" + hat3s + "&shirt=" + shirts + "&pants=" + pants + "&tshirt=" + tshirts + "&port=" + port + "&PlaceId=1818" + "&hc=" + HeadColor + "&tc=" + TorsoColor + "&la=" + LeftArmColor + "&ll=" + LeftLegColor + "&ra=" + RightArmColor + "&rl=" + RightLegColor + "&avatartype=" + AvatarTypeStr + "\" -t \"0\" -a \"http://www.roblox.com/Login/Negotiate.ashx\"");
                        Directory.SetCurrentDirectory("..\\..\\..");
                        if (AvatarFetchRequired)
                        {
                            var request = (HttpWebRequest)WebRequest.Create("http://" + ipaddr + ":53642/v1.1/set-avatar/?userid=" + ID + "&headc=" + HeadColor + "&torsoc=" + TorsoColor + "&rarmc=" + RightArmColor + "&larmc=" + LeftArmColor + "&llegc=" + LeftLegColor + "&rlegc=" + RightLegColor + "&shirt=" + shirts + "&tshirt=" + tshirts + "&pants=" + pants + "&face=0&hat1=" + hat1 + "&hat2=" + hat2s + "&hat3=" + hat3s + "&torsop=&lap=0&llp=0&rap=0&rlp=0&hp=0&avatartype=" + AvatarTypeStr);
                            var response = (HttpWebResponse)request.GetResponse();
                            string responseString;
                            using (var stream = response.GetResponseStream())
                            {
                                using (var reader = new StreamReader(stream))
                                {
                                    responseString = reader.ReadToEnd();
                                }
                            }
                        }
                    }

                    if (Is2007)
                    {
                        string[] values = { "GlobalHat1", "GlobalHat2", "GlobalHat3", "GlobalShirt", "GlobalPants", "GlobalTshirt" };
                        for (int i = 0; i < values.Length; i++)
                        {
                            if (string.IsNullOrEmpty(values[i]))
                            {
                                values[i] = "0";
                            }
                        }

                        string someText = "loadfile(\"http://www.roblox.com/game/join.ashx?username=" + userName + "&id=" + ID + "&ip=" + ipaddr + "&hat1=" + GlobalHat1 + "&hat2=" + GlobalHat2 + "&hat3=" + GlobalHat3 + "&tshirt=" + GlobalTshirt + "&port=" + port + "&hc=" + HeadColor + "&tc=" + TorsoColor + "&la=" + LeftArmColor + "&ll=" + LeftLegColor + "&ra=" + RightArmColor + "&rl=" + RightLegColor + "\")()";
                        someText = someText.Replace("=0&", "=86487700&");
                        File.WriteAllText(@"clients\\" + selectedClient + "\\Player\\content\\join.lua", someText);

                        Directory.SetCurrentDirectory("clients\\" + selectedClient + "\\Player\\");
                        Process.Start("Roblox.exe", "-script \"" + Directory.GetCurrentDirectory() + "\\content\\join.lua\"");
                        Directory.SetCurrentDirectory("..\\..\\..");
                    }
                }
            }
            catch
            {
                MessageBox.Show("something went wrong.");
            }
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            if (IsRobloxApp)
            {
                string selectedClient = clientBox.SelectedItem.ToString();
                Process.Start("clients\\" + selectedClient + "\\Player\\RobloxApp.exe", "\"" + Directory.GetCurrentDirectory() + "\\files\\web\\1818");
            }
        }
        private void cacheEnabled(object sender, EventArgs e)
        {
            if (assetCache.Checked)
            {
                if (File.Exists("files\\webroot\\asset\\nocache.php"))
                {
                    File.Delete("files\\webroot\\asset\\nocache.php");
                }

                if (File.Exists("files\\webroot\\asset\\cacher.php"))
                {
                    File.Replace("files\\webroot\\asset\\cacher.php", "files\\webroot\\asset\\index.php", "files\\webroot\\asset\\nocache.php");
                }

                if (File.Exists("files\\webroot\\api\\asset\\nocache.php"))
                {
                    File.Delete("files\\webroot\\api\\asset\\nocache.php");
                };

                if (File.Exists("files\\webroot\\api\\asset\\cacher.php"))
                {
                    File.Replace("files\\webroot\\api\\asset\\cacher.php", "files\\webroot\\api\\asset\\index.php", "files\\webroot\\api\\asset\\nocache.php");
                }
            }
            else
            {
                if (File.Exists("files\\webroot\\asset\\nocache.php"))
                {
                    File.Replace("files\\webroot\\asset\\nocache.php", "files\\webroot\\asset\\index.php", "files\\webroot\\asset\\cacher.php");
                }

                if (File.Exists("files\\webroot\\api\\asset\\nocache.php"))
                {
                    File.Replace("files\\webroot\\api\\asset\\nocache.php", "files\\webroot\\api\\asset\\index.php", "files\\webroot\\api\\asset\\cacher.php");
                }
            }
        }

        private void charRemove(object sender, EventArgs e)
        {
            switch (((PictureBox)sender).Name)
            {
                case "Hat1Box":
                    {
                        listBox0.ClearSelected();
                        hatName.Text = string.Empty;
                        Hat1Box.Image = null;
                        GlobalHat1 = "0";
                    }
                    break;
                case "Hat2Box":
                    {
                        listBox1.ClearSelected();
                        textBox2.Text = string.Empty;
                        Hat2Box.Image = null;
                        GlobalHat2 = "0";
                    }
                    break;
                case "Hat3Box":
                    {
                        listBox2.ClearSelected();
                        textBox3.Text = string.Empty;
                        Hat3Box.Image = null;
                        GlobalHat3 = "0";
                    }
                    break;
                case "ShirtBox":
                    {
                        listBox3.ClearSelected();
                        textBox4.Text = string.Empty;
                        ShirtBox.Image = null;
                        GlobalShirt = "0";
                    }
                    break;
                case "PantsBox":
                    {
                        listBox4.ClearSelected();
                        textBox5.Text = string.Empty;
                        PantsBox.Image = null;
                        GlobalPants = "0";
                    }
                    break;
                case "TShirtBox":
                    {
                        listBox4.ClearSelected();
                        textBox5.Text = string.Empty;
                        TShirtBox.Image = null;
                        GlobalTshirt = "0";
                    }
                    break;
            }
        }

        static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            var dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            Directory.CreateDirectory(destinationDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        private void fileSystemWatcher1_Created(object sender, FileSystemEventArgs e) { mapBox.Items.Add(e.FullPath); }

        private void fileSystemWatcher1_Deleted(object sender, FileSystemEventArgs e) { mapBox.Items.Remove(e.FullPath); }

        public static void Decompress(FileInfo fileToDecompress)
        {
            using (FileStream originalFileStream = fileToDecompress.OpenRead())
            {
                string currentFileName = fileToDecompress.FullName;
                string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

                using (FileStream decompressedFileStream = File.Create(@"files\web\1818"))
                {
                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                    }
                }
            }
        }

        private void tabPage1_Click(object sender, EventArgs e) { }

        private void Settings_Click(object sender, EventArgs e) { }

        private void bodyColorsChanged(object sender, EventArgs e)
        {
            var textBox = (TextBox)sender;

            switch (textBox.Name)
            {
            case "headColor":
                HeadColor = textBox.Text;
                break;
            case "torsoColor":
                TorsoColor = textBox.Text;
                break;
            case "leftArmColor":
                LeftArmColor = textBox.Text;
                break;
            case "leftLegColor":
                LeftLegColor = textBox.Text;
                break;
            case "rightArm":
                RightArmColor = textBox.Text;
                break;
            case "rightLeg":
                RightLegColor = textBox.Text;
                break;
            }
        }
    }
}

