using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace WebApp
{
    /// <summary>
    /// Base class for Application.
    /// </summary>
    public abstract partial class Application
    {
        private volatile bool m_KeepRunning = false;
        private ILogger m_Logger = null;
        private IDisposable m_LoggingScope = null;
        private WebSockets_ m_WebSockets = null;

        /// <summary>
        /// Root Directory to Application Entry-Assembly.
        /// </summary>
        public static readonly string RootDirectory;

        /// <summary>
        /// 
        /// </summary>
        static Application() => RootDirectory
            = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        /// <summary>
        /// Initialize the Application.
        /// </summary>
        public Application()
        {
            m_WebSockets = new WebSockets_();
        }

        /// <summary>
        /// 
        /// </summary>
        internal WebSockets_ WebSockets => m_WebSockets;

        /// <summary>
        /// Run the application.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task Run(params string[] args)
        {
            var Host = Prepare(WebHost
                .CreateDefaultBuilder(args))
                .Build();

            await Host.RunAsync();
        }

        /// <summary>
        /// Prepare the web-host builder.
        /// And calls abstract method to prepare user-codes.
        /// </summary>
        private IWebHostBuilder Prepare(IWebHostBuilder App)
        {
            Initialization Host = Configure(new Initialization(this));
            return Prepare(Host, App) /* Execute Initialization Callbacks. */
                .ConfigureServices(Services => Host.Execute(Services))
                .Configure((Context, Builder) => Host.Execute(Context, Builder));
        }

        /// <summary>
        /// Prepare the web-host builder.
        /// This method should return Builder instance which given through parameter.
        /// Note: Default Implementation executes: App.UseUrl("http://*:80").
        /// </summary>
        protected virtual IWebHostBuilder Prepare(Initialization Host, IWebHostBuilder App)
            => App.UseUrls("http://*:80");

        /// <summary>
        /// Configure the Application (Application-ize)
        /// </summary>
        /// <param name="App"></param>
        /// <returns></returns>
        private Initialization Configure(Initialization App)
        {
            return App

            /* Register the Instance of Runtime class as Singleton. */
            .With(int.MinValue, Services =>
            {
                var Entry = Assembly.GetEntryAssembly();
                IMvcBuilder Mvc;

                Services.AddSingleton<IHostedService>(Provider
                    => new Runtime(this, Provider.GetService<ILogger<Runtime>>()));

                Services.AddLogging(X => X.AddConsole());

                Mvc = Services.AddMvc()
                    .AddApplicationPart(Entry);

                Mvc = Services.AddRazorPages()
                    .AddApplicationPart(Entry);

                Mvc = Services.AddControllers()
                    .AddApplicationPart(Entry);
            })

            /* Register WebSocket Handling routines. */
            .Using(int.MaxValue, Builder =>
            {
                Builder.UseWebSockets(new WebSocketOptions()
                {
                    KeepAliveInterval = TimeSpan.FromMilliseconds(60 * 1000),
                    ReceiveBufferSize = 2048
                });

                Builder.Use(async (Http, Next) =>
                {
                    if (Http.WebSockets.IsWebSocketRequest)
                    {
                        if (m_WebSockets.Check(Http.Request))
                        {
                            await m_WebSockets.Handle(Http);
                            return;
                        }

                        /* It isn't a path for WebSockets. */
                        Http.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return;
                    }

                    await Next();
                });

                Builder.UseRouting();
                Builder.UseEndpoints(X =>
                {
                    X.MapControllers();
                    X.MapRazorPages();
                });
            });
        }

        /// <summary>
        /// Register the Application Part.
        /// </summary>
        /// <param name="AddAppPart"></param>
        protected virtual void OnAppPart(Action<Assembly> Register) { }

        /// <summary>
        /// Called when the application started.
        /// </summary>
        private void OnStartup(ILogger Logger)
        {
            lock(this)
            {
                if (m_KeepRunning)
                    return;

                m_Logger = Logger;
                m_KeepRunning = true;
            }

            m_LoggingScope = m_Logger.BeginScope(this);
            Startup();
        }

        /// <summary>
        /// Called when the application stopped.
        /// </summary>
        private void OnShutdown()
        {
            lock(this)
            {
                if (!m_KeepRunning)
                    return;

                m_KeepRunning = false;
            }

            Shutdown();
            m_LoggingScope.Dispose();
            m_LoggingScope = null;
        }

        /// <summary>
        /// Called when the application started.
        /// </summary>
        protected virtual void Startup() { }

        /// <summary>
        /// Called when the application stopped.
        /// </summary>
        protected virtual void Shutdown() { }

    }
}
