using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using SoapstoneLib.Proto;
using SoapstoneLib.Proto.Internal;

namespace SoapstoneLib
{
    /// <summary>
    /// Client API for a Soapstone server instance. Clients are initialized through SoapstoneClient.Provider objects.
    /// 
    /// This makes RPCs to the server. If the server program closes, RPC methods may throw an RpcException with an
    /// Unavailable status. This can be guarded against by using Provider.TryGetClient before calling the client,
    /// but it is not fully avoidable if the server becomes unavailable immediately after that.
    /// 
    /// Any server exceptions will result in client RpcExceptions with the appropriate status and error message.
    /// </summary>
    public sealed class SoapstoneClient
    {
        private readonly TimeSpan? deadline;
        private readonly GrpcChannel channel;
        private readonly Soapstone.SoapstoneClient client;

        /// <summary>
        /// Create a SoapstoneClient provider, which can be used to dynamically get a functioning client.
        /// 
        /// This is required for all clients to use at the moment. Any time you need to call a server, first
        /// get the client from a provider, which will return the same client as before if it's still available.
        /// This is meant to handle server editors being opened and closed, as well as configuring settings
        /// in client editors.
        /// </summary>
        public static Provider GetProvider(KnownServer server = null)
        {
            return new Provider(server);
        }

        internal SoapstoneClient(string address, TimeSpan? deadline)
        {
            // This is needed for .NET Core 3 but not .NET >5. Use in case backporting is needed.
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            SocketsHttpHandler handler = new SocketsHttpHandler
            {
                // This is the only default value set if a handler is not specified.
                // It comes from gRPC-internal HttpHandlerFactory
                EnableMultipleHttp2Connections = true,
                // The main thing to configure here: be more aggressive about checking if the server
                // is up, since we'd prefer to mark clients unavailable on the faster side.
                KeepAlivePingDelay = TimeSpan.FromSeconds(5),
                KeepAlivePingTimeout = TimeSpan.FromSeconds(5),
            };
            channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions { HttpHandler = handler });
            client = new Soapstone.SoapstoneClient(channel);
            this.deadline = deadline;
        }

        /// <summary>
        /// Returns basic info about the editor's current state. All servers must implement this.
        /// Editor resources are parts of an editor which can be individually loaded or unloaded.
        /// Once a resource is loaded, various objects and functionality can be straightforwardly accessed within it.
        /// </summary>
        public async Task<ServerInfoResponse> GetServerInfo()
        {
            ServerInfoRequest request = new ServerInfoRequest();
            return await client.GetServerInfoAsync(request);
        }

        /// <summary>
        /// Get objects matching a search query, with requested properties.
        /// A property does not have to be requested to search against it.
        /// </summary>
        public async Task<List<SoulsObject>> SearchObjects(
            EditorResource resource,
            SoulsKeyType resultType,
            PropertySearch search,
            RequestedProperties properties = null,
            SearchOptions options = null)
        {
            SearchObjectsRequest request = new SearchObjectsRequest
            {
                Resource = resource,
                ResultType = InternalConversions.ToPrimaryKeyType(resultType),
                Search = InternalConversions.ToPropertySearch(search),
                Properties = { properties?.Properties ?? new List<RequestedProperty>() },
                Options = options,
            };
            SearchObjectsResponse response = await client.SearchObjectsAsync(request, MakeOptions());
            return response.Results.Select(InternalConversions.FromGameObject).ToList();
        }

        /// <summary>
        /// Returns a single object, within an editor resource type.
        /// This can be used if the exact key is known, or to get more properties after a broader search.
        /// It returns null if object is not found (as opposed to an error).
        /// </summary>
        public async Task<SoulsObject> GetObject(
            EditorResource resource,
            SoulsKey key,
            RequestedProperties properties = null)
        {
            GetObjectRequest request = new GetObjectRequest
            {
                Resource = resource,
                Key = InternalConversions.ToPrimaryKey(key),
                Properties = { properties?.Properties ?? new List<RequestedProperty>() },
            };
            GetObjectResponse response = await client.GetObjectAsync(request, MakeOptions());
            // At present, this may be empty, as the client may know the exact key and request it
            return response.Result == null ? null : InternalConversions.FromGameObject(response.Result);
        }

        /// <summary>
        /// Returns objects in batch, within an editor resource type.
        /// It returns only the objects which could be found.
        /// </summary>
        public async Task<List<SoulsObject>> GetObjects(
            EditorResource resource,
            IEnumerable<SoulsKey> keys,
            RequestedProperties properties = null)
        {
            BatchGetObjectsRequest request = new BatchGetObjectsRequest
            {
                Resource = resource,
                Keys = { keys.Select(InternalConversions.ToPrimaryKey) },
                Properties = { properties?.Properties ?? new List<RequestedProperty>() },
            };
            BatchGetObjectsResponse response = await client.BatchGetObjectsAsync(request, MakeOptions());
            return response.Results.Select(InternalConversions.FromGameObject).ToList();
        }

        /// <summary>
        /// Open a resource, like a map by name.
        /// </summary>
        public async Task OpenResource(EditorResource resource)
        {
            OpenResourceRequest request = new OpenResourceRequest
            {
                Resource = resource,
            };
            await client.OpenResourceAsync(request, MakeOptions());
        }

        /// <summary>
        /// Jump to or frame the given object within the editor.
        /// </summary>
        public async Task OpenObject(EditorResource resource, SoulsKey key)
        {
            OpenObjectRequest request = new OpenObjectRequest
            {
                Resource = resource,
                Key = InternalConversions.ToPrimaryKey(key),
            };
            await client.OpenObjectAsync(request, MakeOptions());
        }

        /// <summary>
        /// Start a given search in the editor.
        /// </summary>
        public async Task OpenSearch(
            EditorResource resource,
            SoulsKeyType resultType,
            PropertySearch search,
            bool openFirstResult)
        {
            OpenSearchRequest request = new OpenSearchRequest
            {
                Resource = resource,
                ResultType = InternalConversions.ToPrimaryKeyType(resultType),
                Search = InternalConversions.ToPropertySearch(search),
                OpenFirstResult = openFirstResult,
            };
            await client.OpenSearchAsync(request, MakeOptions());
        }

        private CallOptions MakeOptions()
        {
            DateTime? deadlineTime = null;
            if (deadline is TimeSpan span)
            {
                deadlineTime = DateTime.UtcNow.Add(span);
            }
            return new CallOptions(deadline: deadlineTime);
        }

        internal Task ShutdownAsync() => channel.ShutdownAsync();

        internal bool IsAvailable()
        {
            // At least for local connections, it appears as though the channel is immediately in Ready state,
            // and stays that way. Connecting state is used after a disconnection occurs, with random backoff.
            // This will need to be revisited if Connecting can occur at the start, since the result of this
            // is used to determine if a new connection may be needed.
            if (channel.State == ConnectivityState.Ready)
            {
                // We could also add additional checking here if requested, like a manual health check.
                return true;
            }
            return false;
        }

        /// <summary>
        /// Client initializer which is tolerant of servers going up/down and ports changing.
        /// 
        /// To get a client, call TryGetClient every time a client is required. If the server is
        /// detected as running, it will output a client, potentially changing server address and
        /// client configuration as needed.
        /// </summary>
        public sealed class Provider
        {
            private KnownServer targetServer;
            private TimeSpan? targetDeadline;
            private SoapstoneClient lastClient;
            private bool findServer;
            private int lastClientPort;

            internal Provider(KnownServer server = null)
            {
                targetServer = server;
                // Default deadline. Should be enough for almost all cases, unless server hangs.
                targetDeadline = TimeSpan.FromSeconds(30);
            }

            /// <summary>
            /// The server to connect to the next time TryGetClient is called.
            /// 
            /// If this is set to null, no connection will be attempted.
            /// </summary>
            public KnownServer Server
            {
                get => targetServer;
                set
                {
                    findServer = true;
                    targetServer = value;
                }
            }

            /// <summary>
            /// The deadline for all RPCs from the client side. By default, this is 30 seconds.
            /// Per-RPC overrides are currently not supported.
            /// 
            /// This means that after the client has been making an RPC for this length of the time,
            /// it will return early with a DeadlineExceeded status. The server call may also end
            /// early, but only if it checks its CancellationToken.
            /// 
            /// If this is set to null, no deadline will be enforced.
            /// </summary>
            public TimeSpan? Deadline
            {
                get => targetDeadline;
                set
                {
                    findServer = true;
                    targetDeadline = value;
                }
            }

            /// <summary>
            /// For informational purposes, the remote localhost port a server was last detected at
            /// when TryGetClient was last called, if the RPC channel still appears to be active.
            /// </summary>
            public int? LastPort
            {
                get
                {
                    if (lastClient != null && lastClient.IsAvailable() && lastClientPort > 0)
                    {
                        return lastClientPort;
                    }
                    return null;
                }
            }

            /// <summary>
            /// Outputs a client instance for accessing the Server spec.
            /// 
            /// If this returns false, no server could be found running on the given process/port.
            /// </summary>
            public bool TryGetClient(out SoapstoneClient client)
            {
                client = null;
                if (targetServer == null)
                {
                    return false;
                }
                if (!findServer && lastClient != null && lastClient.IsAvailable())
                {
                    client = lastClient;
                    return true;
                }
                if (!targetServer.FindServer(out int actualPort))
                {
                    return false;
                }
                if (actualPort == lastClientPort && lastClient != null && lastClient.IsAvailable())
                {
                    findServer = false;
                    client = lastClient;
                    return true;
                }
                if (lastClient != null)
                {
                    // Clean up in background
                    lastClient.ShutdownAsync();
                }
                lastClientPort = actualPort;
                lastClient = new SoapstoneClient("http://localhost:" + actualPort, targetDeadline);
                findServer = false;
                client = lastClient;
                return true;
            }
        }
    }
}