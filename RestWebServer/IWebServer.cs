using System;

namespace RestWebServer
{
    /// <summary>
    /// Specifies interfaces for a REST-capable web server.
    /// </summary>
    public interface IWebServer
    {
        /// <summary>
        /// Starts listening for incoming connections and processing them.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops listening for incoming connections.
        /// Any connections that have already started processing will continue.
        /// Connections that are currently waiting in the queue will be abandoned.
        /// </summary>
        void Stop();

        /// <summary>
        /// Registers a handler for a static route.
        /// Static routes do not contain resources.
        /// </summary>
        /// <param name="verb">The HTTP verb.</param>
        /// <param name="route">The path of the URL.</param>
        /// <param name="handler">The handler for the request.</param>
        void RegisterStaticRoute(string verb, string route, RequestHandler handler);

        /// <summary>
        /// Registers a handler for a route that references resources.
        /// Resources are marked with <see cref="ResourceMarker"/>.
        /// </summary>
        /// <param name="verb">The HTTP verb.</param>
        /// <param name="route">The path of the URL.</param>
        /// <param name="handler">The handler for the request.</param>
        void RegisterResourceRoute(string verb, string route, RequestHandler handler);
    }
}
