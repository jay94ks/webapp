using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Extensions
{
    public static class TaskExtension
    {
        /// <summary>
        /// Spawn a task after the task completed and,
        /// Return the spawned task.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Task"></param>
        /// <returns></returns>
        public static Task Then(this Task This, Func<Task> Task)
        {
            var TCS = new TaskCompletionSource<bool>();

            This.ContinueWith(X =>
            {
                Task().ContinueWith(Y =>
                {
                    TCS.SetResult(true);
                });
            });

            return TCS.Task;
        }

        /// <summary>
        /// Spawn a task after the task completed and,
        /// Return the spawned task.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Task"></param>
        /// <returns></returns>
        public static Task Then(this Task This, Func<Task, Task> Task)
        {
            var TCS = new TaskCompletionSource<bool>();

            This.ContinueWith(X =>
            {
                Task(X).ContinueWith(Y =>
                {
                    TCS.SetResult(true);
                });
            });

            return TCS.Task;
        }

        /// <summary>
        /// Spawn a task after all tasks in collection completed and,
        /// Return the spawned task.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Task"></param>
        /// <returns></returns>
        public static Task Then(this ICollection<Task> This, Func<Task> Task)
             => This.All().Then(Task);

        /// <summary>
        /// Create Waiting task which completed when both tasks completed.
        /// </summary>
        /// <returns></returns>
        public static Task<Task[]> And(this Task This, Task Other)
        {
            if (This == Other)
                return Task.FromResult(new Task[] { This });

            List<Task> Tasks = new List<Task>();
            var TCS = new TaskCompletionSource<Task[]>();

            Tasks.Add(This);
            Tasks.Add(Other);

            Task.WhenAny(This, Other)
                .ContinueWith(X =>
                {
                    bool Completed;
                    lock (Tasks)
                    {
                        Tasks.Remove(X.Result);
                        Completed = Tasks.Count <= 0;
                    }

                    if (Completed)
                        TCS.SetResult(new Task[] { This, Other });
                });

            return TCS.Task;
        }

        /// <summary>
        /// Create Waiting task which completed when all tasks in collection completed.
        /// </summary>
        /// <param name="This"></param>
        /// <returns></returns>
        public static Task<Task[]> All(this ICollection<Task> This)
        {
            if (This.Count <= 0)
                return Task.FromResult(new Task[0]);

            Task[] AllTasks = This.ToArray();

            List<Task> Tasks = new List<Task>(AllTasks);
            var TCS = new TaskCompletionSource<Task[]>();

            Task.WhenAny(This)
                .ContinueWith(X =>
                {
                    bool Completed;
                    lock (Tasks)
                    {
                        Tasks.Remove(X.Result);
                        Completed = Tasks.Count <= 0;
                    }

                    if (Completed)
                        TCS.SetResult(AllTasks);
                });

            return TCS.Task;
        }
    }
}
