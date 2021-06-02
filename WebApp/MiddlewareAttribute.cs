using System;

namespace WebApp
{
    /// <summary>
    /// Configure the Middleware automatically.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class MiddlewareAttribute : Attribute
    {
        /// <summary>
        /// Get or Set Priority to load the Middleware.
        /// </summary>
        public int Priority { get; set; } = int.MaxValue;
    }

    /// <summary>
    /// Configure the Middleware to specific routes only.
    /// </summary>
    public sealed class RoutewareAttribute : Attribute
    {
        public RoutewareAttribute(string Route)
            => this.Route = Route;

        /// <summary>
        /// Route to configure the Middleware.
        /// </summary>
        public string Route { get; }

        /// <summary>
        /// Get or Set Priority to load the Middleware.
        /// </summary>
        public int Priority { get; set; } = int.MaxValue;
    }
}
