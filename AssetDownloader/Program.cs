using System;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using System.Net.Http;
using System.IO.Compression;

namespace Aphylla
{
    class AssetInfo
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Location { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            if (args.Length <= 0)
            {
                Console.WriteLine("Missing command line arguments.");
            }

            using (var _httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                _httpClient.BaseAddress = new Uri("https://economy.roblox.com/v2/assets/" + args[0] + "/details");

                HttpResponseMessage response = _httpClient.GetAsync(string.Empty).Result;
                response.EnsureSuccessStatusCode();

                JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
                AssetInfo assetInfo = jsSerializer.Deserialize<AssetInfo>(response.Content.ReadAsStringAsync().Result);
                string name = assetInfo.Name;
                AssetInfo assetInfoFile = new AssetInfo()
                {
                    Name = name
                };

                string jsonData = jsSerializer.Serialize(assetInfoFile);
                File.WriteAllText(args[0] + ".info.json", jsonData);
            }

            using (var _httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                _httpClient.BaseAddress = new Uri("https://thumbnails.roblox.com/v1/assets?assetIds=" + args[0] + "&returnPolicy=PlaceHolder&size=250x250&format=Png&isCircular=false");
                
                var response = _httpClient.GetAsync(string.Empty).Result;
                response.EnsureSuccessStatusCode();

                var jsSerializer = new JavaScriptSerializer();
                var assetThumb = jsSerializer.Deserialize<AssetInfo>("{ " + response.Content.ReadAsStringAsync().Result.Split(",".ToCharArray())[2] + " }");
                DownloadThumbnail(assetThumb.ImageUrl);

            }

            using (var _httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                _httpClient.BaseAddress = new Uri("https://assetdelivery.roblox.com/v2/asset/?id=" + args[0] + "&version=1");
                
                HttpResponseMessage response = _httpClient.GetAsync(string.Empty).Result;
                response.EnsureSuccessStatusCode();

                var jsSerializer = new JavaScriptSerializer();
                var asset = jsSerializer.Deserialize<AssetInfo>("{ " + response.Content.ReadAsStringAsync().Result.Split(",".ToCharArray())[1].Replace(']', ' ')); //Not how this should be done, but it's ok for now
                DownloadAssetUrl(asset.Location);
            }

            void DownloadThumbnail(string Target)
            {
                using (var _webClient = new WebClient() { Proxy = null })
                {
                    _webClient.DownloadFile(Target, args[0] + ".thumb.png");
                }
            }

            void DownloadAssetUrl(string Target)
            {
                using (var _webClient = new WebClient() { Proxy = null })
                {
                    _webClient.Headers.Add("Accept-Encoding", string.Empty);
                    
                    var responseStream = new GZipStream(_webClient.OpenRead(Target), CompressionMode.Decompress);
                    var asset = new StreamWriter($"{Directory.GetCurrentDirectory()}\\{args[0]}");
                    var memoryStream = new MemoryStream();
                    
                    responseStream.CopyTo(memoryStream);
                    asset.BaseStream.Write(memoryStream.ToArray(), 0, memoryStream.ToArray().Length);
                }
            }
        }
    }
}
