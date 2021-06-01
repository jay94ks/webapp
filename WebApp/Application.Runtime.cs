using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebApp
{
    public abstract partial class Application
    {
        /// <summary>
        /// Runtime class that receives `Start` and `Stop` message.
        /// </summary>
        private class Runtime : IHostedService
        {
            internal Runtime(Application Application, ILogger<Runtime> Logger)
            {
                this.Application = Application;
                this.Logger = Logger;
            }

            /// <summary>
            /// Application Instance. 
            /// </summary>
            public Application Application { get; }

            /// <summary>
            /// Logger Instance.
            /// </summary>
            public ILogger<Runtime> Logger { get; }

            /// <summary>
            /// Receive `Start` message.
            /// </summary>
            /// <param name="cancellationToken"></param>
            /// <returns></returns>
            public Task StartAsync(CancellationToken cancellationToken) 
                => Task.Run(() => Application.OnStartup(Logger));

            /// <summary>
            /// Receive `Stop` message.
            /// </summary>
            /// <param name="cancellationToken"></param>
            /// <returns></returns>
            public Task StopAsync(CancellationToken cancellationToken)
                => Task.Run(() => Application.OnShutdown());
        }
    }
}
