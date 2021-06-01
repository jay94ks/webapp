using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using WebApp.WebSockets;

namespace WebApp
{
    public abstract partial class Application
    {
        /// <summary>
        /// WebSocket Connection Manager.
        /// </summary>
        internal class WebSockets_
        {
            private Dictionary<string, WebSocketListener> m_Listeners
                = new Dictionary<string, WebSocketListener>();

            /// <summary>
            /// Add WebSocket Listener.
            /// Implementation should never call this after `Prepare` method called.
            /// </summary>
            /// <param name="Listener"></param>
            internal WebSockets_ With(WebSocketListener Listener)
            {
                m_Listeners[Listener.Path] = Listener;
                return this;
            }

            /// <summary>
            /// Check the requested path is registered for WebSocket or not.
            /// </summary>
            /// <param name="Request"></param>
            /// <returns></returns>
            internal bool Check(HttpRequest Request)
                => m_Listeners.ContainsKey( Request.Path);

            /// <summary>
            /// Accept the WebSocket and pass it to listener.
            /// </summary>
            /// <param name="Http"></param>
            /// <returns></returns>
            internal async Task Handle(HttpContext Http) {
                if (!m_Listeners.TryGetValue(Http.Request.Path, out var Listener))
                    return;

                WebSocket WS = await Http.WebSockets.AcceptWebSocketAsync();

                if (WS is null || /* If failed to accept, waste it. */
                    WS.State != WebSocketState.Open)
                    return;

                /* Pass the WebSocket to Listener. */
                await Listener.OnAccept(WS, Http.Request);
            }
        }

    }
}
