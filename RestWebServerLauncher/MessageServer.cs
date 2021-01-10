using Newtonsoft.Json;
using RestWebServer;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace RestWebServerLauncher
{
    /// <summary>
    /// Implements a small server that can be used for storing and retrieving text messages.
    /// </summary>
    public class MessageServer
    {
        private readonly Dictionary<int, string> _messages;
        private readonly IWebServer _web;
        private volatile int _count;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageServer"/> class with routes accessible over HTTP
        /// for listing, reading, writing, updating and deleting messages.
        /// </summary>
        public MessageServer(IWebServer webServer)
        {
            _messages = new Dictionary<int, string>();
            _web = webServer;
            _count = 0;

            _web.RegisterStaticRoute("GET", "/messages", _ => Task.FromResult(new RestResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(ListMessages()))));
            _web.RegisterStaticRoute("POST", "/messages", ctx => Task.FromResult(new RestResponse(HttpStatusCode.Created, JsonConvert.SerializeObject(AddMessage(ctx.Payload)))));
            _web.RegisterResourceRoute("PUT", "/messages/%", ctx =>
            {
                int id = ParseIntOrZero(ctx.Resources[0]);

                if (id <= 0)
                    return Task.FromResult(new RestResponse(HttpStatusCode.BadRequest, "Invalid message id."));

                bool created = UpdateMessage(id, ctx.Payload);
                return Task.FromResult(new RestResponse(created ? HttpStatusCode.Created : HttpStatusCode.OK, string.Empty));
            });
            _web.RegisterResourceRoute("DELETE", "/messages/%", ctx =>
            {
                int id = ParseIntOrZero(ctx.Resources[0]);

                if (id <= 0)
                    return Task.FromResult(new RestResponse(HttpStatusCode.BadRequest, "Invalid message id."));

                bool deleted = RemoveMessage(id);
                return Task.FromResult(new RestResponse(deleted ? HttpStatusCode.OK : HttpStatusCode.NotFound, string.Empty));
            });
            _web.RegisterResourceRoute("GET", "/messages/%", ctx =>
            {
                int id = ParseIntOrZero(ctx.Resources[0]);

                if (id <= 0)
                    return Task.FromResult(new RestResponse(HttpStatusCode.BadRequest, "Invalid message id."));

                var text = GetMessage(id);
                return Task.FromResult(new RestResponse(text is null ? HttpStatusCode.NotFound : HttpStatusCode.OK,
                    text is null ? string.Empty : JsonConvert.SerializeObject(text)));
            });
        }

        /// <summary>
        /// Starts listening for incoming requests.
        /// </summary>
        public void Start()
            => _web.Start();

        /// <summary>
        /// Stops listening for incoming requests.
        /// </summary>
        public void Stop()
            => _web.Stop();

        private Dictionary<int, string> ListMessages()
            => _messages;

        private string? GetMessage(int id)
            => _messages.ContainsKey(id) ? _messages[id] : null;

        private int AddMessage(string text)
        {
            lock (_messages)
            {
                while (_messages.ContainsKey(++_count)) { }
                _messages[_count] = text;
                return _count;
            }
        }

        private bool UpdateMessage(int id, string text)
        {
            bool created;

            lock (_messages)
            {
                created = !_messages.ContainsKey(id);
                _messages[id] = text;
            }

            return created;
        }

        private bool RemoveMessage(int id)
        {
            lock (_messages)
                return _messages.Remove(id);
        }

        /// <summary>
        /// Attempts to parse an <see cref="Int32"/>.
        /// If parsing fails, 0 is returned.
        /// </summary>
        /// <param name="input">Input text to parse.</param>
        /// <returns>Converted integer or 0.</returns>
        private static int ParseIntOrZero(string input)
        {
            int.TryParse(input, out var num);
            return num;
        }
    }
}
