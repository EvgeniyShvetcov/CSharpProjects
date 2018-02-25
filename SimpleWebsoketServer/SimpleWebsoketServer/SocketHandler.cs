using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleWebsoketServer
{
    public class SocketHandler
    {
        public const int BufferSize = 4096;
        WebSocket _socket;
        SocketHandler(WebSocket socket)
        {
            this._socket = socket;
        }

        async Task EchoLoop()
        {
            var buffer = new byte[BufferSize];
            var segment = new ArraySegment<byte>(buffer);

            while(this._socket.State == WebSocketState.Open)
            {
                var incoming = await this._socket.ReceiveAsync(segment, CancellationToken.None);
                var outgoing = new ArraySegment<byte>(buffer, 0, incoming.Count);
                string recievedMessage = Encoding.UTF8.GetString(buffer);
                await this._socket.SendAsync(outgoing, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        static async Task Acceptor(HttpContext context, Func<Task> func)
        {
            if(!context.WebSockets.IsWebSocketRequest)
                return;

            var socket = await context.WebSockets.AcceptWebSocketAsync();
            var h = new SocketHandler(socket);
            await h.EchoLoop();
        }

        public static void Map(IApplicationBuilder app)
        {
            app.UseWebSockets();
            app.Use(SocketHandler.Acceptor);
        }
    }
}
