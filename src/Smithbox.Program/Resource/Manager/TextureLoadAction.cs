using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Resource.ResourceManager;

namespace StudioCore.Resource;

public struct LoadTPFResourcesAction
{
    /// <summary>
    /// Job this action belongs to.
    /// </summary>
    public ResourceJob _job;

    /// <summary>
    /// Virtual resource path used for this TPF
    /// </summary>
    public string _virtpathbase = null;

    /// <summary>
    /// TPF container
    /// </summary>
    public TPF _tpf = null;

    /// <summary>
    /// Absolute resource path used for this TPF
    /// </summary>
    public string _filePath = null;

    /// <summary>
    /// Resource access level
    /// </summary>
    public AccessLevel _accessLevel = AccessLevel.AccessGPUOptimizedOnly;

    public LoadTPFResourcesAction(ResourceJob job, string virtpathbase, TPF tpf, AccessLevel al)
    {
        _job = job;
        _virtpathbase = virtpathbase;
        _tpf = tpf;
        _accessLevel = al;
    }

    public LoadTPFResourcesAction(ResourceJob job, string virtpathbase, string filePath, AccessLevel al)
    {
        _job = job;
        _virtpathbase = virtpathbase;
        _filePath = filePath;
        _accessLevel = al;
    }
}