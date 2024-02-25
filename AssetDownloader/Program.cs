﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using System.Net.Http;


namespace AssetDownloader
{

    class AssetInfo
    {
        public string Name { get; set; }
        public string imageUrl { get; set; }
    }

       

    class Program
    {
        public string thumbnailurl { get; set; }
        static void Main(string[] args)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            if (args[0] == null)
            {
                Console.WriteLine("run with asset id. example: AssetDownloader.exe 1804739");
            }
            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                client.BaseAddress = new Uri("https://economy.roblox.com/v2/assets/" + args[0] + "/details");
                HttpResponseMessage response = client.GetAsync("").Result;
                response.EnsureSuccessStatusCode();
                string result = response.Content.ReadAsStringAsync().Result;
                JavaScriptSerializer js = new JavaScriptSerializer();
                AssetInfo assetInfo = js.Deserialize<AssetInfo>(result);
                string name = assetInfo.Name;
                AssetInfo assetInfoFile = new AssetInfo()
                {
                    Name = name
                };

                string jsonData = js.Serialize(assetInfoFile);
                File.WriteAllText(args[0] + ".info.json", jsonData);
            }

            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                client.BaseAddress = new Uri("https://thumbnails.roblox.com/v1/assets?assetIds=" + args[0] + "&returnPolicy=PlaceHolder&size=250x250&format=Png&isCircular=false");
                HttpResponseMessage response = client.GetAsync("").Result;
                response.EnsureSuccessStatusCode();
                string result = response.Content.ReadAsStringAsync().Result;
                // File.WriteAllText("debug.txt", result);
                JavaScriptSerializer js = new JavaScriptSerializer();
                AssetInfo assetthumb = js.Deserialize<AssetInfo>("{ " + result.Split(",".ToCharArray())[2] + " }");
                downloadThumbnail(assetthumb.imageUrl);
                downloadAssetUrl("https://assetdelivery.roblox.com/v1/asset/?id=" + args[0] + "&version=1");

            }
            void downloadThumbnail(string thumburl)
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFile(thumburl, args[0] + ".thumb.png");
            }
            void downloadAssetUrl(string assetUrl)
            {
                WebClient webClient = new WebClient();
                webClient.Headers.Add("Accept-Encoding", "");
                webClient.DownloadFile(assetUrl, args[0]);
            }
        }
    }
}