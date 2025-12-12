using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Resource;

public interface IResourceTask
{
    public void Run();
    public Task RunAsync(IProgress<int> progress);

    /// <summary>
    ///     Get an estimate of the size of a task (i.e. how many files to load)
    /// </summary>
    /// <returns></returns>
    public int GetEstimateTaskSize();
}