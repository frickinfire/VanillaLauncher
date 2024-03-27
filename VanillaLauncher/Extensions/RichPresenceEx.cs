using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordRPC;

namespace VanillaLauncher.Extensions
{
   
    class RichPresence
    {
        public DiscordRpcClient RpcClient;

        public string Details { get; private set; }
        public string State { get; private set; }

        void Start() {}
    }
}
