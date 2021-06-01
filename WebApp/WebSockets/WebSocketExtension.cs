using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebApp.WebSockets
{
    public static class WebSocketExtension
    {
        private const int RECV_BUFFER_SIZE = 2048;

        /// <summary>
        /// Receive String Asynchronously.
        /// </summary>
        /// <param name="CT">Propagates the notification that operations should be canceled.</param>
        /// <returns></returns>
        public static async Task<string> ReceiveStringAsync(this WebSocket This,
            int MaxPayloadBytes = 16 * 1024, CancellationToken CT = default(CancellationToken))
        {
            var Buffer = new ArraySegment<byte>(new byte[RECV_BUFFER_SIZE]);
            using (var MemStream = new MemoryStream())
            {
                WebSocketReceiveResult Result;
                int TotalBytes = 0;

                do
                {
                    CT.ThrowIfCancellationRequested();
                    Result = await This.ReceiveAsync(Buffer, CT);

                    if (Result.MessageType != WebSocketMessageType.Text)
                        return null;

                    if (TotalBytes >= MaxPayloadBytes)
                    {
                        throw new InternalBufferOverflowException(
                            "Client sent too large payload.");
                    }

                    MemStream.Write(Buffer.Array, Buffer.Offset, Buffer.Count);
                    TotalBytes += Buffer.Count;
                }

                while (!Result.EndOfMessage);

                MemStream.Seek(0, SeekOrigin.Begin);
                using (var Converter = new StreamReader(MemStream, Encoding.UTF8))
                    return await Converter.ReadToEndAsync();
            }
        }

        /// <summary>
        /// Send String Asynchronously.
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="CT">Propagates the notification that operations should be canceled.</param>
        /// <returns></returns>
        public static Task SendStringAsync(this WebSocket This,
            string Data, CancellationToken CT = default(CancellationToken))
        {
            var Buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(Data));
            return This.SendAsync(Buffer, WebSocketMessageType.Text, true, CT);
        }
    }
}
