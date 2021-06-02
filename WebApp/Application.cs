using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using WebApp.Extensions;
using WebApp.Services;

namespace WebApp
{
    /// <summary>
    /// Base class for Application.
    /// </summary>
    public abstract partial class Application
    {
        private ILogger m_Logger = null;
        private IDisposable m_LoggingScope = null;
        private Service[] m_Services = null;

        private Task m_Startup;
        private Task m_Shutdown;

        /// <summary>
        /// Root Directory to Application Entry-Assembly.
        /// </summary>
        public static readonly string RootDirectory;

        /// <summary>
        /// Set RootDirectory as EntryAssembly location.
        /// </summary>
        static Application() => RootDirectory
            = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        /// <summary>
        /// Initialize the Application.
        /// </summary>
        public Application() => WebSockets = new WebSockets_();

        /// <summary>
        /// WebSocket Manager.
        /// </summary>
        internal WebSockets_ WebSockets { get; }

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
            var Assemblies = new List<Assembly>();

            Assemblies.Add(typeof(Application).Assembly);
            Assemblies.Add(Assembly.GetEntryAssembly());

            /* Get Application-Part Assemblies. */
            OnAppPart(X =>
            {
                if (Assemblies.Contains(X))
                    return;

                Assemblies.Add(X);
            });

            return App

            /* Register the Instance of Runtime class as Singleton. */
            .With(int.MinValue, Services =>
            {
                Action<IMvcBuilder> AddAppParts = X => {
                    Assemblies.ForEach(Y => X.AddApplicationPart(Y));
                };

                /* Inject dependencies to application. */
                Services.AddSingleton(this);
                Services.AddSingleton<IHostedService>(Provider
                    => new Runtime(this, Provider.GetService<ILogger<Runtime>>()));

                Services.AddLogging(X => X.AddConsole());

                /* Add Application-Parts to Each MVC Builders. */
                AddAppParts(Services.AddMvc());
                AddAppParts(Services.AddRazorPages());
                AddAppParts(Services.AddControllers());

                Assemblies.ForEach(Assembly => ScanServices(Assembly));
            })

            /* Register WebSocket Handling routines. */
            .Using(int.MaxValue, Builder =>
            {
                var Middlewares = new List<(int Priority, Type Type)>();
                var Routewares  = new List<(int Priority, Type Type, string Route)>();

                Assemblies.ForEach(Assembly =>
                {
                    Assembly
                        .GetTypes(Y => Y.IsSubclassOf(typeof(Middleware)) && !Y.IsAbstract)
                        .Each(Y =>
                        {
                            var Attribute = Y.GetCustomAttribute<MiddlewareAttribute>();
                            if (Attribute != null)
                                Middlewares.Add((Attribute.Priority, Y));
                        });
                });

                Builder.UseWebSockets(new WebSocketOptions()
                {
                    KeepAliveInterval = TimeSpan.FromMilliseconds(60 * 1000),
                    ReceiveBufferSize = 2048
                });

                Middlewares.Sort((A, B) => A.Priority - B.Priority);
                Middlewares.ForEach(X => Builder.UseMiddleware(X.Type));

                Builder.UseRouting();
                Builder.UseEndpoints(X =>
                {
                    X.MapControllers();
                    X.MapRazorPages();
                });
            });
        }

        /// <summary>
        /// Scan Services and Instantiates them.
        /// </summary>
        /// <param name="obj"></param>
        private void ScanServices(Assembly Target)
        {
            m_Services = m_Services.Merge(Target
                .GetTypes(X => X.IsSubclassOf(typeof(Service)))
                .Map(X => X.Instantiate<Service>()));
        }

        /// <summary>
        /// Called when the application started.
        /// </summary>
        private Task OnStartup(ILogger Logger)
        {
            lock(this)
            {
                if (m_Startup is null)
                {
                    m_LoggingScope = (m_Logger = Logger).BeginScope(this);
                    m_Startup = Startup();
                }

                return m_Startup;
            }
        }

        /// <summary>
        /// Called when the application stopped.
        /// </summary>
        private Task OnShutdown()
        {
            lock(this)
            {
                if (m_Startup is null)
                    return Task.CompletedTask;

                if (m_Shutdown is null)
                {
                    (m_Shutdown = Shutdown())
                        .Then(X =>
                        {
                            /**
                             * Dispose the logging scope
                             * after completion of shutdown method.
                             */
                            m_LoggingScope.Dispose();
                            m_LoggingScope = null;

                            return Task.CompletedTask;
                        });
                }

                return m_Shutdown;
            }
        }

        /// <summary>
        /// Register the Application Part.
        /// </summary>
        /// <param name="AddAppPart"></param>
        protected virtual void OnAppPart(Action<Assembly> Register) { }

        /// <summary>
        /// Get Service object by predicate.
        /// </summary>
        /// <param name="Predicate">Null isn't allowed.</param>
        /// <returns></returns>
        public Service GetService(Predicate<Service> Predicate)
        {
            if (Predicate is null)
                throw new ArgumentNullException(nameof(Predicate));

            foreach (var Each in m_Services)
            {
                if (Predicate(Each))
                    return EnsureService(Each);
            }

            return null;
        }

        /// <summary>
        /// Get Service object by predicate.
        /// </summary>
        /// <typeparam name="ServiceType"></typeparam>
        /// <param name="Predicate">If null given, returns first found service.</param>
        /// <returns></returns>
        public ServiceType GetService<ServiceType>(Predicate<ServiceType> Predicate = null)
            where ServiceType : Service
        {
            if (Predicate is null)
                Predicate = X => true;

            foreach (var Each in m_Services)
            {
                if ((Each is ServiceType CastedService) && Predicate(CastedService))
                    return EnsureService(CastedService);
            }

            return null;
        }

        /// <summary>
        /// Initialize the Service.
        /// </summary>
        /// <typeparam name="ServiceType"></typeparam>
        /// <param name="Service"></param>
        /// <returns></returns>
        private ServiceType EnsureService<ServiceType>(ServiceType Service)
            where ServiceType : Service
        {
            var Task = Service._Startup();

            /**
             * If the startup method of service not completed, 
             * Wait and ensure its completion.
             */
            if (!Task.IsCompleted)
                 Task.Wait();

            return Service;
        }

        /// <summary>
        /// Called when the application started.
        /// </summary>
        protected virtual Task Startup() 
            => Task.CompletedTask;

        /// <summary>
        /// Called when the application stopped.
        /// </summary>
        protected virtual Task Shutdown() 
            => m_Services.Map(X => X._Shutdown()).All();

    }
}
