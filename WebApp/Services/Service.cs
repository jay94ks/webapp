using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Services
{
    /// <summary>
    /// Service.
    /// </summary>
    public abstract class Service
    {
        private Task m_Startup;
        private Task m_Shutdown;

        /// <summary>
        /// Calls Startup method.
        /// </summary>
        internal Task _Startup()
        {
            lock (this)
            {
                if (m_Startup is null)
                    m_Startup = Startup();

                return m_Startup;
            }
        }

        /// <summary>
        /// Calls Shutdown method.
        /// </summary>
        internal Task _Shutdown()
        {
            lock(this)
            {
                if (m_Startup is null)
                    return Task.CompletedTask;

                if (m_Shutdown is null)
                    m_Shutdown = Shutdown();

                return m_Shutdown;
            }
        }

        /// <summary>
        /// Startup the Service.
        /// </summary>
        /// <returns></returns>
        protected virtual Task Startup() => Task.CompletedTask;

        /// <summary>
        /// Shutdown the Service.
        /// </summary>
        /// <returns></returns>
        protected virtual Task Shutdown() => Task.CompletedTask;
    }
}
