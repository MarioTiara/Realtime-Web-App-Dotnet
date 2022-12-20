using System.Net.WebSockets;
using WebSocketServer;
using WebSocketServer.MiddleWare;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebSocketManager();
// Add services to the container.

// builder.Services.AddControllers();
// // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// builder.Services.AddEndpointsApiExplorer();



var app = builder.Build();
app.UseWebSockets();
app.UseWebSocketServer();
//await WebSocketStartup.Start(app);


 app.Run(async context =>
            {
                Console.WriteLine("Hello from 3rd (terminal) Request Delegate");
                await context.Response.WriteAsync("Hello from 3rd (terminal) Request Delegate");
            });

app.Run();
        