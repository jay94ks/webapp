using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebApp.WebSockets
{
    /// <summary>
    /// WebSocket Listener.
    /// </summary>
    public class WebSocketListener
    {
        private Func<WebSocketSession, Task> m_Callback;

        /// <summary>
        /// Initialize a new Listener.
        /// </summary>
        /// <param name="Path"></param>
        internal WebSocketListener(string Path = "/")
        {
            Path = Path.TrimEnd('/');
            if (!Path.StartsWith('/'))
                Path = "/" + Path;

            this.Path = Path;
        }

        /// <summary>
        /// Path where this listener bound.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Set callback which called when new session created.
        /// </summary>
        /// <param name="Session"></param>
        /// <returns></returns>
        public WebSocketListener On(Func<WebSocketSession, Task> Callback)
        {
            m_Callback = Callback;
            return this;
        }

        /// <summary>
        /// Called when the WebSocket accepted.
        /// </summary>
        internal async Task OnAccept(WebSocket WebSocket, HttpRequest Request)
        {
            if (m_Callback is null)
            {
                /* If callback not set, close it immediately. */
                await WebSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, 
                    "Not Implemented", default(CancellationToken));

                return;
            }

            var Session = new WebSocketSession(WebSocket, Request);

            /* Execute the Callback to program the Session. */
            await m_Callback(Session);

            /* Run the WebSocketSession. */
            await Session.Transceiver.Run();
        }
    }
}
