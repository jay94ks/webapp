using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using WebApp.Extensions;

namespace WebApp.WebSockets
{
    public partial class WebSocketSession
    {
        /// <summary>
        /// WebSocket Transceiver.
        /// </summary>
        internal class Transceiver_
        {
            private TaskCompletionSource<bool> m_Awaiter;

            private WebSocketSession m_Session;
            private WebSocket m_WebSocket;

            private Queue<string> m_Queue;
            private Task m_CurrentSend;

            private bool m_Alive = true;

            /// <summary>
            /// Initialize a new Transceiver.
            /// </summary>
            /// <param name="Session"></param>
            /// <param name="WebSocket"></param>
            public Transceiver_(WebSocketSession Session, WebSocket WebSocket)
            {
                m_Session = Session; m_WebSocket = WebSocket;
                m_Awaiter = new TaskCompletionSource<bool>();
                m_Queue = new Queue<string>();
            }

            /// <summary>
            /// Task to wait completion of the Session.
            /// </summary>
            public Task Run()
            {
                Task.Run(() => OnOpen())
                    .ContinueWith(X =>
                    {
                        if (X.IsCompletedSuccessfully)
                             OnReceive();

                        else OnError(X.Exception);
                    });

                return m_Awaiter.Task;
            }

            /// <summary>
            /// Send Message to client.
            /// </summary>
            /// <param name="Message"></param>
            public void Send(string Message)
            {
                if (m_WebSocket.State != WebSocketState.Open)
                    return;

                lock (m_Queue)
                {
                    m_Queue.Enqueue(Message);

                    if (m_CurrentSend is null)
                        OnSend();
                }
            }

            /// <summary>
            /// Asynchronous Receive Loop.
            /// </summary>
            private void OnReceive()
            {
                m_WebSocket.ReceiveStringAsync()
                    .ContinueWith(X =>
                    {
                        if (X.IsCompletedSuccessfully)
                        {
                            Task.Run(() => OnMessage(X.Result))
                                .ContinueWith(Y =>
                                {
                                    if (Y.IsCompletedSuccessfully)
                                         OnReceive();

                                    else OnError(Y.Exception);
                                });
                        }

                        else OnClose();
                    });
            }

            /// <summary>
            /// Asynchronous Send Loop.
            /// </summary>
            private void OnSend()
            {
                string Message;
                lock(m_Queue)
                {
                    if (m_Queue.Count <= 0)
                    {
                        m_CurrentSend = null;
                        return;
                    }

                    if (!m_Alive)
                    {
                        m_Queue.Clear();
                        return;
                    }

                    Message = m_Queue.Dequeue();
                }

                m_WebSocket.SendStringAsync(Message)
                    .ContinueWith(X =>
                    {
                        if (X.IsCompletedSuccessfully)
                             OnSend(); 

                        else OnClose();
                    });
            }

            /// <summary>
            /// Called when the connection opened.
            /// </summary>
            private Task OnOpen() => m_Session.m_Open();

            /// <summary>
            /// Called when message arrived.
            /// </summary>
            private Task OnMessage(string Message)
                => m_Session.m_Message(Message);

            /// <summary>
            /// Called when exception occurred until executing message handler.
            /// </summary>
            private Task OnError(AggregateException e)
            {
                return m_Session
                    .m_Error(e.InnerException)
                    .Then(() => OnClose());
            }

            /// <summary>
            /// Called when the connection closed.
            /// </summary>
            private Task OnClose()
            {
                lock(m_Queue)
                {
                    if (!m_Alive)
                        return Task.CompletedTask;

                    m_Alive = false;
                    m_Queue.Clear();
                }

                /* Shutdown the middleware pipeline. */
                m_Awaiter.SetResult(true);
                return m_Session.m_Close();
            }
        }
    }
}
