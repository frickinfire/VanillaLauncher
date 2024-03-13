using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Xml;
using System.Web;
using System.Security.Principal;
using System.IO.Compression;
using Newtonsoft.Json;
using System.Security;
using System.Data.SQLite;
using Titanium.Web.Proxy.Examples.Basic;
using System.Reflection;

namespace VanillaLauncher
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
        }
        string curItem { get; set; }
        bool is2007 { get; set; }
        bool isRobloxApp { get; set; }
        bool isRobloxPlayerBeta { get; set; }
        bool isRCCService { get; set; }
        bool avatarFetchRequired { get; set; }
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
        string GlobalTshirt{ get; set; }


        public Vanilla()
        {
            InitializeComponent();


            string hostsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts");

            WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            bool administrativeMode = principal.IsInRole(WindowsBuiltInRole.Administrator);
            // this sucks but it doesnt launch if we don't do this
            bool administrativeMode2 = principal.IsInRole(WindowsBuiltInRole.Administrator);
            if (!administrativeMode2)
            {
                Process.Start("CMD.exe", "/C taskkill /F /IM nginx.exe");
                Process.Start("CMD.exe", "/C taskkill /F /IM php-cgi.exe");
                Process.Start("CMD.exe", "/C taskkill /F /IM RunHiddenConsole.exe");

                string httpdconf = File.ReadAllText("files\\webserver\\conf\\nginx.conf");
                if (httpdconf.Contains(@"C:/Vanilla/files/webroot"))
                {
                    string CurrentDirFixed = Directory.GetCurrentDirectory().Replace(@"\", @"/");
                    string fixedconf = httpdconf.Replace(@"C:/Vanilla/files/webroot", CurrentDirFixed + @"/files/webroot");
                    File.WriteAllText("files\\webserver\\conf\\nginx.conf", fixedconf);
                }
            }
            if (!administrativeMode)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.Verb = "runas";
                startInfo.FileName = Application.ExecutablePath;
                try
                {

                    System.Threading.Thread.Sleep(3000);
                    Process.Start("files\\webserver\\php\\RunHiddenConsole.exe", "/r " + Directory.GetCurrentDirectory() + "\\files\\webserver\\php\\php-cgi.exe -b 127.0.0.1:9123");
                    Directory.SetCurrentDirectory(Directory.GetCurrentDirectory() + "\\files\\webserver\\");
                    Process.Start(Directory.GetCurrentDirectory() + "\\nginx.exe");
                    Directory.SetCurrentDirectory("..\\..");
                    Process.Start(startInfo);
                    Process.GetCurrentProcess().Kill();
                }
                catch
                {
                    return;
                }
                return;
            }
            if (!File.Exists(Directory.GetCurrentDirectory() + @"\files\vanilla.sqlite"))
            {
                SQLiteConnection.CreateFile(Directory.GetCurrentDirectory() + @"\files\vanilla.sqlite");

                using (var sqlite2 = new SQLiteConnection(@"Data Source=" + Directory.GetCurrentDirectory() + @"\files\vanilla.sqlite"))
                {
                    sqlite2.Open();
                    string sql = "create table characterappearance (torsocolor text, leftlegcolor text, leftarmcolor text, rightlegcolor text, rightarmcolor text, headcolor text, asset1 text, asset2 text, asset3 text, asset4 text, asset5 text, asset6 text, asset7 text, asset8 text, asset9 text, asset10 text, asset11 text, asset12 text, asset13 text, userid text)";
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
            Application.ApplicationExit += new EventHandler(onshutdown);
            string pathfile = Environment.GetEnvironmentVariable("PATH");
            if (!pathfile.Contains(Directory.GetCurrentDirectory() + @"\files\webserver\php"))
            {
                var name = "PATH";
                var scope = EnvironmentVariableTarget.Machine;
                var oldValue = Environment.GetEnvironmentVariable(name, scope);
                var newValue = oldValue + @";" + Directory.GetCurrentDirectory() + @"\files\webserver\php";
                Environment.SetEnvironmentVariable(name, newValue, scope);
            }
            string pathfile2 = Environment.GetEnvironmentVariable("PATH");
            if (!pathfile2.Contains(Directory.GetCurrentDirectory() + @"\files\webserver\openssl"))
            {
                var name = "PATH";
                var scope = EnvironmentVariableTarget.Machine;
                var oldValue = Environment.GetEnvironmentVariable(name, scope);
                var newValue = oldValue + @";" + Directory.GetCurrentDirectory() + @"\files\webserver\openssl";
                Environment.SetEnvironmentVariable(name, newValue, scope);
            }
            string pathfile3 = Environment.GetEnvironmentVariable("OPENSSL_CONF");
            if (pathfile3 != null)
            {
                var name = "OPENSSL_CONF";
                var scope = EnvironmentVariableTarget.Machine;
                var newValue = Directory.GetCurrentDirectory() + @"\files\webserver\php\extras\ssl\openssl.cnf";
                Environment.SetEnvironmentVariable(name, newValue, scope);

            }
            else
            {
                var name = "OPENSSL_CONF";
                var scope = EnvironmentVariableTarget.Machine;
                var newValue = Directory.GetCurrentDirectory() + @"\files\webserver\php\extras\ssl\openssl.cnf";
                Environment.SetEnvironmentVariable(name, newValue, scope);
            }
            FileInfo fileInfo = new FileInfo(hostsFile);
            if (fileInfo.IsReadOnly)
            {
                MessageBox.Show(
                         "Vanilla will not work until you have disabled 'Read-Only' on your HOSTS file! Do this in C:\\Windows\\System32\\drivers\\etc.",
                         "Warning",
                          MessageBoxButtons.OK,
                          MessageBoxIcon.Warning);

            }
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
            if (File.Exists("files\\settings.json"))
            {
                dynamic val = JsonConvert.DeserializeObject(File.ReadAllText("files\\settings.json"));
                hostPortNew.Text = val["HostPort"];
                idBox.Text = val["ID"];
                userNameBox.Text = val["Username"];
                curItem = val["Client"];
                GlobalHat1 = val["Hat1"];
                GlobalHat2 = val["Hat2"];
                GlobalHat3 = val["Hat3"];
                GlobalShirt = val["Shirt"];
                GlobalPants = val["Pants"];
                GlobalTshirt = val["TShirt"];
                ClientInfo.Text = "selected client: None";
                string[] values = { GlobalShirt, GlobalPants, GlobalHat1, GlobalHat2, GlobalHat3, GlobalHat3, GlobalTshirt };

                foreach(string goon in values)
                {

                    //string prefix = "listBox";
                    //var increment = -1;
                    // UGH !!!! i will make this work LATER. for NOW... yandere dev code
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
                }
            }
            ProxyTestController controller = new ProxyTestController();

            // Start proxy controller
//controller.StartProxy();

           

        }
       
        public void setglobal(object sender, EventArgs e)
        {
            if (sender == hostPortNew)
            {
                GlobalHostPort = hostPortNew.Text;
            }
            if (sender == userNameBox)
            {
                GlobalUsername = userNameBox.Text;
            }
            if (sender == idBox)
            {
                GlobalID = idBox.Text;
            }
        }
        public void onshutdown(object sender, EventArgs e)
        {

            if (File.Exists("files\\settings.json"))
            {
                File.Delete("files\\settings.json");
            }
            SettingsFile jsonfile = new SettingsFile
            {
                Username = GlobalUsername,
                ID = GlobalID,
                HostPort = GlobalHostPort,
                Client = curItem,
                Map = GlobalMap,
                Hat1 = GlobalHat1,
                Hat2 = GlobalHat2,
                Hat3 = GlobalHat3,
                Shirt = GlobalShirt,
                Pants = GlobalPants,
                TShirt = GlobalTshirt
            };
            File.WriteAllText(@"files\\settings.json", JsonConvert.SerializeObject(jsonfile));
            using (StreamWriter file = File.CreateText(@"files\\settings.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, jsonfile);
            }
            string hostsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts");
            // don't use file.replace here or it'll cause issues
            File.Delete(hostsFile);
            File.Copy(hostsFile + ".bak", hostsFile);
            Process.Start("CMD.exe", "/C taskkill /f /im RunHiddenConsole.exe");
            Process.Start("CMD.exe", "/C taskkill /F /IM nginx.exe");
            Process.Start("CMD.exe", "/C taskkill /F /IM php-cgi.exe");

        }
        private void Form1_Load(object sender, EventArgs e)
        {

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
            if (sender == listBox0)
            {
                if (listBox0.SelectedItem == null)
                {
                    return;
                }
                dynamic val = JsonConvert.DeserializeObject(File.ReadAllText("files\\char\\hats\\" + listBox0.SelectedItem.ToString() + ".info.json"));
                hatName.Text = val["Name"];
                pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox2.Image = Image.FromFile("files\\char\\hats\\" + listBox0.SelectedItem.ToString() + ".thumb.png");
                pictureBox2.Refresh();
                GlobalHat1 = listBox0.SelectedItem.ToString();
            }
            if (sender == listBox1)
            {
                if (listBox1.SelectedItem == null)
                {
                    return;
                }
                dynamic val = JsonConvert.DeserializeObject(File.ReadAllText("files\\char\\hats\\" + listBox1.SelectedItem.ToString() + ".info.json"));
                textBox2.Text = val["Name"];
                pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox3.Image = Image.FromFile("files\\char\\hats\\" + listBox1.SelectedItem.ToString() + ".thumb.png");
                pictureBox3.Refresh();
                pictureBox3.Visible = true;
                GlobalHat2 = listBox1.SelectedItem.ToString();
            }
            if (sender == listBox2)
            {
                if (listBox2.SelectedItem == null)
                {
                    return;
                }
                dynamic val = JsonConvert.DeserializeObject(File.ReadAllText("files\\char\\hats\\" + listBox2.SelectedItem.ToString() + ".info.json"));
                textBox3.Text = val["Name"];
                pictureBox4.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox4.Image = Image.FromFile("files\\char\\hats\\" + listBox2.SelectedItem.ToString() + ".thumb.png");
                pictureBox4.Refresh();
                pictureBox4.Visible = true;
                GlobalHat3 = listBox2.SelectedItem.ToString();
            }
            if (sender == listBox3)
            {
                if (listBox3.SelectedItem == null)
                {
                    return;
                }
                dynamic val = JsonConvert.DeserializeObject(File.ReadAllText("files\\char\\shirts\\" + listBox3.SelectedItem.ToString() + ".info.json"));
                textBox4.Text = val["Name"];
                pictureBox5.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox5.Image = Image.FromFile("files\\char\\shirts\\" + listBox3.SelectedItem.ToString() + ".thumb.png");
                pictureBox5.Refresh();
                pictureBox5.Visible = true;
                GlobalShirt = listBox3.SelectedItem.ToString();
            }
            if (sender == listBox4)
            {
                if (listBox4.SelectedItem == null)
                {
                    return;
                }
                dynamic val = JsonConvert.DeserializeObject(File.ReadAllText("files\\char\\pants\\" + listBox4.SelectedItem.ToString() + ".info.json"));
                textBox5.Text = val["Name"];
                pictureBox6.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox6.Image = Image.FromFile("files\\char\\pants\\" + listBox4.SelectedItem.ToString() + ".thumb.png");
                pictureBox6.Refresh();
                pictureBox6.Visible = true;
                GlobalPants = listBox4.SelectedItem.ToString();
            }
            if (sender == listBox5)
            {
                if (listBox5.SelectedItem == null)
                {
                    return;
                }
                dynamic val = JsonConvert.DeserializeObject(File.ReadAllText("files\\char\\t-shirts\\" + listBox5.SelectedItem.ToString() + ".info.json"));
                textBox6.Text = val["Name"];
                pictureBox7.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox7.Image = Image.FromFile("files\\char\\t-shirts\\" + listBox5.SelectedItem.ToString() + ".thumb.png");
                pictureBox7.Refresh();
                pictureBox7.Visible = true;
                GlobalTshirt = listBox5.SelectedItem.ToString();
            }
        }

        private void clientChanged(object Sender, System.EventArgs e)
        {
            if (clientBox.SelectedItem == null)
            {
                return;
            }
            else
            {
                System.IO.DirectoryInfo di = new DirectoryInfo("files\\webroot\\");
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
                curItem = clientBox.SelectedItem.ToString();
                System.IO.Compression.ZipFile.ExtractToDirectory("files\\filesets\\" + curItem + ".zip", "files\\webroot");
                System.IO.Compression.ZipFile.ExtractToDirectory("files\\filesets\\common.zip", "files\\webroot");
                if (assetCache.Checked)
                {
                    File.Replace("files\\webroot\\asset\\cacher.php", "files\\webroot\\asset\\index.php", "files\\webroot\\asset\\nocache.php");

                }
                ClientInfo.Text = "selected client: " + curItem;
                if (File.Exists("clients\\" + curItem + "\\client.json"))
                {
                    dynamic val = JsonConvert.DeserializeObject(File.ReadAllText("clients\\" + curItem + "\\client.json"));
                    isRobloxApp = val["isRobloxApp"] == "true";
                    isRobloxPlayerBeta = val["isRobloxPlayerBeta"] == "true";
                    isRCCService = val["isRCCService"] == "true";
                    is2007 = val["is2007"] == "true";
                    avatarFetchRequired = val["avatarFetchRequired"] == "true";
                    isRobloxPlayer = val["isRobloxPlayer"] == "true";
                }
                try
                {
                    ScreenShot.Image = Image.FromFile("clients\\" + curItem + "\\photo.png");
                }
                catch
                {
                    ScreenShot.Image = null;
                }
            }

        }

      
        private void HostButton_Click_1(object sender, EventArgs e)
        {
            string selectedClient = curItem;
            string hostPortstring = hostPortNew.Text;
            if (isRCCService)
            {
                if (selectedClient == "2015M")
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
                    System.Threading.Thread.Sleep(9000);
                    Classes.SOAP.Execute(selectedClient);
                }

            }
            if (isRobloxPlayer)
            {
                Directory.SetCurrentDirectory("clients\\" + selectedClient + "\\Player\\");
                Process.Start("RobloxPlayer.exe", "-joinScriptUrl \"http://www.roblox.com/game/gameserver.ashx?port=" + hostPortstring + "\"");
                Directory.SetCurrentDirectory("..\\..\\..");
            }
            if (isRobloxApp && !isRCCService)
            {
                Directory.SetCurrentDirectory("clients\\" + selectedClient + "\\Player\\");
                Process.Start("RobloxApp.exe", "-no3d -script \"loadfile('http://www.roblox.com/game/gameserver.ashx?port="+ hostPortstring + "')()\"");
                Directory.SetCurrentDirectory("..\\..\\..");
            }
            if (isRobloxPlayerBeta && !isRCCService)
            { 
                Directory.SetCurrentDirectory("clients\\" + selectedClient + "\\Player\\");
                Process.Start("RobloxPlayerBeta.exe", "-j \"http://www.roblox.com/game/gameserver.ashx?port=53640\" -t \"0\" -a \"http://roblox.com/goon\"");
                Directory.SetCurrentDirectory("..\\..\\..");
            }
            if (is2007)
            {
                Directory.SetCurrentDirectory("clients\\" + selectedClient + "\\Player\\");
                Process.Start("Roblox.exe", "-no3d -script \"" + Directory.GetCurrentDirectory() + "\\content\\gameserver.lua\" \"" + Directory.GetCurrentDirectory() +"\\..\\..\\..\\" + GlobalMap);

                Directory.SetCurrentDirectory("..\\..\\..");
            }
       
            //Directory.SetCurrentDirectory("..\\..\\..");
            //Process.GetCurrentProcess().Kill();

        }

        private void JoinButton_Click_1(object sender, EventArgs e)
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
                if (isRobloxApp && !isRobloxPlayer)
                {
                    string[] values = { shirts, pants, hat1, hat2s, hat3s, tshirts };
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (String.IsNullOrEmpty(values[i]))
                        {
                            values[i] = "0";
                        }
                    }

                    Directory.SetCurrentDirectory("clients\\" + selectedClient + "\\Player\\");
                    Process.Start("RobloxApp.exe", "-build -script \"loadfile('http://www.roblox.com/game/join.ashx?username=" + userName + "&id=" + ID + "&ip=" + ipaddr + "&hat1=" + hat1 + "&hat2=" + hat2s + "&hat3=" + hat3s + "&shirt=" + shirts + "&pants=" + pants + "&tshirt=" + tshirts + "&port=" + port + "')()\"");
                    Directory.SetCurrentDirectory("..\\..\\..");

                }

                if (isRobloxPlayer)
                {
                    Directory.SetCurrentDirectory("clients\\" + selectedClient + "\\Player\\");
                    Process.Start("RobloxPlayer.exe", "-joinScriptUrl \"http://www.roblox.com/game/join.ashx?username=" + userName + "&id=" + ID + "&ip=" + ipaddr + "&hat1=" + hat1 + "&hat2=" + hat2s + "&hat3=" + hat3s + "&shirt=" + shirts + "&pants=" + pants + "&tshirt=" + tshirts + "&port=" + port + "\"");
                    Directory.SetCurrentDirectory("..\\..\\..");
                }
                if (isRobloxPlayerBeta)
                {
                    if (avatarFetchRequired)
                    {
                        var request = (HttpWebRequest)WebRequest.Create("http://" + ipaddr + ":53642/v1.1/set-avatar/?userid=" + ID + "&headc=null&torsoc=null&rarmc=null&larmc=null&llegc=null&rlegc=null&shirt=" + shirts + "&tshirt=" + tshirts + "&pants=" + pants + "&face=0&hat1=" + hat1 + "&hat2=" + hat2s + "&hat3=" + hat3s + "&torsop=0&lap=0&llp=0&rap=0&rlp=0&hp=0");
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
                        Directory.SetCurrentDirectory("clients\\" + selectedClient + "\\Player\\");
                        Process.Start("RobloxPlayerBeta.exe", "-j \"http://www.roblox.com/game/join.ashx?username=" + userName + "&id=" + ID + "&ip=" + ipaddr + "&hat1=" + hat1 + "&hat2=" + hat2s + "&hat3=" + hat3s + "&shirt=" + shirts + "&pants=" + pants + "&tshirt=" + tshirts + "&port=" + port + "&PlaceId=1818\" -t \"0\" -a \"http://www.roblox.com/Login/Negotiate.ashx\"");
                        Directory.SetCurrentDirectory("..\\..\\..");
                    
                }

                if (is2007)
                {
                    string[] values = { "GlobalHat1", "GlobalHat2", "GlobalHat3", "GlobalShirt", "GlobalPants", "GlobalTshirt" };
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (String.IsNullOrEmpty(values[i]))
                        {
                            values[i] = "0";
                        }
                    }

                    string someText = "loadfile(\"http://www.roblox.com/game/join.ashx?username=" + userName + "&id=" + ID + "&ip=" + ipaddr + "&hat1=" + GlobalHat1 + "&hat2=" + GlobalHat2 + "&hat3=" + GlobalHat3 + "&tshirt=" + GlobalTshirt + "&port=" + port + "\")()";
                    someText = someText.Replace("=0&", "=86487700&");
                    File.WriteAllText(@"clients\\" + selectedClient + "\\Player\\content\\join.lua", someText);

                    Directory.SetCurrentDirectory("clients\\" + selectedClient + "\\Player\\");
                    Process.Start("Roblox.exe", "-script \"" + Directory.GetCurrentDirectory() + "\\content\\join.lua\"");
                    Directory.SetCurrentDirectory("..\\..\\..");
                }
                if (isRCCService)
                {
                    Directory.SetCurrentDirectory("clients\\" + selectedClient + "\\Player\\");
                    Process.Start("RobloxPlayerBeta.exe", "-j \"http://www.roblox.com/game/join.ashx?username=" + userName + "&id=" + ID + "&ip=" + ipaddr + "&hat1=" + hat1 + "&hat2=" + hat2s + "&hat3=" + hat3s + "&shirt=" + shirts + "&pants=" + pants + "&tshirt=" + tshirts + "&port=" + port + "\" -t \"0\" -a \"http://www.roblox.com/Login/Negotiate.ashx\"");
                    Directory.SetCurrentDirectory("..\\..\\..");
                }

            }

     
            


            }

            private void EditButton_Click(object sender, EventArgs e)
        {
            if (isRobloxApp)
            {
                string selectedClient = clientBox.SelectedItem.ToString();
                Process.Start("clients\\" + selectedClient + "\\Player\\RobloxApp.exe", "\"" + Directory.GetCurrentDirectory() + "\\" + GlobalMap + "\"");
            }
        }
        private void cacheEnabled(object sender, EventArgs e)
        {
            if (assetCache.Checked)
            {
                if (File.Exists("files\\webroot\\asset\\nocache.php")) { File.Delete("files\\webroot\\asset\\nocache.php"); }
                if (File.Exists("files\\webroot\\asset\\cacher.php"))
                {

                    File.Replace("files\\webroot\\asset\\cacher.php", "files\\webroot\\asset\\index.php", "files\\webroot\\asset\\nocache.php");
                }
            }
            else
            {
                if (File.Exists("files\\webroot\\asset\\nocache.php"))
                {
                    File.Replace("files\\webroot\\asset\\nocache.php", "files\\webroot\\asset\\index.php", "files\\webroot\\asset\\cacher.php");
                }
            }
        }
        private void earlyCorescripts(object sender, EventArgs e)
        {
            if (File.Exists("clients\\2015M\\Player\\ClientSettings\\ClientAppSettingsEarly.json"))
            {
                if (Directory.Exists("clients\\2015M"))
                {
                    if (File.Exists("clients\\2015M\\Player\\ClientSettings\\ClientAppSettingsEarly.json"))
                    {
                        File.Replace("clients\\2015M\\Player\\ClientSettings\\ClientAppSettingsEarly.json", "clients\\2015M\\player\\ClientSettings\\ClientAppSettings.json", "clients\\2015M\\player\\ClientSettings\\ClientAppSettingsMid.json");
                    }
                }
            }
            else
            {
                if (Directory.Exists("clients\\2015M"))
                {
                    if (File.Exists("clients\\2015M\\Player\\ClientSettings\\ClientAppSettingsMid.json"))
                    {
                        File.Replace("clients\\2015M\\Player\\ClientSettings\\ClientAppSettingsMid.json", "clients\\2015M\\player\\ClientSettings\\ClientAppSettings.json", "clients\\2015M\\player\\ClientSettings\\ClientAppSettingsEarly.json");
                    }
                }
            }

        }

        private void charRemove(object sender, EventArgs e)
        {

            if (sender == pictureBox2)
            {
                listBox0.ClearSelected();
                hatName.Text = "";
                pictureBox2.Image = null;
                GlobalHat1 = "0";
            }
            if (sender == pictureBox3)
            {
                listBox1.ClearSelected();
                textBox2.Text = "";
                pictureBox3.Image = null;
                GlobalHat2 = "0";
            }
            if (sender == pictureBox4)
            {
                listBox2.ClearSelected();
                textBox3.Text = "";
                pictureBox4.Image = null;
                GlobalHat3 = "0";
            }
            if (sender == pictureBox5)
            {
                listBox3.ClearSelected();
                textBox4.Text = "";
                pictureBox5.Image = null;
                GlobalShirt = "0";
            }
            if (sender == pictureBox6)
            {
                listBox4.ClearSelected();
                textBox5.Text = "";
                pictureBox6.Image = null;
                GlobalPants = "0";
            }
            if (sender == pictureBox7)
            {
                listBox5.ClearSelected();
                textBox6.Text = "";
                pictureBox7.Image = null;
                GlobalTshirt = "0";
            }



        }


        private void button1_Click(object sender, EventArgs e)
        {

        }
        // https://www.codeproject.com/Articles/19911/Dynamically-Invoke-A-Method-Given-Strings-with-Met 
        public static string InvokeStringMethod(string typeName, string methodName)
        {
            // Get the Type for the class
            Type calledType = Type.GetType(typeName);

            // Invoke the method itself. The string returned by the method winds up in s
            String s = (String)calledType.InvokeMember(
                            methodName,
                            BindingFlags.InvokeMethod | BindingFlags.Public |
                                BindingFlags.Static,
                            null,
                            null,
                            null);

            // Return the string that was returned by the called method.
            return s;
        }

        private void fileSystemWatcher1_Created(object sender, FileSystemEventArgs e) { mapBox.Items.Add(e.FullPath); }

        private void fileSystemWatcher1_Deleted(object sender, FileSystemEventArgs e)     { mapBox.Items.Remove(e.FullPath);  }


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
    }
}

