using System.Threading.Tasks;

namespace StudioCore.Renderer;

/// <summary>
///     Builds a <see cref="ResourceJob" />. Normally you don't need this: <see cref="ResourcePool.LoadContainer(string, LoadOptions?)" />
///     creates and runs a job for you.
/// </summary>
public class ResourceJobBuilder
{
    private readonly ResourceJob _job;

    public ResourceJobBuilder(string name)
    {
        _job = new ResourceJob(name);
    }

    internal void SetHidden(bool hidden)
    {
        _job.Hidden = hidden;
    }

    internal void AddContainer(ContainerLoadAction action)
    {
        _job.AddContainer(action);
    }

    public Task Complete()
    {
        // Build the job, register it with the task manager, and start it
        ResourceManager.ActiveJobProgress[_job] = 0;
        return _job.Complete();
    }
}
