using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp
{
    public abstract class Middleware
    {
        private RequestDelegate m_Next;

        /// <summary>
        /// Initialize the new Middleware.
        /// </summary>
        /// <param name="Next"></param>
        public Middleware(RequestDelegate Next) => m_Next = Next;

        /// <summary>
        /// Invoke the Middleware.
        /// </summary>
        /// <param name="Http"></param>
        /// <param name="App"></param>
        /// <returns></returns>
        public virtual Task Invoke(HttpContext Http, Application App) => m_Next(Http);
    }
}
