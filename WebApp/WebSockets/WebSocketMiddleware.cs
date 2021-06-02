using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace WebApp.WebSockets
{
    [Middleware(Priority = int.MaxValue / 2)]
    internal class WebSocketMiddleware : Middleware
    {
        public WebSocketMiddleware(RequestDelegate Next)
            : base(Next)
        {
        }

        /// <summary>
        /// Make WebSocket Pipeline.
        /// </summary>
        /// <param name="Http"></param>
        /// <param name="App"></param>
        /// <returns></returns>
        public override Task Invoke(HttpContext Http, Application App)
        {
            if (Http.WebSockets.IsWebSocketRequest)
            {
                if (App.WebSockets.Check(Http.Request))
                    return App.WebSockets.Handle(Http);

                /* It isn't a path for WebSockets. */
                Http.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Task.CompletedTask;
            }

            return base.Invoke(Http, App);
        }
    }
}
