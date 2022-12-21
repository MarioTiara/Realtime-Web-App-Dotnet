using System;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace SignalRServer
{
    public class ChatHub:Hub
    {
        public override Task OnConnectedAsync()
        {
            Console.WriteLine("--> COnnection Establish "+Context.ConnectionId);
            Clients.Client(Context.ConnectionId).SendAsync("ReceiveConnID", Context.ConnectionId);
            return base.OnConnectedAsync();
        }
    }
}