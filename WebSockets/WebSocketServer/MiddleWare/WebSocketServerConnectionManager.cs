using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.WebSockets;

namespace WebSocketServer.MiddleWare
{
    public class WebSocketServerConnectionManager
    {
        private ConcurrentDictionary<string, WebSocket> _socket= new ConcurrentDictionary<string, WebSocket>();
        public ConcurrentDictionary<string, WebSocket> GetAllSockets(){
            return _socket;
        }

        public string AddSocket(WebSocket socket){
            string ConnId=Guid.NewGuid().ToString();
            _socket.TryAdd(ConnId, socket);
            Console.WriteLine("Connection Added: "+ConnId);
            return ConnId;
        }
    }
}