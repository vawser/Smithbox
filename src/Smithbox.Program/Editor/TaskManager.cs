using Microsoft.Extensions.Logging;
using StudioCore.Editors.ParamEditor.META;
using StudioCore.Tasks;
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

    public static ConcurrentDictionary<string, LiveTask> GetTasks()
    {
        return _liveTasks;
    }

    public static void ThrowTaskExceptions()
    {
        // Allows exceptions in tasks to be caught by crash handler.
        foreach (KeyValuePair<string, LiveTask> task in _liveTasks)
        {
            AggregateException ex = task.Value.Task.Exception;
            if (ex != null)
            {
                // Use this for XML errors to see the file before the file with the bad syntax
                throw ex;
            }
        }
    }

    public class LiveTask
    {
        public readonly LogPriority LogPriority;

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

        public readonly string TaskName;
        public readonly string TaskCompletedMessage;
        public readonly string TaskFailedMessage;

        /// <summary>
        ///     If true, task will run again after finishing.
        /// </summary>
        public bool HasScheduledRequeue;

        /// <summary>
        ///     True for tasks that are intended to be running as long as DSMS is running.
        /// </summary>
        public bool PassiveTask;

        public LiveTask() { }

        public LiveTask(string taskId, string taskName, string taskCompletedMessage, string taskFailedMessage, RequeueType requeueType, bool silentFail, Action act)
        {
            TaskId = taskId;
            TaskName = taskName;
            TaskCompletedMessage = taskCompletedMessage;
            TaskFailedMessage = taskFailedMessage;

            RequeueBehavior = requeueType;
            SilentFail = silentFail;
            LogPriority = LogPriority.Normal;
            TaskAction = act;
        }

        public LiveTask(string taskId, string taskName, string taskCompletedMessage, string taskFailedMessage, RequeueType requeueType, bool silentFail, LogPriority logPriority,
            Action act)
        {
            TaskId = taskId;
            TaskName = taskName;
            TaskCompletedMessage = taskCompletedMessage;
            TaskFailedMessage = taskFailedMessage;
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
                    if (TaskName != "")
                    {
                        TaskLogs.AddLog($"{TaskName} {TaskCompletedMessage}",
                            LogLevel.Information, LogPriority);
                    }
                }

                try
                {
                    TaskAction.Invoke();
                    if (TaskName != "")
                    {
                        TaskLogs.AddLog($"{TaskName} {TaskCompletedMessage}",
                        LogLevel.Information, LogPriority);
                    }
                }
                catch (Exception e)
                {
                    if (SilentFail)
                    {
                        if (e.InnerException != null)
                        {
                            e = e.InnerException;
                        }

                        if (TaskName != "")
                        {
                            TaskLogs.AddLog($"{TaskName} {TaskFailedMessage}",
                            LogLevel.Error, LogPriority, e);
                        }
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
