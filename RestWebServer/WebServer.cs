using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RestWebServer
{
    /// <summary>
    /// Encapsulates a handler for a REST request.
    /// </summary>
    /// <param name="requestContext">Context of the request.</param>
    /// <returns>Response to the request.</returns>
    public delegate Task<RestResponse> RequestHandler(RequestContext requestContext);

    /// <summary>
    /// A basic, asynchronous REST-capable web server using <see cref="TcpListener"/>.
    /// </summary>
    public class WebServer : IWebServer, IDisposable
    {
        private readonly ITcpListener _listener;
        private readonly Thread _listenerThread;
        private volatile bool _listening;
        private readonly Dictionary<string, Dictionary<string, RequestHandler>> _staticHandlers;
        private readonly Dictionary<string, Dictionary<string, RequestHandler>> _resourceHandlers;

        /// <summary>
        /// Used for marking a resource part in a URL.
        /// </summary>
        public const string ResourceMarker = "%";

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServer"/> class with no registered routes.
        /// </summary>
        /// <param name="localEP">The endpoint to which to bind the listener.</param>
        public WebServer(IPEndPoint localEP)
        {
            _listener = new TcpListenerWrapper(new TcpListener(localEP));
            _listenerThread = new Thread(Run) { Name = "Listener Thread" };
            _listening = false;
            _staticHandlers = new Dictionary<string, Dictionary<string, RequestHandler>>(StringComparer.OrdinalIgnoreCase);
            _resourceHandlers = new Dictionary<string, Dictionary<string, RequestHandler>>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServer"/> class with no registered routes.
        /// </summary>
        /// <remarks>
        /// This constructor exists for testing purposes.
        /// </remarks>
        /// <param name="listener">The custom listener.</param>
        public WebServer(ITcpListener listener)
        {
            _listener = listener;
            _listenerThread = new Thread(Run) { Name = "Listener Thread" };
            _listening = false;
            _staticHandlers = new Dictionary<string, Dictionary<string, RequestHandler>>(StringComparer.OrdinalIgnoreCase);
            _resourceHandlers = new Dictionary<string, Dictionary<string, RequestHandler>>(StringComparer.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public void Start()
        {
            _listener.Start();
            _listening = true;
            _listenerThread.Start();
        }

        /// <inheritdoc/>
        public void Stop()
        {
            _listening = false;
            _listener.Stop();
        }

        /// <inheritdoc/>
        public void RegisterStaticRoute(string verb, string route, RequestHandler handler)
        {
            if (_listening)
                throw new InvalidOperationException("Cannot add route while server is listening.");

            if (_staticHandlers.TryGetValue(route, out var methodDict) && methodDict.ContainsKey(verb))
                throw new ArgumentException("This route has already been registered.");

            if (!_staticHandlers.ContainsKey(route))
                _staticHandlers[route] = new Dictionary<string, RequestHandler>(StringComparer.OrdinalIgnoreCase);

            _staticHandlers[route][verb] = handler;
        }

        /// <inheritdoc/>
        public void RegisterResourceRoute(string verb, string route, RequestHandler handler)
        {
            if (_listening)
                throw new InvalidOperationException("Cannot add route while server is listening.");

            if (_resourceHandlers.TryGetValue(route, out var methodDict) && methodDict.ContainsKey(verb))
                throw new ArgumentException("This route has already been registered.");

            if (!_resourceHandlers.ContainsKey(route))
                _resourceHandlers[route] = new Dictionary<string, RequestHandler>(StringComparer.OrdinalIgnoreCase);

            _resourceHandlers[route][verb] = handler;
        }

        /// <summary>
        /// Main loop for accepting incoming connections.
        /// Launches <see cref="RunSingle(TcpClient)"/> for each request.
        /// </summary>
        private void Run()
        {
            Trace.TraceInformation("Listening thread has started listening for incoming connections.");

            try
            {
                while (_listening)
                {
                    var connection = _listener.AcceptTcpClient();
                    Task.Run(() => RunSingle(connection)).ConfigureAwait(false);
                    //new Thread(() => RunSingle(connection)) { Name = "Processing Thread" }.Start();
                }
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.Interrupted)
                    Trace.TraceInformation("Listening thread was interrupted.");
                else
                    Trace.TraceWarning("Listening thread was stopped: " + e.SocketErrorCode);
            }
            catch (InvalidOperationException) // might occur during testing, seems to be a race-condition
            {
                Trace.TraceWarning("Listening thread attempted to accept when not listening.");
            }

            Trace.TraceInformation("Listening thread has stopped listening for incoming connections.");
        }

        /// <summary>
        /// General handler for parsing incoming requests, invoking the respective handlers and returning responses.
        /// </summary>
        /// <param name="connection">Connection to the client.</param>
        private async Task RunSingle(ITcpClient connection)
        {
            try
            {
                Trace.TraceInformation($"Starting to process incoming request from {connection.RemoteEndPoint}.");
                // reader and writer automatically get closed
                using var reader = new StreamReader(connection.GetReadStream());
                using var writer = new StreamWriter(connection.GetWriteStream()) { NewLine = "\r\n" };

                var firstLine = await reader.ReadLineAsync();

                // check if stream immediately ends
                if (firstLine is null)
                {
                    await WriteHttpErrorToStream(writer, HttpStatusCode.BadRequest);
                    return;
                }

                var firstLineParts = firstLine.Split(' ');

                // check for correct elements in header
                if (firstLineParts.Length != 3 || firstLineParts[2] != "HTTP/1.1")
                {
                    await WriteHttpErrorToStream(writer, HttpStatusCode.BadRequest);
                    return;
                }

                var verb = firstLineParts[0];
                var route = firstLineParts[1];

                // check if the route is known
                if (!(GetBestHandlerForRoute(route) is (var methodDict, var resources)))
                {
                    await WriteHttpErrorToStream(writer, HttpStatusCode.NotFound);
                    return;
                }

                // check if the method (verb) is allowed on the resource
                if (!methodDict.TryGetValue(verb, out var handler))
                {
                    await WriteHttpErrorToStream(writer, HttpStatusCode.MethodNotAllowed);
                    return;
                }

                // gather headers
                var headers = new Dictionary<string, string>();
                int requestLength = 0;
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();

                    // empty line separates header from body
                    if (String.IsNullOrEmpty(line))
                        break;

                    var delimiterPos = line.IndexOf(':');

                    // check for no delimiter
                    if (delimiterPos < 0)
                        continue;

                    var key = line[..delimiterPos];
                    var value = line[(delimiterPos + 1)..].Trim(' ');

                    // check for length of request
                    if (String.Compare(key, "Content-Length", StringComparison.OrdinalIgnoreCase) == 0
                        && !int.TryParse(value, out requestLength))
                    {
                        await WriteHttpErrorToStream(writer, HttpStatusCode.LengthRequired);
                        return;
                    }

                    headers[key] = value;
                }

                string body = string.Empty;
                if (requestLength > 0)
                {
                    // rest of request is body/payload
                    var bodyBuffer = new char[requestLength];
                    //var bodyBuffer = new byte[requestLength];
                    await reader.ReadBlockAsync(bodyBuffer);
                    //reader.BaseStream.Read(bodyBuffer);
                    body = new string(bodyBuffer);
                    //body = reader.CurrentEncoding.GetString(bodyBuffer);
                }

                var requestContext = new RequestContext(verb, route, resources, headers, body);
                Trace.TraceInformation($"Invoking request handler for {verb} {route}.");

                // invoke handler and send response
                try
                {
                    var response = await handler(requestContext);
                    await writer.WriteLineAsync("HTTP/1.1 " + (int)response.StatusCode + " " + response.StatusCode);

                    foreach (var header in response.Headers)
                        await writer.WriteLineAsync(header.Key + ": " + header.Value);

                    if (response.Payload.Length > 0)
                    {
                        await writer.WriteLineAsync("Content-Length: " + writer.Encoding.GetByteCount(response.Payload));
                        await writer.WriteLineAsync();
                        await writer.WriteAsync(response.Payload);
                    }
                    await writer.FlushAsync();
                }
                catch (Exception e)
                {
                    await WriteHttpErrorToStream(writer, HttpStatusCode.InternalServerError);
                    Trace.TraceError(e.ToString());
                    Debug.Fail("Exception occurred during processing.",
                        $"Request: {verb} {route}" + Environment.NewLine +
                        $"Headers: {String.Join(Environment.NewLine, headers.Select(h => $"{h.Key}: {h.Value}"))}" + Environment.NewLine +
                        $"Payload: {body}");
                }
            }
            catch (IOException e)
            {
                Trace.TraceError(e.ToString());
                Debug.Fail(e.Message, e.ToString());
            }
            finally
            {
                connection.Close();
                Trace.TraceInformation("Finished processing request.");
            }
        }

        /// <summary>
        /// Helper method for generating errors and sending them to the client.
        /// </summary>
        /// <param name="writer">Writer for responding to the client.</param>
        /// <param name="statusCode">Which HTTP status code to respond with.</param>
        private static async Task WriteHttpErrorToStream(StreamWriter writer, HttpStatusCode statusCode)
        {
            Trace.TraceWarning("Client-side error; StatusCode=" + statusCode);
            await writer.WriteLineAsync("HTTP/1.1 " + (int)statusCode + " " + statusCode);
            await writer.WriteLineAsync("Content-Length: 0");
            await writer.WriteLineAsync();
            await writer.FlushAsync();
        }

        /// <summary>
        /// Helper method for finding the appropriate handler for a route.
        /// </summary>
        /// <remarks>
        /// This method does not actually return a handler but rather a dictionary containing potential handlers.
        /// </remarks>
        /// <param name="route">The route whose handler is sought.</param>
        /// <returns>Either a tuple containing the found handlers as well as
        /// the extracted resource strings or <see langword="null"/>.</returns>
        private (Dictionary<string, RequestHandler> methodDict, string[] resources)? GetBestHandlerForRoute(string route)
        {
            // check whether the route is a known static route
            if (_staticHandlers.TryGetValue(route, out var methodDict))
                return (methodDict, new string[] { });

            // check whether the route is a known resource route
            // e.g. GET /api/user/testuser/inventory should match GET /api/user/%/inventory
            var parts = route.Split('/'); // parts to compare against
            foreach (var rroute in _resourceHandlers.Keys)
            {
                var rparts = rroute.Split('/'); // parts that should be matched

                string[]? resources = MatchRoutes(parts, rparts); // resources returned if routes matched

                if (resources != null) // matching route found
                    return (_resourceHandlers[rroute], resources);
            }

            return null; // not known

            // helper method for comparing routes
            static string[]? MatchRoutes(string[] actual, string[] template)
            {
                // skip if lengths don't match
                if (actual.Length != template.Length)
                    return null;

                List<string> resources = new List<string>();

                // check each part that isn't referencing a resource
                for (int i = 0; i < actual.Length; ++i)
                {
                    if (template[i] == ResourceMarker)
                        resources.Add(actual[i]); // save resources
                    else if (actual[i] != template[i])
                        return null; // routes don't match
                }

                return resources.ToArray();
            }
        }

        public void Dispose()
        {
            _listening = false;
            _listener.Stop();
        }
    }
}
