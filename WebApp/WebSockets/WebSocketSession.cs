using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace WebApp.WebSockets
{
    public sealed partial class WebSocketSession
    {
        private Func<Task> 
            m_Open = () => Task.CompletedTask, 
            m_Close = () => Task.CompletedTask;

        private Func<Exception, Task> 
            m_Error = X => Task.CompletedTask;

        private Func<string, Task>
            m_Message = X => Task.CompletedTask;

        /// <summary>
        /// Initialize a new WebSocket Session.
        /// </summary>
        /// <param name="Socket"></param>
        /// <param name="Request"></param>
        internal WebSocketSession(WebSocket WebSocket, HttpRequest Request)
        {
            this.Request = Request; 
            Transceiver = new Transceiver_(this, WebSocket);
        }

        /// <summary>
        /// Http Request (upgrade request).
        /// </summary>
        public HttpRequest Request { get; }

        /// <summary>
        /// Transceiver for this Session.
        /// </summary>
        internal Transceiver_ Transceiver { get; }

        /// <summary>
        /// Set `Open` Handler for this Session.
        /// It will be called before starting communication once.
        /// </summary>
        /// <param name="Action"></param>
        /// <returns></returns>
        public WebSocketSession Open(Func<Task> Action)
        {
            m_Open = Action;
            return this;
        }

        /// <summary>
        /// Set `Error` Handler for this Session.
        /// It will be called when exception occurred 
        /// during execution of Message or Error handler.
        /// </summary>
        /// <param name="Action"></param>
        /// <returns></returns>
        public WebSocketSession Error(Func<Exception, Task> Action)
        {
            m_Error = Action;
            return this;
        }

        /// <summary>
        /// Set `Message` Handler for this Session.
        /// Do not change handler at runtime.
        /// </summary>
        /// <param name="Action"></param>
        /// <returns></returns>
        public WebSocketSession Message(Func<string, Task> Action)
        {
            m_Message = Action;
            return this;
        }

        /// <summary>
        /// Set `Close` Handler for this Session.
        /// It will be called when the session closed.
        /// (even called when error occurred)
        /// </summary>
        /// <param name="Action"></param>
        /// <returns></returns>
        public WebSocketSession Close(Func<Task> Action)
        {
            m_Close = Action;
            return this;
        }
    }
}
