using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebSocketServer.MiddleWare
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly WebSocketServerConnectionManager _manager;
        public WebSocketMiddleware(RequestDelegate next, WebSocketServerConnectionManager manager){
            _next=next;
            _manager=manager;
        }

        public async Task InvokeAsync(HttpContext context){
                if (context.WebSockets.IsWebSocketRequest)
                {
                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    Console.WriteLine("WebSocket Connected");
                    string ConnId= _manager.AddSocket(webSocket);
                    await SendConnIDAsync(webSocket, ConnId);
                    await ReceiveMessage(webSocket, async(result, buffer)=>{
                        if (result.MessageType==WebSocketMessageType.Text){
                            Console.WriteLine("Message received");
                            var message=Encoding.UTF8.GetString(buffer,0,result.Count);
                            Console.WriteLine($"Messsage:{message}");
                            await RouteJSONMessageAsync(message);
                        }else if (result.MessageType==WebSocketMessageType.Close){
                            Console.WriteLine("Receive Close message");
                            return;
                        }
                    });
                }
                else
                {
                    Console.WriteLine("Hello form 2nd request delegate.");
                    await _next(context);
                }
        }

        private static async Task ReceiveMessage(WebSocket socket,
                                            Action<WebSocketReceiveResult,byte[]>
                                             handleMessage)
        {
            var buffer= new byte[1024 *4];
            while(socket.State==WebSocketState.Open){
                var result=await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer),
                        cancellationToken:CancellationToken.None
                    );
                    handleMessage(result, buffer);
            }
        }

        private static async Task SendConnIDAsync(WebSocket socket, string ConnId){
            var buffer= Encoding.UTF8.GetBytes("ConnID: "+ConnId);
            await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task RouteJSONMessageAsync(string message){
            var routeOb= JsonSerializer.Deserialize<MessageTDO> (message);
            if (Guid.TryParse(routeOb.To, out Guid result)){
                Console.WriteLine(result);
                var socket=_manager.GetAllSockets().FirstOrDefault(s=>s.Key==routeOb.To);
                if (socket.Value!=null){
                    if (socket.Value.State==WebSocketState.Open){
                        await socket.Value.SendAsync(Encoding.UTF8.GetBytes(routeOb.Message),
                        WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }else{
                    Console.WriteLine("Invalid recipient");
                }
            }else{
                Console.WriteLine("BrodCast");
                foreach( var socket in _manager.GetAllSockets()){
                    if (socket.Value.State==WebSocketState.Open){
                        await socket.Value.SendAsync(Encoding.UTF8.GetBytes(routeOb.Message),
                        WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
            }
            
        }

        
    }

    public class MessageTDO{
        public string From {get;set;}
        public string To { get; set; }
        public string Message { get; set; }
    }
}