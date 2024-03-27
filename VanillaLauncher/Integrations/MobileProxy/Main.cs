using System;
using System.Net;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Http;
using Titanium.Web.Proxy.Models;

namespace VanillaLauncher.MobileProxy
{
    public class Main
    {
        public void StartProxy()
        {            
            var proxyServer = new ProxyServer();
            proxyServer.CertificateManager.CertificateEngine = Titanium.Web.Proxy.Network.CertificateEngine.DefaultWindows; 
            proxyServer.CertificateManager.EnsureRootCertificate();

            var explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Any, 8000, true);
            explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Any, 8000);
            explicitEndPoint.BeforeTunnelConnectRequest += OnBeforeTunnelConnectRequest; // Fired when a CONNECT request is received.

            /*
                An explicit endpoint is where the client knows about the existence of a proxy,
                so client sends request in a proxy friendly manner.
            */
            proxyServer.AddEndPoint(explicitEndPoint);
            proxyServer.Start();

        }
    
        private async Task OnBeforeTunnelConnectRequest(object sender, TunnelConnectSessionEventArgs e)
        {
            string hostName = e.HttpClient.Request.RequestUri.Host;

            if (hostName.Contains("dropbox.com"))
            {
                e.DecryptSsl = false; // // Excludes addresses that you don't want to proxy.
            }
        }

        public async Task OnRequest(object sender, SessionEventArgs e)
        {
            Console.WriteLine(e.HttpClient.Request.Url);

            var requestHeaders = e.HttpClient.Request.Headers;
            var method = e.HttpClient.Request.Method.ToUpper();
            if ((method == "POST" || method == "PUT" || method == "PATCH"))
            {
                byte[] bodyBytes = await e.GetRequestBody();
                e.SetRequestBody(bodyBytes);

                string bodyString = await e.GetRequestBodyAsString();
                e.SetRequestBodyString(bodyString);

                e.UserData = e.HttpClient.Request;
            }

            if (e.HttpClient.Request.RequestUri.AbsoluteUri.Contains("google.com"))
            {
                e.Ok("<!DOCTYPE html>" +
                    "<html><body><h1>" +
                    "Website Blocked" +
                    "</h1>" +
                    "<p>Blocked by titanium web proxy.</p>" +
                    "</body>" +
                    "</html>");
            }

            if (e.HttpClient.Request.RequestUri.AbsoluteUri.Contains("wikipedia.org"))
            {
                e.Redirect("https://www.paypal.com");
            }
        }

        public async Task OnResponse(object sender, SessionEventArgs e)
        {
            var responseHeaders = e.HttpClient.Response.Headers;

            if (e.HttpClient.Request.Method == "GET" || e.HttpClient.Request.Method == "POST")
            {
                if (e.HttpClient.Response.StatusCode == 200)
                {
                    if (e.HttpClient.Response.ContentType != null && e.HttpClient.Response.ContentType.Trim().ToLower().Contains("text/html"))
                    {
                        byte[] bodyBytes = await e.GetResponseBody();
                        e.SetResponseBody(bodyBytes);

                        string body = await e.GetResponseBodyAsString();
                        e.SetResponseBodyString(body);
                    }
                }
            }

            if (e.UserData != null)
            {
                var request = (Request)e.UserData; // Access request from UserData property where we stored it in RequestHandler.
            }
        }

        // Allows overriding default certificate validation logic.
        public Task OnCertificateValidation(object sender, CertificateValidationEventArgs e)
        {
            // Set IsValid to true/false based on certificate errors.
            if (e.SslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
            {
                e.IsValid = true;
            }
            return Task.CompletedTask;
        }

        // Allows overriding default client certificate selection logic during mutual authentication.
        public Task OnCertificateSelection(object sender, CertificateSelectionEventArgs e)
            => Task.CompletedTask; // Set e.clientCertificate to override.
    }
}
