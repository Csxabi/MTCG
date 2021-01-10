using System;
using System.Collections.Generic;

namespace RestWebServer
{
    /// <summary>
    /// Provides context for a HTTP request to the handler.
    /// </summary>
    public class RequestContext
    {
        /// <summary>
        /// The HTTP verb or method.
        /// </summary>
        public string Verb { get; }

        /// <summary>
        /// The path part of the route.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// The query part of the route.
        /// </summary>
        public string Query { get; }

        /// <summary>
        /// The fragment part of the route.
        /// </summary>
        public string Fragment { get; }

        /// <summary>
        /// Resources contained within the route.
        /// </summary>
        /// <remarks>
        /// Resources are like variables. They may be any valid string.
        /// </remarks>
        public string[] Resources { get; }

        /// <summary>
        /// Headers contained in the request.
        /// </summary>
        public IDictionary<string, string> Headers { get; }

        /// <summary>
        /// The body of the request.
        /// </summary>
        /// <remarks>
        /// The body is optional. As such it may be empty.
        /// </remarks>
        public string Payload { get; }

        public RequestContext(string verb, string route, string[] resources, IDictionary<string, string> headers, string payload)
        {
            Verb = verb;
            Headers = headers;
            Resources = resources;
            Payload = payload;

            Path = route;
            Query = string.Empty;
            Fragment = string.Empty;

            int queryBegin = route.IndexOf('?');
            int fragmentBegin = route.IndexOf('#');

            if (queryBegin != -1)
            {
                Path = route[..queryBegin]; // route.Substring(0, queryBegin)
                Query = route[queryBegin..(fragmentBegin != -1 ? fragmentBegin : ^0)];
                // route.Substring(queryBegin, fragmentBegin != -1 ? fragmentBegin - queryBegin : route.Length - queryBegin);
            }

            if (fragmentBegin != -1)
            {
                if (queryBegin == -1)
                    Path = route[..fragmentBegin]; // route.Substring(0, fragmentBegin)

                Fragment = route[fragmentBegin..]; // route.Substring(fragmentBegin, route.Length - fragmentBegin)
            }
        }
    }
}
