using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using WebApp.WebSockets;

namespace WebApp
{
    public abstract partial class Application
    {
        /// <summary>
        /// Initialization Manager.
        /// </summary>
        public sealed class Initialization
        {
            private List<(int Priority, Action<IServiceCollection> Action)> m_With;
            private List<(int Priority, Action<WebHostBuilderContext, IApplicationBuilder> Action)> m_Using;

            internal Initialization(Application Application)
            {
                m_With = new List<(int Priority, Action<IServiceCollection> Action)>();
                m_Using = new List<(int Priority, Action<WebHostBuilderContext, IApplicationBuilder> Action)>();
                this.Application = Application;
            }

            /// <summary>
            /// Application Instance.
            /// </summary>
            public Application Application { get; }

            /// <summary>
            /// Execute `With` Callbacks.
            /// </summary>
            internal void Execute(IServiceCollection Services)
            {
                lock (this)
                {
                    m_With.Sort((A, B) => A.Priority - B.Priority);

                    foreach (var Each in m_With)
                        Each.Action(Services);

                    m_With.Clear();
                }
            }

            /// <summary>
            /// Execute `Using` Callbacks.
            /// </summary>
            /// <param name="Context"></param>
            /// <param name="Builder"></param>
            internal void Execute(WebHostBuilderContext Context, IApplicationBuilder Builder)
            {
                lock (this)
                {
                    m_Using.Sort((A, B) => A.Priority - B.Priority);

                    foreach (var Each in m_Using)
                        Each.Action(Context, Builder);

                    m_Using.Clear();
                }
            }
            
            /// <summary>
            /// Application With ...
            /// </summary>
            /// <param name="Action"></param>
            /// <returns></returns>
            public Initialization With(Action<IServiceCollection> Action)
                => With(int.MaxValue, Action);

            /// <summary>
            /// Application Using ... 
            /// </summary>
            /// <param name="Action"></param>
            /// <returns></returns>
            public Initialization Using(Action<WebHostBuilderContext, IApplicationBuilder> Action)
                => Using(int.MaxValue, Action);

            /// <summary>
            /// Application Using ... 
            /// </summary>
            /// <param name="Priority"></param>
            /// <param name="Action"></param>
            /// <returns></returns>
            public Initialization Using(Action<IApplicationBuilder, IWebHostEnvironment> Action)
                => Using(int.MaxValue, Action);

            /// <summary>
            /// Application Using ... 
            /// </summary>
            /// <param name="Action"></param>
            /// <returns></returns>
            public Initialization Using(Action<IApplicationBuilder> Action)
                => Using(int.MaxValue, Action);

            /// <summary>
            /// Application With ...
            /// </summary>
            /// <param name="Priority"></param>
            /// <param name="Action"></param>
            /// <returns></returns>
            public Initialization With(int Priority, Action<IServiceCollection> Action)
            {
                m_With.Add((Priority, Action));
                return this;
            }

            /// <summary>
            /// Application Using ... 
            /// </summary>
            /// <param name="Priority"></param>
            /// <param name="Action"></param>
            /// <returns></returns>
            public Initialization Using(int Priority, Action<WebHostBuilderContext, IApplicationBuilder> Action)
            {
                m_Using.Add((Priority, Action));
                return this;
            }

            /// <summary>
            /// Application Using ... 
            /// </summary>
            /// <param name="Priority"></param>
            /// <param name="Action"></param>
            /// <returns></returns>
            public Initialization Using(int Priority, Action<IApplicationBuilder, IWebHostEnvironment> Action)
            {
                m_Using.Add((Priority, (A, B) => Action(B, A.HostingEnvironment)));
                return this;
            }

            /// <summary>
            /// Application Using ... 
            /// </summary>
            /// <param name="Priority"></param>
            /// <param name="Action"></param>
            /// <returns></returns>
            public Initialization Using(int Priority, Action<IApplicationBuilder> Action)
            {
                m_Using.Add((Priority, (A, B) => Action(B)));
                return this;
            }

            /// <summary>
            /// Application Implements ...
            /// </summary>
            /// <param name="Path"></param>
            /// <param name="Action"></param>
            /// <returns></returns>
            public Initialization Implements(string Path, Action<WebSocketListener> Action)
            {
                var WSL = new WebSocketListener(Path);
                Application.WebSockets.With(WSL);

                Action(WSL);
                return this;
            }
        }
    }
}
