using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace TSM.Hubs
{
    public class myHub:Hub
    {
        public void ServerListener(string UserName)
        {
            Clients.Others.notifyUpdates($"Got Connected");
        }
    }
}
