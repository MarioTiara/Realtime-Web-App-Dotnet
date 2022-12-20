using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
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
                            Console.WriteLine($"Messsage:{Encoding.UTF8.GetString(buffer,0,result.Count)}");
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
    }
}