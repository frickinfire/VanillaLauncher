using Titanium.Web.Proxy.EventArguments;

namespace Titanium.Web.Proxy.Examples.Basic
{
    public static class ProxyEventArgsBaseExtensions
    {
        public static SampleClientState GetState(this ProxyEventArgsBase Args)
        {
            if (Args.ClientUserData == null)
            {
                Args.ClientUserData = new SampleClientState();
            }
            return (SampleClientState)Args.ClientUserData;
        }
    }
}