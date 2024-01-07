using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SoapstoneLib.Proto;
using SoapstoneLib.Proto.Internal;

namespace SoapstoneLib
{
    /// <summary>
    /// Runs a Soapstone server given a service implementation.
    /// 
    /// Full server exception stack traces will visible to clients when RpcException is not used.
    /// </summary>
    public static class SoapstoneServer
    {
        private static readonly object initLock = new object();
        private static volatile SoapstoneServiceV1 service;
        private static volatile IHost host;

        /// <summary>
        /// Initializes a Kestrel server with the given Soapstone service instance.
        /// 
        /// The server runs on the KnownServer port, if it is available, and a random port otherwise.
        /// 
        /// This can only be done once.
        /// </summary>
        public static async Task RunAsync(KnownServer server, SoapstoneServiceV1 newService, string[] args = null)
        {
            if (newService == null)
            {
                throw new ArgumentNullException(nameof(newService));
            }
            // By default, ASP.NET gRPC is static.
            // https://github.com/grpc/grpc-dotnet/issues/1628 has some potential pointers to alternatives.
            lock (initLock)
            {
                if (service != null)
                {
                    throw new InvalidOperationException($"SoapstoneService already initialized with {service}");
                }
                service = newService;
            }
            int port = server.PortHint;
            if (server.IsPortInUse())
            {
                port = 0;
            }
            host = CreateHostBuilder(port, args ?? Array.Empty<string>()).Build();
            await host.RunAsync();
        }

        /// <summary>
        /// Determines the port the Kestrel server is currently running at.
        /// </summary>
        public static int? GetRunningPort()
        {
            if (host == null)
            {
                return null;
            }
            try
            {
                // https://stackoverflow.com/questions/71865319/net-core-6-0-get-kestrels-dynamically-bound-port-port-0
                IServer server = host.Services.GetRequiredService<IServer>();
                IServerAddressesFeature addressFeature = server.Features.Get<IServerAddressesFeature>();
                if (addressFeature.Addresses.Count > 0)
                {
                    return new Uri(addressFeature.Addresses.First()).Port;
                }
            }
            catch (Exception)
            {
            }
            return null;
        }

        private static SoapstoneServiceV1 GetService()
        {
            if (service == null)
            {
                throw new RpcException(new Status(StatusCode.Internal, "SoapstoneService initialized in an invalid state"));
            }
            return service;
        }

        private sealed class SoapstoneImpl : Soapstone.SoapstoneBase
        {
            public override async Task<ServerInfoResponse> GetServerInfo(ServerInfoRequest request, ServerCallContext context)
            {
                // Note: context.Peer is the caller port. context.Host is this port
                SoapstoneServiceV1 service = GetService();
                return await service.GetServerInfo(context);
            }

            public override async Task<SearchObjectsResponse> SearchObjects(SearchObjectsRequest request, ServerCallContext context)
            {
                SoapstoneServiceV1 service = GetService();
                EditorResource resource = CheckResource(request.Resource, "resource");
                PropertySearch search = InternalConversions.FromPropertySearch(CheckNonNull(request.Search, "search"));
                RequestedProperties properties = new RequestedProperties { Properties = request.Properties };
                SearchOptions options = request.Options ?? new SearchOptions();
                IEnumerable<SoulsObject> result = new List<SoulsObject>();
                if (InternalConversions.FromPrimaryKeyType(CheckNonNull(request.ResultType, "resultType"), out SoulsKeyType resultType))
                {
                    result = await service.SearchObjects(context, resource, resultType, search, properties, options);
                }
                return new SearchObjectsResponse { Results = { result.Select(InternalConversions.ToGameObject) } };
            }

            public override async Task<GetObjectResponse> GetObject(GetObjectRequest request, ServerCallContext context)
            {
                SoapstoneServiceV1 service = GetService();
                EditorResource resource = CheckResource(request.Resource, "resource");
                SoulsKey key = InternalConversions.FromPrimaryKey(CheckNonNull(request.Key, "key"));
                RequestedProperties properties = new RequestedProperties { Properties = request.Properties };
                IEnumerable<SoulsObject> result = await service.GetObjects(context, resource, new List<SoulsKey> { key }, properties);
                SoulsObject first = result.First();
                // Could also throw NotFound here, but keep things nullable for now.
                // Would just require a client/server update if changed.
                return new GetObjectResponse { Result = first == null ? null : InternalConversions.ToGameObject(first) };
            }

            public override async Task<BatchGetObjectsResponse> BatchGetObjects(BatchGetObjectsRequest request, ServerCallContext context)
            {
                SoapstoneServiceV1 service = GetService();
                EditorResource resource = CheckResource(request.Resource, "resource");
                List<SoulsKey> keys = request.Keys.Select(InternalConversions.FromPrimaryKey).ToList();
                RequestedProperties properties = new RequestedProperties { Properties = request.Properties };
                IEnumerable<SoulsObject> result = await service.GetObjects(context, resource, keys, properties);
                return new BatchGetObjectsResponse { Results = { result.Select(InternalConversions.ToGameObject) } };
            }

            public override async Task<OpenResourceResponse> OpenResource(OpenResourceRequest request, ServerCallContext context)
            {
                SoapstoneServiceV1 service = GetService();
                EditorResource resource = CheckResource(request.Resource, "resource");
                await service.OpenResource(context, resource);
                return new OpenResourceResponse();
            }

            public override async Task<OpenObjectResponse> OpenObject(OpenObjectRequest request, ServerCallContext context)
            {
                SoapstoneServiceV1 service = GetService();
                EditorResource resource = CheckResource(request.Resource, "resource");
                SoulsKey key = InternalConversions.FromPrimaryKey(CheckNonNull(request.Key, "key"));
                await service.OpenObject(context, resource, key);
                return new OpenObjectResponse();
            }

            public override async Task<OpenSearchResponse> OpenSearch(OpenSearchRequest request, ServerCallContext context)
            {
                SoapstoneServiceV1 service = GetService();
                EditorResource resource = CheckResource(request.Resource, "resource");
                PropertySearch search = InternalConversions.FromPropertySearch(CheckNonNull(request.Search, "search"));
                if (InternalConversions.FromPrimaryKeyType(CheckNonNull(request.ResultType, "resultType"), out SoulsKeyType resultType))
                {
                    await service.OpenSearch(context, resource, resultType, search, request.OpenFirstResult);
                }
                return new OpenSearchResponse();
            }

            private static T CheckNonNull<T>(T val, string name)
            {
                if (val == null)
                {
                    throw new ArgumentNullException(name);
                }
                return val;
            }

            private static EditorResource CheckResource(EditorResource val, string name)
            {
                if (val == null)
                {
                    throw new ArgumentNullException(name);
                }
                if (val.Type == EditorResourceType.Unspecified)
                {
                    throw new ArgumentException($"EditorResource argument must define a type: {val}");
                }
                if (val.Game == FromSoftGame.Unspecified)
                {
                    throw new ArgumentException($"EditorResource argument must define a game: {val}");
                }
                return val;
            }
        }

        // Default ASP.NET gRPC setup, as of September 2022.
        // TODO: Should this switch to use WebApplication?
        private static IHostBuilder CreateHostBuilder(int port, string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseKestrel()
                        .UseStartup<Startup>()
                        .ConfigureKestrel(serverOptions =>
                        {
                            serverOptions.ConfigureEndpointDefaults(listenOptions =>
                            {
                                // Otherwise, with default value (1 and 2), results in "Error starting gRPC call.
                                // HttpRequestException: An HTTP/2 connection could not be established because
                                // the server did not complete the HTTP/2 handshake."
                                listenOptions.Protocols = HttpProtocols.Http2;
                            });
                        })
                        .UseUrls($"http://127.0.0.1:{port}");
                });
        }

        private sealed class Startup
        {
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddHostFiltering(options =>
                {
                    options.AllowedHosts = new[] { "localhost", "127.0.0.1", "[::1]" };
                });
                // This option returns exception messages, but unfortunately stack traces will require interceptors.
                // Something like https://stackoverflow.com/questions/52078579/global-exception-handling-in-grpc-c-sharp
                services.AddGrpc(options => options.EnableDetailedErrors = true);
            }

            public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }

                app.UseRouting();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapGrpcService<SoapstoneImpl>();
                });
            }
        }
    }
}
