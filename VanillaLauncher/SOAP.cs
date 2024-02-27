using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Net.Http;
using System.Net;
using System.Xml;
using System.Data;
using System.IO;
namespace VanillaLauncher
{
    class SOAP
    {
        public static void Execute(string Client)
        {
            HttpWebRequest httpWebRequest = CreateWebRequest();
            httpWebRequest.Timeout = 1000;
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(File.ReadAllText("clients\\" + Client + "\\RCC\\SOAP.xml"));
            using (Stream outStream = httpWebRequest.GetRequestStream())
            {
                xmlDocument.Save(outStream);
            }
            // have to get response for some reason, but it errors or times out so we do this
            try
            {
                using (WebResponse webResponse = httpWebRequest.GetResponse())
                {
                }
            }
            catch
            {
                return;
            }

        }

        public static HttpWebRequest CreateWebRequest()
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:64989");
            httpWebRequest.Headers.Add("SOAP:Action");
            httpWebRequest.ContentType = "application/xml;charset=\"utf-8\"";
            httpWebRequest.Accept = "*/*";
            httpWebRequest.Method = "POST";
            return httpWebRequest;
        }
    }
}
