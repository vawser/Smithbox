using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using SoapstoneLib.Proto;

namespace SoapstoneLib
{
    /// <summary>
    /// Service class to be implemented by servers.
    /// 
    /// This can throw RpcException to return specific error statuses. Otherwise, other exceptions
    /// are transformed into Internal error statuses, alongside the full server stack trace.
    /// </summary>
    public abstract class SoapstoneServiceV1
    {
        /// <summary>
        /// Returns basic info about the editor's current state. All servers must implement this.
        /// Editor resources are parts of an editor which can be individually loaded or unloaded.
        /// Once a resource is loaded, various objects and functionality can be straightforwardly accessed within it.
        /// </summary>
        public abstract Task<ServerInfoResponse> GetServerInfo(ServerCallContext context);

        /// <summary>
        /// Get objects matching a search query, with requested properties.
        /// A property does not have to be requested to search against it.
        /// </summary>
        public virtual Task<IEnumerable<SoulsObject>> SearchObjects(
            ServerCallContext context,
            EditorResource resource,
            SoulsKeyType resultType,
            PropertySearch search,
            RequestedProperties properties,
            SearchOptions options)
        {
            throw new RpcException(new Status(StatusCode.Unimplemented, "Not supported in server instance"));
        }

        /// <summary>
        /// Returns game objects, within an editor resource type.
        /// This can be used if the exact key is known, or to get more properties after a broader search.
        /// </summary>
        public virtual Task<IEnumerable<SoulsObject>> GetObjects(
            ServerCallContext context,
            EditorResource resource,
            List<SoulsKey> keys,
            RequestedProperties properties)
        {
            throw new RpcException(new Status(StatusCode.Unimplemented, "Not supported in server instance"));
        }

        /// <summary>
        /// Open a resource, like a map by name.
        /// </summary>
        public virtual Task OpenResource(ServerCallContext context, EditorResource resource)
        {
            throw new RpcException(new Status(StatusCode.Unimplemented, "Not supported in server instance"));
        }

        /// <summary>
        /// Jump to or frame the given object within the editor.
        /// </summary>
        public virtual Task OpenObject(ServerCallContext context, EditorResource resource, SoulsKey key)
        {
            throw new RpcException(new Status(StatusCode.Unimplemented, "Not supported in server instance"));
        }

        /// <summary>
        /// Start a given search in the editor.
        /// </summary>
        public virtual Task OpenSearch(
            ServerCallContext context,
            EditorResource resource,
            SoulsKeyType resultType,
            PropertySearch search,
            bool openFirstResult)
        {
            throw new RpcException(new Status(StatusCode.Unimplemented, "Not supported in server instance"));
        }
    }
}
