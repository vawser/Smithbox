using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using SoapstoneLib.Proto;
using SoapstoneLib.Proto.Internal;

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

        /// <summary>
        /// Execute a mass edit script on the server's param data.
        /// The script uses the same syntax as the Param Editor's Mass Edit system
        /// (semicolon-delimited commands: param, row, cell operations, etc.).
        /// </summary>
        public virtual Task<ExecuteMassEditResponse> ExecuteMassEdit(ServerCallContext context, string script)
        {
            throw new RpcException(new Status(StatusCode.Unimplemented, "Not supported in server instance"));
        }

        /// <summary>
        /// Hot-reload param changes to the running game process.
        /// If paramNames is null or empty, all reloadable params are reloaded.
        /// Returns information about which params were reloaded and which failed.
        /// </summary>
        public virtual Task<ReloadParamsResponse> ReloadParams(ServerCallContext context, string[] paramNames)
        {
            throw new RpcException(new Status(StatusCode.Unimplemented, "Not supported in server instance"));
        }

        /// <summary>
        /// List all available param names in the currently loaded project.
        /// </summary>
        public virtual Task<ListParamsResponse> ListParams(ServerCallContext context)
        {
            throw new RpcException(new Status(StatusCode.Unimplemented, "Not supported in server instance"));
        }

        /// <summary>
        /// Get detailed information about a specific param: its type, row count, and field definitions.
        /// </summary>
        public virtual Task<DescribeParamResponse> DescribeParam(ServerCallContext context, string paramName)
        {
            throw new RpcException(new Status(StatusCode.Unimplemented, "Not supported in server instance"));
        }

        /// <summary>
        /// Get all field values for a specific row in a param, by row ID.
        /// </summary>
        public virtual Task<GetParamRowResponse> GetParamRow(ServerCallContext context, string paramName, int rowIndex, bool vanilla = false)
        {
            throw new RpcException(new Status(StatusCode.Unimplemented, "Not supported in server instance"));
        }

        /// <summary>
        /// Get all rows that share a given row ID (handles duplicate IDs).
        /// </summary>
        public virtual Task<GetParamRowsResponse> GetParamRows(ServerCallContext context, string paramName, int rowId, bool vanilla = false)
        {
            throw new RpcException(new Status(StatusCode.Unimplemented, "Not supported in server instance"));
        }

        /// <summary>
        /// List all rows in a param (ID and Name only, for quick overview).
        /// </summary>
        /// <summary>
        /// Set a single cell value by row index. Handles duplicate-ID params precisely.
        /// </summary>
        public virtual Task<SetParamCellResponse> SetParamCell(
            ServerCallContext context, string paramName, int rowIndex, string fieldName, string value)
        {
            throw new RpcException(new Status(StatusCode.Unimplemented, "Not supported in server instance"));
        }

        /// <summary>
        /// List all rows in a param (ID and Name only, for quick overview).
        /// </summary>
        public virtual Task<ListParamRowsResponse> ListParamRows(ServerCallContext context, string paramName, bool vanilla = false)
        {
            throw new RpcException(new Status(StatusCode.Unimplemented, "Not supported in server instance"));
        }
    }
}
