using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editor;

public class TaskManager
{
    /// <summary>
    ///     Behavior of a LiveTask when the same task ID is already running.
    /// </summary>
    public enum RequeueType
    {
        // Don't requeue.
        None,

        // Wait for already-active task to finish, then run this task.
        WaitThenRequeue,

        // Already-active task is told to run again after it finishes.
        Repeat
    }

    private static volatile ConcurrentDictionary<string, LiveTask> _liveTasks = new();

    /// <summary>
    ///     Number of non-passive tasks that are currently running.
    /// </summary>
    public static int ActiveTaskNum { get; private set; }

    public static void Run(LiveTask liveTask)
    {
        liveTask.Run();
    }

    public static void RunPassiveTask(LiveTask liveTask)
    {
        liveTask.PassiveTask = true;
        liveTask.Run();
    }

    public static void WaitAll()
    {
        while (GetActiveTasks().Any())
        {
            IEnumerator<LiveTask> e = GetActiveTasks().GetEnumerator();
            e.MoveNext();
            e.Current.Task.Wait();
        }
    }

    public static IEnumerable<LiveTask> GetActiveTasks()
    {
        return _liveTasks.Values.ToList().Where(t => !t.PassiveTask);
    }

    /// <summary>
    ///     Number of active tasks. Dpes not include passive tasks.
    /// </summary>
    public static bool AnyActiveTasks()
    {
        return ActiveTaskNum > 0;
    }

    public static List<string> GetLiveThreads()
    {
        return new List<string>(_liveTasks.Keys);
    }

    public static void ThrowTaskExceptions()
    {
        // Allows exceptions in tasks to be caught by crash handler.
        foreach (KeyValuePair<string, LiveTask> task in _liveTasks)
        {
            AggregateException ex = task.Value.Task.Exception;
            if (ex != null)
            {
                throw ex;
            }
        }
    }

    public class LiveTask
    {
        public readonly TaskLogs.LogPriority LogPriority;

        /// <summary>
        ///     Behavior of a LiveTask when the same task ID is already running.
        /// </summary>
        public readonly RequeueType RequeueBehavior;

        /// <summary>
        ///     If true, exceptions will be suppressed and logged.
        /// </summary>
        public readonly bool SilentFail;

        public readonly Action TaskAction;

        /// <summary>
        ///     Unique identifier for task.
        ///     If more than one LiveTask with the same ID is run, RequeueType is checked.
        /// </summary>
        public readonly string TaskId;

        /// <summary>
        ///     If true, task will run again after finishing.
        /// </summary>
        public bool HasScheduledRequeue;

        /// <summary>
        ///     True for tasks that are intended to be running as long as DSMS is running.
        /// </summary>
        public bool PassiveTask;

        public LiveTask() { }

        public LiveTask(string taskId, RequeueType requeueType, bool silentFail, Action act)
        {
            TaskId = taskId;
            RequeueBehavior = requeueType;
            SilentFail = silentFail;
            LogPriority = TaskLogs.LogPriority.Normal;
            TaskAction = act;
        }

        public LiveTask(string taskId, RequeueType requeueType, bool silentFail, TaskLogs.LogPriority logPriority,
            Action act)
        {
            TaskId = taskId;
            RequeueBehavior = requeueType;
            SilentFail = silentFail;
            LogPriority = logPriority;
            TaskAction = act;
        }

        public Task Task { get; private set; }

        public void Run()
        {
            if (_liveTasks.TryGetValue(TaskId, out LiveTask oldLiveTask))
            {
                if (oldLiveTask.RequeueBehavior == RequeueType.WaitThenRequeue)
                {
                    oldLiveTask.Task.Wait();
                }
                else if (oldLiveTask.RequeueBehavior == RequeueType.Repeat)
                {
                    oldLiveTask.HasScheduledRequeue = true;
                    return;
                }
                else
                {
                    return;
                }
            }

            if (!PassiveTask)
            {
                ActiveTaskNum++;
            }

            _liveTasks[TaskId] = this;

            CreateTask();
            Task.Start();
        }

        private void CreateTask()
        {
            Task = new Task(() =>
            {
                if (PassiveTask)
                {
                    TaskLogs.AddLog($"Running passive task: {TaskId}",
                        LogLevel.Information, LogPriority);
                }

                try
                {
                    TaskAction.Invoke();
                    TaskLogs.AddLog($"Task Completed: {TaskId}",
                        LogLevel.Information, LogPriority);
                }
                catch (Exception e)
                {
                    if (SilentFail)
                    {
                        if (e.InnerException != null)
                        {
                            e = e.InnerException;
                        }
                        TaskLogs.AddLog($"Task Failed: {TaskId}",
                            LogLevel.Error, LogPriority, e);
                    }
                    else
                    {
                        throw;
                    }
                }

                if (HasScheduledRequeue)
                {
                    HasScheduledRequeue = false;
                    CreateTask();
                    Task.Start();
                }
                else
                {
                    if (!PassiveTask)
                    {
                        ActiveTaskNum--;
                    }

                    _liveTasks.TryRemove(TaskId, out _);
                }
            });
        }
    }
}
