using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Exceptions;
using Titanium.Web.Proxy.Helpers;
using Titanium.Web.Proxy.Http;
using Titanium.Web.Proxy.Models;
using Titanium.Web.Proxy.StreamExtended.Network;

namespace Titanium.Web.Proxy.Examples.Basic
{
    public class ProxyTestController : IDisposable
    {
        private readonly ProxyServer proxyServer;

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private readonly ConcurrentQueue<Tuple<ConsoleColor?, string>> ConsoleMessageQueue
            = new ConcurrentQueue<Tuple<ConsoleColor?, string>>();

        private ExplicitProxyEndPoint explicitEndPoint;

        public ProxyTestController()
        {
            Task.Run(() => ListenToConsole());

            proxyServer = new ProxyServer();

            proxyServer.ExceptionFunc = async exception =>
            {
                if (exception is ProxyHttpException phex)
                    WriteToConsole(exception.Message + ": " + phex.InnerException?.Message, ConsoleColor.Red);
                else
                    WriteToConsole(exception.Message, ConsoleColor.Red);
            };

            proxyServer.TcpTimeWaitSeconds = 10;
            proxyServer.ConnectionTimeOutSeconds = 15;
            proxyServer.ReuseSocket = false;
            proxyServer.EnableConnectionPool = false;
            proxyServer.ForwardToUpstreamGateway = true;
            proxyServer.CertificateManager.SaveFakeCertificates = true;
        }

        private CancellationToken CancellationToken => cancellationTokenSource.Token;

        public void Dispose()
        {
            cancellationTokenSource.Dispose();
            proxyServer.Dispose();
        }

        public void StartProxy()
        {
            proxyServer.BeforeRequest += OnRequest;
            proxyServer.BeforeResponse += OnResponse;
            proxyServer.AfterResponse += OnAfterResponse;

            proxyServer.ServerCertificateValidationCallback += OnCertificateValidation;
            proxyServer.ClientCertificateSelectionCallback += OnCertificateSelection;

            explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Any, 8000);

            // Fired when a CONNECT request is received.
            explicitEndPoint.BeforeTunnelConnectRequest += OnBeforeTunnelConnectRequest;
            explicitEndPoint.BeforeTunnelConnectResponse += OnBeforeTunnelConnectResponse;

            /*
                An explicit endpoint is where the client knows about the existence of a proxy,
                so client sends request in a proxy friendly manner.
            */
            proxyServer.AddEndPoint(explicitEndPoint);
            proxyServer.Start();

            foreach (var endpoint in proxyServer.ProxyEndPoints)
            {
                Console.WriteLine("Listening on '{0}' endpoint at Ip {1} and port: {2} ",
                    endpoint.GetType().Name, endpoint.IpAddress, endpoint.Port);
            }

            // Only explicit proxies can be set as system proxy!
            if (RunTime.IsWindows)
            {
                proxyServer.SetAsSystemProxy(explicitEndPoint, ProxyProtocolType.AllHttp);
            }
        }

        public void Stop()
        {
            explicitEndPoint.BeforeTunnelConnectRequest -= OnBeforeTunnelConnectRequest;
            explicitEndPoint.BeforeTunnelConnectResponse -= OnBeforeTunnelConnectResponse;

            proxyServer.BeforeRequest -= OnRequest;
            proxyServer.BeforeResponse -= OnResponse;
            proxyServer.ServerCertificateValidationCallback -= OnCertificateValidation;
            proxyServer.ClientCertificateSelectionCallback -= OnCertificateSelection;

            proxyServer.Stop();
        }

        private async Task<IExternalProxy> OnGetCustomUpStreamProxyFunc(SessionEventArgsBase Arg)
        {
            Arg.GetState().PipelineInfo.AppendLine(nameof(OnGetCustomUpStreamProxyFunc));

            // This is just to show the functionality, provided values are junk.
            return new ExternalProxy
            {
                BypassLocalhost = false,
                HostName = "127.0.0.9",
                Port = 9090,
                Password = "fake",
                UserName = "fake",
                UseDefaultCredentials = false
            };
        }

        private async Task<IExternalProxy> OnCustomUpStreamProxyFailureFunc(SessionEventArgsBase Arg)
        {
            Arg.GetState().PipelineInfo.AppendLine(nameof(OnCustomUpStreamProxyFailureFunc));

            // this is just to show the functionality, provided values are junk
            return new ExternalProxy
            {
                BypassLocalhost = false,
                HostName = "127.0.0.10",
                Port = 9191,
                Password = "fake2",
                UserName = "fake2",
                UseDefaultCredentials = false
            };
        }

        private async Task OnBeforeTunnelConnectRequest(object sender, TunnelConnectSessionEventArgs e)
        {
            var hostName = e.HttpClient.Request.RequestUri.Host;
            e.GetState().PipelineInfo.AppendLine(nameof(OnBeforeTunnelConnectRequest) + ":" + hostName);
            WriteToConsole("Tunnel to: " + hostName);

            var clientLocalIp = e.ClientLocalEndPoint.Address;
            if (!clientLocalIp.Equals(IPAddress.Loopback) && !clientLocalIp.Equals(IPAddress.IPv6Loopback))
            {
                e.HttpClient.UpStreamEndPoint = new IPEndPoint(clientLocalIp, 0);
            }

            if (hostName.Contains("dropbox.com"))
            {
                e.DecryptSsl = false; // Excludes addresses that you don't want to proxy.
            }
        }

        private void WebSocket_DataSent(object sender, DataEventArgs e)
        {
            var args = (SessionEventArgs)sender;
            WebSocketDataSentReceived(args, e, true);
        }

        private void WebSocket_DataReceived(object sender, DataEventArgs e)
        {
            var args = (SessionEventArgs)sender;
            WebSocketDataSentReceived(args, e, false);
        }

        private void WebSocketDataSentReceived(SessionEventArgs Args, DataEventArgs e, bool Sent)
        {
            var color = Sent ? ConsoleColor.Green : ConsoleColor.Blue;

            foreach (var frame in Args.WebSocketDecoder.Decode(e.Buffer, e.Offset, e.Count))
            {
                if (frame.OpCode == WebsocketOpCode.Binary)
                {
                    var data = frame.Data.ToArray();
                    var str = string.Join(",", data.ToArray().Select(x => x.ToString("X2")));
                    WriteToConsole(str, color);
                }

                if (frame.OpCode == WebsocketOpCode.Text)
                {
                    WriteToConsole(frame.GetText(), color);
                }
            }
        }

        private Task OnBeforeTunnelConnectResponse(object sender, TunnelConnectSessionEventArgs e)
        {
            e.GetState().PipelineInfo
                .AppendLine(nameof(OnBeforeTunnelConnectResponse) + ":" + e.HttpClient.Request.RequestUri);

            return Task.CompletedTask;
        }

        // Intercept & cancel redirect or update requests.
        private async Task OnRequest(object sender, SessionEventArgs e)
        {
            e.GetState().PipelineInfo
                .AppendLine(nameof(OnRequest) + ":" + e.HttpClient.Request.RequestUri);

            var clientLocalIp = e.ClientLocalEndPoint.Address;
            if (!clientLocalIp.Equals(IPAddress.Loopback) && !clientLocalIp.Equals(IPAddress.IPv6Loopback))
            {
                e.HttpClient.UpStreamEndPoint = new IPEndPoint(clientLocalIp, 0);
            }

            if (e.HttpClient.Request.Url.Contains("yahoo.com"))
            {
                e.CustomUpStreamProxy = new ExternalProxy("localhost", 8888);
            }

            WriteToConsole("Active Client Connections:" + ((ProxyServer)sender).ClientConnectionCount);
            WriteToConsole(e.HttpClient.Request.Url);
        }

        // Modify response.
        private async Task MultipartRequestPartSent(object sender, MultipartRequestPartSentEventArgs e)
        {
            e.GetState().PipelineInfo.AppendLine(nameof(MultipartRequestPartSent));

            WriteToConsole("Multipart form data headers:");

            foreach (var header in e.Headers)
            {
                WriteToConsole(header.ToString());
            }
        }

        private async Task OnResponse(object sender, SessionEventArgs e)
        {
            e.GetState().PipelineInfo.AppendLine(nameof(OnResponse));

            if (e.HttpClient.ConnectRequest?.TunnelType == TunnelType.Websocket)
            {
                e.DataSent += WebSocket_DataSent;
                e.DataReceived += WebSocket_DataReceived;
            }

            WriteToConsole("Active Server Connections:" + ((ProxyServer)sender).ServerConnectionCount);
        }

        private async Task OnAfterResponse(object sender, SessionEventArgs e)
            => WriteToConsole($"Pipelineinfo: {e.GetState().PipelineInfo}", ConsoleColor.Yellow);

        public Task OnCertificateValidation(object sender, CertificateValidationEventArgs e)
        {
            e.GetState().PipelineInfo.AppendLine(nameof(OnCertificateValidation));

            // Set IsValid to true/false based on certificate errors.
            if (e.SslPolicyErrors == SslPolicyErrors.None)
            {
                e.IsValid = true;
            }

            return Task.CompletedTask;
        }

        public Task OnCertificateSelection(object sender, CertificateSelectionEventArgs e)
        {
            e.GetState().PipelineInfo.AppendLine(nameof(OnCertificateSelection));

            // Set e.clientCertificate to override.

            return Task.CompletedTask;
        }

        private void WriteToConsole(string Message, ConsoleColor? consoleColor = null)
            => ConsoleMessageQueue.Enqueue(new Tuple<ConsoleColor?, string>(consoleColor, Message));

        private async Task ListenToConsole()
        {
            while (!CancellationToken.IsCancellationRequested)
            {
                while (ConsoleMessageQueue.TryDequeue(out var item))
                {
                    var consoleColor = item.Item1;
                    var message = item.Item2;

                    if (consoleColor.HasValue)
                    {
                        var existing = Console.ForegroundColor;
                        Console.ForegroundColor = consoleColor.Value;
                        Console.WriteLine(message);
                        Console.ForegroundColor = existing;
                    }
                    else
                    {
                        Console.WriteLine(message);
                    }
                }

                // Reduce CPU usage.
                await Task.Delay(50);
            }
        }
    }
}