using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Web;


namespace VanillaLauncher
{
    class Host
    {
		public static async Task HostAsync(string selectedClient)
		{
			if (File.Exists("clients\\" + selectedClient + "\\RCC\\content\\place.rbxl"))
			{
				File.Delete("clients\\" + selectedClient + "\\RCC\\content\\place.rbxl");
			}
			File.Copy("files\\web\\1818.rbxl", "clients\\" + selectedClient + "\\RCC\\content\\place.rbxl");
			Directory.SetCurrentDirectory("clients\\" + selectedClient + "\\RCC");
			Process.Start("CMD.exe", "/C RCCService.exe -console -start -placeid:1818");
            //Process.GetCurrentProcess().Kill();
        }
    }
}
